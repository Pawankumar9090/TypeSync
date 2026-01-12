using System;
using TypeSync.Internal;

namespace TypeSync;

/// <summary>
/// Provides access to the mapping configuration.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Finds the TypeMap for the specified source and destination types.
    /// </summary>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    /// <returns>The TypeMap if found, otherwise null.</returns>
    TypeMap? FindTypeMapFor(Type sourceType, Type destinationType);
}
