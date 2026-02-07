namespace BindMapper;

/// <summary>
/// Configuration builder for creating type mappings.
/// This API is analyzed by the Source Generator at compile-time to detect mapping configurations.
/// The fluent methods return this same instance for method chaining.
/// </summary>
/// <remarks>
/// <para><strong>IMPORTANT: This is a compile-time-only API.</strong></para>
/// <para>While these methods exist at runtime, they are called within [MapperConfiguration] methods,
/// which are never executed at runtime. The Source Generator extracts configuration
/// from the method body and generates optimized mapping code.</para>
/// <para>All method calls are analyzed statically — their return values are ignored.</para>
/// <para>Example usage:</para>
/// <code>
/// [MapperConfiguration]
/// public static void Configure()
/// {
///     MapperSetup.CreateMap&lt;User, UserDto&gt;()
///         .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName))
///         .ForMember(d => d.Age, opt => opt.Ignore());
/// }
/// </code>
/// </remarks>
/// <typeparam name="TSource">The source type to map from.</typeparam>
/// <typeparam name="TDestination">The destination type to map to.</typeparam>
public sealed class MapperConfiguration<TSource, TDestination>
{
    /// <summary>
    /// Configures custom mapping for a specific destination member.
    /// Used to customize property mapping behavior for individual properties.
    /// </summary>
    /// <typeparam name="TMember">The type of the destination member being configured.</typeparam>
    /// <param name="destinationMember">Lambda expression selecting the destination property/field to configure.
    /// Must be a simple property access expression, e.g., d => d.Name</param>
    /// <param name="memberOptions">Action to configure options for this member.
    /// Call MapFrom() to specify custom source, or Ignore() to skip this property.</param>
    /// <returns>The configuration instance for method chaining.</returns>
    /// <remarks>
    /// <para>Example uses:</para>
    /// <code>
    /// // Rename property
    /// .ForMember(d => d.DisplayName, opt => opt.MapFrom(s => s.Name))
    /// 
    /// // Custom transformation
    /// .ForMember(d => d.Total, opt => opt.MapFrom(s => s.Price * s.Quantity))
    /// 
    /// // Skip mapping this property
    /// .ForMember(d => d.InternalId, opt => opt.Ignore())
    /// </code>
    /// </remarks>
    public MapperConfiguration<TSource, TDestination> ForMember<TMember>(
        System.Linq.Expressions.Expression<Func<TDestination, TMember>> destinationMember,
        Action<MemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        _ = destinationMember;
        _ = memberOptions;
        // This is analyzed by the Source Generator
        return this;
    }

    /// <summary>
    /// Configures reverse mapping, enabling bidirectional mapping (TDestination → TSource).
    /// When enabled, the Source Generator will also generate a mapping from TDestination back to TSource.
    /// </summary>
    /// <returns>The configuration instance for method chaining.</returns>
    /// <remarks>
    /// <para><strong>Requirements for ReverseMap to work:</strong></para>
    /// <list type="bullet">
    ///     <item>TSource must have all writable properties that correspond to TDestination members</item>
    ///     <item>Read-only properties on TSource will cause a compile warning (VMAPPER003)</item>
    ///     <item>Property names must match (or use [MapFrom] attribute on TSource)</item>
    /// </list>
    /// <para>Example:</para>
    /// <code>
    /// [MapperConfiguration]
    /// public static void Configure()
    /// {
    ///     MapperSetup.CreateMap&lt;User, UserDto&gt;()
    ///         .ReverseMap();  // Also enables UserDto → User
    /// }
    /// 
    /// // Now both directions work:
    /// var dto = Mapper.To&lt;UserDto&gt;(user);    // User → UserDto
    /// var user = Mapper.To&lt;User&gt;(userDto);    // UserDto → User
    /// </code>
    /// </remarks>
    public MapperConfiguration<TSource, TDestination> ReverseMap()
    {
        // This is analyzed by the Source Generator
        return this;
    }
}

/// <summary>
/// Provides member-level mapping configuration options.
/// Used inside ForMember() to customize how individual properties are mapped.
/// </summary>
/// <remarks>
/// <para>Available options:</para>
/// <list type="bullet">
///     <item>MapFrom(Expression): Map from a custom source expression</item>
///     <item>Ignore(): Skip mapping this member (won't be initialized)</item>
/// </list>
/// <para>Example:</para>
/// <code>
/// .ForMember(d => d.FullName, 
///     opt => opt.MapFrom(s => s.FirstName + " " + s.LastName))
/// 
/// .ForMember(d => d.InternalId,
///     opt => opt.Ignore())
/// </code>
/// </remarks>
public sealed class MemberConfigurationExpression<TSource, TDestination, TMember>
{
    /// <summary>
    /// Maps from a custom source expression (simple form).
    /// The expression receives only the source object.
    /// </summary>
    /// <param name="sourceMember">Lambda expression that extracts/transforms the source value.
    /// Example: s => s.Name, or s => s.Price * s.Quantity</param>
    /// <remarks>
    /// Use this for simple property access or transformations.
    /// If you need access to both source and destination, use the overload with TDestination parameter.
    /// </remarks>
    public void MapFrom(System.Linq.Expressions.Expression<Func<TSource, TMember>> sourceMember)
    {
        _ = sourceMember;
        // This is analyzed by the Source Generator
    }

    /// <summary>
    /// Maps from a custom source expression with full context.
    /// The expression receives both the source object and the destination object.
    /// </summary>
    /// <param name="valueResolver">Lambda expression with 2 parameters: source and destination.
    /// Example: (s, d) => s.Price * (1 + d.TaxRate)</param>
    /// <remarks>
    /// Use this when mapping a property needs to consider properties on the destination object.
    /// For example, applying conditional logic based on existing destination state.
    /// </remarks>
    public void MapFrom(System.Linq.Expressions.Expression<Func<TSource, TDestination, TMember>> valueResolver)
    {
        _ = valueResolver;
        // This is analyzed by the Source Generator
    }

    /// <summary>
    /// Ignores this member during mapping.
    /// The destination property will not be set, retaining its default value.
    /// </summary>
    /// <remarks>
    /// <para>Use this when:</para>
    /// <list type="bullet">
    ///     <item>Source doesn't have a corresponding property</item>
    ///     <item>You want to skip mapping a property for security/performance reasons</item>
    ///     <item>The property requires manual initialization</item>
    /// </list>
    /// <para>Example:</para>
    /// <code>
    /// .ForMember(d => d.PasswordHash, opt => opt.Ignore())  // Don't expose password
    /// .ForMember(d => d.InternalId, opt => opt.Ignore())    // No source property
    /// </code>
    /// </remarks>
    public void Ignore()
    {
        // This is analyzed by the Source Generator
    }
}
