namespace BindMapper;

/// <summary>
/// Marks a static method as the Source Generator entry point for mapping configuration.
/// The method is analyzed at compile-time ONLY — it is never executed at runtime.
/// </summary>
/// <remarks>
/// <para><strong>IMPORTANT RULES:</strong></para>
/// <list type="number">
///     <item>The decorated method MUST be static and public</item>
///     <item>The decorated method body is NEVER executed at runtime</item>
///     <item>Only MapperSetup.CreateMap&lt;T1, T2&gt;() calls inside this method are analyzed</item>
///     <item>CreateMap calls OUTSIDE [MapperConfiguration] methods are ignored</item>
///     <item>You can have multiple [MapperConfiguration] methods (last configuration wins for duplicates)</item>
/// </list>
/// <para><strong>Usage:</strong></para>
/// <code>
/// [MapperConfiguration]
/// public static void ConfigureMappings()
/// {
///     // All CreateMap calls here are analyzed by the Source Generator at compile-time
///     MapperSetup.CreateMap&lt;User, UserDto&gt;();
///     
///     // Customize specific properties
///     MapperSetup.CreateMap&lt;Product, ProductDto&gt;()
///         .ForMember(d => d.TotalPrice, opt => opt.MapFrom(s => s.Price * s.Quantity));
///     
///     // Enable bidirectional mapping
///     MapperSetup.CreateMap&lt;Address, AddressDto&gt;()
///         .ReverseMap();
/// }
/// </code>
/// <para><strong>Then use at runtime:</strong></para>
/// <code>
/// var user = new User { Id = 1, Name = "John" };
/// 
/// // Single object mapping
/// var userDto = Mapper.To&lt;UserDto&gt;(user);
/// 
/// // Collection mapping
/// var users = GetUsers();
/// var dtos = Mapper.MapList(users, u => Mapper.To&lt;UserDto&gt;(u));
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MapperConfigurationAttribute : Attribute
{
}
