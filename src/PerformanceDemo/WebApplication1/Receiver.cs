namespace WebApplication1;

using Microsoft.Data.SqlClient;
using System.Text;
using log4net;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Worker;

[ApiController]
public class ReceiverController : ControllerBase
{

	#region Fields: Private

	private readonly IConfiguration _configuration;
	private readonly ShadowProcessor _shadowProcessor;
	private readonly ILog _logger;

	#endregion

	#region Constructors: Public

	public ReceiverController(IConfiguration configuration, ShadowProcessor shadowProcessor, ILog logger) {
		_logger = logger;
		_configuration = configuration;
		_shadowProcessor = shadowProcessor;
	}

	#endregion

	#region Methods: Public

	[HttpPost("receive")]
	public ObjectResult Receive() {
		_logger.Debug("Message received!");
		using var reader = new StreamReader(Request.Body, Encoding.UTF8, true, 1024, true);
		var json = reader.ReadToEndAsync().Result;
		try {
			var connectionString = _configuration.GetConnectionString("db");
			using var connection = new SqlConnection(connectionString);
			connection.Open();
			var command = connection.CreateCommand();
			command.CommandText = $"insert into raw (json) values('{json}')";
			command.ExecuteNonQuery();
			_shadowProcessor.NewDataAvailable.Set();
			return Ok("");
		} catch (Exception e) {
			_logger.Error("Saving message failed", e);
			return BadRequest(e);
		}
	}

	#endregion

}