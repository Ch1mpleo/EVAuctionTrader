using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EVAuctionTrader.Presentation.Pages.FeePages;

[Authorize(Roles = "Admin")]
public sealed class CreateModel : PageModel
{
    private readonly IFeeService _feeService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IFeeService feeService, ILogger<CreateModel> logger)
    {
        _feeService = feeService;
        _logger = logger;
    }

    [BindProperty]
    public FeeRequestDto FeeRequest { get; set; } = new();

    public List<SelectListItem> FeeTypes { get; set; } = new();

    public IActionResult OnGet()
    {
        LoadFeeTypes();
        return Page();
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
            var result = await _feeService.CreateFeeAsync(FeeRequest);
            TempData["SuccessMessage"] = $"Fee '{result.Type}' created successfully with amount ${result.Amount:N0}.";
            return RedirectToPage("/FeePages/Index");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create fee");
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
            _logger.LogError(ex, "Error creating fee");
            ModelState.AddModelError(string.Empty, "An error occurred while creating the fee.");
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
