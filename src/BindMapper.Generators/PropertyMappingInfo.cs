using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BindMapper.Generators;

/// <summary>
/// Enhanced property information with fluent configuration cached.
/// Consolidates all mapping metadata for a single property to avoid repeated dictionary lookups.
/// </summary>
internal sealed class PropertyMappingInfo
{
    /// <summary>Base property information from symbol analysis.</summary>
    public PropertyInfo Base { get; }

    /// <summary>Resolution type: Direct, Nested, Constant, Expression, or Ignored.</summary>
    public MappingResolutionType ResolutionType { get; }

    /// <summary>For Direct mappings: which source property to map from.</summary>
    public PropertyInfo? SourceProperty { get; }

    /// <summary>For Nested mappings: the mapping configuration to use.</summary>
    public MappingConfiguration? NestedMapping { get; }

    /// <summary>For Constant mappings: the constant value string.</summary>
    public string? ConstantValue { get; }

    /// <summary>For Expression mappings: the custom expression.</summary>
    public string? CustomExpression { get; }

    /// <summary>Whether this property should be skipped entirely.</summary>
    public bool ShouldIgnore { get; }

    public PropertyMappingInfo(PropertyInfo baseInfo)
    {
        Base = baseInfo;
        ResolutionType = MappingResolutionType.Unresolved;
        ShouldIgnore = baseInfo.IsIgnored;
    }

    private PropertyMappingInfo(
        PropertyInfo baseInfo,
        MappingResolutionType resolutionType,
        PropertyInfo? sourceProperty = null,
        MappingConfiguration? nestedMapping = null,
        string? constantValue = null,
        string? customExpression = null,
        bool shouldIgnore = false)
    {
        Base = baseInfo;
        ResolutionType = resolutionType;
        SourceProperty = sourceProperty;
        NestedMapping = nestedMapping;
        ConstantValue = constantValue;
        CustomExpression = customExpression;
        ShouldIgnore = shouldIgnore;
    }

    /// <summary>Creates a direct property-to-property mapping.</summary>
    public static PropertyMappingInfo CreateDirect(PropertyInfo baseInfo, PropertyInfo sourceProperty)
    {
        return new(baseInfo, MappingResolutionType.Direct, sourceProperty: sourceProperty);
    }

    /// <summary>Creates a nested type mapping (requires call to mapped type's To method).</summary>
    public static PropertyMappingInfo CreateNested(PropertyInfo baseInfo, PropertyInfo sourceProperty, MappingConfiguration nestedMapping)
    {
        return new(baseInfo, MappingResolutionType.Nested, sourceProperty, nestedMapping);
    }

    /// <summary>Creates a constant value assignment.</summary>
    public static PropertyMappingInfo CreateConstant(PropertyInfo baseInfo, string constantValue)
    {
        return new(baseInfo, MappingResolutionType.Constant, constantValue: constantValue);
    }

    /// <summary>Creates a custom expression mapping.</summary>
    public static PropertyMappingInfo CreateExpression(PropertyInfo baseInfo, string customExpression)
    {
        return new(baseInfo, MappingResolutionType.Expression, customExpression: customExpression);
    }

    /// <summary>Creates an ignored property (won't be mapped).</summary>
    public static PropertyMappingInfo CreateIgnored(PropertyInfo baseInfo)
    {
        return new(baseInfo, MappingResolutionType.Ignored, shouldIgnore: true);
    }

    /// <summary>Unresolved property - couldn't determine source.</summary>
    public static PropertyMappingInfo CreateUnresolved(PropertyInfo baseInfo)
    {
        return new(baseInfo, MappingResolutionType.Unresolved);
    }
}

/// <summary>Determines how a destination property will be populated during mapping.</summary>
internal enum MappingResolutionType
{
    /// <summary>Direct property-to-property: destination.Prop = source.Prop</summary>
    Direct = 0,

    /// <summary>Nested mapping call: destination.Prop = To(source.Prop)</summary>
    Nested = 1,

    /// <summary>Constant value: destination.Prop = constantValue</summary>
    Constant = 2,

    /// <summary>Custom expression: destination.Prop = s.Field1 + s.Field2</summary>
    Expression = 3,

    /// <summary>Property is marked ignored or read-only</summary>
    Ignored = 4,

    /// <summary>Couldn't determine resolution - skip silently</summary>
    Unresolved = 5,
}
