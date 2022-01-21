using System;
using System.Threading.Tasks;

namespace RestVerifier.Core;

public static class ReflectionExtensions
{
    public static bool IsVoid(this Type? type)
    {
        return type == null || type == typeof(void) || type == typeof(Task);
    }
}