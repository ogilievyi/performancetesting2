namespace RmqClient;

using System;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

public class RmqConnectionProvider
{

	#region Fields: Private

	private IConnection _connection;
	private ConnectionFactory _connectionFactory;
	private readonly IConfiguration _configuration;

	#endregion

	#region Constructors: Public

	public RmqConnectionProvider(IConfiguration configuration) {
		_configuration = configuration;
	}

	#endregion

	#region Properties: Private

	private ConnectionFactory ConnectionFactory {
		get {
			if (_connectionFactory != null) {
				return _connectionFactory;
			}
			var uriString = _configuration.GetConnectionString("rmq");
			_connectionFactory = new ConnectionFactory {
				Uri = new Uri(uriString)
			};
			return _connectionFactory;
		}
	}

	#endregion

	#region Properties: Public

	public IConnection Connection {
		get {
			if (_connection is { IsOpen: true }) {
				return _connection;
			}
			_connection = ConnectionFactory.CreateConnection();
			return _connection;
		}
	}

	#endregion

}