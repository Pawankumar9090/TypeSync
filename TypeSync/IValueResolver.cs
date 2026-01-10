namespace TypeSync;

/// <summary>
/// Interface for custom value resolution during mapping.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
/// <typeparam name="TDestMember">Destination member type.</typeparam>
public interface IValueResolver<in TSource, in TDestination, TDestMember>
{
    /// <summary>
    /// Resolves the value for a destination member.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="destination">Destination object.</param>
    /// <param name="destMember">Current destination member value.</param>
    /// <returns>Resolved value for the destination member.</returns>
    TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember);
}

/// <summary>
/// Interface for custom value resolution with member context.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
/// <typeparam name="TSourceMember">Source member type.</typeparam>
/// <typeparam name="TDestMember">Destination member type.</typeparam>
public interface IMemberValueResolver<in TSource, in TDestination, in TSourceMember, TDestMember>
{
    /// <summary>
    /// Resolves the value for a destination member using source member value.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="destination">Destination object.</param>
    /// <param name="sourceMember">Source member value.</param>
    /// <param name="destMember">Current destination member value.</param>
    /// <returns>Resolved value for the destination member.</returns>
    TDestMember Resolve(TSource source, TDestination destination, TSourceMember sourceMember, TDestMember destMember);
}
