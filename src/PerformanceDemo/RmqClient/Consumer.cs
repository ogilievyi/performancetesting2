namespace RmqClient;

using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public abstract class Consumer : BaseRmqExecutor
{

	#region Fields: Protected

	protected readonly IConfiguration _configuration;

	#endregion

	#region Constructors: Protected

	protected Consumer(IConfiguration configuration, IConnection connection)
		: base(connection) {
		_configuration = configuration;
		Receive();
	}

	#endregion

	#region Methods: Private

	private void Receive() {
		Channel.ConfirmSelect();
		var queue = _configuration["RmqQueueName"];
		var routingKey = _configuration["RoutingKey"];
		Channel.ExchangeDeclare(queue, ExchangeType.Direct);
		Channel.QueueDeclare(queue,
			true,
			false,
			false,
			null);
		Channel.QueueBind(queue,
			queue,
			routingKey);
		var consumer = new EventingBasicConsumer(Channel);
		consumer.Received += (model, ea) => {
			var body = ea.Body.ToArray();
			var message = Encoding.UTF8.GetString(body);
			Receive(message, ea.DeliveryTag);
		};
		Channel.BasicConsume(queue, false, consumer);
	}

	#endregion

	#region Methods: Protected

	protected abstract void Receive(string message, ulong deliveryTag);

	#endregion

}