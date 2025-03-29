using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Mnemonics.CodingTools.Configuration;
using Mnemonics.CodingTools.Interfaces;
using Mnemonics.CodingTools.Models;

namespace Mnemonics.CodingTools;

/// <summary>
/// Provides functionality to dynamically build a class with properties at runtime.
/// </summary>
public class DynamicClassBuilder : IDynamicClassBuilder
{
    private readonly List<DynamicPropertyMetadata> _properties = [];
    private readonly string _outputDirectory;
    private readonly string _className;

    /// <summary>
    /// Initializes a new instance of <see cref="DynamicClassBuilder"/> using configured options.
    /// </summary>
    /// <param name="className">The name of the class to build.</param>
    /// <param name="options">Injected configuration options.</param>
    public DynamicClassBuilder(string className, IOptions<CodingToolsOptions> options)
    {
        _className = className ?? throw new ArgumentNullException(nameof(className));
        _outputDirectory = options?.Value?.AssemblyDirectory ?? "GeneratedAssemblies";
        Directory.CreateDirectory(_outputDirectory);
    }

 /// <inheritdoc />
    public void AddProperty(DynamicPropertyMetadata dynamicProperty)
    {
        _properties.Add(dynamicProperty);
    }

    /// <inheritdoc />
    public Type Build()
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        var typeBuilder = moduleBuilder.DefineType(_className, TypeAttributes.Public | TypeAttributes.Class);

        foreach (var property in _properties)
        {
            var fieldBuilder = typeBuilder.DefineField($"_{property.Name}", property.Type, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.Type, null);

            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;

            var getter = typeBuilder.DefineMethod($"get_{property.Name}", getSetAttr, property.Type, Type.EmptyTypes);
            var getterIl = getter.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getterIl.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getter);

            var setter = typeBuilder.DefineMethod($"set_{property.Name}", getSetAttr, null, new[] { property.Type });
            var setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, fieldBuilder);
            setterIl.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setter);

            var attributeConstructor = typeof(FieldWithAttributes).GetConstructor(
            [
                typeof(string), 
                typeof(string), 
                typeof(bool),
                typeof(string), 
                typeof(string), 
                typeof(bool), 
                typeof(string)
            ]) ?? throw new InvalidOperationException("Constructor not found for FieldWithAttributes.");

            var optionsJson = JsonSerializer.Serialize(property.Options ?? Array.Empty<string>());
            var controlParamsJson = JsonSerializer.Serialize(property.ControlParameters ?? new Dictionary<string, object>());
            var dataSetControlsJson = JsonSerializer.Serialize(property.DataSetControls ?? new Dictionary<string, object>());

            var attributeBuilder = new CustomAttributeBuilder(attributeConstructor,
            [
                property.ControlType,
                property.Placeholder,
                property.IsRequired,
                optionsJson,
                controlParamsJson,
                property.IsDisplayField,
                dataSetControlsJson
            ]);

            propertyBuilder.SetCustomAttribute(attributeBuilder);
        }

        return typeBuilder.CreateType()!;
    }
}