using System;

namespace Mnemonics.CodingTools.Annotations
{
    /// <summary>
    ///     Marks a property as part of the primary key for the entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IsKeyFieldAttribute : Attribute { }

}
