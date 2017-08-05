// BonzaException.cs
// Application-specific exception class, to avoid using a too generic one
//
// 2017-07-24   PV      Split from program.cs to prepare new merged application


using System;

namespace Bonza.Generator
{
    // To avoid use of generic Exception and make code analyzer complain about it!
    public class BonzaException : Exception
    {
        public BonzaException() { }
        public BonzaException(string message) : base(message) { }
        public BonzaException(string message, Exception innerException) : base(message, innerException) { }
    }
}
