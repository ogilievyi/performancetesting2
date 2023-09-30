namespace MicroserviceWorker;

using System.Data.SqlClient;
using System.Text.Json;
using log4net;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RmqClient;

public class DemoConsumer : Consumer
{

	#region Fields: Private

	private static readonly HttpClient _client = new();
	private readonly ILog _logger;

	#endregion

	#region Constructors: Public

	public DemoConsumer(IConfiguration configuration, IConnection connection, ILog logger)
		: base(configuration, connection) {
		_logger = logger;
		var prefetchCount = ushort.Parse(configuration["PrefetchCount"]);
		Channel.BasicQos(0, prefetchCount, true);
		_logger.Info($"{nameof(DemoConsumer)} started with PrefetchCount {prefetchCount}");
	}

	#endregion

	#region Methods: Private

	private Uri GetAccountUri(string id) {
		var connectionString = _configuration.GetConnectionString("db");
		using var connection = new SqlConnection(connectionString);
		connection.Open();
		using var command = connection.CreateCommand();
		command.CommandText = $"select Uri from Account where Id = '{id}'";
		using var reader = command.ExecuteReaderAsync().Result;
		while (reader.Read()) {
			var uriStr = reader["Uri"].ToString();
			Uri.TryCreate($"{uriStr}/receive", UriKind.RelativeOrAbsolute, out var uri);
			return uri;
		}
		return null;
	}

	private void SendMessage(string message, Uri uri) {
		var content = new StringContent(message);
		var response = _client.PostAsync(uri, content).Result;
		response.EnsureSuccessStatusCode();
	}

	#endregion

	#region Methods: Protected

	protected override void Receive(string message, ulong deliveryTag) {
		_logger.Debug($"Received message {message}");
		try {
			var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(message);
			if (dict != null) {
				var id = dict["Id"];
				var uri = GetAccountUri(id);
				SendMessage(message, uri);
			}
			Channel.BasicAck(deliveryTag, false);
		} catch (Exception e) {
			_logger.Error("Something went wrong", e);
			Channel.BasicNack(deliveryTag, false, true);
		}
	}

	#endregion

}