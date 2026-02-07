using Microsoft.CodeAnalysis;

namespace BindMapper.Generators;

/// <summary>
/// Diagnostic descriptors for the BindMapper source generator.
/// All diagnostics are deterministic and provide actionable error messages.
/// </summary>
public static class DiagnosticsDescriptors
{
    /// <summary>
    /// Reported when a source property required by destination mapping doesn't exist.
    /// Severity: Warning (will not break compilation, but indicates missing data)
    /// </summary>
    public static readonly DiagnosticDescriptor MissingSourceProperty = new(
        id: "VMAPPER001",
        title: "Source property not found for mapping",
        messageFormat: "Cannot map to '{0}'. Source type '{1}' does not have property '{2}'. " +
                       "This property will not be mapped, resulting in null/default value.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER001");

    /// <summary>
    /// Reported when duplicate mappings are configured for the same type pair.
    /// Severity: Warning (last configuration wins, but indicates potential configuration error)
    /// </summary>
    public static readonly DiagnosticDescriptor DuplicateMapping = new(
        id: "VMAPPER002",
        title: "Duplicate mapping configuration",
        messageFormat: "Mapping from '{0}' to '{1}' is already configured in another [MapperConfiguration] method. " +
                       "The previous configuration will be overridden. Consider consolidating into a single method.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER002");

    /// <summary>
    /// Reported when ReverseMap is applied but destination has read-only members.
    /// Severity: Warning (ReverseMap will fail at runtime)
    /// </summary>
    public static readonly DiagnosticDescriptor ReverseMapReadOnlyConflict = new(
        id: "VMAPPER003",
        title: "ReverseMap may fail at runtime",
        messageFormat: "ReverseMap configured for '{0}' -> '{1}', but destination type has read-only properties: {2}. " +
                       "These properties cannot be assigned, causing mapping to fail at runtime.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER003");

    /// <summary>
    /// Reported when ForMember configuration uses invalid syntax.
    /// Severity: Warning (configuration silently ignored)
    /// </summary>
    public static readonly DiagnosticDescriptor InvalidForMemberSyntax = new(
        id: "VMAPPER004",
        title: "Invalid ForMember configuration syntax",
        messageFormat: "ForMember configuration for destination property could not be parsed. " +
                       "Ensure you use lambda expressions: .ForMember(d => d.Prop, opt => opt.MapFrom(s => s.Source)). " +
                       "This configuration will be ignored.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER004");

    /// <summary>
    /// Reported when a custom expression in MapFrom cannot be validated.
    /// Severity: Warning (expression may fail at runtime)
    /// </summary>
    public static readonly DiagnosticDescriptor UnvalidatedExpression = new(
        id: "VMAPPER005",
        title: "Custom MapFrom expression could not be validated",
        messageFormat: "The MapFrom expression for property '{0}' uses complex lambda syntax that cannot be validated at compile-time. " +
                       "Ensure the expression is valid and returns a type assignable to the destination property. " +
                       "Invalid expressions will cause runtime errors.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER005");

    /// <summary>
    /// Reported when CreateMap is used outside a [MapperConfiguration] method.
    /// Severity: Warning (configuration will be ignored by generator)
    /// </summary>
    public static readonly DiagnosticDescriptor CreateMapOutsideConfiguration = new(
        id: "VMAPPER006",
        title: "CreateMap called outside [MapperConfiguration] method",
        messageFormat: "CreateMap<{0}, {1}>() should only be called within methods decorated with [MapperConfiguration]. " +
                       "This call will be ignored by the source generator.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER006");

    /// <summary>
    /// Reported when type mismatch occurs in custom mapping.
    /// Severity: Warning (type conversion will fail)
    /// </summary>
    public static readonly DiagnosticDescriptor TypeMismatchInMapping = new(
        id: "VMAPPER007",
        title: "Type mismatch in property mapping",
        messageFormat: "Cannot automatically convert source property '{0}' of type '{1}' to destination property '{2}' of type '{3}'. " +
                       "Consider using .MapFrom() to provide custom conversion logic.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER007");

    /// <summary>
    /// Reports the mapping successfully generated (informational).
    /// </summary>
    public static readonly DiagnosticDescriptor MappingGenerated = new(
        id: "VMAPPER900",
        title: "Mapping successfully generated",
        messageFormat: "Mapping from '{0}' to '{1}' has been generated.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: false);

    /// <summary>
    /// IMPROVED: Reported when implicit numeric type conversion may lose data.
    /// Severity: Warning (information loss may occur)
    /// </summary>
    public static readonly DiagnosticDescriptor ImplicitNumericConversion = new(
        id: "VMAPPER008",
        title: "Implicit numeric conversion may lose data",
        messageFormat: "Source property '{0}' is type '{1}' but destination property '{2}' is type '{3}'. " +
                       "This implicit conversion may result in data loss. " +
                       "Consider using explicit .MapFrom() with proper conversion logic.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER008");

    /// <summary>
    /// IMPROVED: Reported when non-nullable destination property has no source.
    /// Severity: Warning (will cause NullReferenceException at runtime)
    /// </summary>
    public static readonly DiagnosticDescriptor NonNullableWithoutSource = new(
        id: "VMAPPER009",
        title: "Non-nullable destination property has no source",
        messageFormat: "Destination property '{0}' of type '{1}' is non-nullable, but no source property '{0}' exists on source type '{2}'. " +
                       "This will result in a NullReferenceException at runtime unless a constant value is provided via .UseValue() or source property is added.",
        category: "BindMapper",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        helpLinkUri: "https://github.com/velocity-mapper/docs/VMAPPER009");
}
