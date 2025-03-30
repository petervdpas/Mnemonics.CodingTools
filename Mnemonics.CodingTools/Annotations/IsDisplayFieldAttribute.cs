using System;

namespace Mnemonics.CodingTools.Annotations
{
    /// <summary>
    ///     Marks a property as a display field (used in summaries, lists, or UIs).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IsDisplayFieldAttribute : Attribute { }
}