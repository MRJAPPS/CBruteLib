using CBrute.Core;
using System;

namespace CBrute.Worker
{
    /// <summary>
    /// This delegate is used to handle the OnThreadEnd event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="brute">The instance of BruteForce class that caused the event.</param>
    /// <param name="pass">An array of objects that represents the last password that was generated.</param>
    /// <param name="result">A boolean value indicating whether a password was found or not</param>
    /// <returns>
    /// Return True if you intend for the activity of other threads to end as well. 
    /// The best practice is to always return the "result" parameter.
    /// </returns>
    public delegate bool delegate_OnThreadEnd(Worker sender, BruteForce brute, object[] pass, bool result);
    /// <summary>
    /// This delegate is used to handle the OnThreadStart event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="brute">The instance of BruteForce class that caused the event.</param>
    public delegate void delegate_OnThreadStart(Worker sender, BruteForce brute);
    /// <summary>
    /// This delegate is used to handle the OnThreadPause and OnThreadResume events.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="brute">The instance of BruteForce class that caused the event.</param>
    /// <param name="generated">The number of generated passwords.</param>
    /// <param name="total">Total generatable passwords.</param>
    public delegate void delegate_OnThreadPauseOrResume(Worker sender, BruteForce brute, long generated, long total);
    /// <summary>
    /// This delegate is used to handle the OnThreadStop and OnThreadRestart events.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="brute">The instance of BruteForce class that caused the event.</param>
    /// <param name="pass">An array of objects that represents the last password that was generated.</param>
    public delegate void delegate_OnThreadStopOrRestart(Worker sender, BruteForce brute, object[] pass);
    /// <summary>
    /// This delegate is used to handle the OnThreadError event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="brute">The instance of BruteForce class that caused the event.</param>
    /// <param name="e">An instance that contains information about the occurred error.</param>
    /// <param name="tryAgain">If you want to retry the state that caused the error, set the value of this parameter to True, otherwise False. The default value for this parameter is False.</param>
    public delegate void delegate_OnThreadError(Worker sender, BruteForce brute, Exception e, ref bool tryAgain);
    /// <summary>
    /// This delegate is used to manage some of the events created by a Worker.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    public delegate void delegate_WrokerEvent(Worker sender);
    /// <summary>
    /// This delegate is used to handle the OnStop and OnEnd events.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="result">A boolean value indicating whether a password was found or not</param>
    public delegate void delegate_OnStopOrEnd(Worker sender, bool result);
    /// <summary>
    /// This delegate is used to handle the OnError event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="lastEx">An instance that contains information about the occurred error.</param>
    public delegate void delegate_OnError(Worker sender, Exception lastEx);
    /// <summary>
    /// This delegate is used as a callback function to check the generated password. All threads use this delegate to check the password.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="brute">The instance of BruteForce class that caused the PasswordGenerated event.</param>
    /// <param name="pass">An array of objects that represents the last password that was generated.</param>
    /// <param name="generated">The number of generated passwords in the corresponding thread.</param>
    /// <param name="total">Total generatable passwords in the corresponding thread.</param>
    /// <returns>If you find the password, return the value True, otherwise return False. If you return the value True, the OnThreadEnd event will be triggered.</returns>
    public delegate bool delegate_CheckPassword(Worker sender, BruteForce brute, object[] pass, long generated, long total);
}