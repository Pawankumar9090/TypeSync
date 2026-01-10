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

    /// <summary>
    /// Creates a mapping configuration between source and destination types.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>Mapping expression for further configuration.</returns>
    protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        if (_configuration == null)
        {
            throw new InvalidOperationException("Profile has not been configured. Use MapperConfiguration to register this profile.");
        }

        return _configuration.CreateMap<TSource, TDestination>();
    }

    /// <summary>
    /// Configures this profile with the specified configuration.
    /// Called internally by MapperConfiguration.
    /// </summary>
    /// <param name="configuration">The mapper configuration.</param>
    internal void Configure(MapperConfiguration configuration)
    {
        _configuration = configuration;
        ConfigureMappings();
    }

    /// <summary>
    /// Override this method to configure mappings, or configure them in the constructor.
    /// </summary>
    protected virtual void ConfigureMappings()
    {
        // Default implementation does nothing.
        // Mappings can be configured either here or in the constructor.
    }
}
