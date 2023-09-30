namespace WebApplication1.Pages;

using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{

	#region Fields: Private

	private readonly ILogger<IndexModel> _logger;

	#endregion

	#region Constructors: Public

	public IndexModel(ILogger<IndexModel> logger) {
		_logger = logger;
	}

	#endregion

	#region Methods: Public

	public void OnGet() {
	}

	#endregion

}