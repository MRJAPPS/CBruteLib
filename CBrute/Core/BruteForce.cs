using System;
using System.Diagnostics;
using System.Threading;

namespace CBrute.Core
{
    /// <summary>
    /// This class is the base class for all classes that want to generate passwords.
    /// </summary>
    public abstract class BruteForce : BaseClass
    {
        /// <summary>
        /// The equivalent of null for testArrays. Since testArrays cannot be null, you can use this array instead.
        /// </summary>
        public static readonly object[] JunkArray = new object[1] { '\\' };
        /// <summary>
        /// Has error handling methods.
        /// </summary>
        public enum ErrorHandlingType 
        {
            /// <summary>
            /// Error handling is done through try/catch blocks. It is recommended not to use this option if you are using multithreading.
            /// </summary>
            TryCath,
            /// <summary>
            /// Error handling using events. It is recommended to use this method when working with threads.
            /// </summary>
            Event
        }
        /// <summary>
        /// A unique identifier for each BruteForce instance in each thread.
        /// </summary>
        protected int threadID = -1;
        /// <summary>
        /// If the password generation process needs to be stopped, this variable should be set to True.
        /// </summary>
        protected bool stopped;
        /// <summary>
        /// If the password generation process needs to be paused, this variable should be set to True.
        /// </summary>
        protected bool pause;
        /// <summary>
        /// When the password generation process begins, the value of this variable should be True.
        /// </summary>
        protected bool started;
        /// <summary>
        /// When StartPos changes during runtime, the value of this variable is True.
        /// </summary>
        protected bool needToRestart;
        /// <summary>
        /// Total number of possible passwords that can be generated.
        /// </summary>
        protected long total;
        /// <summary>
        /// determines the starting position for generating passwords.
        /// </summary>
        protected long startPos;
        /// <summary>
        /// determines the position of the last password to be generated. 
        /// </summary>
        protected long endPos;
        /// <summary>
        /// The minimum password length.
        /// </summary>
        protected int min;
        /// <summary>
        /// The maximum password length.
        /// </summary>
        protected int max;
        /// <summary>
        /// This object is used for locks in parallel programming.
        /// </summary>
        private readonly object locker = new object();
        /// <summary>
        /// When a Worker successfully guesses a password, this variable becomes true.
        /// </summary>
        private bool workerFoundSomething = false;
        /// <summary>
        /// To pause the operation, set its value to True, and to resume the operation, set it to False.
        /// This is only applicable when <see cref="Started"/> is equal to true.
        /// </summary>
        public bool Pause
        {
            get => pause;
            set => pause = (started || !value) && value;
        }
        /// <summary>
        /// determines the interval, in milliseconds, at which the program checks if the operation is paused.
        /// </summary>
        public int WaitForPauseCheckingMillisecond { get; set; } = 215;
        /// <summary>
        /// This property determines the starting position for generating passwords.
        /// It should not be less than or equal to 0 or greater than <see cref="EndPos"/>.
        /// If you change this property in another thread while generating passwords, the OnRestart event will be triggered and the generation will start from the beginning.
        /// </summary>
        public long StartPos
        {
            get => startPos;
            set
            {
                if (started)
                {
                    pause = true;
                    Thread.Sleep(WaitForPauseCheckingMillisecond);
                    startPos = value;
                    NeedErrorsChecking();
                    total = CalculateTotal();
                    needToRestart = true;
                    //if (Debugger.IsAttached) Console.WriteLine($"DEBUG: StartPos, ManagedThreadId: \"{Thread.CurrentThread.ManagedThreadId}\"");
                    pause = false;
                }
                else
                    startPos = value;
            }
        }
        /// <summary>
        /// This property determines the position of the last password to be generated.
        /// If you want all passwords to be generated, you can use a number smaller than or equal to 0. 
        /// This property should not exceed the maximum number of passwords that can be generated. 
        /// If it changes in another thread during execution, OnRestart is not triggered
        /// </summary>
        public long EndPos
        {
            get => endPos;
            set
            {
                if (started)
                {
                    pause = true;
                    Thread.Sleep(WaitForPauseCheckingMillisecond);
                    endPos = value;
                    NeedErrorsChecking();
                    total = CalculateTotal();
                    //if (Debugger.IsAttached) Console.WriteLine($"DEBUG: EndPos, ManagedThreadId: \"{Thread.CurrentThread.ManagedThreadId}\"");
                    pause = false;
                }
                else endPos = value;
            }
        }
        /// <summary>
        /// Minimum length for passwords.
        /// </summary>
        public int MinimumPassLength { get => min; }
        /// <summary>
        /// Maximum length for passwords.
        /// </summary>
        public int MaximumPassLength { get => max; }
        /// <summary>
        /// A unique identifier for each BruteForce instance in each thread.
        /// </summary>
        public int ThreadID { get => threadID; }
        /// <summary>
        /// Indicates whether password generation operation has started or not.
        /// </summary>
        public bool Started { get => started; }
        /// <summary>
        /// When a Worker successfully guesses a password, this variable becomes true.
        /// </summary>
        internal bool WorkerFoundSomething
        {
            get
            {
                bool ret;
                lock (locker) ret = workerFoundSomething;
                return ret;
            }
            set
            {
                lock (locker) workerFoundSomething = value;
            }
        }
        /// <summary>
        /// An abstract method that any class derived from <see cref="BruteForce"/> must implement. This method starts the password generation process.
        /// </summary>
        protected abstract void StartBrute();
        /// <summary>
        /// In this method, errors are handled through try/catch blocks.
        /// </summary>
        private void StartBruteForceTryCatch() => StartBrute();
        /// <summary>
        /// In this method, errors are reported through events.
        /// </summary>
        protected abstract void StartBruteForceEvent();
        /// <summary>
        /// This method should detect and report potential errors that may occur before running the brute force operation.
        /// </summary>
        protected abstract void NeedErrorsChecking();
        /// <summary>
        /// The total number of passwords that can be generated should be calculated based on <see cref="StartPos"/>, <see cref="EndPos"/>, and if there are any ExtraLengths.
        /// </summary>
        /// <returns>Returns the total number of producible passwords</returns>
        protected abstract long CalculateTotal();
        /// <summary>
        /// This method is used to stop the whole password generation process.
        /// </summary>
        public void Stop() => stopped = started;
        /// <summary>
        /// Waits until the <see cref="Pause"/> flag is set to false. If the <see cref="stopped"/> flag is set to true, the function will return true and the entire password generation process will stop.
        /// </summary>
        /// <returns>True if the password generation process should stop, false otherwise.</returns>
        protected bool waitUntilPause()
        {
            while (Pause)
            {
                Thread.Sleep(20);
                if (stopped)
                {
                    started = false;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// This method resets the <see cref="stopped"/> and <see cref="pause"/> flags to false.
        /// </summary>
        protected void resetStopPause()
        {
            stopped = false;
            pause = false;
        }
        /// <summary>
        /// Starts the password generation process. No thread is used here!
        /// </summary>
        /// <param name="handlingType">Specifies the error handling method</param>
        public void Start(ErrorHandlingType handlingType = ErrorHandlingType.TryCath)
        {
            workerFoundSomething = false;
            if(Debugger.IsAttached && handlingType == ErrorHandlingType.TryCath)
            {
                StartBruteForceTryCatch();
                return;
            }
            if (handlingType == ErrorHandlingType.TryCath)
            try { StartBruteForceTryCatch(); }
            catch (Exception ex)
            {
                resetStopPause();
                started = false;
                throw ex;
            }
            else if (handlingType == ErrorHandlingType.Event)
                StartBruteForceEvent();
            else throw new ArgumentException(nameof(handlingType));
        }
    }
}
