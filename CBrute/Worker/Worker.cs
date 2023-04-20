using CBrute.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CBrute.Worker
{
    /// <summary>
    /// All classes that intend to perform password generation operations in a multithreaded manner must implement this abstract class.
    /// </summary>
    public abstract class Worker : BaseClass, IDisposable
    {
        /// <summary>
        /// The range that is intended to be generated in parallel across different threads.(<see cref="StartPos"/>,<see cref="EndPos"/>)
        /// </summary>
        protected long startPos, endPos;
        /// <summary>
        /// Countdown event for pausing the password generation process
        /// </summary>
        protected CountdownEvent pauseCounter = null!;
        /// <summary>
        /// Countdown event for resuming the password generation process
        /// </summary>
        protected CountdownEvent resumeCounter = null!;
        /// <summary>
        /// Countdown event for stopping the password generation process
        /// </summary>
        protected CountdownEvent stopCounter = null!;
        /// <summary>
        /// Countdown event for indicating the end of the password generation process
        /// </summary>
        protected CountdownEvent endCounter = null!;
        /// <summary>
        /// Countdown event for indicating an error in the password generation process
        /// </summary>
        protected CountdownEvent errorCounter = null!;
        /// <summary>
        /// Countdown event for indicating the number of passwords found during the generation process
        /// </summary>
        protected CountdownEvent foundedCounter = null!;
        /// <summary>
        /// The minium password length.
        /// </summary>
        protected int min;
        /// <summary>
        /// The maximum password length.
        /// </summary>
        protected int max;
        /// <summary>
        /// Each element of this field is executed in a separate thread and the ID of each thread is equal to its index in the "list".
        /// </summary>
        protected List<BruteForce> list = null!;
        /// <summary>
        /// If the entire activity needs to be stopped, this variable must be True.
        /// </summary>
        protected bool stoped;
        /// <summary>
        /// If the entire activity needs to be paused, this variable must be True.
        /// </summary>
        protected bool pause;
        /// <summary>
        /// Indicates whether password generation activity has started or not.
        /// </summary>
        protected bool started;
        /// <summary>
        /// Number of threads.
        /// </summary>
        protected int threadCount = 1;
        /// <summary>
        /// Used as a callback function to check the generated passwords. Later, this delegate is used in different threads to check the passwords.
        /// </summary>
        protected delegate_CheckPassword check = null!;
        /// <summary>
        /// The number of threads
        /// </summary>
        public int ThreadCount { get => threadCount; }
        /// <summary>
        /// Starting position of password generation
        /// </summary>
        public long StartPos { get => startPos; }
        /// <summary>
        /// Ending position of password generation
        /// </summary>
        public long EndPos { get => endPos; }
        /// <summary>
        /// The minimum password length.
        /// </summary>
        public int MinimumPassLength { get => min; }
        /// <summary>
        /// The maximum password length.
        /// </summary>
        public int MaximumPassLength { get => max; }
        /// <summary>
        /// By using this indexer, you can access all instances of the BruteForce class in threads by their ID(threadID).
        /// </summary>
        /// <param name="threadID">The thread ID</param>
        /// <returns>Returns an instance of the BruteForce class whose ID matches the given <paramref name="threadID"/>.</returns>
        public BruteForce this[int threadID] { get => list[threadID]; }
        /// <summary>
        /// To pause the operation, set its value to True, and to resume the operation, set it to False.
        /// This is only applicable when <seealso cref="Started"/> is equal to true."
        /// </summary>
        public bool Pause
        {
            get => pause;
            set
            {
                pause = (started || !value) && value;
                if (started) pauseThreads();//We checked "pause" in "pauseThreads" function
            }
        }
        /// <summary>
        /// Indicates whether password generation activity has started or not.
        /// </summary>
        public bool Started { get => started; }
        /// <summary>
        /// You can use this function to completely stop the password generation process.
        /// </summary>
        public void Stop()
        {
            stoped = started;
            if (stoped)
                stopThreads();
        }
        /// <summary>
        /// This is used to start password generation operation in parallel.
        /// </summary>
        /// <param name="waitForWork">
        /// If you set the value of this parameter to True, the whole program will wait for other threads to finish their work, otherwise the program will not stop and continue its work.
        /// In Console applications, it is better to set the value to True.
        /// </param>
        /// <returns>If the operation has been successfully started, it returns True. This value does not indicate the success of the end of the activity!</returns>
        public abstract bool DoWork(bool waitForWork);
        /// <summary>
        /// This function stops all threads in order.
        /// </summary>
        protected void stopThreads()
        {
            foreach (BruteForce B in list)
            {
                if (!B.Started)
                    stopCounter.Signal();
                B.Stop();
            }
        }
        /// <summary>
        /// This function pauses all threads in order.
        /// </summary>
        protected void pauseThreads()
        {
            foreach (BruteForce B in list)
            {
                if (!B.Started && pause)
                    pauseCounter.Signal();
                if (!B.Started && !pause)
                    resumeCounter.Signal();
                B.Pause = pause;
            }
        }
        /// <summary>
        /// This function divides the task among different threads by setting a range for each thread based on the threadCount parameter.
        /// </summary>
        /// <param name="start">Starting position of password generation</param>
        /// <param name="end">Ending position of password generation</param>
        /// <param name="threadCount">The number of threads</param>
        /// <returns>It returns an array whose even elements represent the StartPos and odd elements represent the EndPos.</returns>
        protected static long[] getRanges(long start, long end, int threadCount)
        {
            long total = end - start + 1;
            long eachThread = total / threadCount;
            bool hasRemainder = total % threadCount != 0;
            int length = threadCount;
            long[] ranges = new long[length * 2];
            long temp = start;
            for (int i = 0; i < length; ++i)
            {
                ranges[i * 2] = temp;
                ranges[i * 2 + 1] = temp + eachThread - 1;
                temp = ranges[i * 2 + 1] + 1;
            }
            if (hasRemainder) ranges[ranges.Length - 1] = end;
            return ranges;
        }
        /// <summary>
        /// This function is used for initializing or resetting the CountdownEvents.
        /// </summary>
        protected void initializeCounters()
        {
            if (endCounter == null) endCounter = new CountdownEvent(threadCount);
            else endCounter.Reset(threadCount);
            if (pauseCounter == null) pauseCounter = new CountdownEvent(threadCount);
            else pauseCounter.Reset(threadCount);
            if (stopCounter == null) stopCounter = new CountdownEvent(threadCount);
            else stopCounter.Reset(threadCount);
            if (resumeCounter == null) resumeCounter = new CountdownEvent(threadCount);
            else resumeCounter.Reset(threadCount);
            if (errorCounter == null) errorCounter = new CountdownEvent(threadCount);
            else errorCounter.Reset(threadCount);
            if (foundedCounter == null) foundedCounter = new CountdownEvent(threadCount);
            else foundedCounter.Reset(threadCount);
        }
        /// <summary>
        /// This function is used to terminate the password generation process.
        /// </summary>
        protected void theEnd()
        {
            foreach (BruteForce B in list)
                if (B.Started) B.WorkerFoundSomething = true;
        }
        /// <summary>
        /// Destroys the threads before termination.
        /// </summary>
        public void Dispose() => Stop();
    }
}
