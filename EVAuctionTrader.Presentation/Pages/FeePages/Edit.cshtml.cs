using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVAuctionTrader.Presentation.Pages.FeePages;

[Authorize(Roles = "Admin")]
public sealed class EditModel : PageModel
{
    private readonly IFeeService _feeService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IFeeService feeService, ILogger<EditModel> logger)
    {
        _feeService = feeService;
        _logger = logger;
    }

    [BindProperty]
    public FeeRequestDto FeeRequest { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public List<SelectListItem> FeeTypes { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var fee = await _feeService.GetFeeByIdAsync(Id);

            if (fee == null)
            {
                TempData["ErrorMessage"] = "Fee not found.";
                return RedirectToPage("/FeePages/Index");
            }

            FeeRequest = new FeeRequestDto
            {
                Type = fee.Type,
                Amount = fee.Amount,
                Description = fee.Description
            };

            LoadFeeTypes();
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            TempData["ErrorMessage"] = "You don't have permission to edit fees.";
            return RedirectToPage("/FeePages/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading fee {FeeId}", Id);
            TempData["ErrorMessage"] = "An error occurred while loading the fee.";
            return RedirectToPage("/FeePages/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadFeeTypes();
            return Page();
        }

        try
        {
            var result = await _feeService.UpdateFeeAsync(Id, FeeRequest);

            if (result == null)
            {
                TempData["ErrorMessage"] = "Fee not found.";
                return RedirectToPage("/FeePages/Index");
            }

            TempData["SuccessMessage"] = $"Fee '{result.Type}' updated successfully.";
            return RedirectToPage("/FeePages/Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to update fee");
            ModelState.AddModelError(string.Empty, ex.Message);
            LoadFeeTypes();
            return Page();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid fee data");
            ModelState.AddModelError(string.Empty, ex.Message);
            LoadFeeTypes();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee {FeeId}", Id);
            ModelState.AddModelError(string.Empty, "An error occurred while updating the fee.");
            LoadFeeTypes();
            return Page();
        }
    }

    private void LoadFeeTypes()
    {
        FeeTypes = Enum.GetValues<FeeType>()
            .Select(ft => new SelectListItem
            {
                Value = ((int)ft).ToString(),
                Text = ft.ToString()
            })
            .ToList();
    }
}
