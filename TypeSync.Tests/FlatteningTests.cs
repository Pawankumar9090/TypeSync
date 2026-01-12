using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for property flattening (nested object mapping to flat properties).
/// </summary>
public class FlatteningTests
{
    [Fact]
    public void Flatten_SingleLevel_ShouldMapNestedProperties()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.CreateMapper();

        var order = new Order
        {
            Id = 1,
            Total = 99.99m,
            OrderDate = new DateTime(2024, 1, 15),
            Customer = new Customer
            {
                Id = 10,
                Name = "John Doe",
                Email = "john@example.com"
            }
        };

        // Act
        var result = mapper.Map<Order, OrderDto>(order);

        // Assert
        result.Id.Should().Be(1);
        result.Total.Should().Be(99.99m);
        result.CustomerName.Should().Be("John Doe");
        result.CustomerEmail.Should().Be("john@example.com");
    }

    [Fact]
    public void Flatten_MultiLevel_ShouldMapDeeplyNestedProperties()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.CreateMapper();

        var order = new Order
        {
            Id = 1,
            Customer = new Customer
            {
                Name = "John Doe",
                Address = new Address
                {
                    Street = "123 Main St",
                    City = "New York",
                    Country = "USA"
                }
            }
        };

        // Act
        var result = mapper.Map<Order, OrderDto>(order);

        // Assert
        result.CustomerAddressCity.Should().Be("New York");
    }

    [Fact]
    public void Flatten_NullNestedObject_ShouldHandleGracefully()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.CreateMapper();

        var order = new Order
        {
            Id = 1,
            Total = 50.00m,
            Customer = null!
        };

        // Act
        var result = mapper.Map<Order, OrderDto>(order);

        // Assert
        result.Id.Should().Be(1);
        result.Total.Should().Be(50.00m);
        result.CustomerName.Should().BeNull(); // Null when nested object is null
    }

    [Fact]
    public void Flatten_PartiallyNullPath_ShouldHandleGracefully()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>();
        });
        var mapper = config.CreateMapper();

        var order = new Order
        {
            Id = 1,
            Customer = new Customer
            {
                Name = "John Doe",
                Address = null!
            }
        };

        // Act
        var result = mapper.Map<Order, OrderDto>(order);

        // Assert
        result.CustomerName.Should().Be("John Doe");
        result.CustomerAddressCity.Should().BeNull(); // Null when nested path is null
    }

    [Fact]
    public void Flatten_CombinedWithForMember_ShouldWorkTogether()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name.ToUpper()));
        });
        var mapper = config.CreateMapper();

        var order = new Order
        {
            Id = 1,
            Customer = new Customer
            {
                Name = "john doe"
            }
        };

        // Act
        var result = mapper.Map<Order, OrderDto>(order);

        // Assert
        result.CustomerName.Should().Be("JOHN DOE");
    }
}
