using System.Reflection;
using TypeSync.Internal;

namespace TypeSync;

/// <summary>
/// Stores and manages all mapping configurations.
/// </summary>
public class MapperConfiguration
{
    private readonly Dictionary<(Type, Type), TypeMap> _typeMaps = new();
    private readonly List<MappingProfile> _profiles = [];

    /// <summary>
    /// Creates a new MapperConfiguration with the specified configuration action.
    /// </summary>
    /// <param name="configure">Action to configure mappings.</param>
    public MapperConfiguration(Action<MapperConfiguration> configure)
    {
        configure(this);
    }

    /// <summary>
    /// Creates a new MapperConfiguration using profiles from specified assemblies.
    /// </summary>
    /// <param name="assemblies">Assemblies to scan for profiles.</param>
    public MapperConfiguration(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            AddProfilesFromAssembly(assembly);
        }
    }

    /// <summary>
    /// Creates a mapping configuration between source and destination types.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>Mapping expression for further configuration.</returns>
    public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
    {
        var typeMap = new TypeMap(typeof(TSource), typeof(TDestination));
        _typeMaps[(typeof(TSource), typeof(TDestination))] = typeMap;
        return new MappingExpression<TSource, TDestination>(typeMap, this);
    }

    /// <summary>
    /// Adds a mapping profile.
    /// </summary>
    /// <typeparam name="TProfile">Profile type.</typeparam>
    public void AddProfile<TProfile>() where TProfile : MappingProfile, new()
    {
        var profile = new TProfile();
        AddProfile(profile);
    }

    /// <summary>
    /// Adds a mapping profile instance.
    /// </summary>
    /// <param name="profile">Profile instance.</param>
    public void AddProfile(MappingProfile profile)
    {
        _profiles.Add(profile);
        profile.Configure(this);
    }

    /// <summary>
    /// Adds all profiles from the specified assembly.
    /// </summary>
    /// <param name="assembly">Assembly to scan.</param>
    public void AddProfilesFromAssembly(Assembly assembly)
    {
        var profileTypes = assembly.GetTypes()
            .Where(t => typeof(MappingProfile).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var profileType in profileTypes)
        {
            var profile = (MappingProfile)Activator.CreateInstance(profileType)!;
            AddProfile(profile);
        }
    }

    /// <summary>
    /// Adds all profiles from the calling assembly.
    /// </summary>
    public void AddMaps(Assembly assembly) => AddProfilesFromAssembly(assembly);

    /// <summary>
    /// Creates a mapper instance from this configuration.
    /// </summary>
    /// <returns>Configured mapper instance.</returns>
    public IMapper CreateMapper()
    {
        return new Mapper(new MappingEngine(_typeMaps));
    }

    /// <summary>
    /// Validates that all destination members have a source.
    /// Throws an exception if validation fails.
    /// </summary>
    public void AssertConfigurationIsValid()
    {
        var errors = new List<string>();

        foreach (var typeMap in _typeMaps.Values)
        {
            foreach (var propertyMap in typeMap.PropertyMaps)
            {
                if (!propertyMap.Ignored && !propertyMap.CanResolve)
                {
                    errors.Add($"Unmapped property: {typeMap.DestinationType.Name}.{propertyMap.DestinationProperty.Name}");
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Mapping configuration is invalid:\n{string.Join("\n", errors)}");
        }
    }

    internal Dictionary<(Type, Type), TypeMap> GetTypeMaps() => _typeMaps;
}
