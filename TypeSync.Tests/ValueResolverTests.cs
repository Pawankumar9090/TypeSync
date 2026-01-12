using FluentAssertions;

namespace TypeSync.Tests;

#region Value Resolvers

public class TotalWithTaxResolver : IValueResolver<Invoice, InvoiceDto, decimal>
{
    public decimal Resolve(Invoice source, InvoiceDto destination, decimal destMember)
    {
        return source.Subtotal * (1 + source.TaxRate);
    }
}

public class FullNameResolver : IValueResolver<User, UserDto, string>
{
    public string Resolve(User source, UserDto destination, string destMember)
    {
        return $"{source.FirstName} {source.LastName}";
    }
}

public class UpperCaseNameResolver : IValueResolver<Product, ProductDto, string>
{
    public string Resolve(Product source, ProductDto destination, string destMember)
    {
        return source.Name.ToUpperInvariant();
    }
}

#endregion

/// <summary>
/// Tests for custom IValueResolver implementations.
/// </summary>
public class ValueResolverTests
{
    [Fact]
    public void MapFrom_ValueResolver_ShouldUseCustomResolver()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.TotalWithTax, opt => opt.MapFrom<TotalWithTaxResolver>());
        });
        var mapper = config.CreateMapper();

        var invoice = new Invoice
        {
            Id = 1,
            Subtotal = 100.00m,
            TaxRate = 0.10m // 10%
        };

        // Act
        var result = mapper.Map<Invoice, InvoiceDto>(invoice);

        // Assert
        result.Id.Should().Be(1);
        result.Subtotal.Should().Be(100.00m);
        result.TaxRate.Should().Be(0.10m);
        result.TotalWithTax.Should().Be(110.00m);
    }

    [Fact]
    public void MapFrom_FullNameResolver_ShouldCombineNames()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom<FullNameResolver>());
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
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void MapFrom_UpperCaseResolver_ShouldTransformValue()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom<UpperCaseNameResolver>());
        });
        var mapper = config.CreateMapper();

        var product = new Product
        {
            Id = 1,
            Name = "Widget",
            Price = 29.99m
        };

        // Act
        var result = mapper.Map<Product, ProductDto>(product);

        // Assert
        result.Name.Should().Be("WIDGET");
        result.Price.Should().Be(29.99m);
    }

    [Fact]
    public void MapFrom_MultipleResolvers_ShouldWorkTogether()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom<FullNameResolver>())
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToLower()));
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "JOHN@EXAMPLE.COM"
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void ValueResolver_WithCalculation_ShouldComputeCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Invoice, InvoiceDto>()
                .ForMember(dest => dest.TotalWithTax, opt => opt.MapFrom<TotalWithTaxResolver>());
        });
        var mapper = config.CreateMapper();

        var invoices = new List<Invoice>
        {
            new() { Id = 1, Subtotal = 100, TaxRate = 0.10m },
            new() { Id = 2, Subtotal = 200, TaxRate = 0.15m },
            new() { Id = 3, Subtotal = 50, TaxRate = 0.05m }
        };

        // Act
        var results = mapper.Map<List<Invoice>, List<InvoiceDto>>(invoices);

        // Assert
        results[0].TotalWithTax.Should().Be(110.00m);
        results[1].TotalWithTax.Should().Be(230.00m);
        results[2].TotalWithTax.Should().Be(52.50m);
    }
}
