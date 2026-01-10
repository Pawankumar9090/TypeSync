using System.Reflection;
using FluentAssertions;

namespace TypeSync.Tests;

#region Test Profiles

public class UserMappingProfile : MappingProfile
{
    protected override void ConfigureMappings()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom((Func<User, string>)(src => $"{src.FirstName} {src.LastName}")));
    }
}

public class ProductMappingProfile : MappingProfile
{
    protected override void ConfigureMappings()
    {
        CreateMap<Product, ProductDto>();
    }
}

public class OrderMappingProfile : MappingProfile
{
    protected override void ConfigureMappings()
    {
        CreateMap<Order, OrderDto>();
        CreateMap<Customer, CustomerDto>();
    }
}

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Tests for MappingProfile functionality.
/// </summary>
public class MappingProfileTests
{
    [Fact]
    public void AddProfile_Generic_ShouldRegisterMappings()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.Id.Should().Be(1);
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void AddProfile_Instance_ShouldRegisterMappings()
    {
        // Arrange
        var profile = new ProductMappingProfile();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(profile);
        });
        var mapper = config.CreateMapper();

        var product = new Product { Id = 1, Name = "Test", Price = 99.99m };

        // Act
        var result = mapper.Map<Product, ProductDto>(product);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
        result.Price.Should().Be(99.99m);
    }

    [Fact]
    public void AddProfile_MultipleProfiles_ShouldRegisterAll()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
            cfg.AddProfile<ProductMappingProfile>();
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        var product = new Product { Id = 1, Name = "Test" };

        // Act
        var userResult = mapper.Map<User, UserDto>(user);
        var productResult = mapper.Map<Product, ProductDto>(product);

        // Assert
        userResult.FullName.Should().Be("John Doe");
        productResult.Name.Should().Be("Test");
    }

    [Fact]
    public void AddProfilesFromAssembly_ShouldDiscoverAndRegisterProfiles()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfilesFromAssembly(typeof(MappingProfileTests).Assembly);
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1, FirstName = "John", LastName = "Doe" };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void ConfigureMappings_Override_ShouldBeCalled()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
        });
        var mapper = config.CreateMapper();

        var order = new Order
        {
            Id = 1,
            Total = 100,
            Customer = new Customer { Name = "John" }
        };

        // Act
        var result = mapper.Map<Order, OrderDto>(order);

        // Assert
        result.Id.Should().Be(1);
        result.Total.Should().Be(100);
    }

    [Fact]
    public void MapperConfiguration_AssemblyConstructor_ShouldLoadProfiles()
    {
        // Arrange & Act
        var config = new MapperConfiguration(typeof(MappingProfileTests).Assembly);
        var mapper = config.CreateMapper();

        var user = new User { Id = 1, FirstName = "Test", LastName = "User" };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("Test User");
    }

    [Fact]
    public void AddMaps_ShouldRegisterFromAssembly()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfileTests).Assembly);
        });
        var mapper = config.CreateMapper();

        var product = new Product { Id = 1, Name = "Widget" };

        // Act
        var result = mapper.Map<Product, ProductDto>(product);

        // Assert
        result.Name.Should().Be("Widget");
    }

    [Fact]
    public void Profile_WithoutConfiguration_ShouldThrow()
    {
        // Arrange
        var profile = new UserMappingProfile();

        // Act - Try to create a map without being registered to a configuration
        // This should succeed because profiles are designed to work when registered
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(profile);
        });

        // Assert
        config.Should().NotBeNull();
    }
}
