using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for basic object-to-object mapping functionality.
/// </summary>
public class BasicMappingTests
{
    [Fact]
    public void Map_SimpleProperties_ShouldMapMatchingProperties()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Age = 30,
            IsActive = true
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john@example.com");
        result.Age.Should().Be(30);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Map_NullSource_ShouldReturnDefault()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        var mapper = config.CreateMapper();

        // Act
        var result = mapper.Map<User, UserDto>(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Map_ToExistingDestination_ShouldUpdateProperties()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var existingDto = new UserDto
        {
            Id = 999,
            FirstName = "Old",
            MappedAt = DateTime.Now.AddDays(-1) // This should not change since User doesn't have it
        };

        // Act
        mapper.Map(user, existingDto);

        // Assert
        existingDto.Id.Should().Be(1);
        existingDto.FirstName.Should().Be("John");
        existingDto.LastName.Should().Be("Doe");
        existingDto.Email.Should().Be("john@example.com");
    }

    [Fact]
    public void Map_WithRuntimeTypeDiscovery_ShouldMapCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        var mapper = config.CreateMapper();

        object user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = mapper.Map<UserDto>(user);

        // Assert
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
    }

    [Fact]
    public void Map_WithTypeParameters_ShouldMapCorrectly()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>();
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1, FirstName = "John" };

        // Act
        var result = mapper.Map(user, typeof(User), typeof(UserDto));

        // Assert
        result.Should().BeOfType<UserDto>();
        ((UserDto)result).Id.Should().Be(1);
        ((UserDto)result).FirstName.Should().Be("John");
    }

    [Fact]
    public void Map_TypeConversion_IntToString_ShouldConvert()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithNumbers, DestWithStrings>();
        });
        var mapper = config.CreateMapper();

        var source = new SourceWithNumbers
        {
            IntValue = 42,
            DoubleValue = 3.14,
            StringNumber = "100"
        };

        // Act
        var result = mapper.Map<SourceWithNumbers, DestWithStrings>(source);

        // Assert
        result.IntValue.Should().Be("42");
        result.DoubleValue.Should().Be("3.14");
        result.StringNumber.Should().Be(100);
    }

    [Fact]
    public void Map_StringToEnum_ShouldConvert()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithEnum, DestWithEnum>();
        });
        var mapper = config.CreateMapper();

        var source = new SourceWithEnum { Status = "Pending" };

        // Act
        var result = mapper.Map<SourceWithEnum, DestWithEnum>(source);

        // Assert
        result.Status.Should().Be(Status.Pending);
    }

    [Fact]
    public void Map_NestedComplexObject_ShouldMapRecursively()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeDto>();
            cfg.CreateMap<Department, DepartmentDto>();
        });
        var mapper = config.CreateMapper();

        var dept = new Department
        {
            Id = 1,
            Name = "Engineering",
            Manager = new Employee
            {
                Id = 10,
                Name = "Jane Smith",
                Email = "jane@example.com"
            }
        };

        // Act
        var result = mapper.Map<Department, DepartmentDto>(dept);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("Engineering");
        result.Manager.Should().NotBeNull();
        result.Manager.Id.Should().Be(10);
        result.Manager.Name.Should().Be("Jane Smith");
        result.Manager.Email.Should().Be("jane@example.com");
    }
}
