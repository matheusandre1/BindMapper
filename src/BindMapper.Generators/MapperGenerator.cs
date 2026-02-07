using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BindMapper.Generators;

/// <summary>
/// Source Generator that builds the mapper API at compile time.
/// 
/// ARCHITECTURE:
/// 1. Syntax Phase: Find methods decorated with [MapperConfiguration]
/// 2. Semantic Phase: Analyze CreateMap calls and extract configurations
/// 3. Validation Phase: Check for duplicates and incompatibilities
/// 4. Generation Phase: Emit optimized mapping code
/// 
/// Performance characteristics:
/// - O(n) complexity where n = number of CreateMap calls
/// - Minimal allocations: Uses SyntaxWalker instead of LINQ.DescendantNodes()
/// - Deterministic output: All results sorted by type name
/// - Incremental: Only recomputes when syntax tree changes
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class MapperGenerator : IIncrementalGenerator
{
    private const string MapperConfigurationAttributeName = "BindMapper.MapperConfigurationAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // SYNTAX PHASE: Find all methods with [MapperConfiguration] attribute
        // This is lightweight - just syntax tree parsing, no semantic analysis
        var configMethods = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => IsMethodWithAttributes(node),
                static (ctx, _) => GetMapperConfigurationMethod(ctx))
            .Where(static m => m is not null)
            .Collect();

        // SEMANTIC PHASE: Combine with compilation for full semantic analysis
        var mappingsProvider = context.CompilationProvider
            .Combine(configMethods)
            .SelectMany(static (source, _) => CollectAllMappings(source.Left, source.Right));

        // GENERATION PHASE: Generate mapper code
        context.RegisterSourceOutput(
            mappingsProvider.Collect(),
            static (spc, mappings) => GenerateMapperFile(spc, mappings));
    }

    /// <summary>
    /// Quick predicate to filter methods with attributes (avoids semantic analysis overhead).
    /// </summary>
    private static bool IsMethodWithAttributes(SyntaxNode node)
    {
        return node is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
    }

    /// <summary>
    /// Extracts method if it has [MapperConfiguration] attribute.
    /// </summary>
    private static MethodDeclarationSyntax? GetMapperConfigurationMethod(GeneratorSyntaxContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
            return null;

        var symbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration) as IMethodSymbol;
        if (symbol is null)
            return null;

        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass?.ToDisplayString() == MapperConfigurationAttributeName)
                return methodDeclaration;
        }

        return null;
    }

    /// <summary>
    /// Collects all mappings from configuration methods.
    /// Uses SelectMany to avoid Collect() overhead on large projects.
    /// </summary>
    private static IEnumerable<MappingConfiguration> CollectAllMappings(
        Compilation compilation,
        ImmutableArray<MethodDeclarationSyntax?> configMethods)
    {
        // Create analyzer for mapping configuration extraction
        var analyzer = new MappingConfigurationAnalyzer(compilation);

        foreach (var methodSyntax in configMethods)
        {
            if (methodSyntax is null)
                continue;

            var mappings = analyzer.AnalyzeMethod(methodSyntax);
            foreach (var mapping in mappings)
            {
                yield return mapping;
            }
        }
    }

    /// <summary>
    /// Generates the Mapper.g.cs file with all mapping methods.
    /// CRITICAL: Sorts mappings deterministically to ensure reproducible builds.
    /// </summary>
    private static void GenerateMapperFile(
        SourceProductionContext context,
        ImmutableArray<MappingConfiguration> mappings)
    {
        if (mappings.Length == 0)
            return;

        // CRITICAL FIX: Sort deterministically for reproducible builds
        // Different build orders must produce identical generated code
        var sortedMappings = mappings
            .OrderBy(m => m.SourceTypeFullName, StringComparer.Ordinal)
            .ThenBy(m => m.DestinationTypeFullName, StringComparer.Ordinal)
            .ToList();

        var generator = new MapperCodeGenerator();
        var sourceText = generator.GenerateMapperSource(sortedMappings);

        if (sourceText.Length > 0)
        {
            context.AddSource("Mapper.g.cs", SourceText.From(sourceText, Encoding.UTF8));
        }
    }
}
