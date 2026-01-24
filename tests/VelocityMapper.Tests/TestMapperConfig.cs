using VelocityMapper.Tests.Models;

namespace VelocityMapper.Tests;

/// <summary>
/// Mapper configuration for all test scenarios.
/// </summary>
public static class TestMapperConfig
{
    private static bool _isConfigured;
    private static readonly object _lock = new();

    public static void EnsureConfigured()
    {
        if (_isConfigured) return;
        
        lock (_lock)
        {
            if (_isConfigured) return;
            Configure();
            _isConfigured = true;
        }
    }

    [MapperConfiguration]
    public static void Configure()
    {
        // Basic mappings
        VelocityMap.CreateMap<Person, PersonDto>();
        VelocityMap.CreateMap<Address, AddressDto>();
        
        // Nested object mappings
        VelocityMap.CreateMap<Order, OrderDto>();
        VelocityMap.CreateMap<Customer, CustomerDto>();
        
        // Mapping with attributes
        VelocityMap.CreateMap<UserWithAttributes, UserWithAttributesDto>();
        
        // Simple mappings
        VelocityMap.CreateMap<SimpleSource, SimpleDestination>();
    }
}
