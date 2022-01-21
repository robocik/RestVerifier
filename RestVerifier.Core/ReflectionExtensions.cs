using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestVerifier.Core;

public static class ReflectionExtensions
{
    public static bool IsVoid(this Type? type)
    {
        return type == null || type == typeof(void) || type == typeof(Task);
    }

    public static Type GetTypeWithoutTask( this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
        {
            type = type.GetGenericArguments().First();
        }

        return type;
    }
}