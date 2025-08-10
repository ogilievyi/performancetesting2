namespace WebApplication1.Worker;

using log4net;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.Json;

public class ShadowProcessor
{

	#region Fields: Private

	private readonly string _connectionString;
	private int _batchCount;
	private readonly TimeSpan _timeout;
	private readonly ILog _logger;

	#endregion

	#region Constructors: Public

	public ShadowProcessor(IConfiguration configuration, ILog logger) {
		_logger = logger;
		_logger.Info("ShadowProcessor created");
        _batchCount = int.Parse(configuration[$"{nameof(_batchCount)}"]);
		var timeoutSecond = int.Parse(configuration["timeoutSecond"]);
		_timeout = TimeSpan.FromSeconds(timeoutSecond);
		_connectionString = configuration.GetConnectionString("db");
		OnStartHandle();
	}

	#endregion

	#region Properties: Public

	public ManualResetEventSlim NewDataAvailable { get; } = new(false);

	#endregion

	#region Methods: Private

	private void ExecuteNonQuery(string sql) {
		if (string.IsNullOrEmpty(sql)) {
			return;
		}
		using var connection = new SqlConnection(_connectionString);
		connection.Open();
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		command.ExecuteNonQuery();
	}

	private void InsertResults(IEnumerable<Person> people) {
		var sql = "Insert into Person (FirstName, LastName, Gender, BirthDate) values ('{0}', '{1}', '{2}', '{3}');";
		var builder = new StringBuilder();
		foreach(var person in people) {
			//так, це тупо. Можна значно швидше ;)
			var finalSql = string.Format(sql, person.FirstName, person.LastName, person.Gender,
				person.BirthDate.Date.ToString("yyyy-MM-dd"));
			builder.Append(finalSql);

		}
		ExecuteNonQuery(builder.ToString());

    }

    private List<RawRecord> ReadRaw() {
		using var connection = new SqlConnection(_connectionString);
		connection.Open();
		using var command = connection.CreateCommand();
		command.CommandText =
			$"select top {_batchCount} Id, json, status from raw (nolock) where status = {(int)RawRecordStatus.New}";
		using var reader = command.ExecuteReaderAsync().Result;
		var result = new List<RawRecord>();
		while (reader.Read()) {
			var raw = new RawRecord {
				Id = int.Parse(reader["Id"].ToString() ?? "0"),
				Status = (RawRecordStatus)int.Parse(reader["Status"].ToString() ?? "0"),
				Json = reader["Json"].ToString()
			};
			result.Add(raw);
		}
		return result;
	}

	private IEnumerable<Person> TryParse(IEnumerable<RawRecord> rawRecords, out IEnumerable<RawRecord> failed) {
		var failedRecords = new List<RawRecord>();
		var parsedRecords = new List<Person>();
		foreach (var rawRecord in rawRecords) {
			try {
				var person = JsonSerializer.Deserialize<Person>(rawRecord.Json);
				rawRecord.Status = RawRecordStatus.Parsed;
				if (person != null) {
					parsedRecords.Add(person);
				}
			} catch {
				rawRecord.Status = RawRecordStatus.Failed;
				failedRecords.Add(rawRecord);
			}
		}
		failed = failedRecords;
		return parsedRecords;
	}

	private void UpdateRaw(IEnumerable<RawRecord> rawRecords, RawRecordStatus status) {
		var sql = "Update Raw set Status = {0} where Id = {1};";
		var builder = new StringBuilder();
		foreach (var rawRecord in rawRecords) {
			//так, це тупо. Можна значно швидше ;)
			var finalSql = string.Format(sql, (int)status, rawRecord.Id);
			builder.Append(finalSql);

		}
		ExecuteNonQuery(builder.ToString());

    }

    #endregion

    #region Methods: Public

    public void OnStartHandle() {
		_logger.Info($"OnStartHandle in with parameters batchCount {_batchCount}, timeout {_timeout}!");
        var task = new Thread(() => {
			_logger.Info("Shadow task run!");

            while(true) {
				NewDataAvailable.Wait();
                try {
					var rawRecords = ReadRaw();
					_logger.Info($"Found new {rawRecords.Count} raw records");

                    if(rawRecords == null || !rawRecords.Any()) {
						NewDataAvailable.Reset();
						continue;
					}
					var parsedData = TryParse(rawRecords, out var failed);
					_logger.Info($"Parsed {parsedData.Count()}  failed {failed.Count()}");
                    InsertResults(parsedData);
					UpdateRaw(rawRecords, RawRecordStatus.Parsed);
					UpdateRaw(failed, RawRecordStatus.Failed);
					_logger.Info($"Batch processed");
				} catch (Exception e) {
					_logger.Error("Shadow task failed!", e);
				}
                Thread.Sleep(_timeout);
			}
		});
		task.Start();
	}

	#endregion

}