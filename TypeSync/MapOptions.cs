namespace TypeSync;

/// <summary>
/// Options for customizing mapping behavior at runtime.
/// </summary>
public class MapOptions
{
    /// <summary>
    /// Property names to ignore during this mapping operation.
    /// </summary>
    public HashSet<string> IgnoreProperties { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Creates empty map options.
    /// </summary>
    public MapOptions() { }

    /// <summary>
    /// Creates map options with properties to ignore.
    /// </summary>
    /// <param name="propertiesToIgnore">Property names to ignore.</param>
    public MapOptions(params string[] propertiesToIgnore)
    {
        foreach (var prop in propertiesToIgnore)
        {
            IgnoreProperties.Add(prop);
        }
    }

    /// <summary>
    /// Adds a property to ignore during mapping.
    /// </summary>
    /// <param name="propertyName">Property name to ignore.</param>
    /// <returns>This options instance for chaining.</returns>
    public MapOptions Ignore(string propertyName)
    {
        IgnoreProperties.Add(propertyName);
        return this;
    }

    /// <summary>
    /// Adds multiple properties to ignore during mapping.
    /// </summary>
    /// <param name="propertyNames">Property names to ignore.</param>
    /// <returns>This options instance for chaining.</returns>
    public MapOptions Ignore(params string[] propertyNames)
    {
        foreach (var prop in propertyNames)
        {
            IgnoreProperties.Add(prop);
        }
        return this;
    }
}

/// <summary>
/// Builder for creating MapOptions with fluent syntax.
/// </summary>
public static class MapOptionsBuilder
{
    /// <summary>
    /// Creates new map options with the specified properties to ignore.
    /// </summary>
    /// <param name="propertyNames">Property names to ignore.</param>
    /// <returns>Configured MapOptions instance.</returns>
    public static MapOptions IgnoreProperties(params string[] propertyNames)
    {
        return new MapOptions(propertyNames);
    }
}
