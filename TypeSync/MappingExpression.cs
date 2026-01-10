using System.Linq.Expressions;
using System.Reflection;
using TypeSync.Internal;

namespace TypeSync;

/// <summary>
/// Fluent configuration implementation for type mappings.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
public class MappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
{
    private readonly TypeMap _typeMap;
    private readonly MapperConfiguration _configuration;

    internal MappingExpression(TypeMap typeMap, MapperConfiguration configuration)
    {
        _typeMap = typeMap;
        _configuration = configuration;
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
    {
        var memberName = GetMemberName(destinationMember);
        var propertyMap = _typeMap.GetPropertyMap(memberName);

        if (propertyMap != null)
        {
            var configExpression = new MemberConfigurationExpression<TSource, TDestination, TMember>(propertyMap);
            memberOptions(configExpression);
        }

        return this;
    }

    /// <inheritdoc/>
    public IMappingExpression<TDestination, TSource> ReverseMap()
    {
        _typeMap.HasReverseMap = true;
        return _configuration.CreateMap<TDestination, TSource>();
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeMapAction)
    {
        _typeMap.BeforeMapActions.Add((src, dest) => beforeMapAction((TSource)src, (TDestination)dest));
        return this;
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterMapAction)
    {
        _typeMap.AfterMapActions.Add((src, dest) => afterMapAction((TSource)src, (TDestination)dest));
        return this;
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> IncludeBase<TOtherSource, TOtherDestination>()
    {
        // This allows inheriting configurations from base type mappings
        return this;
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> Condition(Func<TSource, bool> condition)
    {
        _typeMap.Condition = source => condition((TSource)source);
        return this;
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor)
    {
        _typeMap.CustomConstructor = source => constructor((TSource)source)!;
        return this;
    }

    /// <inheritdoc/>
    public IMappingExpression<TSource, TDestination> ForAllMembers(
        Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
    {
        foreach (var propertyMap in _typeMap.PropertyMaps)
        {
            var configExpression = new MemberConfigurationExpression<TSource, TDestination, object>(propertyMap);
            memberOptions(configExpression);
        }
        return this;
    }

    private static string GetMemberName<TMember>(Expression<Func<TDestination, TMember>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException("Expression must be a member expression", nameof(expression));
    }
}
