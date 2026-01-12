using System.Linq.Expressions;
namespace TypeSync;

/// <summary>
/// Base class for organizing mapping configurations into profiles.
/// Inherit from this class and configure mappings in the constructor.
/// </summary>
/// <example>
/// <code>
/// public class UserProfile : MappingProfile
/// {
///     public UserProfile()
///     {
///         CreateMap&lt;User, UserDto&gt;()
///             .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
///     }
/// }
/// </code>
/// </example>
public abstract class MappingProfile
{
    private MapperConfiguration? _configuration;
    private readonly List<Action<MapperConfiguration>> _pendingConfigurations = [];

    /// <summary>
    /// Creates a mapping configuration between source and destination types.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>Mapping expression for further configuration.</returns>
    protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        if (_configuration != null)
        {
            return _configuration.CreateMap<TSource, TDestination>();
        }

        var deferred = new DeferredMappingExpression<TSource, TDestination>();
        _pendingConfigurations.Add(config =>
        {
            var map = config.CreateMap<TSource, TDestination>();
            deferred.Replay(map);
        });

        return deferred;
    }

    /// <summary>
    /// Configures this profile with the specified configuration.
    /// Called internally by MapperConfiguration.
    /// </summary>
    /// <param name="configuration">The mapper configuration.</param>
    internal void Configure(MapperConfiguration configuration)
    {
        _configuration = configuration;
        ConfigureMappings(); // Allow overrides if used

        // Execute pending actions from Constructor
        foreach (var action in _pendingConfigurations)
        {
            action(configuration);
        }
        _pendingConfigurations.Clear();
    }

    /// <summary>
    /// Override this method to configure mappings, or configure them in the constructor.
    /// </summary>
    protected virtual void ConfigureMappings()
    {
        // Default implementation does nothing.
        // Mappings can be configured either here or in the constructor.
    }

    private class DeferredMappingExpression<TSource, TDestination> : IMappingExpression<TSource, TDestination>
    {
        private readonly List<Action<IMappingExpression<TSource, TDestination>>> _actions = [];

        public void Replay(IMappingExpression<TSource, TDestination> realExpression)
        {
            foreach (var action in _actions)
            {
                action(realExpression);
            }
        }

        public IMappingExpression<TSource, TDestination> ForMember<TMember>(
            Expression<Func<TDestination, TMember>> destinationMember, 
            Action<IMemberConfigurationExpression<TSource, TDestination, TMember>> memberOptions)
        {
            _actions.Add(e => e.ForMember(destinationMember, memberOptions));
            return this;
        }



        public IMappingExpression<TDestination, TSource> ReverseMap()
        {
            var reverseDeferred = new DeferredMappingExpression<TDestination, TSource>();
            _actions.Add(forwardExpr => 
            {
                var reverseExpr = forwardExpr.ReverseMap();
                reverseDeferred.Replay(reverseExpr);
            });
            return reverseDeferred;
        }

        public IMappingExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> ctor)
        {
            _actions.Add(e => e.ConstructUsing(ctor));
            return this;
        }

        public IMappingExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> beforeFunction)
        {
            _actions.Add(e => e.BeforeMap(beforeFunction));
            return this;
        }

        public IMappingExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> afterFunction)
        {
            _actions.Add(e => e.AfterMap(afterFunction));
            return this;
        }

        public IMappingExpression<TSource, TDestination> IncludeBase<TOtherSource, TOtherDestination>()
        {
            _actions.Add(e => e.IncludeBase<TOtherSource, TOtherDestination>());
            return this;
        }

        public IMappingExpression<TSource, TDestination> Condition(Func<TSource, bool> condition)
        {
            _actions.Add(e => e.Condition(condition));
            return this;
        }

        public IMappingExpression<TSource, TDestination> ForAllMembers(Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            _actions.Add(e => e.ForAllMembers(memberOptions));
            return this;
        }
    }
}
