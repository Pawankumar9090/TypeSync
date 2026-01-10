using System.Collections;
using System.Reflection;

namespace TypeSync.Internal;

/// <summary>
/// Engine that performs actual object mapping using reflection.
/// </summary>
internal class MappingEngine
{
    private readonly Dictionary<(Type, Type), TypeMap> _typeMaps;
    
    /// <summary>
    /// Maximum depth for nested property resolution to prevent stack overflow.
    /// </summary>
    private const int MaxNestedDepth = 10;

    public MappingEngine(Dictionary<(Type, Type), TypeMap> typeMaps)
    {
        _typeMaps = typeMaps;
    }

    public object Map(object source, Type sourceType, Type destinationType, MapOptions? options = null)
    {
        if (source == null!)
        {
            return null!;
        }

        // Check if it's a collection
        if (IsCollection(destinationType) && IsCollection(sourceType))
        {
            return MapCollection(source, sourceType, destinationType, options);
        }

        var typeMap = GetTypeMap(sourceType, destinationType);
        
        // Check condition
        if (typeMap.Condition != null && !typeMap.Condition(source))
        {
            return CreateInstance(destinationType)!;
        }

        // Create destination instance
        object destination;
        if (typeMap.CustomConstructor != null)
        {
            destination = typeMap.CustomConstructor(source);
        }
        else
        {
            destination = CreateInstance(destinationType)!;
        }

        // Execute before map actions
        foreach (var action in typeMap.BeforeMapActions)
        {
            action(source, destination);
        }

        // Map properties
        MapProperties(source, destination, typeMap, options);

        // Execute after map actions
        foreach (var action in typeMap.AfterMapActions)
        {
            action(source, destination);
        }

        return destination;
    }

    public void Map(object source, object destination, Type sourceType, Type destinationType, MapOptions? options = null)
    {
        if (source == null!) return;

        var typeMap = GetTypeMap(sourceType, destinationType);

        if (typeMap.Condition != null && !typeMap.Condition(source))
        {
            return;
        }

        foreach (var action in typeMap.BeforeMapActions)
        {
            action(source, destination);
        }

        MapProperties(source, destination, typeMap, options);

        foreach (var action in typeMap.AfterMapActions)
        {
            action(source, destination);
        }
    }

    private TypeMap GetTypeMap(Type sourceType, Type destinationType)
    {
        if (_typeMaps.TryGetValue((sourceType, destinationType), out var typeMap))
        {
            return typeMap;
        }

        // Create an implicit mapping
        typeMap = new TypeMap(sourceType, destinationType);
        _typeMaps[(sourceType, destinationType)] = typeMap;
        return typeMap;
    }

    private void MapProperties(object source, object destination, TypeMap typeMap, MapOptions? options)
    {
        var ignoreProperties = options?.IgnoreProperties;

        foreach (var propertyMap in typeMap.PropertyMaps)
        {
            // Check runtime ignore list
            if (ignoreProperties != null && ignoreProperties.Contains(propertyMap.DestinationProperty.Name))
            {
                continue;
            }

            if (!propertyMap.ShouldMap(source, destination))
            {
                continue;
            }

            if (!propertyMap.CanResolve)
            {
                continue;
            }

            try
            {
                var value = ResolveValue(source, destination, propertyMap, options);
                
                if (value == null && propertyMap.HasNullSubstitute)
                {
                    value = propertyMap.NullSubstitute;
                }

                if (value != null || !propertyMap.UseDestinationValue)
                {
                    var convertedValue = ConvertValue(value, propertyMap.DestinationProperty.PropertyType);
                    propertyMap.DestinationProperty.SetValue(destination, convertedValue);
                }
            }
            catch (Exception ex)
            {
                // Log mapping failures for debugging purposes
                System.Diagnostics.Debug.WriteLine(
                    $"TypeSync: Failed to map property '{propertyMap.DestinationProperty.Name}': {ex.Message}");
            }
        }
    }

    private object? ResolveValue(object source, object destination, PropertyMap propertyMap, MapOptions? options)
    {
        // Custom resolver takes priority
        if (propertyMap.CustomResolver != null)
        {
            return propertyMap.CustomResolver(source);
        }

