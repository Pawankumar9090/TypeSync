using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for BeforeMap and AfterMap actions.
/// </summary>
public class MapActionsTests
{
    [Fact]
    public void BeforeMap_ShouldExecuteBeforeMapping()
    {
        // Arrange
        var beforeMapExecuted = false;
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .BeforeMap((src, dest) =>
                {
                    beforeMapExecuted = true;
                });
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1, FirstName = "John" };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        beforeMapExecuted.Should().BeTrue();
        result.Id.Should().Be(1);
    }

    [Fact]
    public void AfterMap_ShouldExecuteAfterMapping()
    {
        // Arrange
        var afterMapExecuted = false;
        DateTime mappedAt = default;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .AfterMap((src, dest) =>
                {
                    afterMapExecuted = true;
                    dest.MappedAt = DateTime.UtcNow;
                    mappedAt = dest.MappedAt;
                });
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1, FirstName = "John" };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        afterMapExecuted.Should().BeTrue();
        result.MappedAt.Should().Be(mappedAt);
        result.MappedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void BeforeAndAfterMap_ShouldExecuteInCorrectOrder()
    {
        // Arrange
        var executionOrder = new List<string>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .BeforeMap((src, dest) =>
                {
                    executionOrder.Add("Before");
                })
                .AfterMap((src, dest) =>
                {
                    executionOrder.Add("After");
                });
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1 };

        // Act
        mapper.Map<User, UserDto>(user);

        // Assert
        executionOrder.Should().HaveCount(2);
        executionOrder[0].Should().Be("Before");
        executionOrder[1].Should().Be("After");
    }

    [Fact]
    public void MultipleBeforeMapActions_ShouldExecuteAll()
    {
        // Arrange
        var actionCount = 0;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .BeforeMap((src, dest) => actionCount++)
                .BeforeMap((src, dest) => actionCount++)
                .BeforeMap((src, dest) => actionCount++);
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1 };

        // Act
        mapper.Map<User, UserDto>(user);

        // Assert
        actionCount.Should().Be(3);
    }

    [Fact]
    public void MultipleAfterMapActions_ShouldExecuteAll()
    {
        // Arrange
        var actionCount = 0;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .AfterMap((src, dest) => actionCount++)
                .AfterMap((src, dest) => actionCount++);
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 1 };

        // Act
        mapper.Map<User, UserDto>(user);

        // Assert
        actionCount.Should().Be(2);
    }

    [Fact]
    public void AfterMap_CanModifyDestination_ShouldReflectChanges()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .AfterMap((src, dest) =>
                {
                    dest.FullName = $"{dest.FirstName} {dest.LastName}";
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
    }

    [Fact]
    public void BeforeMap_CanAccessSourceAndDestination_ShouldHaveCorrectValues()
    {
        // Arrange
        User? capturedSource = null;
        UserDto? capturedDest = null;

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .BeforeMap((src, dest) =>
                {
                    capturedSource = src;
                    capturedDest = dest;
                });
        });
        var mapper = config.CreateMapper();

        var user = new User { Id = 42, FirstName = "John" };

        // Act
        mapper.Map<User, UserDto>(user);

        // Assert
        capturedSource.Should().NotBeNull();
        capturedSource!.Id.Should().Be(42);
        capturedDest.Should().NotBeNull();
    }
}
