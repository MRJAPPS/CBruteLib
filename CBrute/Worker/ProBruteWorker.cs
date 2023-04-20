using CBrute.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CBrute.Worker
{
    /// <summary>
    /// With this class, you can perform the password generation process using ProBrute in parallel.
    /// When managing events, try not to stop or remove a thread yourself as much as possible.
    /// Generally, just focus on cracking and nothing else! Otherwise, errors may occur.
    /// Try not to use any code that causes long interruptions when managing OnThread events.
    /// </summary>
    public sealed class ProBruteWorker : Worker
    {
        /// <summary>
        /// Constructor of the ProBruteWorker class
        /// </summary>
        /// <param name="startPos">
        /// This parameter determines the starting position for generating passwords.
        /// It should not be less than or equal to 0 or greater than <paramref name="endPos"/>.
        /// </param>
        /// <param name="endPos">
        /// This parameter determines the position of the last password to be generated. 
        /// If you want all passwords to be generated, you can use a number smaller than or equal to 0. 
        /// This parameter should not exceed the maximum number of passwords that can be generated. 
        /// </param>
        /// <param name="minimumPasswordLength">
        /// Minimum password length. This should not be grater than <paramref name="maximumPasswordLength"/> or smaller than or equals to 0
        /// </param>
        /// <param name="maximumPasswordLength">Maximum password length.</param>
        /// <param name="test">
        /// This array contains the states that make up the passwords.
        /// Cannot be empty, contain duplicate elements, or contain null elements.
        /// This array is used in positions that are not defined in <paramref name="testInfos"/>.
        /// </param>
        /// <param name="testInfos">
        /// By using this parameter, you can define more complex states for password generation.
        /// For example, you can specify that the last element in all passwords is always equal to "abcd",
        /// or you can specify that the eighth element in all passwords only contains the array {1,2,3}.
        /// Don't worry about the password length, as CBrute manages it.
        /// </param>
        /// <param name="threadCount">The number of threads</param>
        /// <param name="passChecker">Used as a callback function to check the generated passwords. Later, this delegate is used in different threads to check the passwords.</param>
        public ProBruteWorker(long startPos, long endPos, int minimumPasswordLength, int maximumPasswordLength,
            object[] test, ProBrute.PassTestInfo[]? testInfos, int threadCount, delegate_CheckPassword passChecker)
        {
            ProBruteWorkerE(startPos, endPos, minimumPasswordLength, maximumPasswordLength,
                test, testInfos, threadCount, passChecker);
            this.startPos = startPos;
            this.endPos = endPos;
            this.min = minimumPasswordLength;
            this.max = maximumPasswordLength;
            this.threadCount = threadCount;
            this.test = test;
            this.testInfos = testInfos;
            this.check = passChecker;
        }
        #region errorChecking_R
        /// <summary>
        /// This function is used to detect errors that may occur during the instantiation of <see cref="ProBruteWorker"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ProBruteWorkerE(long startPos, long endPos, int minimumPasswordLength, int maximumPasswordLength,
            object[] test, ProBrute.PassTestInfo[]? testInfos, int threadCount, delegate_CheckPassword passChecker)
        {
            ProBrute.ProBruteE(startPos, ref endPos, minimumPasswordLength, maximumPasswordLength, test, testInfos, null);
            if (passChecker == null) throw new ArgumentNullException(nameof(passChecker));
            if (threadCount <= 0 || threadCount >= (endPos - startPos + 1))
                throw new ArgumentOutOfRangeException(nameof(threadCount));
        }
        #endregion
        #region private_R
        /// <summary>
        /// This function initializes the <see cref="Worker.list"/> field.
        /// </summary>
        private void initializeBruteFoceList()
        {
            long[] ranges = getRanges(StartPos, endPos, threadCount);
            threadCount = ranges.Length / 2;
            list = new List<BruteForce>(threadCount);
            for (int threadID = 0; threadID < ThreadCount; ++threadID)
            {
                long start = ranges[threadID * 2];
                long end = ranges[threadID * 2 + 1];
                ProBrute PB = new ProBrute(start, end, min, max, test, testInfos, null, threadID);
                list.Add(PB);
            }
        }
        /// <summary>
        /// This function adds parallel programming related events to <paramref name="PB"/>.
        /// </summary>
        private void initializeBruteFoceListEvents(ProBrute PB)
        {
            PB.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) =>
            check(this, sender, pass, generated, total);
            PB.OnError += (BruteForce sender, Exception e) =>
            {
                bool tryAgain = false;
                OnThreadError?.Invoke(this, sender, e, ref tryAgain);
                if (tryAgain)
                {
                    //We restart the process from the realPos point.
                    ProBrute onError_PB = (ProBrute)sender;
                    onError_PB.StartPos = onError_PB.RealPos;
                    onError_PB.Start(BruteForce.ErrorHandlingType.Event);
                }
                else
                {
                    errorCounter.Signal();
                    if (errorCounter.IsSet) OnError?.Invoke(this, e);
                }
            };
            PB.OnPause += (BruteForce sender, long generated, long total) =>
            {
                OnThreadPause?.Invoke(this, sender, generated, total);
                pauseCount();
            };
            PB.OnResume += (BruteForce sender, long generated, long total) =>
            {
                OnThreadResume?.Invoke(this, sender, generated, total);
                resumeCounter.Signal();
                if (resumeCounter.IsSet)
                {
                    resumeCounter.Reset();
                    OnResume?.Invoke(this);
                }
            };
            PB.OnRestart += (BruteForce sender, object[] pass) => OnThreadRestart?.Invoke(this, sender, pass);
            PB.OnStart += (BruteForce sender) => OnThreadStart?.Invoke(this, sender);
            PB.OnStop += (BruteForce sender, object[] pass) =>
            {
                OnThreadStop?.Invoke(this, sender, pass);
                stopCount();
            };
            PB.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {//PB has managed to find a password!
                if (result)
                {
                    foundedCounter.Signal();
                    if ((bool)OnThreadEnd?.Invoke(this, sender, pass, result)!) theEnd();
                    endCount();
                    return;
                }
                else
                {
                    if (stopCounter.CurrentCount < stopCounter.InitialCount)
                    {
                        stopCount();
                        return;
                    }
                    else if (pauseCounter.CurrentCount < pauseCounter.InitialCount) pauseCount();
                }
                OnThreadEnd?.Invoke(this, sender, pass, result);
                endCount();
            };
            //local methods:
            void stopCount()
            {
                stopCounter.Signal();
                if (stopCounter.IsSet)
                    OnStop?.Invoke(this, foundedCounter.CurrentCount < foundedCounter.InitialCount);
            }
            void pauseCount()
            {
                pauseCounter.Signal();
                if (pauseCounter.IsSet)
                {
                    pauseCounter.Reset();
                    OnPause?.Invoke(this);
                }
            }
            void endCount()
            {
                endCounter.Signal();
                if (endCounter.IsSet)
                {
                    OnEnd?.Invoke(this, foundedCounter.CurrentCount < foundedCounter.InitialCount);
                    return;
                }
            }
        }

        #region field_R
        //This array contains the states that make up the passwords.
        private readonly object[] test;
        private readonly ProBrute.PassTestInfo[]? testInfos;

        #endregion
        #endregion
        #region public_R
        #region property_R
        /// <summary>
        /// This array contains the states that make up the passwords.
        /// </summary>
        public object[] Test { get => test; }
        /// <summary>
        /// By using this parameter, you can define more complex states for password generation.
        /// For example, you can specify that the last element in all passwords is always equal to "abcd",
        /// or you can specify that the eighth element in all passwords only contains the array {1,2,3}.
        /// Don't worry about the password length, as CBrute manages it. 
        /// </summary>
        public ProBrute.PassTestInfo[]? TestInfos { get => testInfos; }

        #endregion
        #region events_R

        /*
        * I have tried my best to ensure that all OnThread events occur before any other main events.
        * However, there may be situations where some OnThread events occur after the main events.
        * For example, when you change the StartPos of a thread during its execution, the thread may occur later after the main event.
        * It is unlikely to be a significant problem, but if you can fix this issue, it would be better.
        * For more information see The initializeBruteFoceListEvents method
       */
        /// <summary>
        /// This event occurs when a thread finishes its task.
        /// </summary>
        public event delegate_OnThreadEnd? OnThreadEnd = null;
        /// <summary>
        /// This event occurs before the password generation operation starts in a thread.
        /// </summary>
        public event delegate_OnThreadStart? OnThreadStart = null;
        /// <summary>
        /// This event occurs when a thread is paused.
        /// </summary>
        public event delegate_OnThreadPauseOrResume? OnThreadPause = null;
        /// <summary>
        /// This event occurs when a thread is resumed.
        /// </summary>
        public event delegate_OnThreadPauseOrResume? OnThreadResume = null;
        /// <summary>
        /// This event occurs when a thread is stoped.
        /// </summary>
        public event delegate_OnThreadStopOrRestart? OnThreadStop = null;
        /// <summary>
        /// This event occurs when a thread is restarted.
        /// </summary>
        public event delegate_OnThreadStopOrRestart? OnThreadRestart = null;
        /// <summary>
        /// When an error occurs in a thread, this event is triggered.
        /// </summary>
        public event delegate_OnThreadError? OnThreadError = null;
        /// <summary>
        /// This event occurs when the entire password generation process is completed.
        /// </summary>
        public event delegate_OnStopOrEnd? OnEnd = null;
        /// <summary>
        /// This event occurs before the password generation process starts.
        /// </summary>
        public event delegate_WrokerEvent? OnStart = null;
        /// <summary>
        /// This event occurs when the password generation process is paused.
        /// </summary>
        public event delegate_WrokerEvent? OnPause = null;
        /// <summary>
        /// This event occurs when the password generation process is resumed.
        /// </summary>
        public event delegate_WrokerEvent? OnResume = null;
        /// <summary>
        /// This event occurs when the password generation process is completely stopped.
        /// </summary>
        public event delegate_OnStopOrEnd? OnStop = null;
        /// <summary>
        /// This event occurs when all threads encounter an error.
        /// </summary>
        public event delegate_OnError? OnError = null;

        #endregion
        /// <summary>
        /// This is used to start password generation operation in parallel.
        /// </summary>
        /// <param name="waitForWork">
        /// If you set the value of this parameter to True, the whole program will wait for other threads to finish their work, otherwise the program will not stop and continue its work.
        /// In Console applications, it is better to set the value to True.
        /// </param>
        /// <returns>If the operation has been successfully started, it returns True. This value does not indicate the success of the end of the activity!</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:Use index operator", Justification = "<Pending>")]
        public override bool DoWork(bool waitForWork = false)
        {
            if (started) return false;
            initializeBruteFoceList();
            initializeCounters();
            List<Thread> threads = new List<Thread>();
            foreach (ProBrute PB in list.Cast<ProBrute>()) initializeBruteFoceListEvents(PB);
            started = true;
            OnStart?.Invoke(this);
            void onStopOrEndOrError(Worker p1, bool p2) => started = false;//local function
            OnEnd += onStopOrEndOrError;
            OnStop += onStopOrEndOrError;
            OnError += (p1, p2) => onStopOrEndOrError(p1, foundedCounter.CurrentCount < foundedCounter.InitialCount);
            foreach (ProBrute PB in list.Cast<ProBrute>())
            {
                threads.Add(new Thread(() => PB.Start(BruteForce.ErrorHandlingType.Event)));
                threads[threads.Count - 1].Start();
            }
            if (waitForWork) foreach (Thread t in threads) t.Join();
            return true;
        }

        #endregion
    }
}
