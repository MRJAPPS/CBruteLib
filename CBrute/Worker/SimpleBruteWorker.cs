using CBrute.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CBrute.Worker
{
    /// <summary>
    /// With this class, you can perform the password generation process using SimpleBrute in parallel.
    /// When managing events, try not to stop or remove a thread yourself as much as possible.
    /// Generally, just focus on cracking and nothing else! Otherwise, errors may occur.
    /// Try not to use any code that causes long interruptions when managing OnThread events.
    /// </summary>
    public sealed class SimpleBruteWorker : Worker
    {
        /// <summary>
        /// Constructor of the SimpleBruteWorker class
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
        /// <param name="test">This array contains the states that make up the passwords. Cannot be empty, contain duplicate elements, or contain null elements.</param>
        /// <param name="threadCount">The number of threads</param>
        /// <param name="passChecker">Used as a callback function to check the generated passwords. Later, this delegate is used in different threads to check the passwords.</param>
        public SimpleBruteWorker(long startPos, long endPos, int minimumPasswordLength, int maximumPasswordLength,
            object[] test, int threadCount, delegate_CheckPassword passChecker)
        {
            SimpleBruteWorkerE(startPos, ref endPos, minimumPasswordLength, maximumPasswordLength, test, threadCount, passChecker);
            this.startPos = startPos;
            this.endPos = endPos;
            this.min = minimumPasswordLength;
            this.max = maximumPasswordLength;
            this.threadCount = threadCount;
            this.test = test;
            this.check = passChecker;
        }
        #region errorChecking_R
        /// <summary>
        /// This function is used to detect errors that may occur during the instantiation of <see cref="SimpleBruteWorker"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void SimpleBruteWorkerE(long startPos, ref long endPos, int minimumPasswordLength, int maximumPasswordLength,
            object[] test, int threadCount, delegate_CheckPassword passChecker)
        {
            SimpleBrute.SimpleBruteE(startPos, ref endPos, minimumPasswordLength, maximumPasswordLength, test, null);
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
                SimpleBrute SB = new SimpleBrute(start, end, min, max, test, null, threadID);
                list.Add(SB);
            }
        }
        /// <summary>
        /// This function adds parallel programming related events to <paramref name="SB"/>.
        /// </summary>
        private void initializeBruteFoceListEvents(SimpleBrute SB)
        {
            SB.PasswordGenerated += (BruteForce sender, object[] pass, long generated, long total) => check(this, sender, pass, generated, total);
            SB.OnError += (BruteForce sender, Exception e) =>
            {
                bool tryAgain = false;
                OnThreadError?.Invoke(this, sender, e, ref tryAgain);
                if (tryAgain)
                {//We restart the process from the realPos point.
                    SimpleBrute onError_SB = (SimpleBrute)sender;
                    onError_SB.StartPos = onError_SB.RealPos;
                    onError_SB.Start(BruteForce.ErrorHandlingType.Event);
                }
                else
                {
                    errorCounter.Signal();
                    if (errorCounter.IsSet) OnError?.Invoke(this, e);
                }
            };
            SB.OnPause += (BruteForce sender, long generated, long total) =>
            {
                OnThreadPause?.Invoke(this, sender, generated, total);
                pauseCount();
            };
            SB.OnResume += (BruteForce sender, long generated, long total) =>
            {
                OnThreadResume?.Invoke(this, sender, generated, total);
                resumeCounter.Signal();
                if (resumeCounter.IsSet)
                {
                    resumeCounter.Reset();
                    OnResume?.Invoke(this);
                }
            };
            SB.OnRestart += (BruteForce sender, object[] pass) => OnThreadRestart?.Invoke(this, sender, pass);
            SB.OnStart += (BruteForce sender) => OnThreadStart?.Invoke(this, sender);
            SB.OnStop += (BruteForce sender, object[] pass) =>
            {
                OnThreadStop?.Invoke(this, sender, pass);
                stopCount();
            };
            SB.OnEnd += (BruteForce sender, object[] pass, bool result) =>
            {
                if (result)
                {//SB has managed to find a password!
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

        #endregion
        #endregion
        #region public_R
        #region property_R

        /// <summary>
        /// This array contains the states that make up the passwords.
        /// </summary>
        public object[] Test { get => test; }

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
        public override bool DoWork(bool waitForWork = false)
        {
            if (started) return false;
            initializeBruteFoceList();
            initializeCounters();
            List<Thread> threads = new List<Thread>();
            foreach (SimpleBrute SB in list.Cast<SimpleBrute>()) initializeBruteFoceListEvents(SB);
            started = true;
            OnStart?.Invoke(this);
            void onStopOrEndOrError(Worker p1, bool p2) => started = false;//local function
            OnEnd += onStopOrEndOrError;
            OnStop += onStopOrEndOrError;
            OnError += (p1, p2) => onStopOrEndOrError(p1, foundedCounter.CurrentCount < foundedCounter.InitialCount);
            foreach (SimpleBrute SB in list.Cast<SimpleBrute>())
            {
                threads.Add(new Thread(() => SB.Start(BruteForce.ErrorHandlingType.Event)));
                threads[threads.Count - 1].Start();
            }
            if (waitForWork) foreach (Thread t in threads) t.Join();
            return true;
        }

        #endregion
        #region protected_R

        #endregion
    }
}
