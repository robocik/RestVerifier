namespace RestVerifier.AspNetCore;

//public abstract class StartupRestVerifierEngine<TApp,TStartup,TClient>: RestVerifierEngineBase<TClient> where TApp: CustomWebApplicationFactory<TStartup> where TStartup:class
//{
//    protected abstract TApp CreateWebApp();

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
//        var service = CreateWebApp();
//        return service.CreateClient();
//    }
//}