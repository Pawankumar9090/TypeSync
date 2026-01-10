using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TypeSync.DependencyInjection;

/// <summary>
/// Extension methods for configuring EasyMapper with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds EasyMapper services with inline configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure mappings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTypeSync(
        this IServiceCollection services,
        Action<MapperConfiguration> configure)
    {
        var configuration = new MapperConfiguration(configure);
        var mapper = configuration.CreateMapper();

        services.AddSingleton(configuration);
        services.AddSingleton(mapper);

        return services;
    }

    /// <summary>
    /// Adds EasyMapper services by auto-discovering profiles from specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assemblies">Assemblies to scan for MappingProfile implementations.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTypeSync(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            foreach (var assembly in assemblies)
            {
                cfg.AddProfilesFromAssembly(assembly);
            }
        });

        var mapper = configuration.CreateMapper();

        services.AddSingleton(configuration);
        services.AddSingleton(mapper);

        return services;
    }

    /// <summary>
    /// Adds EasyMapper services with a specific profile.
    /// </summary>
    /// <typeparam name="TProfile">Profile type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTypeSync<TProfile>(this IServiceCollection services)
        where TProfile : MappingProfile, new()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<TProfile>();
        });

        var mapper = configuration.CreateMapper();

        services.AddSingleton(configuration);
        services.AddSingleton(mapper);

        return services;
    }

    /// <summary>
    /// Adds EasyMapper services by auto-discovering profiles from the calling assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTypeSync(this IServiceCollection services)
    {
        return services.AddTypeSync(Assembly.GetCallingAssembly());
    }
}
