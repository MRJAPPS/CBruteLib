using System;

namespace CBrute
{
    /// <summary>
    /// When an unexpected event occurs, this exception is thrown.
    /// </summary>
    public class UnexpectedException : Exception
    {
        /// <summary>
        /// Constructor of the UnexpectedException class
        /// </summary>
        /// <param name="code">The error code</param>
        /// <param name="file">The file in which the error occurred.</param>
        public UnexpectedException(int code, string file) : base($"CODE: 0X{code:X3}, {file}")
        { }
    }
}
