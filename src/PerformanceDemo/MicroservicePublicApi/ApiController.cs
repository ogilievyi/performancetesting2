namespace MicroservicePublicApi;

using log4net;
using Microsoft.AspNetCore.Mvc;
using RmqClient;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase
{

	#region Fields: Private

	private readonly Producer _producer;
	private readonly ILog _logger;

	#endregion

	#region Constructors: Public

	public ApiController(Producer producer, ILog  logger) {
		_producer = producer;
		_logger = logger;
	}

	#endregion

	#region Methods: Public

	[HttpGet("data/{id}")]
	public void GetAddress(string id) {
		try {
			_logger.Info($"Income request by Id = {id}");
			var dictionary = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
			dictionary["Id"] = id;
			_producer.Publish(dictionary);
			_logger.Info($"Income request by Id = {id} published");
		} catch (Exception e) {
			_logger.Error($"An error occurred while processing the request Id = {id}", e);
			throw;
		}
	}

	[HttpGet("ok")]
	public object GetOk() {
		return Ok(":)");
	}

	#endregion

}