using System.Reflection;

namespace TypeSync.Internal;

/// <summary>
/// Represents mapping configuration between a source and destination type.
/// </summary>
internal class TypeMap
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public List<PropertyMap> PropertyMaps { get; } = [];
    public Func<object, object>? CustomConstructor { get; set; }
    public Func<object, bool>? Condition { get; set; }
    public List<Action<object, object>> BeforeMapActions { get; } = [];
    public List<Action<object, object>> AfterMapActions { get; } = [];
    public bool HasReverseMap { get; set; }

    public TypeMap(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        InitializeDefaultPropertyMaps();
    }

    private void InitializeDefaultPropertyMaps()
    {
        var sourceProperties = SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var destProperties = DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var destProp in destProperties)
        {
            var propertyMap = new PropertyMap(destProp);

            // Try direct name match
            if (sourceProperties.TryGetValue(destProp.Name, out var sourceProp))
            {
                propertyMap.SourceProperty = sourceProp;
            }
            else
            {
                // Try flattening (e.g., CustomerName -> Customer.Name)
                propertyMap.SourcePropertyPath = TryFindFlattenedPath(destProp.Name);
            }

            PropertyMaps.Add(propertyMap);
        }
    }

    private string[]? TryFindFlattenedPath(string destPropertyName)
    {
        var sourceProperties = SourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        // Try to find a flattened path
        foreach (var sourceProp in sourceProperties)
        {
            if (destPropertyName.StartsWith(sourceProp.Name, StringComparison.OrdinalIgnoreCase))
            {
                var remaining = destPropertyName[sourceProp.Name.Length..];
                if (string.IsNullOrEmpty(remaining))
                {
                    return [sourceProp.Name];
                }

                var nestedPath = TryFindNestedPath(sourceProp.PropertyType, remaining);
                if (nestedPath != null)
                {
                    return [sourceProp.Name, .. nestedPath];
                }
            }
        }

        return null;
    }

    private static string[]? TryFindNestedPath(Type type, string propertyName)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToList();

        foreach (var prop in properties)
        {
            if (propertyName.Equals(prop.Name, StringComparison.OrdinalIgnoreCase))
            {
                return [prop.Name];
            }

            if (propertyName.StartsWith(prop.Name, StringComparison.OrdinalIgnoreCase))
            {
                var remaining = propertyName[prop.Name.Length..];
                var nestedPath = TryFindNestedPath(prop.PropertyType, remaining);
                if (nestedPath != null)
                {
                    return [prop.Name, .. nestedPath];
                }
            }
        }

        return null;
    }

    public PropertyMap? GetPropertyMap(string destinationMemberName)
    {
        return PropertyMaps.FirstOrDefault(pm =>
            pm.DestinationProperty.Name.Equals(destinationMemberName, StringComparison.OrdinalIgnoreCase));
    }
}
