using log4net;
using WebApplication1.Worker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddLogging(c => {
	c.ClearProviders();
	c.AddLog4Net();
});
var log = LogManager.GetLogger("default");
builder.Services.AddSingleton<ILog>(log);
builder.Services.AddSingleton<ShadowProcessor>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.MapControllers();
app.UseRouting();
app.MapRazorPages();
app.UseStaticFiles();
app.Run();

 