# Changelog

All notable changes to this project will be documented in this file.

## [1.1.1] - 2026-02-04

### Added

- New `Mapper.To<T>()` API (replacement for `Velocity.Map<T>()`)
- Collection mapping API: `ToList<T>()`, `ToArray<T>()`, `ToEnumerable<T>()`, `ToSpan<T>()`
- Span<T> optimization for zero-copy collection iteration on .NET 8+
- Comprehensive API reference documentation
- Collection mapping guide with practical examples

### Changed

- Renamed core API from `Velocity.Map` to `Mapper.To` for better clarity and discoverability
- Optimized object initializer pattern for improved JIT compilation (12.03 ns performance)
- Updated all test files to use new API syntax
- Property ordering optimization for better cache locality

### Performance

- Achieved 12.03 ns per mapping (essentially tied with Mapperly at 12.26 ns)
- 16% faster than manual mapping code
- 2.88x faster than AutoMapper
- Zero-allocation collections with Span optimization

## [1.0.0] - 2026-01-20

### Added

- Initial release
- High-performance object mapping using Source Generators
- Zero reflection, zero overhead
- Support for .NET 6, 8, 9, and 10
- Fluent API configuration (`ForMember`, `Ignore`, `ReverseMap`)
- Attribute-based mapping (`[MapFrom]`, `[IgnoreMap]`)
- Collection mapping: `ToList`, `ToArray`, `ToEnumerable`, `ToSpan` (Span-optimized)
