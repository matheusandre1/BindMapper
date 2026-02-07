using System.Runtime.CompilerServices;

namespace BindMapper;

/// <summary>
/// High-performance mapper configuration and setup API.
/// Use this to configure mappings between types at compile-time using Source Generators.
/// </summary>
/// <remarks>
/// <para>IMPORTANT: All methods in this class are analyzed at compile-time ONLY by the Source Generator.</para>
/// <para>The [MapperConfiguration] attribute marks the entry point for the source generator.</para>
/// <para>Usage Pattern:</para>
/// <code>
/// [MapperConfiguration]
/// public static void Configure()
/// {
///     // These calls are analyzed at compile-time to generate mapping code
///     MapperSetup.CreateMap&lt;User, UserDto&gt;();
///     MapperSetup.CreateMap&lt;Product, ProductDto&gt;()
///         .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.Price * s.Quantity));
/// }
/// </code>
/// <para>Then use at runtime (this is what users actually call):</para>
/// <code>
/// // Single object mapping
/// var userDto = Mapper.To&lt;UserDto&gt;(user);
/// 
/// // Zero-allocation update
/// Mapper.To(user, existingDto);
/// 
/// // Collection mapping - use the helper methods
/// var userDtos = Mapper.MapList(users, u => Mapper.To&lt;UserDto&gt;(u));
/// var userArray = Mapper.MapArray(users.ToArray(), u => Mapper.To&lt;UserDto&gt;(u));
/// var dtoEnum = Mapper.MapEnumerable(users, u => Mapper.To&lt;UserDto&gt;(u));
/// </code>
/// <para>⚠️ Rules:</para>
/// <list type="bullet">
///     <item>Only static methods can be decorated with [MapperConfiguration]</item>
///     <item>CreateMap calls MUST be inside [MapperConfiguration] methods (calls elsewhere are ignored)</item>
///     <item>The method body is NEVER executed at runtime</item>
///     <item>For collection mapping, users must pass the mapping function themselves</item>
/// </list>
/// </remarks>
public static class MapperSetup
{
    /// <summary>
    /// Creates a mapping configuration for the Source Generator to analyze at compile-time.
    /// This method is analyzed at compile-time ONLY - the return value is ignored at runtime.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to map to.</typeparam>
    /// <returns>A mapper configuration object for fluent configuration (compile-time only).</returns>
    /// <remarks>
    /// This method should only be called within methods decorated with [MapperConfiguration].
    /// It will not actually configure mapping at runtime - the Source Generator will analyze
    /// the calls and emit optimized code during compilation.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapperConfiguration<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        return new MapperConfiguration<TSource, TDestination>();
    }

    /// <summary>
    /// Creates a mapping configuration with custom fluent method chaining.
    /// </summary>
    /// <typeparam name="TSource">The source type to map from.</typeparam>
    /// <typeparam name="TDestination">The destination type to map to.</typeparam>
    /// <param name="config">Action to configure the mapping using fluent API. Only analyzed at compile-time.</param>
    /// <returns>A mapper configuration object for method chaining.</returns>
    /// <remarks>
    /// The Action parameter is never executed. It is analyzed at compile-time to extract the configuration calls.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MapperConfiguration<TSource, TDestination> CreateMap<TSource, TDestination>(
        Action<MapperConfiguration<TSource, TDestination>> config)
    {
        var cfg = new MapperConfiguration<TSource, TDestination>();
        config(cfg);
        return cfg;
    }
}
