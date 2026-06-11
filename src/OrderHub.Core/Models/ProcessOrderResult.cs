namespace OrderHub.Core.Models;

public sealed class ProcessOrderResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public decimal? Subtotal { get; init; }

    public static ProcessOrderResult Success(decimal subtotal) =>
        new() { IsSuccess = true, Message = "OK", Subtotal = subtotal };

    public static ProcessOrderResult Failure(string message) =>
        new() { IsSuccess = false, Message = message };
}
