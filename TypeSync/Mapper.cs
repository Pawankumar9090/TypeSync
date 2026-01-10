using TypeSync.Internal;

namespace TypeSync;

/// <summary>
/// Implementation of IMapper that performs object-to-object mapping.
/// </summary>
public class Mapper : IMapper
{
    private readonly MappingEngine _engine;

    internal Mapper(MappingEngine engine)
    {
        _engine = engine;
    }

    /// <inheritdoc/>
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null!)
        {
            return default!;
        }

        return (TDestination)_engine.Map(source, typeof(TSource), typeof(TDestination));
    }

    /// <inheritdoc/>
    public TDestination Map<TSource, TDestination>(TSource source, MapOptions options)
    {
        if (source == null!)
        {
            return default!;
        }

        return (TDestination)_engine.Map(source, typeof(TSource), typeof(TDestination), options);
    }

    /// <inheritdoc/>
    public TDestination Map<TDestination>(object source)
    {
        if (source == null!)
        {
            return default!;
        }

        return (TDestination)_engine.Map(source, source.GetType(), typeof(TDestination));
    }

    /// <inheritdoc/>
    public TDestination Map<TDestination>(object source, MapOptions options)
    {
        if (source == null!)
        {
            return default!;
        }

        return (TDestination)_engine.Map(source, source.GetType(), typeof(TDestination), options);
    }

    /// <inheritdoc/>
    public void Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source == null! || destination == null!)
        {
            return;
        }

        _engine.Map(source, destination!, typeof(TSource), typeof(TDestination));
    }

    /// <inheritdoc/>
    public void Map<TSource, TDestination>(TSource source, TDestination destination, MapOptions options)
    {
        if (source == null! || destination == null!)
        {
            return;
        }

        _engine.Map(source, destination!, typeof(TSource), typeof(TDestination), options);
    }

    /// <inheritdoc/>
    public object Map(object source, Type sourceType, Type destinationType)
    {
        if (source == null!)
        {
            return null!;
        }

        return _engine.Map(source, sourceType, destinationType);
    }

    /// <inheritdoc/>
    public object Map(object source, Type sourceType, Type destinationType, MapOptions options)
    {
        if (source == null!)
        {
            return null!;
        }

        return _engine.Map(source, sourceType, destinationType, options);
    }
}
