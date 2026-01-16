# TypeSync

[![Build Status](https://github.com/Pawankumar9090/TypeSync/actions/workflows/ci.yml/badge.svg)](https://github.com/Pawankumar9090/TypeSync/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/TypeSync.svg)](https://www.nuget.org/packages/TypeSync)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A lightweight, convention-based object-to-object mapping library for .NET 8. Similar to AutoMapper with a fluent API, profile support, and dependency injection integration.

## Features

- ✅ **Convention-based mapping** - Automatically maps properties with matching names
- ✅ **Fluent configuration API** - Configure mappings using `CreateMap`, `ForMember`, `ReverseMap`
- ✅ **Profiles** - Organize mappings into reusable profile classes
- ✅ **Flattening** - Map nested properties (e.g., `Customer.Name` → `CustomerName`)
- ✅ **Collection mapping** - Automatic list, array, and ICollection mapping with element type conversion
- ✅ **ProjectTo** - IQueryable projection for efficient EF Core queries
- ✅ **Custom value resolvers** - Implement `IValueResolver` for complex transformations
- ✅ **Conditional mapping** - Skip properties based on conditions
- ✅ **Null substitution** - Provide default values for null properties
- ✅ **Before/After map actions** - Execute logic before or after mapping
- ✅ **Dependency Injection** - First-class support for `IServiceCollection`

## Installation

```bash
dotnet add package TypeSync
```

## Quick Start

### Basic Usage

```csharp
using TypeSync;

// Define your models
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
}

// Configure mappings
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<User, UserDto>()
        .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
});

// Create mapper and map objects
IMapper mapper = config.CreateMapper();

var user = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
var userDto = mapper.Map<User, UserDto>(user);
// userDto.FullName == "John Doe"
```

### Using Profiles

```csharp
// Create a profile
public class UserProfile : MappingProfile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
        
        CreateMap<Order, OrderDto>()
            .ReverseMap();
    }
}

// Register profiles
var config = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<UserProfile>();
});
```

### Dependency Injection (ASP.NET Core)

```csharp
using TypeSync.DependencyInjection;

// In Program.cs or Startup.cs
builder.Services.AddTypeSync(cfg =>
{
    cfg.CreateMap<User, UserDto>();
    cfg.AddProfile<UserProfile>();
});

// Or auto-discover profiles from an assembly
builder.Services.AddTypeSync(typeof(Program).Assembly);

// Inject IMapper in your services
public class UserService
{
    private readonly IMapper _mapper;
    
    public UserService(IMapper mapper)
    {
        _mapper = mapper;
    }
    
    public UserDto GetUser(User user) => _mapper.Map<User, UserDto>(user);
}
```

### Flattening

```csharp
public class Order
{
    public Customer Customer { get; set; }
    public decimal Total { get; set; }
}

public class Customer
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class OrderDto
{
    public string CustomerName { get; set; }  // Automatically mapped from Order.Customer.Name
    public string CustomerEmail { get; set; } // Automatically mapped from Order.Customer.Email
    public decimal Total { get; set; }
}

var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Order, OrderDto>(); // Flattening is automatic!
});
```

### Collection Property Mapping

TypeSync automatically maps collection properties when the element types have a defined mapping:

```csharp
public class Provider
{
    public string Name { get; set; }
    public ICollection<Address> Areas { get; set; }
}

public class ProviderDto
{
    public string Name { get; set; }
    public ICollection<AddressDto> Areas { get; set; }  // Collection is mapped automatically!
}

var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<Provider, ProviderDto>();
    cfg.CreateMap<Address, AddressDto>().ReverseMap();
});

var provider = new Provider 
{ 
    Name = "Test",
    Areas = new List<Address> { new Address { City = "Delhi" } }
};

var dto = mapper.Map<Provider, ProviderDto>(provider);
// dto.Areas contains the mapped AddressDto items
```

**Supported Collection Types:**
- `IEnumerable<T>`, `ICollection<T>`, `IList<T>`
- `List<T>`, `HashSet<T>`
- `T[]` (arrays)

### ProjectTo (IQueryable Projection)

Use `ProjectTo<T>` for efficient database queries with Entity Framework:

```csharp
using TypeSync.QueryableExtensions;

// In your repository or service
var items = await dbContext.Providers
    .Where(p => p.IsActive)
    .ProjectTo<ProviderDto>(_mapper.ConfigurationProvider)
    .ToListAsync();
```

This generates optimized SQL that only selects the required columns, including nested collections.

### Runtime Property Ignore

Use `MapOptions` to ignore properties at runtime for both `Map` and `ProjectTo`:

```csharp
// For Map
var options = new MapOptions("Password", "SecretKey");
var dto = mapper.Map<User, UserDto>(user, options);

// For ProjectTo
var results = query.ProjectTo<UserDto>(config, new MapOptions("Password"));

// Or via IMapper
var results = mapper.ProjectTo<UserDto>(query, new MapOptions("InternalField"));

// Fluent syntax
var options = new MapOptions().Ignore("Email").Ignore("Phone");
var dto = mapper.Map<User, UserDto>(user, options);
```

### Advanced Features

```csharp
cfg.CreateMap<User, UserDto>()
    // Custom mapping
    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
    
    // Ignore a property
    .ForMember(dest => dest.Password, opt => opt.Ignore())
    
    // Conditional mapping
    .ForMember(dest => dest.Email, opt => opt.Condition(src => src.IsEmailVisible))
    
    // Null substitution
    .ForMember(dest => dest.Nickname, opt => opt.NullSubstitute("N/A"))
    
    // Before/After map actions
    .BeforeMap((src, dest) => Console.WriteLine("Mapping started"))
    .AfterMap((src, dest) => dest.MappedAt = DateTime.UtcNow)
    
    // Custom constructor
    .ConstructUsing(src => new UserDto(src.Id))
    
    // Create reverse mapping
    .ReverseMap();
```

### Custom Value Resolver

```csharp
public class FullNameResolver : IValueResolver<User, UserDto, string>
{
    public string Resolve(User source, UserDto destination, string destMember)
    {
        return $"{source.FirstName} {source.LastName}";
    }
}

cfg.CreateMap<User, UserDto>()
    .ForMember(dest => dest.FullName, opt => opt.MapFrom<FullNameResolver>());
```

### Configuration Validation

```csharp
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<User, UserDto>();
});

// Throws if any destination members are unmapped
config.AssertConfigurationIsValid();
```

## API Reference

### IMapper
- `TDestination Map<TSource, TDestination>(TSource source)` - Map to new object
- `TDestination Map<TSource, TDestination>(TSource source, MapOptions options)` - Map with runtime options
- `TDestination Map<TDestination>(object source)` - Map using runtime type
- `void Map<TSource, TDestination>(TSource source, TDestination destination)` - Map to existing object
- `IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)` - Project queryable to destination type
- `IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source, MapOptions options)` - Project with runtime options

### MapperConfiguration
- `CreateMap<TSource, TDestination>()` - Create a type mapping
- `AddProfile<TProfile>()` - Add a mapping profile
- `AddProfilesFromAssembly(Assembly)` - Auto-discover profiles
- `CreateMapper()` - Create an IMapper instance
- `AssertConfigurationIsValid()` - Validate mappings

### IMappingExpression
- `ForMember()` - Configure individual member
- `ReverseMap()` - Create reverse mapping
- `BeforeMap()` / `AfterMap()` - Add actions
- `Condition()` - Add mapping condition
- `ConstructUsing()` - Custom construction

## Security Considerations

TypeSync is designed with security in mind:

- **Trusted Types Only**: Only map types from trusted sources. The library uses reflection to instantiate types, so avoid mapping untrusted or dynamically loaded types.
- **Depth Limiting**: Nested property resolution is limited to 10 levels to prevent stack overflow attacks.
- **No External Access**: TypeSync does not make network calls, file system access, or database connections.
- **Debug Logging**: Mapping failures are logged to `System.Diagnostics.Debug` for troubleshooting without exposing sensitive information.

## License

MIT License
