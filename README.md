# ![image info](./RestVerifier/logoRV.png) RestVerifier

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
This is all what you need to verify your communication layer. RestVerifier will check every method in NoteDataService: create test data and invoke it. On the WebAPI side it will validate if all properties and fields in every parameter has been sent correctly and then it create test return data and sent it back to the client. Then returned value is verified of course.
Of course this example is very easy. In real projects things much more difficult. But don't worry - RestVerifier is very flexible and you can customize your tests.

## Real world scenario
In one commercial project, we had Server application (Windows Service) uses WCF as a transport layer. In WPF application we had a class resposible for entire communication (let's call it NoteDataService from example above). In this class we had over 830 methods for invoking server operations. 
Recently we wanted to migrate this project to .NET 6. The new Server application should be created as WebAPI in ASP.NET and use REST and HTTP protocol. After many days of working, we have migrated all methods to HttpClient invocations. This was a great achievment but we weren't sure if this migration is created correctly. How do you know, if every object and every property has been serialized properly?
We have used RestVerifier to make this verification and guess what. We have found many many bugs in our communication layer:
- sometimes we use different parameter names on both sides
- sometimes we forgot to add [FromBody] attribute
- sometimes we forgot to change route URL
- sometimes we pass wrong clas (for example with Properties returnes interfaces instead of classes etc)
- sometimes we have pass custom structure but we didn't have custom model binder 
- sometimes we even forgot to migrate specific server method to ASP.NET

There are so many potential problems that can occure so having a good unit tests validating your communication layer is a must!

## How to customize tests?

### Specify methods to tests
By default RestVerifier will check every public method in your class. Methods from base classes will be skipped. You can change this scope with GetMethods

```cs
protected override void ConfigureVerifier(IGlobalSetupStarter<FileDataService> builder)
{
   builder.GetMethods(type =>
   {
        return type.GetMethods(BindingFlags.Public);
   });
}
```
### Skip method
If you don't want to verify (invoke) specific method you can do this:
```cs
protected override void ConfigureVerifier(IGlobalSetupStarter<FileDataService> builder)
{
    builder.ConfigureSetup(x =>
    {
        x.Setup(g => g.DeleteFile(Behavior.Generate<Guid>())).Skip();
    });
}
In this case RestVerifier will check all methods returned by GetMethods except DeleteFile.

### Specify test values
By default, RestVerifier generates random (test) data for every method parameters. If you want to invoke a method with specific values, you can:

```cs
protected override void ConfigureVerifier(IGlobalSetupStarter<FileDataService> builder)
{
   builder.ConfigureSetup(x =>
   {
        x.Setup(g => g.UploadAvatarFull(Behavior.Generate<UploadFileParam>(), new MemoryStream()));
   });
}
```
In this example first value will be generated but second is specified as an empty MemoryStream;

### Ignore parameter
RestVerifier generate a value for every parameter. Then on the ASP.NET side it will verify if all transfered parameters are equals. Basically if your client method has 2 parameters, then RestVerifier expects that in your controller you will also have two parameters. But sometimes this is not the case. You can inform RestVerifier that specific parameter should be ignore (it will not be verified on the server side):
For example:
Client side
```cs
Task UploadAvatarFull(UploadFileParam fileParam, Stream fileContent);
```

ASP.NET
```cs
[HttpPost("uploadAvatarFull")]
public async Task<IActionResult> UploadAvatarFull([FromBody]UploadFileParam? meta )
```

Only first parameter is send to the server and should be check. Second (Stream) should be ignored. Here is a configuration:
    
```cs
protected override void ConfigureVerifier(IGlobalSetupStarter<FileDataService> builder)
{
    builder.ConfigureVerify(x =>
    {
        x.Verify(g => g.UploadAvatarFull(Behavior.Verify<UploadFileParam>(), Behavior.Ignore<Stream>()));
    });
}
```
### Simple parameter transformation
In many cases in client code we use complex type as a parameter value, but we send a single value to the Server (for example ID).

**Client side**
```cs
Task DeletePerson(PersonDTO person);
```

**ASP.NET**
```cs
[HttpDelete("deletePerson")]
public async Task<IActionResult> DeletePerson(Guid id )
```

**Configuration**
```cs
protected override void ConfigureVerifier(IGlobalSetupStarter<NoteDataService> builder)
{
   builder.ConfigureVerify(x =>
   {
       x.Verify(v => v.DeletePerson(Behavior.Transform<PersonDTO>(o => o.Id)));
   });
}
```

### Complex parameters transformation
Sometimes on the client side we have a few parameters but on the Server side we have one class with properties representing all those parameters.

**Client side**
```cs
Task UploadAvatarFull(UploadFileParam fileParam, Stream fileContent)
```

**ASP.NET**
```cs
public class UploadAvatarParameter
{
    public Stream? File { get; set; }
    public UploadFileParam? Meta { get; set; }
}
    
[HttpPost("uploadAvatarFull")]
public async Task<IActionResult> UploadAvatarFull(UploadAvatarParameter uploadParam)
```

**Configuration**
```cs
protected override void ConfigureVerifier(IGlobalSetupStarter<FileDataService> builder)
{
    builder.ConfigureVerify(cons =>
    {
        cons.Verify(g => g.UploadAvatarFull(Behavior.Verify<UploadFileParam>(), Behavior.Verify<Stream>()))
        .Transform<UploadFileParam,Stream>((p1, p2) =>
              {
                var param = new UploadAvatarParameter()
                {
                   Meta = p1,
                   File = p2
              };
              return new[] { param };
         });

    });
 }
```
In this example we inform RestVerifier to create an instance of UploadAvatarParameter class and fill it with the client parameters;

### Transform return value
There are cases when your WebAPI return some value and than your client code convert it to another type. In this case, we need to inform RestVerify, how to deal with this:

**Client side**
```cs
Task<Guid> UploadFile(FileMetaData fileParam, Stream fileContent);
```

**ASP.NET**
```cs
[HttpPost]
public async Task<IActionResult> UploadFile([FromBody] FileMetaData? uploadParam)
{
     return new FileAccessToken("url", "token", Guid.NewGuid());
}
```

**Configuration**
```cs
builder.ConfigureVerify(cons =>
{
    cons.Verify(g => g.UploadFile(Behavior.Verify<FileMetaData>(), Behavior.Ignore<Stream>()))
        .Returns<Guid>(g =>
    {
        var token = new FileAccessToken("blob url", "test token",g);
        return token;
    });
});
```

WebAPI returns type FileAccessToken but client code Guid only. In the configuration, we define, how to convert one type to another.

### Global parameters transformation
If you have many client methods where we should ignore the same parameter type, we can configure this globally.

**Client side**
```cs
Task<Guid> TestMethod1(ManualDTO manual,FileMetaData fileParam);

Task<string> TestMethod2(ManualDTO manual,PersonDTO person,DateTime date);
```

**ASP.NET**
```cs
[HttpPost]
public async Task<IActionResult> TestMethod1([FromBody] FileMetaData? uploadParam)
{
     ...
}
[HttpPost]
public async Task<IActionResult> TestMethod2([FromBody] PersonDTO person,[FromQuery]DateTime date)
{
     ...
}
```

**Configuration**
```cs
builder.ConfigureVerify(cons =>
{
    config.Transform((paramInfo, paramValue) =>
    {
         if (paramValue.Value is ManualDTO)
         {
              paramValue.Ignore = true;
         }
    });
});
```
In this example we have two methods with ManualDTO parameter. Do need to ignore them in verification process. We can do this globally (per value Type)

### Global return value transformations

If we have many WebAPI controllers which returns a value with specific type and then on the client code we convert this value to another type, we can inform RestVerifier globally how to deal with this situation

**Client side**
```cs
Task<Stream> TestMethod1(ManualDTO manual,FileMetaData fileParam);

Task<Stream> TestMethod2(ManualDTO manual,PersonDTO person,DateTime date);
```

**ASP.NET**
```cs
[HttpGet]
public async Task<IActionResult> TestMethod1(Guid id)
{
     return File(retValue.Content, retValue.MimeType,retValue.FileName);
}
[HttpGet]
public async Task<IActionResult> TestMethod2(Guid id,DateTime date)
{
     return File(retValue.Content, retValue.MimeType,retValue.FileName);
}
```

**Configuration**
```cs
builder.ConfigureVerify(cons =>
{
    config.ReturnTransform<Stream>(b =>
    {
        var memory = (MemoryStream)b.Content;
        var newMemory = new MemoryStream();
        memory.CopyTo(newMemory);
        newMemory.Position = 0;
        memory.Position = 0;
        return new FileStreamResult(memory, b.MimeType)
        {
            FileDownloadName = b.FileName
        };
    });
});
```
In this case, our controller methods return FileStreamResult but client methods return Stream. In configuration we add a transformation for this situation and it will be used automatically for every method returns Stream
