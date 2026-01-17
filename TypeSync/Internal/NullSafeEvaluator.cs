using System.Linq.Expressions;

namespace TypeSync.Internal;

/// <summary>
/// Provides null-safe evaluation of expressions with property access chains.
/// This prevents NullReferenceException from being thrown when intermediate 
/// properties in a chain are null (e.g., src.Section.Class.Name when Section is null).
/// </summary>
internal static class NullSafeEvaluator
{
    /// <summary>
    /// Evaluates an expression safely, returning default if any part of the chain is null.
    /// </summary>
    public static TResult? Evaluate<TSource, TResult>(Expression<Func<TSource, TResult>> expression, TSource source)
    {
        if (source == null) return default;
        
        // Get the member access chain from the expression
        var memberChain = GetMemberAccessChain(expression.Body);
        
        if (memberChain.Count == 0)
        {
            // No member chain found, just compile and execute
            // This handles simple expressions like src => src.Name
            try
            {
                var compiled = expression.Compile();
                return compiled(source);
            }
            catch
            {
                return default;
            }
        }
        
        // Walk the chain and check for nulls at each step
        object? current = source;
        
        foreach (var member in memberChain)
        {
            if (current == null) return default;
            
            try
            {
                if (member is System.Reflection.PropertyInfo prop)
                {
                    current = prop.GetValue(current);
                }
                else if (member is System.Reflection.FieldInfo field)
                {
                    current = field.GetValue(current);
                }
                else if (member is MethodCallInfo methodCall)
                {
                    // Handle method calls like FirstOrDefault()
                    current = InvokeMethod(current, methodCall);
                }
            }
            catch
            {
                return default;
            }
        }
        
        // Final result
        if (current == null) return default;
        
        if (current is TResult result)
        {
            return result;
        }
        
        // Try conversion
        try
        {
            return (TResult)Convert.ChangeType(current, typeof(TResult));
        }
        catch
        {
            return default;
        }
    }
    
    private static List<object> GetMemberAccessChain(Expression expression)
    {
        var chain = new List<object>();
        
        while (expression != null)
        {
            switch (expression)
            {
                case MemberExpression memberExpr:
                    chain.Insert(0, memberExpr.Member);
                    expression = memberExpr.Expression!;
                    break;
                    
                case MethodCallExpression methodCallExpr:
                    // Handle methods like FirstOrDefault(), ToString(), etc.
                    var methodInfo = new MethodCallInfo
                    {
                        Method = methodCallExpr.Method,
                        Arguments = methodCallExpr.Arguments.Select(EvaluateConstantExpression).ToArray()
                    };
                    chain.Insert(0, methodInfo);
                    
                    // Continue with the object the method is called on
                    if (methodCallExpr.Object != null)
                    {
                        expression = methodCallExpr.Object;
                    }
                    else if (methodCallExpr.Arguments.Count > 0)
                    {
                        // Extension method - first argument is the 'this' parameter
                        expression = methodCallExpr.Arguments[0];
                        methodInfo.IsExtensionMethod = true;
                    }
                    else
                    {
                        expression = null!;
                    }
                    break;
                    
                case UnaryExpression unaryExpr when unaryExpr.NodeType == ExpressionType.Convert:
                    expression = unaryExpr.Operand;
                    break;
                    
                case ParameterExpression:
                    // Reached the source parameter, we're done
                    return chain;
                    
                default:
                    // Unknown expression type, fall back to compiled execution
                    return new List<object>();
            }
        }
        
        return chain;
    }
    
    private static object? EvaluateConstantExpression(Expression expr)
    {
        try
        {
            // Handle lambda expressions (e.g., x => x.Price) - compile them for use with LINQ methods
            if (expr is LambdaExpression lambdaExpr)
            {
                return lambdaExpr.Compile();
            }
            
            // Handle UnaryExpression wrapping a lambda (e.g., Quote expressions)
            if (expr is UnaryExpression unaryExpr && unaryExpr.Operand is LambdaExpression quotedLambda)
            {
                return quotedLambda.Compile();
            }
            
            var lambda = Expression.Lambda<Func<object>>(Expression.Convert(expr, typeof(object)));
            var compiled = lambda.Compile();
            return compiled();
        }
        catch
        {
            return null;
        }
    }
    
    private static object? InvokeMethod(object? target, MethodCallInfo methodCall)
    {
        try
        {
            if (methodCall.IsExtensionMethod)
            {
                // For extension methods, pass target as first argument
                var args = new object?[] { target }.Concat(methodCall.Arguments.Skip(1)).ToArray();
                return methodCall.Method.Invoke(null, args);
            }
            else
            {
                return methodCall.Method.Invoke(target, methodCall.Arguments);
            }
        }
        catch
        {
            // Handle exceptions from methods like Min/Max/Sum on empty sequences
            return null;
        }
    }
    
    private class MethodCallInfo
    {
        public System.Reflection.MethodInfo Method { get; set; } = null!;
        public object?[] Arguments { get; set; } = Array.Empty<object?>();
        public bool IsExtensionMethod { get; set; }
    }
}
