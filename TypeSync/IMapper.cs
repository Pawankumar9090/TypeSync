namespace TypeSync;

/// <summary>
/// Primary interface for object-to-object mapping operations.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps a source object to a new destination object.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <param name="source">Source object to map from.</param>
    /// <returns>Mapped destination object.</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps a source object to a new destination object with runtime options.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <param name="source">Source object to map from.</param>
    /// <param name="options">Runtime mapping options (e.g., properties to ignore).</param>
    /// <returns>Mapped destination object.</returns>
    TDestination Map<TSource, TDestination>(TSource source, MapOptions options);

    /// <summary>
    /// Maps a source object to a new destination object using runtime type discovery.
    /// </summary>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <param name="source">Source object to map from.</param>
    /// <returns>Mapped destination object.</returns>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps a source object to a new destination object with runtime options.
    /// </summary>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <param name="source">Source object to map from.</param>
    /// <param name="options">Runtime mapping options.</param>
    /// <returns>Mapped destination object.</returns>
    TDestination Map<TDestination>(object source, MapOptions options);

    /// <summary>
    /// Maps a source object to an existing destination object.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <param name="source">Source object to map from.</param>
    /// <param name="destination">Existing destination object to map to.</param>
    void Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Maps a source object to an existing destination object with runtime options.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <param name="source">Source object to map from.</param>
    /// <param name="destination">Existing destination object to map to.</param>
    /// <param name="options">Runtime mapping options.</param>
    void Map<TSource, TDestination>(TSource source, TDestination destination, MapOptions options);

    /// <summary>
    /// Maps a source object to a destination type using runtime types.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    /// <returns>Mapped destination object.</returns>
    object Map(object source, Type sourceType, Type destinationType);

    /// <summary>
    /// Maps a source object to a destination type with runtime options.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="sourceType">Source type.</param>
    /// <param name="destinationType">Destination type.</param>
    /// <param name="options">Runtime mapping options.</param>
    /// <returns>Mapped destination object.</returns>
    object Map(object source, Type sourceType, Type destinationType, MapOptions options);
}
