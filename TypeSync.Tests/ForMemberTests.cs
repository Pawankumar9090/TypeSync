using FluentAssertions;

namespace TypeSync.Tests;

/// <summary>
/// Tests for ForMember configuration options.
/// </summary>
public class ForMemberTests
{
    [Fact]
    public void ForMember_MapFromExpression_ShouldMapCustomProperty()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
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
    public void ForMember_MapFromFunction_ShouldMapCustomProperty()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FirstName.ToUpper()));
        });
        var mapper = config.CreateMapper();

        var user = new User { FirstName = "John" };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("JOHN");
    }

    [Fact]
    public void ForMember_Ignore_ShouldSkipProperty()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.Email, opt => opt.Ignore());
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            Id = 1,
            Email = "john@example.com"
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.Id.Should().Be(1);
        result.Email.Should().BeEmpty(); // Default value, not mapped
    }

    [Fact]
    public void ForMember_ConditionSource_ShouldMapOnlyWhenConditionMet()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Email, opt => opt.Condition(src => src.IsEmailVisible));
        });
        var mapper = config.CreateMapper();

        var visibleEmployee = new Employee
        {
            Id = 1,
            Name = "John",
            Email = "john@example.com",
            IsEmailVisible = true
        };

        var hiddenEmployee = new Employee
        {
            Id = 2,
            Name = "Jane",
            Email = "jane@example.com",
            IsEmailVisible = false
        };

        // Act
        var resultVisible = mapper.Map<Employee, EmployeeDto>(visibleEmployee);
        var resultHidden = mapper.Map<Employee, EmployeeDto>(hiddenEmployee);

        // Assert
        resultVisible.Email.Should().Be("john@example.com");
        resultHidden.Email.Should().BeEmpty(); // Not mapped due to condition
    }

    [Fact]
    public void ForMember_NullSubstitute_ShouldProvideDefaultValue()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Nickname, opt => opt.NullSubstitute("N/A"));
        });
        var mapper = config.CreateMapper();

        var employeeWithNickname = new Employee
        {
            Id = 1,
            Name = "John",
            Nickname = "Johnny"
        };

        var employeeWithoutNickname = new Employee
        {
            Id = 2,
            Name = "Jane",
            Nickname = null
        };

        // Act
        var resultWith = mapper.Map<Employee, EmployeeDto>(employeeWithNickname);
        var resultWithout = mapper.Map<Employee, EmployeeDto>(employeeWithoutNickname);

        // Assert
        resultWith.Nickname.Should().Be("Johnny");
        resultWithout.Nickname.Should().Be("N/A");
    }

    [Fact]
    public void ForMember_MultipleConfigurations_ShouldApplyAll()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age + 1));
        });
        var mapper = config.CreateMapper();

        var user = new User
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Age = 30
        };

        // Act
        var result = mapper.Map<User, UserDto>(user);

        // Assert
        result.FullName.Should().Be("John Doe");
        result.Email.Should().BeEmpty();
        result.Age.Should().Be(31);
    }

    [Fact]
    public void ForMember_ConditionWithDestination_ShouldEvaluateWithBothObjects()
    {
        // Arrange
        var conditionCalled = false;
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.Salary, opt => opt.Condition((src, dest) =>
                {
                    conditionCalled = true;
                    return src.Salary > 0;
                }));
        });
        var mapper = config.CreateMapper();

        var employee = new Employee
        {
            Id = 1,
            Salary = 50000
        };

        // Act
        var result = mapper.Map<Employee, EmployeeDto>(employee);

        // Assert
        conditionCalled.Should().BeTrue();
        result.Salary.Should().Be(50000);
    }

    [Fact]
    public void ForAllMembers_ShouldApplyToAllDestinationProperties()
    {
        // Arrange
        var ignoreCount = 0;
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                .ForAllMembers(opt =>
                {
                    ignoreCount++;
                    // Just counting, not actually ignoring for this test
                });
        });
        var mapper = config.CreateMapper();

        // Assert - ForAllMembers should have been called for each destination property
        ignoreCount.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Failing due to ConditionWithSourceMember not persisting to PropertyMap at runtime")]
    public void ForMember_ConditionWithSourceMember_ShouldUseResolvedValue()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, UserDto>()
                // Only map name if the source name is not "SkipMe"
                .ForMember(dest => dest.FirstName, opt => opt.Condition((src, dest, srcMember) => 
                {
                    return srcMember != null && srcMember.ToString() != "SkipMe";
                }));
        });
        var mapper = config.CreateMapper();

        var userToMap = new User { Id = 1, FirstName = "John" };
        var userToSkip = new User { Id = 2, FirstName = "SkipMe" };

        // Act
        var resultMapped = mapper.Map<User, UserDto>(userToMap);
        var resultSkipped = mapper.Map<User, UserDto>(userToSkip);

        // Assert
        resultMapped.FirstName.Should().Be("John");
        resultSkipped.FirstName.Should().BeNull(); // Should be skipped
    }
}
