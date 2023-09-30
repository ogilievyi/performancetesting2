namespace WebApplication1.Worker;

using System.Data.SqlClient;
using System.Text.Json;

public class ShadowProcessor
{

	#region Fields: Private

	private readonly string _connectionString;
	private int _batchCount;
	private readonly TimeSpan _timeout;

	#endregion

	#region Constructors: Public

	public ShadowProcessor(IConfiguration configuration) {
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
		using var connection = new SqlConnection(_connectionString);
		connection.Open();
		using var command = connection.CreateCommand();
		command.CommandText = sql;
		command.ExecuteNonQuery();
	}

	private void InsertResults(IEnumerable<Person> people) {
		var sql = "Insert into Person (FirstName, LastName, Gender, BirthDate) values ('{0}', '{1}', '{2}', '{3}')";
		foreach (var person in people) {
			//так, це тупо. Можна значно швидше ;)
			var finalSql = string.Format(sql, person.FirstName, person.LastName, person.Gender,
				person.BirthDate.Date.ToString("yyyy-MM-dd"));
			ExecuteNonQuery(finalSql);
		}
	}

	private List<RawRecord> ReadRaw() {
		using var connection = new SqlConnection(_connectionString);
		connection.Open();
		using var command = connection.CreateCommand();
		command.CommandText =
			$"select top {_batchCount} Id, json, status from raw where status = {(int)RawRecordStatus.New}";
		using var reader = command.ExecuteReaderAsync().Result;
		var result = new List<RawRecord>();
		while (reader.Read()) {
			var raw = new RawRecord {
				Id = int.Parse(reader[nameof(RawRecord.Id)].ToString() ?? "0"),
				Status = (RawRecordStatus)int.Parse(reader[nameof(RawRecord.Status)].ToString() ?? "0"),
				Json = reader[nameof(RawRecord.Json)].ToString()
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
		var sql = "Update Raw set Status = {0} where Id = {1}";
		foreach (var rawRecord in rawRecords) {
			//так, це тупо. Можна значно швидше ;)
			var finalSql = string.Format(sql, (int)status, rawRecord.Id);
			ExecuteNonQuery(finalSql);
		}
	}

	#endregion

	#region Methods: Public

	public void OnStartHandle() {
		var task = new Task(() => {
			while (true) {
				NewDataAvailable.Wait();
				try {
					var rawRecords = ReadRaw();
					if(rawRecords == null || !rawRecords.Any()) {
						NewDataAvailable.Reset();
						continue;
					}
					var parsedData = TryParse(rawRecords, out var failed);
					InsertResults(parsedData);
					UpdateRaw(rawRecords, RawRecordStatus.Parsed);
					UpdateRaw(failed, RawRecordStatus.Failed);
				} catch (Exception e) {
					Console.WriteLine(e);
					throw;
				}
				
				Thread.Sleep(_timeout);
			}
		});
		task.Start();
	}

	#endregion

}