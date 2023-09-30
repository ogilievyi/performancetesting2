namespace MicroservicePublicApi;

using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RmqClient;

public class Startup
{

	#region Constructors: Public

	public Startup(IWebHostEnvironment env) {
		HostingEnvironment = env;
		Configuration = new ConfigurationBuilder()
			.SetBasePath(HostingEnvironment.ContentRootPath)
			.AddJsonFile("appsettings.json", true, true)
			.AddEnvironmentVariables()
			.Build();
	}

	#endregion

	#region Properties: Public

	public IConfiguration Configuration { get; set; }

	public IWebHostEnvironment HostingEnvironment { get; }

	#endregion

	#region Methods: Protected

	protected void InitLog4NetPath() {
		var repository = LogManager.CreateRepository("default");
		var fileName = Configuration["Log4NetPath"];
		if (string.IsNullOrEmpty(fileName)) {
			fileName = "log4net.config";
		}
		var configFile = new FileInfo(fileName);
		XmlConfigurator.Configure(repository, configFile);
	}

	#endregion

	#region Methods: Public

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
		InitLog4NetPath();
		app.UseSwagger();
		app.UseSwaggerUI(options => {
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
			options.RoutePrefix = string.Empty;
		});
		
		app.UseRouting();
		app.UseHttpsRedirection();
		app.UseEndpoints(e => e.MapControllers());
	}

	public void ConfigureServices(IServiceCollection services) {
		services.AddSingleton(Configuration);
		services.Configure<KestrelServerOptions>(o => { o.AllowSynchronousIO = true; });
		services.AddSingleton(new RmqConnectionProvider(Configuration).Connection);
		services.AddSingleton<Producer>();
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();
		services.AddLogging(c => {
			c.ClearProviders();
			c.AddLog4Net();
		});
		var log = LogManager.GetLogger(GetType());
		services.AddSingleton<ILog>(log);

	}

	#endregion

}