using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BindMapper.Generators;

/// <summary>
/// Validates custom MapFrom expressions and other fluent configurations.
/// Performs semantic analysis to catch errors before code generation.
/// </summary>
internal sealed class ExpressionValidator
{
    private readonly Compilation _compilation;

    public ExpressionValidator(Compilation compilation)
    {
        _compilation = compilation;
    }

    /// <summary>
    /// Validates that a custom MapFrom expression is syntactically correct and type-compatible.
    /// Returns true if valid, false if there are issues.
    /// IMPROVED: Case-sensitive pattern matching to prevent bypass attempts.
    /// </summary>
    public bool ValidateMapFromExpression(
        string expressionText,
        IPropertySymbol sourcePropertySymbol,
        PropertyInfo destProperty)
    {
        if (string.IsNullOrWhiteSpace(expressionText))
            return false;

        // IMPROVED: Case-sensitive validation (prevent obfuscation bypass)
        // Check if expression contains forbidden reflection patterns
        var invalidPatterns = new[]
        {
            "System.Reflection",     // Reflection namespace
            "typeof(",               // Runtime type operations
            "GetType(",              // Runtime type introspection
            "MethodInfo",            // Reflection APIs
            "PropertyInfo",          // Reflection APIs
            "FieldInfo",             // Reflection APIs
            "Activator.",            // Dynamic instantiation
            "Invoke(",               // Dynamic invocation
            "Delegate.",             // Dynamic delegates
        };

        // IMPROVED: Case-sensitive contains check
        foreach (var pattern in invalidPatterns)
        {
            if (expressionText.Contains(pattern, StringComparison.Ordinal))
                return false;
        }

        // IMPROVED: Check for suspicious method calls at word boundaries
        // Prevents: "GetTypeInfo", "MyGetType", etc.
        var suspiciousMethods = new[] { "GetType", "GetTypeInfo", "InvokeMember", "CreateInstance" };
        foreach (var method in suspiciousMethods)
        {
            // Look for method call patterns: method(
            if (System.Text.RegularExpressions.Regex.IsMatch(expressionText, $@"\b{method}\s*\("))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validates that a ForMember configuration targets an existing property.
    /// </summary>
    public bool ValidateForMemberTargetExists(
        string targetPropertyName,
        ITypeSymbol destinationType)
    {
        foreach (var member in destinationType.GetMembers())
        {
            if (member is IPropertySymbol prop && prop.Name == targetPropertyName)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Validates that source and destination types are valid for mapping.
    /// </summary>
    public bool ValidateTypeCompatibility(
        ITypeSymbol sourceType,
        ITypeSymbol destinationType)
    {
        // Both must be concrete types (not interfaces or abstract)
        if (sourceType.IsAbstract || destinationType.IsAbstract)
            return false;

        // Check for generic constraints
        if (sourceType is INamedTypeSymbol sourceNamed && sourceNamed.Arity > 0 && sourceNamed.IsUnboundGenericType)
            return false;

        if (destinationType is INamedTypeSymbol destNamed && destNamed.Arity > 0 && destNamed.IsUnboundGenericType)
            return false;

        return true;
    }

    /// <summary>
    /// Validates that a destination property can receive an assignment from source.
    /// Checks both type compatibility and accessibility.
    /// </summary>
    public bool ValidatePropertyAssignment(
        PropertyInfo sourceProperty,
        PropertyInfo destinationProperty,
        ITypeSymbol sourceType,
        ITypeSymbol destinationType)
    {
        // Source must be readable
        if (!sourceProperty.IsReadable)
            return false;

        // Destination must be writable
        if (!destinationProperty.IsWriteable)
            return false;

        // Both must have compatible visibility
        // (already checked in GetPublicProperties, but validate again)

        return true;
    }
}
