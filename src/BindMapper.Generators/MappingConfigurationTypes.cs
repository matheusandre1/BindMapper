using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BindMapper.Generators;

/// <summary>
/// Cached mapping configuration extracted from source code.
/// This record holds both symbol references (for analysis) and string representations (for code generation).
/// </summary>
internal sealed record MappingConfiguration(
    ITypeSymbol SourceTypeSymbol,
    ITypeSymbol DestinationTypeSymbol,
    string SourceTypeFullName,
    string DestinationTypeFullName,
    string SourceTypeName,
    string DestinationTypeName,
    FluentConfiguration FluentConfig)
{
    /// <summary>
    /// Gets the fully qualified name suitable for code generation.
    /// </summary>
    public string SourceType => SourceTypeFullName;

    /// <summary>
    /// Gets the fully qualified destination type name suitable for code generation.
    /// </summary>
    public string DestinationType => DestinationTypeFullName;
}

/// <summary>
/// Fluent configuration options parsed from CreateMap chain.
/// All fields are initialized to sensible defaults for performance.
/// </summary>
internal sealed class FluentConfiguration
{
    /// <summary>
    /// Whether ReverseMap() was called (creates A->B and B->A mappings).
    /// </summary>
    public bool HasReverseMap { get; set; }

    /// <summary>
    /// Whether IgnoreAllNonExisting() was called (skips unmapped properties).
    /// </summary>
    public bool IgnoreAllNonExisting { get; set; }

    /// <summary>
    /// Members that should be ignored during mapping.
    /// Using HashSet<T> for O(1) lookup in tight loops.
    /// </summary>
    public HashSet<string> IgnoredMembers { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Simple property-to-property mappings: destination -> source member name.
    /// Used when .MapFrom(s => s.SimpleMemberName) is specified.
    /// </summary>
    public Dictionary<string, string> MemberMappings { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Complex expressions that need code generation.
    /// Used when .MapFrom(s => s.Prop1 + s.Prop2) or similar complex logic is specified.
    /// </summary>
    public Dictionary<string, string> MemberExpressions { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Constant values set with .UseValue().
    /// Stored as string representations of literals.
    /// </summary>
    public Dictionary<string, string> MemberValues { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Metadata about a single property for mapping analysis.
/// Extracted from IPropertySymbol for efficient code generation.
/// </summary>
internal sealed record PropertyInfo(
    string Name,
    ITypeSymbol Type,
    bool IsReadable,
    bool IsWriteable,
    bool IsIgnored,
    string? MapFrom);
