namespace WebApplication1.Worker;

internal class RawRecord
{

	#region Properties: Public

	public int Id { get; set; }

	public string Json { get; set; }

	public RawRecordStatus Status { get; set; }

	#endregion

}