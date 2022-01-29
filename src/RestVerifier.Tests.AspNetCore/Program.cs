using M6T.Core.TupleModelBinder;
using Microsoft.AspNetCore.HttpLogging;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
});
builder.Services.AddControllers(options =>
{

}).AddNewtonsoftJson(o =>
{
    o.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
    o.SerializerSettings.Formatting = Formatting.Indented;
    o.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
    o.SerializerSettings.ObjectCreationHandling = ObjectCreationHandling.Replace;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpLogging();
app.UseAuthorization();

app.MapControllers();

app.Run();
