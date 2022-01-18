using System.Reflection;

namespace RestVerifier.AspNetCore;


//public abstract class RestVerifierEngine<TApp,T,TClient> : RestVerifierEngineBase<TClient> where TApp:WebApplicationVerifierBase<T> where T:class
//{

    
//    protected override async Task Invoke(Func<TClient, Task> action)
//    {
//        using (HttpClient client = CreateHttpClient())
//        {
//            var service = CreateClient(client);
//            await action(service);
//        }
//    }

//    protected abstract TClient CreateClient(HttpClient client);

//    protected virtual HttpClient CreateHttpClient()
//    {
//        var test = (WebApplicationVerifierBase<T>)Activator.CreateInstance(typeof(TApp), Validator)!;
//        return test.CreateClient();
//    }
//}