        // Value resolver type
        if (propertyMap.ValueResolverType != null)
        {
            var resolver = Activator.CreateInstance(propertyMap.ValueResolverType);
            var resolveMethod = propertyMap.ValueResolverType.GetMethod("Resolve");
            var currentValue = propertyMap.DestinationProperty.GetValue(destination);
            return resolveMethod?.Invoke(resolver, [source, destination, currentValue]);
        }

        // Direct property mapping
        if (propertyMap.SourceProperty != null)
        {
            var value = propertyMap.SourceProperty.GetValue(source);
            return MapValueIfNeeded(value, propertyMap.SourceProperty.PropertyType, propertyMap.DestinationProperty.PropertyType, options);
        }

        // Flattened path
        if (propertyMap.SourcePropertyPath != null)
        {
            return ResolvePathValue(source, propertyMap.SourcePropertyPath, propertyMap.DestinationProperty.PropertyType, options);
        }

        return null;
    }

    private object? ResolvePathValue(object source, string[] path, Type destinationType, MapOptions? options)
    {
        // Prevent excessively deep nesting that could cause stack overflow
        if (path.Length > MaxNestedDepth)
        {
            System.Diagnostics.Debug.WriteLine(
                $"TypeSync: Path depth {path.Length} exceeds maximum allowed depth of {MaxNestedDepth}");
            return null;
        }
        
        object? current = source;

        foreach (var segment in path)
        {
            if (current == null) return null;

            var prop = current.GetType().GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return null;

            current = prop.GetValue(current);
        }

        return MapValueIfNeeded(current, current?.GetType() ?? typeof(object), destinationType, options);
    }

    private object? MapValueIfNeeded(object? value, Type sourceType, Type destType, MapOptions? options)
    {
        if (value == null) return null;

        // If types are compatible, no mapping needed
        if (destType.IsAssignableFrom(sourceType))
        {
            return value;
        }

        // Check if we have a mapping for nested objects
        var actualSourceType = value.GetType();
        if (_typeMaps.ContainsKey((actualSourceType, destType)) || IsComplexType(destType))
        {
            return Map(value, actualSourceType, destType, options);
        }

        return ConvertValue(value, destType);
    }

    private object MapCollection(object source, Type sourceType, Type destinationType, MapOptions? options)
    {
        var sourceElementType = GetElementType(sourceType);
        var destElementType = GetElementType(destinationType);

        if (sourceElementType == null || destElementType == null)
        {
            return source;
        }

        var sourceEnumerable = (IEnumerable)source;
        var listType = typeof(List<>).MakeGenericType(destElementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var item in sourceEnumerable)
        {
            if (item == null)
            {
                list.Add(null);
            }
            else
            {
                var mappedItem = Map(item, sourceElementType, destElementType, options);
                list.Add(mappedItem);
            }
        }

        // Convert to array if needed
        if (destinationType.IsArray)
        {
            var array = Array.CreateInstance(destElementType, list.Count);
            list.CopyTo(array, 0);
            return array;
        }

        return list;
    }

    private static bool IsCollection(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static Type? GetElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments().FirstOrDefault();
        }

        return null;
    }

    private static bool IsComplexType(Type type)
    {
        return !type.IsPrimitive &&
               !type.IsEnum &&
               type != typeof(string) &&
               type != typeof(decimal) &&
               type != typeof(DateTime) &&
               type != typeof(DateTimeOffset) &&
               type != typeof(TimeSpan) &&
               type != typeof(Guid) &&
               !IsCollection(type);
    }

    private static object? ConvertValue(object? value, Type destinationType)
    {
        if (value == null)
        {
            return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
        }

        var sourceType = value.GetType();

        if (destinationType.IsAssignableFrom(sourceType))
        {
            return value;
        }

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(destinationType);
        if (underlyingType != null)
        {
            return Convert.ChangeType(value, underlyingType);
        }

        // Try direct conversion using safer TryParse for enums
        if (destinationType.IsEnum && value is string strValue)
        {
            try
            {
                if (Enum.TryParse(destinationType, strValue, ignoreCase: true, out var enumResult))
                {
                    return enumResult;
                }
                // Return default enum value if parsing fails
                return Activator.CreateInstance(destinationType);
            }
            catch
            {
                return Activator.CreateInstance(destinationType);
            }
        }

        try
        {
            return Convert.ChangeType(value, destinationType);
        }
        catch
        {
            return value;
        }
    }

    private static object? CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }
}
