namespace WebApplication1.Pages;

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{

	#region Fields: Private

	private readonly ILogger<ErrorModel> _logger;

	#endregion

	#region Constructors: Public

	public ErrorModel(ILogger<ErrorModel> logger) {
		_logger = logger;
	}

	#endregion

	#region Properties: Public

	public string? RequestId { get; set; }

	public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

	#endregion

	#region Methods: Public

	public void OnGet() {
		RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
	}

	#endregion

}