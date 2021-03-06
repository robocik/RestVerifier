# ![RestVerifier logo](./docs/logoRV.png) RestVerifier

[![.NET](https://github.com/robocik/RestVerifier/actions/workflows/dotnet.yml/badge.svg)](https://github.com/robocik/RestVerifier/actions/workflows/dotnet.yml)
[![.NET](https://img.shields.io/nuget/v/RestVerifier)](https://www.nuget.org/packages/RestVerifier)

**RestVerifier** makes it very easy for developers to test and verify entire communication between Client application and the Server, especially you can easly check if all objects sent between Client &lt;--&gt; Server are serialized/deserialized correctly.

For now **RestVerifier** has an extension for **ASP.NET Core** therefore if you have in your project server application created in this technology, you can easy create integration tests to ensure that communication is handled correctly.

Of course if you use other communication technology (WCF, ASP.NET MVC etc), you can also use **RestVerifier** and create integration tests.

## How to start?

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [RestVerifier](https://www.nuget.org/packages/RestVerifier/) from the package manager console in you integration tests project:
```
PM> Install-Package RestVerifier
```
This package is a starting point. It includes main engine ([RestVerifier.Core](https://www.nuget.org/packages/RestVerifier.Core/)) and also comparer module ([RestVerifier.FluentAssertions](https://www.nuget.org/packages/RestVerifier.FluentAssertions/)), test data creator ([RestVerifier.AutoFixture](https://www.nuget.org/packages/RestVerifier.AutoFixture/))

If you have **ASP.NET Core** WebAPI, you should install also [RestVerifier.AspNetCore](https://www.nuget.org/packages/RestVerifier.AspNetCore/):
```
PM> Install-Package RestVerifier.AspNetCore
```

For NUnit tests:
```
PM> Install-Package RestVerifier.NUnit 
```

## Example

Sample project where **RestVerifier** is used:
https://github.com/robocik/NotesApp_V4_With_CommunicationTests

## How this works?

In many projects, we have WebAPI created in ASP.NET and uses REST and HTTP as a transport layer. On the client side we use HttpClient class to send request to the server. It is a good practice to separate code for this communication to the separate class. So this could looks similar to this:

**ASP.NET WebAPI**
```cs
[Route("api/[controller]")]
[ApiController]
public class NotesController: Controller
{
    [HttpGet]
    public async Task<IActionResult> GetNotes([FromQuery] GetNotesParam param)
    {
        var query = Mapper.Map<GetNotesQuery>(param);
        var retValue = await Mediator.Send(query).ConfigureAwait(false);
        return Ok(retValue);
    }
}
```

**Client side**
```cs
public class NoteDataService:DataServiceBase
{
    public async Task<PagedResult<NoteDto>> GetNotes(GetNotesParam param)
    {
        var url = GetUrl("api/notes", param);
            
        return await Execute(async httpClient =>
        {
             var res= await  httpClient.GetFromJsonAsync<PagedResult<NoteDto>>(url, CreateOptions()).ConfigureAwait(false);
             return res!;
         }).ConfigureAwait(false);
     }
}
```
The idea is simple. In our client app we use NodeDataService class to invoke WebAPI methods. This class should serialize parameters to JSON and also retrieve the response. In many cases this serialization process can be problematic. Classes sent as parameters must be created in the correct way - if not, they will not be transfered correctly.
Second problem is that ASP.NET binding mechanism requires that parameter names in Query string are the same what we have in our Controller. It would be nice to have unit tests which can verify if our NoteDataService class is created correctly and can send and receive all data.
RestVerifier is a library you can use to create such tests. It can automate most work related with them. In our test scenario, we could create:
- Test web application - it allows to run entire ASP.NET application in memory. Great for testing

```cs
public class TestWebApplicationFactory
    : RestVerifier.AspNetCore.CustomWebApplicationFactory<Startup>
{

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddScoped<ISession>(s =>
            {
                return new Mock<ISession>().Object;
            });
        });
        return base.CreateHost(builder);
    }    
}
```
- Test class
```cs
public class NoteDataServiceTests : RestVerifier.NUnit.TestCommunicationBase<NoteDataService>
{
    protected override void ConfigureVerifier(IGlobalSetupStarter<NoteDataService> builder)
    {
        builder.CreateClient(v =>
        {
            var service = new TestWebApplicationFactory();
            service.SetCompareRequestValidator(v);
            service.SkipAuthentication = true;
            var httpClient = service.CreateClient();
            var client = new NoteDataService(httpClient);
            return Task.FromResult(client);
        });
    }
}
```
This is all what you need to verify your communication layer. **RestVerifier** will check every method in NoteDataService: create test data and invoke it. On the WebAPI side it will validate if all properties and fields in every parameter has been sent correctly and then it create test return data and sent it back to the client. Then returned value is verified of course.

![Test result](./docs/restverifier_readme1.png) 

Of course this example is very easy. In real projects things much more difficult. But don't worry - **RestVerifier** is very flexible and you can customize your tests.

## Real world scenario
In one commercial project, we had Server application (Windows Service) uses WCF as a transport layer. In **WPF** application we had a class resposible for entire communication (let's call it NoteDataService from example above). In this class we had over 830 methods for invoking server operations. 
Recently we wanted to migrate this project to .NET 6. The new Server application should be created as **WebAPI** in **ASP.NET** and use **REST** and **HTTP** protocol. After many days of working, we have migrated all methods to HttpClient invocations. This was a great achievment but we weren't sure if this migration is created correctly. How do you know, if every object and every property has been serialized properly?
We have used RestVerifier to make this verification and guess what. We have found many many bugs in our communication layer:
- sometimes we use different parameter names on both sides
- sometimes we forgot to add [FromBody] attribute
- sometimes we forgot to change route URL
- sometimes we pass wrong class (for example with Properties returns interfaces instead of classes etc)
- sometimes we have pass custom structure but we didn't have custom model binder 
- sometimes we even forgot to migrate specific server method to ASP.NET

There are so many potential problems that can occure so having a good unit tests validating your communication layer is a must!

## Documentation

Small documentation you can find  in our [Wiki](https://github.com/robocik/RestVerifier/wiki/Documentation/) 
