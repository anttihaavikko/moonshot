// -----------------------------------------------------------------------
// <copyright file="SimpleLogItem.cs" company="">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace TriangleNet.Log
{
    /// <summary>
    ///     Represents an item stored in the log.
    /// </summary>
    public class SimpleLogItem : ILogItem
    {
        public SimpleLogItem(LogLevel level, string message)
            : this(level, message, "")
        {
        }

        public SimpleLogItem(LogLevel level, string message, string info)
        {
            Time = DateTime.Now;
            Level = level;
            Message = message;
            Info = info;
        }

        public DateTime Time { get; }

        public LogLevel Level { get; }

        public string Message { get; }

        public string Info { get; }
    }
}