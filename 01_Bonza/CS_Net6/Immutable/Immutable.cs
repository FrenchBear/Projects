﻿
// Immutable.cs - Support of Immutable class attribute
// From https://blogs.msdn.microsoft.com/kevinpilchbisson/2007/11/20/enforcing-immutability-in-code/
//
// 2017-08-10   PV
// 2021-11-13   PV      Net6 C#10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bonza;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
[Serializable]
public sealed class ImmutableAttribute: Attribute
{
    // in some cases, a type is immutable but can't be proven as such.
    // in these cases, the developer can mark the type with [Immutable(true)]
    // and the code below will take it on faith that the type is immutable,
    // instead of testing explicitly.
    //
    // A common example is a type that contains a List<T>, but doesn't
    // modify it after construction.
    //
    // TODO: replace this with a per-field attribute, to allow the
    // immutability test to run over the rest of the type.
    [NonSerialized]
    public bool OnFaith;

    /// <summary>
    /// Ensures that all types in 'assemblies' that are marked
    /// [Immutable] follow the rules for immutability.
    /// </summary>
    /// <exception cref="ImmutableFailureException">Thrown if a mutability issue appears.</exception>
    public static void VerifyTypesAreImmutable(IEnumerable<Assembly> assemblies, params Type[] whiteList)
    {
        var typesMarkedImmutable = from type in assemblies.GetTypes()
                                   where IsMarkedImmutable(type)
                                   select type;

        foreach (var type in typesMarkedImmutable)
        {
            VerifyTypeIsImmutable(type, whiteList);
        }
    }

    private static bool IsMarkedImmutable(Type type) => ReflectionHelper.TypeHasAttribute<ImmutableAttribute>(type);

    private class WritableFieldException: ImmutableFailureException
    {
        protected WritableFieldException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        internal WritableFieldException(FieldInfo fieldInfo)
            : base(fieldInfo.DeclaringType, FormatMessage(fieldInfo))
        {
        }

        private static string FormatMessage(FieldInfo fieldInfo) => string.Format("'{0}' is mutable because field '{1}' is not marked 'readonly'.", fieldInfo.DeclaringType, fieldInfo.Name);
    }

    private class MutableFieldException: ImmutableFailureException
    {
        protected MutableFieldException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        internal MutableFieldException(FieldInfo fieldInfo, Exception inner)
            : base(fieldInfo.DeclaringType, FormatMessage(fieldInfo), inner)
        {
        }

        private static string FormatMessage(FieldInfo fieldInfo) => string.Format("'{0}' is mutable because '{1}' of type '{2}' is mutable.", fieldInfo.DeclaringType, fieldInfo.Name, fieldInfo.FieldType);
    }

    private class MutableBaseException: ImmutableFailureException
    {
        protected MutableBaseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        internal MutableBaseException(Type type, Exception inner)
            : base(type, FormatMessage(type), inner)
        {
        }

        private static string FormatMessage(Type type) => string.Format("'{0}' is mutable because its base type ('[{1}]') is mutable.", type, type.BaseType);
    }

    public class ImmutableFailureException: Exception
    {
        public readonly Type Type;

        protected ImmutableFailureException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        internal ImmutableFailureException(Type type, string message, Exception inner)
            : base(message, inner) => Type = type;

        internal ImmutableFailureException(Type type, string message)
            : base(message) => Type = type;
    }

    /// <summary>
    /// Ensures that 'type' follows the rules for immutability
    /// </summary>
    /// <exception cref="ImmutableFailureException">Thrown if a mutability issue appears.</exception>
    public static void VerifyTypeIsImmutable(Type type, IEnumerable<Type> whiteList)
    {
        if (whiteList.Contains(type))
            return;
        if (IsWhiteListed(type))
            return;

        try
        {
            VerifyTypeIsImmutable(type.BaseType, whiteList);
        }
        catch (ImmutableFailureException ex)
        {
            throw new MutableBaseException(type, ex);
        }

        foreach (FieldInfo fieldInfo in ReflectionHelper.GetAllDeclaredInstanceFields(type))
        {
            if ((fieldInfo.Attributes & FieldAttributes.InitOnly) == 0)
                throw new WritableFieldException(fieldInfo);

            // if it's marked with [Immutable], that's good enough, as we
            // can be sure that these tests will all be applied to this type
            if (!IsMarkedImmutable(fieldInfo.FieldType))
            {
                try
                {
                    VerifyTypeIsImmutable(fieldInfo.FieldType, whiteList);
                }
                catch (ImmutableFailureException ex)
                {
                    throw new MutableFieldException(fieldInfo, ex);
                }
            }
        }
    }

    private static bool IsWhiteListed(Type type)
    {
        if (type == typeof(Object))
            return true;

        if (type == typeof(String))
            return true;

        if (type == typeof(Guid))
            return true;

        if (type.IsEnum)
            return true;

        // bool, int, etc.
        if (type.IsPrimitive)
            return true;

        // override all checks on this type if [ImmutableAttribute(OnFaith=true)] is set
        ImmutableAttribute immutableAttribute = ReflectionHelper.GetCustomAttribute<ImmutableAttribute>(type);
        if (immutableAttribute != null && immutableAttribute.OnFaith)
            return true;

        return false;
    }
}

[Serializable]
public static class ReflectionHelper
{
    /// <summary>
    /// Find all types in 'assembly' that derive from 'baseType'
    /// </summary>
    internal static IEnumerable<Type> FindAllTypesThatDeriveFrom<TBase>(Assembly assembly) => from type in assembly.GetTypes()
                                                                                              where type.IsSubclassOf(typeof(TBase))
                                                                                              select type;

    /// <summary>
    /// Check if the given type has the given attribute on it.  Don't look at base classes.
    /// </summary>
    public static bool TypeHasAttribute<TAttribute>(Type type)
        where TAttribute : Attribute => Attribute.IsDefined(type, typeof(TAttribute));

    // I find that the default GetFields behavior is not suitable to my needs
    internal static IEnumerable<FieldInfo> GetAllDeclaredInstanceFields(Type type) => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    /// <summary>
    /// A type safe wrapper for Attribute.GetCustomAttribute
    /// </summary>
    /// <remarks>TODO: add overloads for Assembly, Module, and ParameterInfo</remarks>
    internal static TAttribute GetCustomAttribute<TAttribute>(MemberInfo element)
        where TAttribute : Attribute => (TAttribute)Attribute.GetCustomAttribute(element, typeof(TAttribute));

    /// <summary>
    /// All types across multiple assemblies
    /// </summary>
    public static IEnumerable<Type> GetTypes(this IEnumerable<Assembly> assemblies) => from assembly in assemblies
                                                                                       from type in assembly.GetTypes()
                                                                                       select type;
}