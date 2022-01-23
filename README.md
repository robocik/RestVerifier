# RestVerifier

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

