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
        MapperSetup.CreateMap<Person, PersonDto>();
        MapperSetup.CreateMap<Address, AddressDto>();
        
        // Nested object mappings
        MapperSetup.CreateMap<Order, OrderDto>();
        MapperSetup.CreateMap<Customer, CustomerDto>();
        
        // Mapping with attributes
        MapperSetup.CreateMap<UserWithAttributes, UserWithAttributesDto>();
        
        // Simple mappings
        MapperSetup.CreateMap<SimpleSource, SimpleDestination>();
    }
}
