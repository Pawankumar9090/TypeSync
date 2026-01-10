using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for MapperConfiguration.
/// </summary>
public class MapperConfigurationTests
{
    [Fact]
    public void CreateMap_MultipleTypes_ShouldRegisterAll()
    {
        // Arrange & Act
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.CreateMapper();

        // Assert - All mappings should work
        var user = new User { Id = 1, FirstName = "John" };
        var product = new Product { Id = 1, Name = "Test" };
        var order = new Order { Id = 1, Total = 100 };

        var userDto = mapper.Map<User, UserDto>(user);
        var productDto = mapper.Map<Product, ProductDto>(product);
        var orderDto = mapper.Map<Order, OrderDto>(order);

        userDto.Id.Should().Be(1);
        productDto.Name.Should().Be("Test");
        orderDto.Total.Should().Be(100);
    }

    [Fact]
    public void AssertConfigurationIsValid_AllMapped_ShouldNotThrow()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
        });

        // Act & Assert
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void AssertConfigurationIsValid_UnmappedProperty_ShouldThrow()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
            // FullName in UserDto has no matching source property
        });

        // Act & Assert
        var act = () => config.AssertConfigurationIsValid();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unmapped property*FullName*");
    }

    [Fact]
    public void AssertConfigurationIsValid_IgnoredProperty_ShouldNotThrow()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.MappedAt, opt => opt.Ignore());
        });

        // Act & Assert
        var act = () => config.AssertConfigurationIsValid();
        act.Should().NotThrow();
    }

    [Fact]
    public void CreateMapper_ShouldReturnFunctionalMapper()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        // Act
        var mapper = config.CreateMapper();

        // Assert
        mapper.Should().NotBeNull();
        mapper.Should().BeAssignableTo<IMapper>();
    }

    [Fact]
    public void Configuration_WithCondition_ShouldRespectCondition()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .Condition(src => src.IsActive);
        });
        var mapper = config.CreateMapper();

        var activeUser = new User { Id = 1, FirstName = "John", IsActive = true };
        var inactiveUser = new User { Id = 2, FirstName = "Jane", IsActive = false };

        // Act
        var activeResult = mapper.Map<User, UserDto>(activeUser);
        var inactiveResult = mapper.Map<User, UserDto>(inactiveUser);

        // Assert
        activeResult.Id.Should().Be(1);
        activeResult.FirstName.Should().Be("John");

        // For inactive user, mapping condition not met, returns default
        inactiveResult.Id.Should().Be(0);
        inactiveResult.FirstName.Should().BeEmpty();
    }

}
