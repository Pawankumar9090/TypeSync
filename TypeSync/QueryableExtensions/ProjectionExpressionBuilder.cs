using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using TypeSync.Internal;

namespace TypeSync.QueryableExtensions;

internal class ProjectionExpressionBuilder
{
    private readonly IConfigurationProvider _configurationProvider;

    public ProjectionExpressionBuilder(IConfigurationProvider configurationProvider)
    {
        _configurationProvider = configurationProvider;
    }

    public Expression<Func<TSource, TDestination>> GetProjection<TSource, TDestination>()
    {
        var typeMap = _configurationProvider.FindTypeMapFor(typeof(TSource), typeof(TDestination));
        
        var sourceParam = Expression.Parameter(typeof(TSource), "src");
        var bindings = new List<MemberBinding>();

        var destProps = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        foreach (var destProp in destProps)
        {
            var propertyMap = typeMap?.PropertyMaps.FirstOrDefault(pm => pm.DestinationProperty.Name == destProp.Name);

            // 1. Check if ignored
            if (propertyMap?.Ignored == true)
            {
                continue;
            }

            Expression? sourceExpression = null;

            // 2. Custom MapFrom (Expression-based only)
            if (propertyMap?.SourceExpression != null)
            {
                // Replace parameter in the source expression with our sourceParam
                sourceExpression = ReplaceParameter(propertyMap.SourceExpression, sourceParam);
            }
            // 3. Name matching
            else
            {
                var sourceProp = typeof(TSource).GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (sourceProp != null)
                {
                    sourceExpression = Expression.Property(sourceParam, sourceProp);

                    // Try Mapping Collection
                    var collectionProj = TryGetCollectionProjection(sourceExpression, destProp.PropertyType);
                    if (collectionProj != null)
                    {
                        sourceExpression = collectionProj;
                    }
                }
                // 4. Flattening (e.g., CustomerName -> Customer.Name)
                else
                {
                    // Basic flattening support
                    var matchingSourceProp = FindFlattenedProperty(typeof(TSource), destProp.Name, sourceParam);
                    if (matchingSourceProp != null)
                    {
                        sourceExpression = matchingSourceProp;
                    }
                }
            }

            if (sourceExpression != null)
            {
                // Type conversion if needed (basic)
                if (sourceExpression.Type != destProp.PropertyType)
                {
                    // Check if types are assignable (handles nullable reference types, List<T> to List<T>?, etc.)
                    if (destProp.PropertyType.IsAssignableFrom(sourceExpression.Type))
                    {
                        // Types are compatible, no conversion needed (handles List<T> -> List<T>? cases)
                    }
                    else if (IsImplicitlyConvertible(sourceExpression.Type, destProp.PropertyType))
                    {
                        // Only convert for simple value type conversions
                        sourceExpression = Expression.Convert(sourceExpression, destProp.PropertyType);
                    }
                    // For incompatible collection types, skip adding this binding
                    // as the collection projection should have handled it
                    else if (IsCollectionType(sourceExpression.Type) || IsCollectionType(destProp.PropertyType))
                    {
                        // Skip invalid collection conversion - this would cause a runtime error
                        continue;
                    }
                    // Try to handle nested complex type mapping (e.g., Class -> ClassResponse)
                    else if (TryGetNestedObjectProjection(sourceExpression, destProp.PropertyType, out var nestedProjection))
                    {
                        sourceExpression = nestedProjection!;
                    }
                    // For simple value types or types without mappings, skip this property
                    else if (!sourceExpression.Type.IsValueType && !destProp.PropertyType.IsValueType)
                    {
                        // Skip complex types without mappings to avoid runtime errors
                        continue;
                    }
                    else
                    {
                        // Try casting for value types
                        sourceExpression = Expression.Convert(sourceExpression, destProp.PropertyType);
                    }
                }

                bindings.Add(Expression.Bind(destProp, sourceExpression));
            }
        }

        var memberInit = Expression.MemberInit(Expression.New(typeof(TDestination)), bindings);
        return Expression.Lambda<Func<TSource, TDestination>>(memberInit, sourceParam);
    }

    private Expression ReplaceParameter(LambdaExpression expression, ParameterExpression newParam)
    {
        return new ParameterReplacer(expression.Parameters[0], newParam).Visit(expression.Body);
    }

