namespace RmqClient;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

public class Producer : BaseRmqExecutor
{

	#region Fields: Private

	private readonly IConfiguration _configuration;

	#endregion

	#region Constructors: Public

	public Producer(IConfiguration configuration, IConnection connection)
		: base(connection) {
		_configuration = configuration;
	}

	#endregion

	#region Methods: Private

	private void Publish(byte[] body) {
		var routingKey = _configuration["RoutingKey"];
		var exchange = _configuration["Exchange"];
		Channel.BasicPublish(exchange,
			routingKey,
			null,
			body);
	}

	#endregion

	#region Methods: Public

	public void Publish(object obj) {
		var message = JsonSerializer.Serialize(obj);
		Publish(message);
	}

	public void Publish(string message) {
		var body = Encoding.UTF8.GetBytes(message);
		Publish(body);
	}

	#endregion

}