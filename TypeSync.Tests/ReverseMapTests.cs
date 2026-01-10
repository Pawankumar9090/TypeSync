using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for ReverseMap functionality.
/// </summary>
public class ReverseMapTests
{
    [Fact]
    public void ReverseMap_BasicMapping_ShouldCreateBidirectionalMapping()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ReverseMap();
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Forward mapping
        var dto = mapper.Map<User, UserDto>(user);

        // Then reverse mapping
        var reversedUser = mapper.Map<UserDto, User>(dto);

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("John");

        reversedUser.Id.Should().Be(1);
        reversedUser.FirstName.Should().Be("John");
        reversedUser.LastName.Should().Be("Doe");
        reversedUser.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void ReverseMap_WithCollections_ShouldWorkBidirectionally()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>()
                .ReverseMap();
        });
        var mapper = config.CreateMapper();

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.00m },
            new() { Id = 2, Name = "Product 2", Price = 20.00m }
        };

        // Act - Forward
        var dtos = mapper.Map<List<Product>, List<ProductDto>>(products);

        // Reverse
        var reversed = mapper.Map<List<ProductDto>, List<Product>>(dtos);

        // Assert
        dtos.Should().HaveCount(2);
        reversed.Should().HaveCount(2);
        reversed[0].Name.Should().Be("Product 1");
        reversed[1].Price.Should().Be(20.00m);
    }

    [Fact]
    public void ReverseMap_FluentChaining_ShouldReturnNewExpression()
    {
        // Arrange
        var reverseConfigured = false;
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>()
                .ReverseMap()
                .ForMember(dest => dest.Price, opt =>
                {
                    reverseConfigured = true;
                    opt.MapFrom((Func<ProductDto, decimal>)(src => src.Price * 1.1m)); // Apply 10% markup on reverse
                });
        });
        var mapper = config.CreateMapper();

        var dto = new ProductDto { Id = 1, Name = "Product", Price = 100.00m };

        // Act
        var product = mapper.Map<ProductDto, Product>(dto);

        // Assert
        reverseConfigured.Should().BeTrue();
        product.Price.Should().Be(110.00m);
    }

    [Fact]
    public void ReverseMap_WithCustomMappings_ShouldNotAffectReverse()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom((Func<User, string>)(src => $"{src.FirstName} {src.LastName}")))
                .ReverseMap();
        });
        var mapper = config.CreateMapper();

        var dto = new UserDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            FullName = "John Doe"
        };

        // Act
        var user = mapper.Map<UserDto, User>(dto);

        // Assert
        user.Id.Should().Be(1);
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        // FullName doesn't exist in User, so it's ignored
    }
}
