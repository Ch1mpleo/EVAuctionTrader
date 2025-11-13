using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;
using EVAuctionTrader.BusinessObject.Enums;
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

    public FeeResponseDto? VipFee { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Get VIP Post Fee specifically
            VipFee = await _feeService.GetFeeByTypeAsync(FeeType.VipPostFee);
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
            _logger.LogError(ex, "Error loading VIP fee");
            TempData["ErrorMessage"] = "An error occurred while loading the VIP fee.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateFeeAsync(Guid feeId, decimal amount, string description)
    {
        try
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "Fee amount must be greater than zero.";
                return RedirectToPage();
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Description is required.";
                return RedirectToPage();
            }

            var feeRequest = new FeeRequestDto
            {
                Type = FeeType.VipPostFee,
                Amount = amount,
                Description = description.Trim()
            };

            var result = await _feeService.UpdateFeeAsync(feeId, feeRequest);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Fee not found.";
            }
            else
            {
                TempData["SuccessMessage"] = $"VIP fee updated successfully to ${result.Amount:N2}.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to update fees.";
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee data");
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee {FeeId}", feeId);
            TempData["ErrorMessage"] = "An error occurred while updating the fee.";
        }

        return RedirectToPage();
    }
}
