using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderHub.Application;
using OrderHub.Core.Models;

namespace OrderHub.Web.Pages;

public class DemoOrderModel(OrderProcessor orderProcessor) : PageModel
{
  [BindProperty]
  public string ParentEmail { get; set; } = "parent@example.com";

  public string? ResultMessage { get; private set; }

  public void OnGet()
  {
  }

  public async Task<IActionResult> OnPostProcessAsync()
  {
    var result = await orderProcessor.ProcessOrderAsync(
      schoolId: 1,
      lines:
      [
        new OrderLine { Sku = "BLAZER-42", Quantity = 1, Embroidery = "AB" },
        new OrderLine { Sku = "TIE-RED", Quantity = 2 }
      ],
      parentEmail: ParentEmail);

    ResultMessage = result.IsSuccess
      ? $"{result.Message} — subtotal £{result.Subtotal:0.00}"
      : result.Message;

    return Page();
  }
}
