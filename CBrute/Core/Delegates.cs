using System;

namespace CBrute.Core
{
    /// <summary>
    /// This delegate is used to handle the OnEnd event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="pass">An array of objects that represents the last password that was generated.</param>
    /// <param name="result">A boolean value indicating whether a password was found or not</param>
    public delegate void delegate_OnEnd(BruteForce sender, object[] pass, bool result);
    /// <summary>
    /// This delegate is used to handle the OnStop and OnRestart events.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="pass">An array of objects that represents the last password that was generated.</param>
    public delegate void delegate_OnStopOrRestart(BruteForce sender, object[] pass);
    /// <summary>
    /// This delegate is used to handle the OnPause and OnResume events.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="generated">The number of generated passwords.</param>
    /// <param name="total">Total generatable passwords.</param>
    public delegate void delegate_OnPauseOrResume(BruteForce sender, long generated, long total);
    /// <summary>
    /// This delegate is used to handle the OnStart event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    public delegate void delegate_OnStart(BruteForce sender);
    /// <summary>
    /// This delegate is used to handle the PasswordGenerated event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="pass">An array of objects that represents the last password that was generated.</param>
    /// <param name="generated">The number of generated passwords.</param>
    /// <param name="total">Total generatable passwords.</param>
    /// <returns>If you find the password, return the value True, otherwise return False. If you return the value True, the OnEnd event will be triggered.</returns>
    public delegate bool delegate_PasswordGenerated(BruteForce sender, object[] pass, long generated, long total);
    /// <summary>
    /// This delegate is used to handle the OnEnd event.
    /// </summary>
    /// <param name="sender">The object that caused the event.</param>
    /// <param name="e">An instance that contains information about the occurred error.</param>
    public delegate void delegate_OnError(BruteForce sender, Exception e);
}