    private Expression? TryGetCollectionProjection(Expression sourceExpression, Type destPropType)
    {
        var sourcePropType = sourceExpression.Type;
        if (sourcePropType == typeof(string) || destPropType == typeof(string)) return null;

        var sourceElementType = GetElementType(sourcePropType);
        var destElementType = GetElementType(destPropType);

        if (sourceElementType != null && destElementType != null)
        {
            // Find map for elements
            var typeMap = _configurationProvider.FindTypeMapFor(sourceElementType, destElementType);
            if (typeMap == null)
            {
                if (sourceElementType != destElementType)
                {
                    // If types differ and no map exists, we can't map the collection elements.
                    // This prevents the confusing InvalidCastException later.
                    throw new InvalidOperationException($"Missing map configuration from '{sourceElementType.Name}' to '{destElementType.Name}'. This is required to map the collection property.");
                }
                return null;
            }

            if (typeMap != null)
            {
                // Recursive call to get the projection for the element type
                // We need to call GetProjection<SourceElement, DestElement>()
                // Since we are in a generic context regarding the PARENT, but strictly typed here, 
                // we'll recursively call GetProjection via reflection or refactoring GetProjection to take Types.
                // Refactoring GetProjection to take Type arguments is cleaner.
                
                var projectionLambda = GetProjection(sourceElementType, destElementType); // Private internal helper
                if (projectionLambda == null) return null;

                // Create .Select(projection)
                // Enumerable.Select<TSource, TResult>(source, selector)
                var selectMethod = typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Select" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(sourceElementType, destElementType);

                var selectCall = Expression.Call(selectMethod, sourceExpression, projectionLambda);

                // If destination is List<T> or List<T>?, call ToList()
                // Handle nullable reference types by checking if the destination is List<T> or assignable to it
                var listType = typeof(List<>).MakeGenericType(destElementType);
                var isListType = listType == destPropType || 
                                 listType.IsAssignableFrom(destPropType) ||
                                 (destPropType.IsGenericType && destPropType.GetGenericTypeDefinition() == typeof(List<>));
                
                if (isListType)
                {
                    var toListMethod = typeof(Enumerable).GetMethod("ToList")!
                        .MakeGenericMethod(destElementType);
                    return Expression.Call(toListMethod, selectCall);
                }
                
                // If destination is array? ToArray()
                if (destPropType.IsArray)
                {
                     var toArrayMethod = typeof(Enumerable).GetMethod("ToArray")!
                        .MakeGenericMethod(destElementType);
                    return Expression.Call(toArrayMethod, selectCall);
                }

                // If destination is IEnumerable / ICollection compatibility
                 return selectCall;
            }
        }
        return null;
    }

    private Type? GetElementType(Type type)
    {
        if (type.IsArray) return type.GetElementType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) return type.GetGenericArguments()[0];
        
