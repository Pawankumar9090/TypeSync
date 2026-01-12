using FluentAssertions;
using System.Linq;
using Xunit;

namespace TypeSync.Tests;

public class ProjectToTests
{
    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Nested SourceNested { get; set; }
    }

    public class SourceWithCollection : Source
    {
        public List<Nested> NestedCollection { get; set; }
    }

    public class Nested
    {
        public string Description { get; set; }
    }

    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SourceNestedDescription { get; set; } // Flattening
        public string CustomName { get; set; }
        public string IgnoredProp { get; set; }
    }

    public class DestinationWithCollection : Destination
    {
        public List<NestedDto> NestedCollection { get; set; }
    }

    [Fact]
    public void ProjectTo_ShouldMapPropertiesConventionally()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        var sourceList = new List<Source>
        {
            new Source { Id = 1, Name = "Test" }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<Destination>(config).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Test");
    }

    [Fact]
    public void ProjectTo_ShouldFlattenProperties()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        var sourceList = new List<Source>
        {
            new Source { 
                Id = 1, 
                SourceNested = new Nested { Description = "Flattened" } 
            }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<Destination>(config).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].SourceNestedDescription.Should().Be("Flattened");
    }

    [Fact]
    public void ProjectTo_ShouldUseMapFromExpression()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(d => d.CustomName, o => o.MapFrom(s => s.Name + "Custom"));
        });

        var sourceList = new List<Source>
        {
            new Source { Id = 1, Name = "Test" }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<Destination>(config).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].CustomName.Should().Be("TestCustom");
    }

    [Fact]
    public void ProjectTo_ShouldRespectIgnore()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>()
                .ForMember(d => d.IgnoredProp, o => o.Ignore());
        });

        var sourceList = new List<Source>
        {
            new Source { Id = 1, Name = "Test" }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<Destination>(config).ToList();

        // Assert
        result[0].IgnoredProp.Should().BeNull();
    }
    [Fact]
    public void ProjectTo_ShouldMapCollections()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithCollection, DestinationWithCollection>();
            cfg.CreateMap<Nested, NestedDto>();
        });

        var sourceList = new List<SourceWithCollection>
        {
            new SourceWithCollection
            { 
                Id = 1, 
                Name = "Test",
                NestedCollection = new List<Nested> 
                { 
                    new Nested { Description = "Item1" },
                    new Nested { Description = "Item2" }
                }
            }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<DestinationWithCollection>(config).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].NestedCollection.Should().HaveCount(2);
        result[0].NestedCollection[0].Description.Should().Be("Item1");
    }

    [Fact]
    public void ProjectTo_ShouldMapICollectionToList()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithInterface, DestinationWithCollection>();
            cfg.CreateMap<Nested, NestedDto>();
        });

        // Simulating the user's class where property is ICollection<T> but runtime instance is HashSet<T>
        var sourceList = new List<SourceWithInterface>
        {
            new SourceWithInterface
            { 
                Id = 1, 
                NestedCollection = new HashSet<Nested> { new Nested { Description = "Interface" } }
            }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<DestinationWithCollection>(config).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].NestedCollection.Should().HaveCount(1);
        result[0].NestedCollection[0].Description.Should().Be("Interface");
    }

    [Fact]
    public void ProjectTo_ShouldMapHashSetToList()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithHashSet, DestinationWithCollection>();
            cfg.CreateMap<Nested, NestedDto>();
        });

        var sourceList = new List<SourceWithHashSet>
        {
            new SourceWithHashSet
            { 
                Id = 1, 
                NestedCollection = new HashSet<Nested> 
                { 
                    new Nested { Description = "Item1" },
                    new Nested { Description = "Item2" }
                }
            }
        }.AsQueryable();

        // Act
        var result = sourceList.ProjectTo<DestinationWithCollection>(config).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].NestedCollection.Should().HaveCount(2);
        result[0].NestedCollection.Should().BeOfType<List<NestedDto>>();
        result[0].NestedCollection[0].Description.Should().Be("Item1");
    }

    [Fact]
    public void ProjectTo_ShouldThrowForMissingCollectionMap()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithHashSet, DestinationWithCollection>();
            // Missing inner map
        });

        var sourceList = new List<SourceWithHashSet>
        {
            new SourceWithHashSet
            { 
                Id = 1, 
                NestedCollection = new HashSet<Nested> { new Nested() }
            }
        }.AsQueryable();

        // Act
        Action act = () => sourceList.ProjectTo<DestinationWithCollection>(config).ToList();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Missing map configuration*");
    }

    public class SourceWithHashSet : Source
    {
        public HashSet<Nested> NestedCollection { get; set; }
    }

    public class SourceWithInterface : Source
    {
        public ICollection<Nested> NestedCollection { get; set; }
    }

    [Fact]
    public void ProjectTo_ShouldWorkWithSelectAndDistinct()
    {
        // Arrange
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<SourceWithHashSet, DestinationWithCollection>();
            cfg.CreateMap<Nested, NestedDto>();
        });

        // Act
        var result = new List<SourceWithHashSet>().AsQueryable()
            .Select(x => x).Distinct()
            .ProjectTo<DestinationWithCollection>(config).ToList();

        // Assert
        result.Should().NotBeNull();
    }

    public class NestedDto
    {
        public string Description { get; set; }
    }
}
