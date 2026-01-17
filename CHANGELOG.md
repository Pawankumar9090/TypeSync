# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.5] - 2026-01-17

### Fixed
- **ForMember MapFrom with Select Expressions**: Fixed issue where `ForMember` with `MapFrom` containing LINQ `.Select()` expressions (e.g., `src => src.ServicePests.Select(s => s.Pest)`) did not map nested collection elements. The `MappingEngine.ResolveValue` now applies `MapValueIfNeeded` after `CustomResolver` returns.
- **LINQ Iterator Element Type Detection**: Enhanced `GetElementType` to find `IEnumerable<T>` interface for LINQ iterator types (e.g., `SelectICollectionIterator<,>`), ensuring proper collection mapping for deferred LINQ queries.
- **Min/Max on Empty Collections**: Fixed "Nullable object must have a value" error when using `.Min()`, `.Max()`, `.Sum()` in `MapFrom` expressions on empty collections. The `NullSafeEvaluator` now properly handles lambda expressions and gracefully catches exceptions from aggregate functions.

### Usage Example
```csharp
// Now works with .Select() expressions
CreateMap<Service, ServiceResponse>()
    .ForMember(dest => dest.ServicePests, opt => opt.MapFrom(src => src.ServicePests.Select(s => s.Pest)));

// Min on empty collection returns default (0) instead of throwing
CreateMap<Plan, PlanDto>()
    .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src => src.HouseTypes.Min(x => x.Price)));
```

## [1.0.4] - 2026-01-16

### Added
- **Runtime Property Ignore for ProjectTo**: Added `MapOptions` support to `ProjectTo<T>()` method, enabling runtime property ignore functionality matching the existing `Map<T>()` capability.
  - New overload: `IMapper.ProjectTo<T>(IQueryable source, MapOptions options)`
  - New overload: `IQueryable.ProjectTo<T>(IConfigurationProvider config, MapOptions options)`
  - Case-insensitive property name matching for ignore list
  - 5 new test cases for `ProjectTo` with `MapOptions`

### Usage Example
```csharp
// Ignore specific properties at runtime during projection
var options = new MapOptions("Password", "SecretKey");
var results = query.ProjectTo<UserDto>(config, options);

// Or via IMapper
var results = mapper.ProjectTo<UserDto>(query, options);
```


## [1.0.3] - 2026-01-13

### Fixed
- **Collection Property Mapping (Map)**: Fixed issue where collection properties (e.g., `ICollection<AddressDto>` → `ICollection<Address>`) were not being mapped when using `Map<T>()`. The `MapValueIfNeeded` method now properly detects and maps collection types for object properties.
- **ProjectTo ICollection Destination**: Fixed `ProjectTo<T>` not mapping collections when destination property is `ICollection<T>`. Added explicit handling to call `ToList()` since `List<T>` implements `ICollection<T>`.

### Added
- Full support for mapping collection properties with different element types in both `Map<T>()` and `ProjectTo<T>()`
- New test: `ProjectTo_ShouldMapToICollectionDestination`
- Enabled previously skipped tests for nested collection mapping

### Collection Types Supported
All common collection types now work seamlessly:
- `IEnumerable<T>` ↔ `IEnumerable<T>`
- `ICollection<T>` ↔ `ICollection<T>`
- `List<T>` ↔ `List<T>`
- `T[]` ↔ `T[]`
- Mixed types (e.g., `List<T>` → `ICollection<T>`)


## [1.0.2] - 2026-01-12

### Fixed
- **ProjectTo Collection Mapping**: Fixed `InvalidCastException` when projecting to nullable `List<T>?` destination properties
- **Nested Object Mapping**: Added support for mapping nested complex types in ProjectTo (e.g., `Class` → `ClassResponse`) with proper null checking
- **Null-Safe MapFrom**: Replaced try-catch exception handling with `NullSafeEvaluator` that walks expression trees and checks for null at each property access step
- **Type Conversion**: Improved type conversion logic to properly handle assignable types and skip incompatible collection conversions

### Added
- `NullSafeEvaluator` utility class for safe expression evaluation
- `ExpressionReplacer` helper for inlining nested projection expressions
- `TryGetNestedObjectProjection` method for recursive nested type mapping
- Helper methods: `IsCollectionType`, `IsImplicitlyConvertible`, `IsNumericType`

## [1.0.1] - 2026-01-12

### Fixed
- Resolved `CS0121` ambiguity error by removing the `MapFrom(Func)` overload.
- Fixed `CS1593` by implementing a new 3-argument `Condition` overload: `Condition((src, dest, srcMember) => ...)`.
- Fixed type inference issues (`CS0411`) in tests by removing redundant explicit casts.

### Changed
- `MapFrom` now exclusively uses `Expression`-based configuration for better property inspection and validation.


## [1.0.0] - 2026-01-10

### Added
- Initial release of TypeSync
- Convention-based object-to-object mapping
- Fluent configuration API with `CreateMap`, `ForMember`, `ReverseMap`
- Profile support via `MappingProfile` class
- Automatic property flattening (e.g., `Customer.Name` → `CustomerName`)
- Collection mapping for lists, arrays, and other enumerables
- Custom value resolvers via `IValueResolver<TSource, TDestination, TDestMember>`
- Conditional mapping with `Condition()`
- Null substitution with `NullSubstitute()`
- Before/After map actions with `BeforeMap()` and `AfterMap()`
- Custom construction with `ConstructUsing()`
- Dependency injection integration via `AddTypeSync()` extension methods
- Assembly scanning for automatic profile discovery
- Configuration validation with `AssertConfigurationIsValid()`
- Mapping to existing objects
- Ignore property mapping with `Ignore()`

### Security
- Nested property resolution depth limited to 10 levels
- Debug logging for troubleshooting without exposing sensitive data

[Unreleased]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.5...HEAD
[1.0.5]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Pawankumar9090/TypeSync/releases/tag/v1.0.0

