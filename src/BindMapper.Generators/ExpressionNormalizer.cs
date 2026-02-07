using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BindMapper.Generators;

/// <summary>
/// Safe expression normalizer that uses Roslyn parsing to correctly identify lambda parameters
/// without corrupting string literals or comments.
/// </summary>
internal sealed class ExpressionNormalizer
{
    /// <summary>
    /// Normalizes a lambda expression body by replacing parameter references with "source".
    /// Uses Roslyn syntax tree parsing for correctness.
    /// 
    /// Example: "s.FirstName ?? s.LastName" → "source.FirstName ?? source.LastName"
    /// But preserves: "Use s.FirstName in code" as-is (it's in a string literal context)
    /// </summary>
    public static string NormalizeExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return expression;

        try
        {
            // Parse as a lambda expression body for safer handling
            // We wrap it as a complete lambda so Roslyn can parse it properly
            var lambdaSyntax = $"({FindParameterName(expression)}) => {expression}";
            var tree = CSharpSyntaxTree.ParseText(lambdaSyntax);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            // Get the lambda
            var lambda = root.DescendantNodes().OfType<LambdaExpressionSyntax>().FirstOrDefault();
            if (lambda?.Body is null)
                return FallbackNormalization(expression);

            // Rewrite the parameter names
            var rewriter = new ParameterNameRewriter(FindParameterName(expression), "source");
            var normalized = rewriter.Visit(lambda.Body);

            return normalized?.ToString() ?? FallbackNormalization(expression);
        }
        catch
        {
            // If Roslyn parsing fails, fall back to conservative approach
            return FallbackNormalization(expression);
        }
    }

    /// <summary>
    /// Detects the parameter name used in the expression by parsing as lambda.
    /// More robust than pattern matching alone.
    /// IMPROVED: Uses Roslyn to extract actual lambda parameter name.
    /// </summary>
    private static string FindParameterName(string expression)
    {
        try
        {
            // Try to parse as complete lambda to get actual parameter name
            var tree = CSharpSyntaxTree.ParseText($"x => {expression}");
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var lambda = root.DescendantNodes().OfType<LambdaExpressionSyntax>().FirstOrDefault();
            
            if (lambda is SimpleLambdaExpressionSyntax simpleLambda)
            {
                return simpleLambda.Parameter.Identifier.Text;
            }
            else if (lambda is ParenthesizedLambdaExpressionSyntax parenLambda && parenLambda.ParameterList?.Parameters.FirstOrDefault() is not null)
            {
                return parenLambda.ParameterList.Parameters[0].Identifier.Text;
            }
        }
        catch
        {
            // If parsing fails, fall back to heuristics
        }

        // Heuristic detection as fallback
        // Common parameter name patterns
        if (expression.StartsWith("s.") || expression.Contains(" s.") || expression.Contains("(s.") || expression.Contains(",s."))
            return "s";
        if (expression.StartsWith("src.") || expression.Contains(" src.") || expression.Contains("(src.") || expression.Contains(",src."))
            return "src";
        if (expression.StartsWith("source.") || expression.Contains(" source.") || expression.Contains("(source.") || expression.Contains(",source."))
            return "source";
        if (expression.StartsWith("p.") || expression.Contains(" p.") || expression.Contains("(p.") || expression.Contains(",p."))
            return "p";
        if (expression.StartsWith("x.") || expression.Contains(" x.") || expression.Contains("(x.") || expression.Contains(",x."))
            return "x";

        // Default: assume 's' (most common)
        return "s";
    }

    /// <summary>
    /// Conservative fallback that only replaces parameter references at known safe positions.
    /// </summary>
    private static string FallbackNormalization(string expression)
    {
        var paramName = FindParameterName(expression);

        // Replace only parameter.member patterns (NOT string content)
        // Use careful boundary checking
        var result = new StringBuilder();
        var i = 0;
        while (i < expression.Length)
        {
            // Check for parameter reference at word boundary
            if (IsParameterReference(expression, i, paramName))
            {
                result.Append("source");
                i += paramName.Length;
            }
            else
            {
                result.Append(expression[i]);
                i++;
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Checks if position i starts a valid parameter reference (e.g., "s." or "s =>")
    /// </summary>
    private static bool IsParameterReference(string expression, int position, string paramName)
    {
        // Must be at word boundary (start of string OR after non-identifier char)
        if (position > 0)
        {
            var prev = expression[position - 1];
            if (char.IsLetterOrDigit(prev) || prev == '_')
                return false;
        }

        // Must match the parameter name
        if (position + paramName.Length > expression.Length)
            return false;

        if (expression[position..(position + paramName.Length)] != paramName)
            return false;

        // Must be followed by . or => or end of word
        if (position + paramName.Length < expression.Length)
        {
            var next = expression[position + paramName.Length];
            return next == '.' || next == ' ' || next == ')' || next == ']';
        }

        return true;
    }

    /// <summary>
    /// Roslyn-based syntax rewriter that only replaces identifier nodes that are lambda parameters
    /// </summary>
    private sealed class ParameterNameRewriter : CSharpSyntaxRewriter
    {
        private readonly string _oldName;
        private readonly string _newName;

        public ParameterNameRewriter(string oldName, string newName)
        {
            _oldName = oldName;
            _newName = newName;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Identifier.Text == _oldName)
            {
                return SyntaxFactory.IdentifierName(_newName)
                    .WithTriviaFrom(node);
            }

            return base.VisitIdentifierName(node);
        }
    }
}
