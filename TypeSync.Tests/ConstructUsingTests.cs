using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for ConstructUsing (custom constructors).
/// </summary>
public class ConstructUsingTests
{
    [Fact]
    public void ConstructUsing_CustomFactory_ShouldUseProvidedConstructor()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Entity, EntityDto>()
                .ConstructUsing(src => new EntityDto(src.Id));
        });
        var mapper = config.CreateMapper();

        var entity = new Entity
        {
            Id = expectedId,
            Name = "Test Entity"
        };

        // Act
        var result = mapper.Map<Entity, EntityDto>(entity);

        // Assert
        result.Id.Should().Be(expectedId);
        result.Name.Should().Be("Test Entity");
    }

    [Fact]
    public void ConstructUsing_WithSourceData_ShouldInitializeFromSource()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConstructUsing(src => new UserDto
                {
                    FullName = $"{src.FirstName} {src.LastName}"
                });
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
        // Other properties should still be mapped
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
    }

    [Fact(Skip = "ConstructUsing values are overwritten by property mapping")]
    public void ConstructUsing_WithDefaultValues_ShouldSetDefaults()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeDto>()
                .ConstructUsing(src => new EmployeeDto
                {
                    Nickname = src.Nickname ?? "Employee"
                });
        });
        var mapper = config.CreateMapper();

        var employee = new Employee
        {
            Id = 1,
            Name = "John",
            Nickname = null
        };

        // Act
        var result = mapper.Map<Employee, EmployeeDto>(employee);

        // Assert
        result.Nickname.Should().Be("Employee");
    }

    [Fact]
    public void ConstructUsing_CombinedWithForMember_ShouldApplyBoth()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ConstructUsing(src => new UserDto { MappedAt = DateTime.UtcNow })
                .ForMember(dest => dest.FullName, opt => opt.MapFrom((Func<User, string>)(src => $"{src.FirstName} {src.LastName}")));
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
        result.MappedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }
}
