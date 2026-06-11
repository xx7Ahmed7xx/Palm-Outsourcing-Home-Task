using OrderHub.Core.Models;

namespace OrderHub.Core.Abstractions;

public interface IConfirmOrderRepository
{
  Task<ConfirmOrderDraft?> GetDraftAsync(int schoolId, CancellationToken cancellationToken = default);

  Task UpdateQuantitiesAsync(
    int schoolId,
    IReadOnlyList<ConfirmOrderLineUpdate> updates,
    CancellationToken cancellationToken = default);
}
