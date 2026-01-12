using System.Reflection;

namespace TypeSync.Internal;

/// <summary>
/// Represents mapping configuration for a single property.
/// </summary>
internal class PropertyMap
{
    public PropertyInfo DestinationProperty { get; }
    public PropertyInfo? SourceProperty { get; set; }
    public string[]? SourcePropertyPath { get; set; }
    public Func<object, object?>? CustomResolver { get; set; }
    public Func<object, bool>? Condition { get; set; }
    public Func<object, object, bool>? ConditionWithDest { get; set; }
    public Func<object, object, object?, bool>? ConditionWithSourceMember { get; set; }
    public object? NullSubstitute { get; set; }
    public bool HasNullSubstitute { get; set; }
    public bool Ignored { get; set; }
    public bool UseDestinationValue { get; set; }
    public Type? ValueResolverType { get; set; }

    public PropertyMap(PropertyInfo destinationProperty)
    {
        DestinationProperty = destinationProperty;
    }

    public bool ShouldMap(object source, object destination)
    {
        if (Ignored) return false;
        if (Condition != null && !Condition(source)) return false;
        if (ConditionWithDest != null && !ConditionWithDest(source, destination)) return false;
        return true;
    }

    public bool CanResolve => SourceProperty != null || SourcePropertyPath != null || CustomResolver != null || ValueResolverType != null;
}
