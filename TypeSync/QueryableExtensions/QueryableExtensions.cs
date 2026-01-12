using System.Linq;
using System.Linq.Expressions;
using TypeSync.QueryableExtensions;

namespace TypeSync;

/// <summary>
/// Extensions for IQueryable to support projection.
/// </summary>
public static class TypeSyncQueryableExtensions
{
    public static IQueryable<TDestination> ProjectTo<TDestination>(
        this IQueryable source, 
        IConfigurationProvider configuration)
    {
        var sourceType = source.ElementType;
        var method = typeof(TypeSyncQueryableExtensions).GetMethod(nameof(ProjectToImpl), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .MakeGenericMethod(sourceType, typeof(TDestination));

        try
        {
            return (IQueryable<TDestination>)method.Invoke(null, new object[] { source, configuration })!;
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
        IConfigurationProvider configuration)
    {
        var builder = new ProjectionExpressionBuilder(configuration);
        var expression = builder.GetProjection<TSource, TDestination>();
        return source.Select(expression);
    }
}
