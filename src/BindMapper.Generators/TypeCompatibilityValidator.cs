using Microsoft.CodeAnalysis;

namespace BindMapper.Generators;

/// <summary>
/// Validates type compatibility for mapping operations, specifically for generic boxing scenarios.
/// </summary>
internal static class TypeCompatibilityValidator
{
    /// <summary>
    /// Checks if a source type can be safely boxed/cast to a destination generic type constraint.
    /// This prevents runtime crashes in collection mappers.
    /// </summary>
    public static bool IsSafeForGenericBoxing(ITypeSymbol sourceType, ITypeSymbol destTypeConstraint)
    {
        // Both reference types: Safe (boxing unnecessary)
        if (sourceType.IsReferenceType && destTypeConstraint.IsReferenceType)
            return true;

        // Both value types with same shape: Safe if assignable
        if (!sourceType.IsReferenceType && !destTypeConstraint.IsReferenceType)
            return AreTypesDirectlyAssignable(sourceType, destTypeConstraint);

        // Source is value type, dest is reference type: UNSAFE
        // (boxing value type then casting to reference type fails)
        if (!sourceType.IsReferenceType && destTypeConstraint.IsReferenceType)
            return false;

        // Source is reference type, dest is value type: UNSAFE
        // (cannot unbox non-matching types)
        if (sourceType.IsReferenceType && !destTypeConstraint.IsReferenceType)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if source and destination types are directly assignable without conversion.
    /// </summary>
    private static bool AreTypesDirectlyAssignable(ITypeSymbol source, ITypeSymbol dest)
    {
        if (SymbolEqualityComparer.Default.Equals(source, dest))
            return true;

        // Check if source implements dest interface
        foreach (var intf in source.Interfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(intf, dest))
                return true;
        }

        // Check base classes
        var current = source.BaseType;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, dest))
                return true;
            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Determines if collection boxing approach is safe for a type pair.
    /// Returns true if we can use (TDestination)(object)To(item) pattern safely.
    /// </summary>
    public static bool CanUseBoxingPattern(MappingConfiguration config)
    {
        // Only safe if BOTH are reference types
        // OR both are value types with exact same name
        if (config.SourceTypeSymbol.IsReferenceType && config.DestinationTypeSymbol.IsReferenceType)
            return true;

        if (!config.SourceTypeSymbol.IsReferenceType && !config.DestinationTypeSymbol.IsReferenceType)
        {
            // Both value types - only safe if identical or compatible
            return SymbolEqualityComparer.Default.Equals(
                config.SourceTypeSymbol,
                config.DestinationTypeSymbol) || 
                AreTypesDirectlyAssignable(config.SourceTypeSymbol, config.DestinationTypeSymbol);
        }

        // Mixed reference/value type - NOT SAFE
        return false;
    }
}
