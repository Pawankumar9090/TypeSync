using System.Linq.Expressions;

namespace TypeSync;

/// <summary>
/// Fluent interface for configuring mappings between source and destination types.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
public interface IMappingExpression<TSource, TDestination>
{
    /// <summary>
    /// Configures a custom mapping for a specific destination member.
    /// </summary>
    /// <typeparam name="TMember">Destination member type.</typeparam>
    /// <param name="destinationMember">Expression selecting the destination member.</param>
    /// <param name="memberOptions">Action to configure the member mapping.</param>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions);

    /// <summary>
    /// Creates a reverse mapping from destination to source.
    /// </summary>
    /// <returns>Mapping expression for the reverse mapping.</returns>
    IMappingExpression<TDestination, TSource> ReverseMap();

    /// <summary>
    /// Adds an action to execute before mapping.
    /// </summary>
    /// <param name="beforeMapAction">Action to execute before mapping.</param>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeMapAction);

    /// <summary>
    /// Adds an action to execute after mapping.
    /// </summary>
    /// <param name="afterMapAction">Action to execute after mapping.</param>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterMapAction);

    /// <summary>
    /// Includes mappings from another profile.
    /// </summary>
    /// <typeparam name="TOtherSource">Other source type.</typeparam>
    /// <typeparam name="TOtherDestination">Other destination type.</typeparam>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> IncludeBase<TOtherSource, TOtherDestination>();

    /// <summary>
    /// Specifies a condition for the entire mapping.
    /// </summary>
    /// <param name="condition">Condition predicate.</param>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> Condition(Func<TSource, bool> condition);

    /// <summary>
    /// Uses a custom constructor for creating destination objects.
    /// </summary>
    /// <param name="constructor">Factory function to create destination.</param>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> constructor);

    /// <summary>
    /// Ignores all unmapped destination members.
    /// </summary>
    /// <returns>This mapping expression for chaining.</returns>
    IMappingExpression<TSource, TDestination> ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions);
}

/// <summary>
/// Interface for configuring individual member mappings.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
/// <typeparam name="TMember">Destination member type.</typeparam>
public interface IMemberConfigurationExpression<TSource, TDestination, TMember>
{
    /// <summary>
    /// Maps from a specific source member.
    /// </summary>
    /// <typeparam name="TSourceMember">Source member type.</typeparam>
    /// <param name="sourceMember">Expression selecting the source member.</param>
    void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember);

    /// <summary>
    /// Maps using a custom function.
    /// </summary>
    /// <param name="resolver">Function to resolve the value.</param>
    void MapFrom(Func<TSource, TMember> resolver);

    /// <summary>
    /// Maps using a value resolver.
    /// </summary>
    /// <typeparam name="TValueResolver">Value resolver type.</typeparam>
    void MapFrom<TValueResolver>() where TValueResolver : IValueResolver<TSource, TDestination, TMember>, new();

    /// <summary>
    /// Ignores this member during mapping.
    /// </summary>
    void Ignore();

    /// <summary>
    /// Sets a condition for mapping this member.
    /// </summary>
    /// <param name="condition">Condition predicate.</param>
    void Condition(Func<TSource, bool> condition);

    /// <summary>
    /// Sets a condition for mapping this member with destination context.
    /// </summary>
    /// <param name="condition">Condition predicate.</param>
    void Condition(Func<TSource, TDestination, bool> condition);

    /// <summary>
    /// Specifies a value to use when source is null.
    /// </summary>
    /// <param name="nullSubstitute">Value to substitute for null.</param>
    void NullSubstitute(TMember nullSubstitute);

    /// <summary>
    /// Uses an existing value if present.
    /// </summary>
    void UseDestinationValue();
}
