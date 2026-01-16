using System.Linq;
using System.Linq.Expressions;
using TypeSync.QueryableExtensions;

namespace TypeSync;

/// <summary>
/// Extensions for IQueryable to support projection.
/// </summary>
public static class TypeSyncQueryableExtensions
{
    /// <summary>
    /// Projects a queryable to the destination type.
    /// </summary>
    public static IQueryable<TDestination> ProjectTo<TDestination>(
        this IQueryable source, 
        IConfigurationProvider configuration)
    {
        return ProjectTo<TDestination>(source, configuration, null);
    }

    /// <summary>
    /// Projects a queryable to the destination type with runtime options.
    /// </summary>
    /// <param name="source">Source queryable.</param>
    /// <param name="configuration">Configuration provider.</param>
    /// <param name="options">Runtime mapping options (e.g., properties to ignore).</param>
    public static IQueryable<TDestination> ProjectTo<TDestination>(
        this IQueryable source, 
        IConfigurationProvider configuration,
        MapOptions? options)
    {
        var sourceType = source.ElementType;
        var method = typeof(TypeSyncQueryableExtensions).GetMethod(nameof(ProjectToImpl), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(sourceType, typeof(TDestination));

        try
        {
            return (IQueryable<TDestination>)method.Invoke(null, new object?[] { source, configuration, options })!;
        }
        catch (System.Reflection.TargetInvocationException ex)
        {
            if (ex.InnerException != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
            throw;
        }
    }

    private static IQueryable<TDestination> ProjectToImpl<TSource, TDestination>(
        IQueryable<TSource> source, 
        IConfigurationProvider configuration,
        MapOptions? options)
    {
        var builder = new ProjectionExpressionBuilder(configuration, options);
        var expression = builder.GetProjection<TSource, TDestination>();
        return source.Select(expression);
    }
}
