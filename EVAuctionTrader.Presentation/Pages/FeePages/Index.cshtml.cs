using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.FeePages;

[Authorize(Roles = "Admin")]
public sealed class IndexModel : PageModel
{
    private readonly IFeeService _feeService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IFeeService feeService, ILogger<IndexModel> logger)
    {
        _feeService = feeService;
        _logger = logger;
    }

    public Pagination<FeeResponseDto>? Fees { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Fees = await _feeService.GetAllFeesAsync(PageNumber, PageSize);
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to fee management");
            TempData["ErrorMessage"] = "You don't have permission to access this page.";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading fees");
            TempData["ErrorMessage"] = "An error occurred while loading fees.";
            Fees = new Pagination<FeeResponseDto>(new List<FeeResponseDto>(), 0, 1, 10);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var result = await _feeService.DeleteFeeAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Fee deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete fee. Fee may not exist.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to delete fees.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fee {FeeId}", id);
            TempData["ErrorMessage"] = "An error occurred while deleting the fee.";
        }

        return RedirectToPage(new { pageNumber = PageNumber, pageSize = PageSize });
    }
}
