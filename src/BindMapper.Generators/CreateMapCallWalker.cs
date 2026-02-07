using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BindMapper.Generators;

/// <summary>
/// High-performance syntax walker that finds CreateMap invocations without LINQ materialization.
/// Replaces DescendantNodes().OfType().Where() for better performance in large files.
/// </summary>
internal sealed class CreateMapCallWalker : CSharpSyntaxWalker
{
    /// <summary>
    /// All CreateMap invocations found during traversal.
    /// Only invocations matching the pattern CreateMap&lt;T1, T2&gt;() are collected.
    /// IMPROVED: Removed hardcoded capacity - let List grow naturally.
    /// </summary>
    public List<InvocationExpressionSyntax> CreateMapCalls { get; } = new();

    /// <summary>
    /// Visits all invocation expressions and filters those matching CreateMap pattern.
    /// This avoids creating intermediate collections that LINQ would materialize.
    /// </summary>
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        // Check if this is a CreateMap invocation
        if (IsCreateMapCall(node))
        {
            CreateMapCalls.Add(node);
        }

        // Continue visiting deeper nodes
        base.VisitInvocationExpression(node);
    }

    /// <summary>
    /// Determines if an invocation is a CreateMap call with proper generic arguments.
    /// Handles both forms: Mapper.CreateMap&lt;A, B&gt;() and CreateMap&lt;A, B&gt;()
    /// </summary>
    private static bool IsCreateMapCall(InvocationExpressionSyntax invocation)
    {
        GenericNameSyntax? genericName = null;

        // Form 1: Mapper.CreateMap<T1, T2>() or obj.CreateMap<T1, T2>()
        if (invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.Name is GenericNameSyntax gn
            && gn.Identifier.Text == "CreateMap")
        {
            genericName = gn;
        }
        // Form 2: CreateMap<T1, T2>() (direct call)
        else if (invocation.Expression is GenericNameSyntax directGn
                 && directGn.Identifier.Text == "CreateMap")
        {
            genericName = directGn;
        }

        // Must have exactly 2 type arguments: source and destination
        return genericName is not null && genericName.TypeArgumentList.Arguments.Count == 2;
    }
}
