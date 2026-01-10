using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for runtime MapOptions.
/// </summary>
public class MapOptionsTests
{
    [Fact]
    public void MapOptions_IgnoreProperty_ShouldSkipAtRuntime()
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

        var options = new MapOptions("Email");

        // Act
        var result = mapper.Map<User, UserDto>(user, options);

        // Assert
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.Email.Should().BeEmpty(); // Ignored at runtime
    }

    [Fact]
    public void MapOptions_IgnoreMultipleProperties_ShouldSkipAll()
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
            Age = 30
        };

        var options = new MapOptions("Email", "Age", "LastName");

        // Act
        var result = mapper.Map<User, UserDto>(user, options);

        // Assert
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.LastName.Should().BeEmpty();
        result.Email.Should().BeEmpty();
        result.Age.Should().Be(0);
    }

    [Fact]
    public void MapOptions_FluentIgnore_ShouldAddProperties()
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
            Email = "john@example.com"
        };

        var options = new MapOptions()
            .Ignore("Email")
            .Ignore("Age");

        // Act
        var result = mapper.Map<User, UserDto>(user, options);

        // Assert
        result.Email.Should().BeEmpty();
        result.Age.Should().Be(0);
    }

    [Fact]
    public void MapOptionsBuilder_IgnoreProperties_ShouldCreateConfiguredOptions()
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
            Email = "john@example.com"
        };

        var options = MapOptionsBuilder.IgnoreProperties("Email", "Age");

        // Act
        var result = mapper.Map<User, UserDto>(user, options);

        // Assert
        result.Email.Should().BeEmpty();
    }

    [Fact]
    public void MapOptions_CaseInsensitive_ShouldIgnoreRegardlessOfCase()
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
            Email = "john@example.com"
        };

        var options = new MapOptions("EMAIL"); // Different case

        // Act
        var result = mapper.Map<User, UserDto>(user, options);

        // Assert
        result.Email.Should().BeEmpty();
    }

    [Fact]
    public void MapOptions_WithExistingDestination_ShouldRespectIgnores()
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
            Email = "new@example.com"
        };

        var existingDto = new UserDto
        {
            Id = 999,
            Email = "existing@example.com"
        };

        var options = new MapOptions("Email");

        // Act
        mapper.Map(user, existingDto, options);

        // Assert
        existingDto.Id.Should().Be(1);
        existingDto.FirstName.Should().Be("John");
        existingDto.Email.Should().Be("existing@example.com"); // Not overwritten
    }

    [Fact]
    public void MapOptions_WithRuntimeTypes_ShouldWork()
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
            Email = "john@example.com"
        };

        var options = new MapOptions("Email");

        // Act
        var result = mapper.Map(user, typeof(User), typeof(UserDto), options);

        // Assert
        ((UserDto)result).Email.Should().BeEmpty();
    }

    [Fact]
    public void MapOptions_EmptyOptions_ShouldMapNormally()
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
            Email = "john@example.com"
        };

        var options = new MapOptions();

        // Act
        var result = mapper.Map<User, UserDto>(user, options);

        // Assert
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
        result.Email.Should().Be("john@example.com");
    }
}
