# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[Unreleased]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/Pawankumar9090/TypeSync/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Pawankumar9090/TypeSync/releases/tag/v1.0.0
