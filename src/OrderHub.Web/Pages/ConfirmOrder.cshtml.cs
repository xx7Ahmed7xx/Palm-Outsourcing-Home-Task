using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Core.Abstractions;
using OrderHub.Core.Models;

namespace OrderHub.Web.Pages;

public class ConfirmOrderModel(IConfirmOrderRepository confirmOrderRepository) : PageModel
{
  private const int DemoSchoolId = 1;

  public string SchoolName { get; private set; } = string.Empty;

  [BindProperty]
  public List<ConfirmOrderLineViewModel> Lines { get; set; } = [];

  public decimal Subtotal => Lines.Sum(line => line.UnitPrice * line.Quantity);

  [TempData]
  public string? ConfirmationMessage { get; set; }

  public async Task<IActionResult> OnGetAsync()
  {
    await LoadDraftAsync();
    return Page();
  }

  [ValidateAntiForgeryToken]
  public async Task<IActionResult> OnPostConfirmAsync()
  {
    var draft = await confirmOrderRepository.GetDraftAsync(DemoSchoolId);
    if (draft is null)
    {
      return NotFound();
    }

    SchoolName = draft.SchoolName;

    foreach (var line in Lines)
    {
      line.Quantity = Math.Max(1, line.Quantity);
    }

    await confirmOrderRepository.UpdateQuantitiesAsync(
      DemoSchoolId,
      Lines.Select(line => new ConfirmOrderLineUpdate { LineId = line.Id, Quantity = line.Quantity }).ToList());

    ConfirmationMessage = $"Order confirmed — subtotal £{Subtotal:0.00}.";
    await LoadDraftAsync();
    return Page();
  }

  private async Task LoadDraftAsync()
  {
    var draft = await confirmOrderRepository.GetDraftAsync(DemoSchoolId)
      ?? throw new InvalidOperationException("Demo order draft was not seeded.");

    SchoolName = draft.SchoolName;
    Lines = draft.Lines.Select(line => new ConfirmOrderLineViewModel
    {
      Id = line.Id,
      Sku = line.Sku,
      Embroidery = line.Embroidery,
      UnitPrice = line.UnitPrice,
      Quantity = line.Quantity
    }).ToList();
  }
}

public sealed class ConfirmOrderLineViewModel
{
  public int Id { get; set; }
  public string Sku { get; set; } = string.Empty;
  public string? Embroidery { get; set; }
  public decimal UnitPrice { get; set; }
  public int Quantity { get; set; }
  public decimal LineTotal => UnitPrice * Quantity;
}