        var enumInterface = type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumInterface?.GetGenericArguments()[0];
    }

    /// <summary>
    /// Tries to create a projection for a nested object mapping (e.g., Class -> ClassResponse).
    /// </summary>
    private bool TryGetNestedObjectProjection(Expression sourceExpression, Type destType, out Expression? projection)
    {
        projection = null;
        
        var sourceType = sourceExpression.Type;
        
        // Skip primitive types and strings
        if (sourceType.IsPrimitive || sourceType == typeof(string) || 
            destType.IsPrimitive || destType == typeof(string))
        {
            return false;
        }
        
        // Get the non-nullable destination type
        var destNonNullableType = Nullable.GetUnderlyingType(destType) ?? destType;
        
        // Check if there's a mapping defined for these types
        var typeMap = _configurationProvider.FindTypeMapFor(sourceType, destNonNullableType);
        if (typeMap == null)
        {
            return false;
        }
        
        try
        {
            // Get the projection lambda for the nested type
            var nestedProjection = GetProjection(sourceType, destNonNullableType);
            if (nestedProjection == null)
            {
                return false;
            }
            
            // Create an invocation: nestedProjection.Compile()(sourceExpression)
            // But for EF queries, we need to inline the projection expression
            // Replace the parameter in the nested projection with our source expression
            var inlinedBody = new ExpressionReplacer(nestedProjection.Parameters[0], sourceExpression).Visit(nestedProjection.Body);
            
            // Add null check: sourceExpression == null ? null : inlinedBody
            if (!sourceType.IsValueType)
            {
                var nullCheck = Expression.Equal(sourceExpression, Expression.Constant(null, sourceType));
                var nullValue = Expression.Constant(null, destType);
                projection = Expression.Condition(nullCheck, nullValue, Expression.Convert(inlinedBody, destType));
            }
            else
            {
                projection = inlinedBody;
            }
            
            return true;
        }
        catch
        {
            // If projection fails for any reason, return false
            return false;
        }
    }
    
    // Internal non-generic helper to avoid MakeGenericMethod overhead for recursion
    // or just overload GetProjection
    private LambdaExpression? GetProjection(Type sourceType, Type destType)
    {
        // Use reflection to call the generic public GetProjection or implement logic here?
        // Implementing logic here (extracting core logic) is better to avoid recursion loops with generics
        // But GetProjection is public generic...
        
        // Let's invoke the public generic one for simplicity now, optimizes later
        var method = this.GetType().GetMethod(nameof(GetProjection), Type.EmptyTypes)!
            .MakeGenericMethod(sourceType, destType);
        try
        {
            return (LambdaExpression)method.Invoke(this, null)!;
        }
        catch (TargetInvocationException ex)
        {
            if (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            }
            throw;
        }
    }
    private Expression? FindFlattenedProperty(Type sourceType, string destPropertyName, Expression sourceParam)
    {
        var props = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in props)
        {
            if (destPropertyName.StartsWith(prop.Name, StringComparison.OrdinalIgnoreCase))
            {
                var remainingName = destPropertyName.Substring(prop.Name.Length);
                var nestedParam = Expression.Property(sourceParam, prop);
                
                // Try to find the remaining part on the nested type
                var nestedProp = prop.PropertyType.GetProperty(remainingName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (nestedProp != null)
                {
                    // return nestedParam.nestedProp WITH null check
                    var propertyAccess = Expression.Property(nestedParam, nestedProp);
                    
                    // Check if parent (nestedParam) is null
                    if (!prop.PropertyType.IsValueType)
                    {
                         var nullCheck = Expression.NotEqual(nestedParam, Expression.Constant(null, prop.PropertyType));
                         var defaultValue = Expression.Default(nestedProp.PropertyType);
                         return Expression.Condition(nullCheck, propertyAccess, defaultValue);
                    }
                    
                    return propertyAccess;
                }
            }
        }
        return null;
    }

    private bool IsCollectionType(Type type)
    {
        if (type == typeof(string)) return false;
        if (type.IsArray) return true;
        
        // Check for generic collections
        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            return genericDef == typeof(List<>) ||
                   genericDef == typeof(IList<>) ||
                   genericDef == typeof(ICollection<>) ||
                   genericDef == typeof(IEnumerable<>) ||
                   genericDef == typeof(HashSet<>) ||
                   genericDef == typeof(ISet<>);
        }
        
        // Check interfaces
        return type.GetInterfaces().Any(i => 
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    private bool IsImplicitlyConvertible(Type from, Type to)
    {
        // Handle nullable value types
        var underlyingFrom = Nullable.GetUnderlyingType(from) ?? from;
        var underlyingTo = Nullable.GetUnderlyingType(to) ?? to;
        
        // Both are the same underlying type, just nullability difference
        if (underlyingFrom == underlyingTo) return true;
        
        // Handle numeric conversions
        if (IsNumericType(underlyingFrom) && IsNumericType(underlyingTo))
        {
            return true;
        }
        
        return false;
    }

    private bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
               type == typeof(byte) || type == typeof(sbyte) || type == typeof(uint) ||
               type == typeof(ulong) || type == typeof(ushort) || type == typeof(float) ||
               type == typeof(double) || type == typeof(decimal);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    /// <summary>
    /// Replaces a parameter expression with any expression type.
    /// Used for inlining nested projections.
    /// </summary>
    private class ExpressionReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly Expression _replacement;

        public ExpressionReplacer(ParameterExpression oldParameter, Expression replacement)
        {
            _oldParameter = oldParameter;
            _replacement = replacement;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _replacement : base.VisitParameter(node);
        }
    }
}
