// BonzaException.cs
// Application-specific exception class, to avoid using a too generic one
//
// 2017-07-24   PV      Split from program.cs to prepare new merged application
// 2021-11-13   PV      Net6 C#10
// 2024-11-15	PV		Net9 C#13

using System;

namespace Bonza.Generator;

// To avoid use of generic Exception and make code analyzer complain about it!
public class BonzaException: Exception
{
    public BonzaException()
    {
    }

    public BonzaException(string message) : base(message)
    {
    }

    public BonzaException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
