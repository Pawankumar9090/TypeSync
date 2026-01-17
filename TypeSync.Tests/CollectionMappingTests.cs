using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for collection mapping (lists, arrays).
/// </summary>
public class CollectionMappingTests
{
    [Fact]
    public void Map_ListToList_ShouldMapAllItems()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
        });
        var mapper = config.CreateMapper();

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.00m },
            new() { Id = 2, Name = "Product 2", Price = 20.00m },
            new() { Id = 3, Name = "Product 3", Price = 30.00m }
        };

        // Act
        var result = mapper.Map<List<Product>, List<ProductDto>>(products);

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Product 1");
        result[0].Price.Should().Be(10.00m);
        result[2].Id.Should().Be(3);
    }

    [Fact]
    public void Map_ArrayToArray_ShouldMapAllItems()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<Warehouse, WarehouseDto>();
        });
        var mapper = config.CreateMapper();

        var warehouse = new Warehouse
        {
            Name = "Main Warehouse",
            Inventory = new Product[]
            {
                new() { Id = 1, Name = "Item 1", Price = 5.00m },
                new() { Id = 2, Name = "Item 2", Price = 15.00m }
            }
        };

        // Act
        var result = mapper.Map<Warehouse, WarehouseDto>(warehouse);

        // Assert
        result.Name.Should().Be("Main Warehouse");
        result.Inventory.Should().HaveCount(2);
        result.Inventory[0].Id.Should().Be(1);
        result.Inventory[1].Name.Should().Be("Item 2");
    }

    [Fact]
    public void Map_ListToArray_ShouldConvert()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
        });
        var mapper = config.CreateMapper();

        var products = new List<Product>
        {
            new() { Id = 1, Name = "Product 1" },
            new() { Id = 2, Name = "Product 2" }
        };

        // Act
        var result = mapper.Map<List<Product>, ProductDto[]>(products);

        // Assert
        result.Should().BeOfType<ProductDto[]>();
        result.Should().HaveCount(2);
    }

    [Fact]
    public void Map_NestedCollection_ShouldMapRecursively()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<Category, CategoryDto>();
        });
        var mapper = config.CreateMapper();

        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Products = new List<Product>
            {
                new() { Id = 1, Name = "Phone", Price = 999.00m },
                new() { Id = 2, Name = "Laptop", Price = 1499.00m }
            }
        };

        // Act
        var result = mapper.Map<Category, CategoryDto>(category);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("Electronics");
        result.Products.Should().HaveCount(2);
        result.Products[0].Name.Should().Be("Phone");
        result.Products[1].Price.Should().Be(1499.00m);
    }

    [Fact]
    public void Map_EmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
        });
        var mapper = config.CreateMapper();

        var products = new List<Product>();

        // Act
        var result = mapper.Map<List<Product>, List<ProductDto>>(products);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Map_NullCollection_ShouldReturnNull()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
        });
        var mapper = config.CreateMapper();

        List<Product>? products = null;

        // Act
        var result = mapper.Map<List<Product>, List<ProductDto>>(products!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Map_CollectionWithNullItems_ShouldPreserveNulls()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
        });
        var mapper = config.CreateMapper();

        var products = new List<Product?>
        {
            new() { Id = 1, Name = "Product 1" },
            null,
            new() { Id = 3, Name = "Product 3" }
        };

        // Act
        var result = mapper.Map<List<Product?>, List<ProductDto?>>(products);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().NotBeNull();
        result[1].Should().BeNull();
        result[2].Should().NotBeNull();
    }

    [Fact]
    public void Map_ListOfCategories_ShouldMapNestedCollections()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Product, ProductDto>();
            cfg.CreateMap<Category, CategoryDto>();
        });
        var mapper = config.CreateMapper();

        var categories = new List<Category>
        {
            new()
            {
                Id = 1,
                Name = "Electronics",
                Products = new List<Product>
                {
                    new() { Id = 1, Name = "Phone" }
                }
            },
            new()
            {
                Id = 2,
                Name = "Books",
                Products = new List<Product>
                {
                    new() { Id = 2, Name = "Novel" },
                    new() { Id = 3, Name = "Textbook" }
                }
            }
        };

        // Act
        var result = mapper.Map<List<Category>, List<CategoryDto>>(categories);

        // Assert
        result.Should().HaveCount(2);
        result[0].Products.Should().HaveCount(1);
        result[1].Products.Should().HaveCount(2);
    }

    [Fact]
    public void Map_ForMemberWithSelectExpression_ShouldMapCollectionElements()
    {
        // Arrange - simulates join table pattern like ServicePests -> Pest
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Pest, PestDto>();
            cfg.CreateMap<ServiceEntity, ServiceDto>()
                .ForMember(dest => dest.Pests, opt => opt.MapFrom(src => src.ServicePests.Select(sp => sp.Pest)));
        });
        var mapper = config.CreateMapper();

        var service = new ServiceEntity
        {
            Id = 1,
            Name = "Pest Control",
            ServicePests = new List<ServicePest>
            {
                new() { PestId = 1, Pest = new Pest { Id = 1, Name = "Ant" } },
                new() { PestId = 2, Pest = new Pest { Id = 2, Name = "Roach" } },
                new() { PestId = 3, Pest = new Pest { Id = 3, Name = "Spider" } }
            }
        };

        // Act
        var result = mapper.Map<ServiceEntity, ServiceDto>(service);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("Pest Control");
        result.Pests.Should().HaveCount(3);
        result.Pests.First().Id.Should().Be(1);
        result.Pests.First().Name.Should().Be("Ant");
        result.Pests.Last().Name.Should().Be("Spider");
    }
}

// Test models for join table pattern
public class ServiceEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ServicePest> ServicePests { get; set; } = new List<ServicePest>();
}

public class ServicePest
{
    public int ServiceId { get; set; }
    public int PestId { get; set; }
    public Pest Pest { get; set; } = null!;
}

public class Pest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ServiceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<PestDto> Pests { get; set; } = Enumerable.Empty<PestDto>();
}

public class PestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

