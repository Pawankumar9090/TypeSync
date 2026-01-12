using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TypeSync.DependencyInjection;

namespace TypeSync.Tests;

/// <summary>
/// Tests for Dependency Injection integration.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddTypeSync_WithConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTypeSync(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetService<IMapper>();
        var configuration = provider.GetService<MapperConfiguration>();

        // Assert
        mapper.Should().NotBeNull();
        configuration.Should().NotBeNull();
    }

    [Fact]
    public void AddTypeSync_WithConfiguration_MapperShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTypeSync(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void AddTypeSync_WithAssemblies_ShouldDiscoverProfiles()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTypeSync(typeof(DependencyInjectionTests).Assembly);

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetService<IMapper>();

        // Assert
        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddTypeSync_WithProfile_ShouldRegisterProfileMappings()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTypeSync<UserMappingProfile>();

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var user = new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("Test User");
    }

    [Fact]
    public void AddTypeSync_Singleton_ShouldReturnSameInstance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTypeSync(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();

        // Act
        var mapper1 = provider.GetRequiredService<IMapper>();
        var mapper2 = provider.GetRequiredService<IMapper>();

        // Assert
        mapper1.Should().BeSameAs(mapper2);
    }

    [Fact]
    public void AddTypeSync_Configuration_ShouldBeSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTypeSync(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        var provider = services.BuildServiceProvider();

        // Act
        var config1 = provider.GetRequiredService<MapperConfiguration>();
        var config2 = provider.GetRequiredService<MapperConfiguration>();

        // Assert
        config1.Should().BeSameAs(config2);
    }

    [Fact]
    public void AddTypeSync_WithMultipleProfiles_ShouldRegisterAll()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTypeSync(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
            cfg.AddProfile<ProductMappingProfile>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IMapper>();

        var user = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        var product = new Product { Id = 1, Name = "Widget", Price = 9.99m };

        // Act
        var userResult = mapper.Map<User, UserDto>(user);
        var productResult = mapper.Map<Product, ProductDto>(product);

        // Assert
        userResult.FullName.Should().Be("John Doe");
        productResult.Name.Should().Be("Widget");
    }

    [Fact]
    public void AddTypeSync_Chainable_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTypeSync(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void IMapper_InjectedInService_ShouldWork()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTypeSync(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        services.AddTransient<TestUserService>();

        var provider = services.BuildServiceProvider();
        var userService = provider.GetRequiredService<TestUserService>();

        // Act
        var result = userService.GetUserDto(new User { Id = 1, FirstName = "John" });

        // Assert
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
    }
}

// Helper service for testing DI
public class TestUserService
{
    private readonly IMapper _mapper;

    public TestUserService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public UserDto GetUserDto(User user) => _mapper.Map<User, UserDto>(user);
}
