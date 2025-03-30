using System;

namespace Mnemonics.CodingTools.Annotations
{
    /// <summary>
    ///     Indicates that a property is required and cannot be null.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IsRequiredAttribute : Attribute { }
}
