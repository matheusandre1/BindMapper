using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BindMapper.Generators;

/// <summary>
/// Helper methods for symbol analysis and property manipulation.
/// Centralizes common Roslyn operations used throughout the generator.
/// </summary>
internal static class SymbolAnalysisHelper
{
    /// <summary>
    /// Extracts all public instance properties from a type symbol.
    /// Caches results to avoid repeated symbol analysis.
    /// </summary>
    public static IReadOnlyList<PropertyInfo> GetPublicProperties(ITypeSymbol typeSymbol)
    {
        var properties = new List<PropertyInfo>(capacity: 16);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            // Skip non-public and static properties
            if (property.DeclaredAccessibility != Accessibility.Public || property.IsStatic)
                continue;

            // Check for [IgnoreMap] attribute
            var isIgnored = property.GetAttributes().Any(a =>
                a.AttributeClass?.ToDisplayString() == "BindMapper.IgnoreMapAttribute");

            // Check for [MapFrom] attribute
            var mapFrom = property.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "BindMapper.MapFromAttribute")?
                .ConstructorArguments.FirstOrDefault().Value as string;

            var hasGetter = property.GetMethod is not null &&
                           property.GetMethod.DeclaredAccessibility == Accessibility.Public;
            var hasSetter = property.SetMethod is not null &&
                           property.SetMethod.DeclaredAccessibility == Accessibility.Public;

            properties.Add(new PropertyInfo(
                property.Name,
                property.Type,
                hasGetter,
                hasSetter,
                isIgnored,
                mapFrom));
        }

        return properties;
    }

    /// <summary>
    /// Calculates a weight for type ordering to optimize cache locality in generated code.
    /// Value types are weighted lower (come first), references higher (come last).
    /// This improves CPU cache line utilization.
    /// </summary>
    public static int GetTypeWeightForOrdering(ITypeSymbol type)
    {
        // Weight mapping: bool/byte (1) < short/char (2) < int/float (4) < 
        //                long/double (8) < decimal/datetime (16) < strings/refs (32)
        var specialType = type.SpecialType;
        
        return specialType switch
        {
            SpecialType.System_Boolean => 1,
            SpecialType.System_Byte => 1,
            SpecialType.System_SByte => 1,
            
            SpecialType.System_Char => 2,
            SpecialType.System_Int16 => 2,
            SpecialType.System_UInt16 => 2,
            
            SpecialType.System_Int32 => 4,
            SpecialType.System_UInt32 => 4,
            SpecialType.System_Single => 4,
            
            SpecialType.System_Int64 => 8,
            SpecialType.System_UInt64 => 8,
            SpecialType.System_Double => 8,
            SpecialType.System_DateTime => 8,
            
            SpecialType.System_Decimal => 16,
            
            SpecialType.System_String => 32,  // References go last
            
            // Default: larger value for unknown reference types
            _ => type.IsReferenceType ? 32 : 8
        };
    }

    /// <summary>
    /// Determines if two types are compatible for direct assignment.
    /// Accounts for nullable reference types and type hierarchy.
    /// </summary>
    public static bool AreTypesDirectlyAssignable(ITypeSymbol sourceType, ITypeSymbol destType)
    {
        return SymbolEqualityComparer.Default.Equals(sourceType, destType);
    }

    /// <summary>
    /// Finds a mapping configuration for a source->destination type pair.
    /// </summary>
    public static bool TryFindMapping(
        ITypeSymbol sourceType,
        ITypeSymbol destType,
        IReadOnlyList<MappingConfiguration> mappings,
        out MappingConfiguration? mapping)
    {
        foreach (var m in mappings)
        {
            if (SymbolEqualityComparer.Default.Equals(m.SourceTypeSymbol, sourceType) &&
                SymbolEqualityComparer.Default.Equals(m.DestinationTypeSymbol, destType))
            {
                mapping = m;
                return true;
            }
        }

        mapping = null;
        return false;
    }

    /// <summary>
    /// Determines if a mapping exists for the given type pair.
    /// Faster than TryFindMapping when you only need boolean result.
    /// </summary>
    public static bool MappingExists(
        ITypeSymbol sourceType,
        ITypeSymbol destType,
        IReadOnlyList<MappingConfiguration> mappings)
    {
        foreach (var m in mappings)
        {
            if (SymbolEqualityComparer.Default.Equals(m.SourceTypeSymbol, sourceType) &&
                SymbolEqualityComparer.Default.Equals(m.DestinationTypeSymbol, destType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Validates that a destination property can be assigned from a source property.
    /// Reports diagnostics for incompatibilities.
    /// </summary>
    public static bool ValidatePropertyAssignability(
        PropertyInfo sourceProperty,
        PropertyInfo destProperty,
        IReadOnlyList<MappingConfiguration> mappings,
        GeneratorExecutionContext context)
    {
        // Direct type compatibility
        if (AreTypesDirectlyAssignable(sourceProperty.Type, destProperty.Type))
            return true;

        // Check if there's a mapping for the types
        if (MappingExists(sourceProperty.Type, destProperty.Type, mappings))
            return true;

        // Incompatible types
        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticsDescriptors.TypeMismatchInMapping,
            Location.None,
            sourceProperty.Name,
            sourceProperty.Type.Name,
            destProperty.Name,
            destProperty.Type.Name));

        return false;
    }
}
