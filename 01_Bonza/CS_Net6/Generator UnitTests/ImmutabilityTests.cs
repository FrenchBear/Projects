// ImmutabilityTests
// Validation of types with custom attribute Immutable
// From https://blogs.msdn.microsoft.com/kevinpilchbisson/2007/11/20/enforcing-immutability-in-code/
//
// 2017-08-10   PV
// 2021-11-13   PV      Net6 C#10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;

namespace Bonza_Generator_UnitTests;

[TestClass]
public class Immutable_Tests
{
    private static IEnumerable<Assembly> AssembliesToTest => new[] { typeof(Bonza.Generator.Position).Assembly };

    [TestMethod]
    // It's particularly important that 'struct' types are immutable.
    // for a short discussion, see http://blogs.msdn.com/jaybaz_ms/archive/2004/06/10/153023.aspx
    public void EnsureStructsAreImmutableTest()
    {
        var mutableStructs = from type in AssembliesToTest.GetTypes()
                             where IsMutableStruct(type)
                             select type;

        if (mutableStructs.Any())
            Assert.Fail("'{0}' is a value type, but was not marked with the [Immutable] attribute", mutableStructs.First().FullName);
    }
    [TestMethod]
    // ensure that any type marked [Immutable] has fields that are all immutable
    public void EnsureImmutableTypeFieldsAreMarkedImmutableTest()
    {
        try
        {
            ImmutableAttribute.VerifyTypesAreImmutable(AssembliesToTest);
        }
        catch (ImmutableAttribute.ImmutableFailureException ex)
        {
            Console.Write(FormatExceptionForAssert(ex));
            Assert.Fail("'{0}' failed the immutability test.  See output for details.", ex.Type.Name);
        }
    }
    internal static bool IsMutableStruct(Type type)
    {
        if (!type.IsValueType) 
            return false;
        if (type.IsEnum) 
            return false;
        if (type.IsSpecialName) 
            return false;
        if (type.GetCustomAttribute(typeof(CompilerGeneratedAttribute)) != null) 
            return false;      // PV: Can't actually control internally generated types
        if (type.Name.StartsWith("__", StringComparison.Ordinal)) 
            return false;
        if (ReflectionHelper.TypeHasAttribute<ImmutableAttribute>(type)) 
            return false;
        return true;
    }

    static string FormatExceptionForAssert(Exception ex)
    {
        var sb = new StringBuilder();

        string indent = "";

        for (; ex != null; ex = ex.InnerException)
        {
            sb.Append(indent);
            sb.AppendLine(ex.Message);

            indent += "    ";
        }

        return sb.ToString();
    }
}