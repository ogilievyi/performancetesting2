using MicroserviceWorker;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

var builder = WebHost.CreateDefaultBuilder(args);
builder.UseStartup<Startup>();
var app = builder.Build();
app.Run();