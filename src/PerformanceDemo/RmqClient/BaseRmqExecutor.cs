namespace RmqClient;

using System;
using RabbitMQ.Client;

public abstract class BaseRmqExecutor : IDisposable
{

	#region Fields: Private

	private IModel _channel;
	private readonly IConnection _connection;

	#endregion

	#region Constructors: Protected

	protected BaseRmqExecutor(IConnection connection) {
		_connection = connection;
	}

	#endregion

	#region Properties: Protected

	protected IModel Channel {
		get {
			if (_channel is { IsOpen: true }) {
				return _channel;
			}
			_channel = _connection.CreateModel();
			return _channel;
		}
	}

	#endregion

	#region Methods: Public

	public void Dispose() {
		_channel?.Dispose();
	}

	#endregion

}