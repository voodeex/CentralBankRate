using CentralBankRate.Core.Models;

namespace CentralBankRate.Core.Services.Sender;

public interface ISenderService
{
    Task<Result<bool>> SendAsync(string recipient, string subject, string textBody, byte[]? pdfBytes, string attachmentName,
        CancellationToken cancellationToken = default);
}