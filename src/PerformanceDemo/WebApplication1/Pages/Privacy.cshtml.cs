namespace WebApplication1.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class PrivacyModel : PageModel
{

	#region Fields: Private

	private readonly ILogger<PrivacyModel> _logger;

	#endregion

	#region Constructors: Public

	public PrivacyModel(ILogger<PrivacyModel> logger) {
		_logger = logger;
	}

	#endregion

	#region Methods: Public

	public void OnGet() {
	}

	#endregion

}