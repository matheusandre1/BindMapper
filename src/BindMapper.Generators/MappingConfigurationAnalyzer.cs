using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BindMapper.Generators;

/// <summary>
/// Analyzes CreateMap invocations to extract mapping configurations.
/// Handles deduplication, fluent configuration parsing, and validation.
/// </summary>
internal sealed class MappingConfigurationAnalyzer
{
    private readonly Compilation _compilation;

    // Cache for deduplication: key format "SourceFQN|DestFQN"
    private readonly HashSet<string> _seenMappings = new(StringComparer.Ordinal);
    private readonly List<(Location? Location, string SourceType, string DestType)> _mappingLocations = new();
    
    // Track duplicates for diagnostic reporting
    private readonly List<(Location? Location, string SourceType, string DestType)> _duplicateMappings = new();

    public MappingConfigurationAnalyzer(Compilation compilation)
    {
        _compilation = compilation;
    }

    /// <summary>
    /// Gets the list of duplicate mappings detected during analysis.
    /// IMPROVED: Track duplicates for diagnostic reporting in generator.
    /// </summary>
    public IReadOnlyList<(Location? Location, string SourceType, string DestType)> GetDuplicateMappings()
    {
        return _duplicateMappings.AsReadOnly();
    }

    /// <summary>
    /// Analyzes a single invocation method and extracts all mappings defined within.
    /// Returns null if method couldn't be analyzed.
    /// </summary>
    public IReadOnlyList<MappingConfiguration> AnalyzeMethod(
        MethodDeclarationSyntax methodSyntax)
    {
        if (methodSyntax == null)
            return Array.Empty<MappingConfiguration>();

        var result = new List<MappingConfiguration>();
        var semanticModel = _compilation.GetSemanticModel(methodSyntax.SyntaxTree);

        // Use custom walker instead of LINQ to avoid allocation spikes
        var walker = new CreateMapCallWalker();
        walker.Visit(methodSyntax);

        foreach (var invocation in walker.CreateMapCalls)
        {
            var mapping = AnalyzeCreateMapInvocation(invocation, semanticModel, methodSyntax);
            if (mapping != null)
            {
                // Check for duplicates
                var mapKey = $"{mapping.SourceTypeFullName}|{mapping.DestinationTypeFullName}";
                
                if (_seenMappings.Contains(mapKey))
                {
                    // IMPROVED: Track duplicate for diagnostic reporting
                    _duplicateMappings.Add((invocation.GetLocation(), mapping.SourceTypeName, mapping.DestinationTypeName));
                }
                else
                {
                    _seenMappings.Add(mapKey);
                    _mappingLocations.Add((invocation.GetLocation(), mapping.SourceTypeName, mapping.DestinationTypeName));
                    result.Add(mapping);

                    // Handle ReverseMap
                    if (mapping.FluentConfig.HasReverseMap)
                    {
                        var reverseMapKey = $"{mapping.DestinationTypeFullName}|{mapping.SourceTypeFullName}";
                        if (!_seenMappings.Contains(reverseMapKey))
                        {
                            _seenMappings.Add(reverseMapKey);
                            result.Add(new MappingConfiguration(
                                mapping.DestinationTypeSymbol,
                                mapping.SourceTypeSymbol,
                                mapping.DestinationTypeFullName,
                                mapping.SourceTypeFullName,
                                mapping.DestinationTypeName,
                                mapping.SourceTypeName,
                                new FluentConfiguration()));
                        }
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Analyzes a single CreateMap&lt;TSource, TDest&gt;(...) invocation.
    /// </summary>
    private MappingConfiguration? AnalyzeCreateMapInvocation(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        MethodDeclarationSyntax parentMethod)
    {
        // Extract type arguments
        GenericNameSyntax? genericName = null;
        
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name is GenericNameSyntax gn)
        {
            genericName = gn;
        }
        else if (invocation.Expression is GenericNameSyntax directGn)
        {
            genericName = directGn;
        }

        if (genericName?.TypeArgumentList.Arguments.Count != 2)
            return null;

        var sourceTypeSyntax = genericName.TypeArgumentList.Arguments[0];
        var destTypeSyntax = genericName.TypeArgumentList.Arguments[1];

        var sourceType = semanticModel.GetTypeInfo(sourceTypeSyntax).Type;
        var destType = semanticModel.GetTypeInfo(destTypeSyntax).Type;

        if (sourceType is null || destType is null)
            return null;

        if (sourceType.Kind == SymbolKind.TypeParameter || destType.Kind == SymbolKind.TypeParameter)
        {
            // Generic type parameters not supported at this time
            return null;
        }

        var typeFormat = new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

        // Parse fluent configuration from chain
        var fluentConfig = ParseFluentConfiguration(invocation, semanticModel);

        return new MappingConfiguration(
            sourceType,
            destType,
            sourceType.ToDisplayString(typeFormat),
            destType.ToDisplayString(typeFormat),
            sourceType.Name,
            destType.Name,
            fluentConfig);
    }

    /// <summary>
    /// Parses fluent method chains like .ReverseMap(), .ForMember(), etc.
    /// </summary>
    private static FluentConfiguration ParseFluentConfiguration(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        var config = new FluentConfiguration();
        
        // Walk up the chain to find all chained method calls
        SyntaxNode? current = invocation.Parent;
        
        while (current is MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Parent is InvocationExpressionSyntax chainedInvocation)
            {
                var methodName = memberAccess.Name.Identifier.Text;
                
                switch (methodName)
                {
                    case "ReverseMap":
                        config.HasReverseMap = true;
                        break;
                    case "IgnoreAllNonExisting":
                        config.IgnoreAllNonExisting = true;
                        break;
                    case "ForMember":
                        ParseForMemberCall(chainedInvocation, config, semanticModel);
                        break;
                }
                
                current = chainedInvocation.Parent;
            }
            else
            {
                break;
            }
        }

        return config;
    }

    /// <summary>
    /// Parses .ForMember(d => d.Property, opt => opt.MapFrom(...)) chains.
    /// </summary>
    private static void ParseForMemberCall(
        InvocationExpressionSyntax invocation,
        FluentConfiguration config,
        SemanticModel semanticModel)
    {
        if (invocation.ArgumentList.Arguments.Count < 2)
            return;

        var destMemberArg = invocation.ArgumentList.Arguments[0];
        var optionsArg = invocation.ArgumentList.Arguments[1];

        // Extract destination member name from lambda: d => d.PropertyName
        string? destMemberName = ExtractMemberNameFromLambda(destMemberArg.Expression);
        
        if (destMemberName is null)
            return;

        // Parse the options lambda: opt => opt.Ignore() or opt => opt.MapFrom(...)
        if (optionsArg.Expression is SimpleLambdaExpressionSyntax optionsLambda)
        {
            if (optionsLambda.Body is InvocationExpressionSyntax optInvocation)
            {
                if (optInvocation.Expression is MemberAccessExpressionSyntax optMemberAccess)
                {
                    var optMethod = optMemberAccess.Name.Identifier.Text;

                    switch (optMethod)
                    {
                        case "Ignore":
                            config.IgnoredMembers.Add(destMemberName);
                            break;
                        case "MapFrom":
                            if (optInvocation.ArgumentList.Arguments.Count > 0)
                            {
                                var mapFromArg = optInvocation.ArgumentList.Arguments[0];
                                if (mapFromArg.Expression is SimpleLambdaExpressionSyntax mapFromLambda)
                                {
                                    ExtractMapFromExpression(mapFromLambda, destMemberName, config);
                                }
                            }
                            break;
                        case "UseValue":
                            if (optInvocation.ArgumentList.Arguments.Count > 0)
                            {
                                config.MemberValues[destMemberName] = 
                                    optInvocation.ArgumentList.Arguments[0].ToString();
                            }
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Safely extracts member name from lambda without materialization overhead.
    /// </summary>
    private static string? ExtractMemberNameFromLambda(ExpressionSyntax expression)
    {
        if (expression is not SimpleLambdaExpressionSyntax lambda)
            return null;

        if (lambda.Body is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Name.Identifier.Text;
        }

        return null;
    }

    /// <summary>
    /// Extracts mapping expression from MapFrom lambda safely.
    /// </summary>
    private static void ExtractMapFromExpression(
        SimpleLambdaExpressionSyntax lambda,
        string destMemberName,
        FluentConfiguration config)
    {
        if (lambda.Body is MemberAccessExpressionSyntax memberAccess)
        {
            // Simple case: s.Property
            config.MemberMappings[destMemberName] = memberAccess.Name.Identifier.Text;
        }
        else
        {
            // Complex expression: store as-is and validate later
            config.MemberExpressions[destMemberName] = lambda.Body.ToString();
        }
    }
}
