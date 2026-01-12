using System.Linq.Expressions;
using TypeSync.Internal;

namespace TypeSync;

/// <summary>
/// Fluent configuration implementation for member mappings.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
/// <typeparam name="TMember">Destination member type.</typeparam>
public class MemberConfigurationExpression<TSource, TDestination, TMember> : IMemberConfigurationExpression<TSource, TDestination, TMember>
{
    private readonly PropertyMap _propertyMap;

    internal MemberConfigurationExpression(PropertyMap propertyMap)
    {
        _propertyMap = propertyMap;
    }

    /// <inheritdoc/>
    public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
    {
        var compiled = sourceMember.Compile();
        _propertyMap.CustomResolver = source => compiled((TSource)source);
    }



    /// <inheritdoc/>
    public void MapFrom<TValueResolver>() where TValueResolver : IValueResolver<TSource, TDestination, TMember>, new()
    {
        _propertyMap.ValueResolverType = typeof(TValueResolver);
    }

    /// <inheritdoc/>
    public void Ignore()
    {
        _propertyMap.Ignored = true;
    }

    /// <inheritdoc/>
    public void Condition(Func<TSource, bool> condition)
    {
        _propertyMap.Condition = source => condition((TSource)source);
    }

    /// <inheritdoc/>
    public void Condition(Func<TSource, TDestination, bool> condition)
    {
        _propertyMap.ConditionWithDest = (source, dest) => condition((TSource)source, (TDestination)dest);
    }

    /// <inheritdoc/>
    public void Condition(Func<TSource, TDestination, object?, bool> condition)
    {
        _propertyMap.ConditionWithSourceMember = (source, dest, member) => condition((TSource)source, (TDestination)dest, member);
    }

    /// <inheritdoc/>
    public void NullSubstitute(TMember nullSubstitute)
    {
        _propertyMap.NullSubstitute = nullSubstitute;
        _propertyMap.HasNullSubstitute = true;
    }

    /// <inheritdoc/>
    public void UseDestinationValue()
    {
        _propertyMap.UseDestinationValue = true;
    }
}
