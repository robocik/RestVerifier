using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;

namespace RestVerifier.Core.Interfaces;

public interface IMethodInvoker
{
    Task Invoke(MethodInfo methodInfo, MethodConfiguration methodConfig, object client, ExecutionContext context);
}