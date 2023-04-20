using System;
using System.Collections.Generic;
using System.Text;

namespace CBrute
{
    /// <summary>
    /// A base class for all classes
    /// </summary>
    public abstract class BaseClass
    {
        /// <summary>
        /// Can contain any type of data.
        /// </summary>
        public object? Tag { get; set; }
    }
}
