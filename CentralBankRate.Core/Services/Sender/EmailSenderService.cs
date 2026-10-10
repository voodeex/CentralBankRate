using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CentralBankRate.Core;
using CentralBankRate.Core.Configurations;
using CentralBankRate.Core.Models;
using CentralBankRate.Core.Services.Sender;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public sealed class EmailSenderService : ISenderService
{
    private readonly SmtpSettings _settings;

    public EmailSenderService(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task<Result<bool>> SendAsync(string recipient, string subject, string textBody, byte[]? pdfBytes, string attachmentName,
        CancellationToken cancellationToken = default)
    {

        if (string.IsNullOrWhiteSpace(_settings.Host)
            || _settings.Port is < 1 or > 65535
            || string.IsNullOrWhiteSpace(_settings.Username)
            || string.IsNullOrEmpty(_settings.Password))
        {
            return Result<bool>.Failure("Настройки SMTP отсутствуют или заполнены некорректно.");
        }

        var parserOptions = new ParserOptions
        {
            AllowAddressesWithoutDomain = false
        };

        if (string.IsNullOrWhiteSpace(_settings.Username)
            || !MailboxAddress.TryParse(parserOptions, _settings.Username, out var sender)
            || sender is null)
        {
            return Result<bool>.Failure("В настройках указан некорректный адрес отправителя.");
        }

        if (string.IsNullOrWhiteSpace(recipient)
            || !MailboxAddress.TryParse(parserOptions, recipient, out var receiver)
            || receiver is null)
        {
            return Result<bool>.Failure("Указан некорректный адрес получателя.");
        }

        if (pdfBytes is null || pdfBytes.Length == 0)
        {
            return Result<bool>.Failure("PDF-отчёт пуст.");
        }

        if (string.IsNullOrWhiteSpace(attachmentName))
        {
            return Result<bool>.Failure("Не указано имя вложения.");
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            return Result<bool>.Failure("Не указана тема письма.");
        }

        using var message = new MimeMessage();

        message.From.Add(sender);
        message.To.Add(receiver);
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            TextBody = textBody
        };

        builder.Attachments.Add(attachmentName, pdfBytes, new ContentType("application", "pdf"));

        message.Body = builder.ToMessageBody();

        using var smtpClient = new SmtpClient()
        {
            CheckCertificateRevocation = false
        };

        var security = _settings.UseSslOnConnect
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;

        try
        {
            await smtpClient.ConnectAsync(_settings.Host, _settings.Port, security, cancellationToken);

            await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);

            await smtpClient.SendAsync(message, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result<bool>.Failure("Отправка письма отменена.");
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
        {
            return Result<bool>.Failure("Превышено время ожидания SMTP. Не удалось подтвердить отправку письма.");
        }
        catch (MailKit.Security.AuthenticationException)
        {
            return Result<bool>.Failure("SMTP-сервер отклонил авторизацию. Проверь логин, пароль и настройки доступа к SMTP.");
        }
        catch (SslHandshakeException)
        {
            return Result<bool>.Failure("Не удалось установить защищённое соединение с SMTP-сервером.");
        }
        catch (SmtpCommandException exception)
        {
            return Result<bool>.Failure(
                $"SMTP-сервер отклонил команду. Код ответа: {(int)exception.StatusCode} ({exception.StatusCode}). {exception.Message}");
        }
        catch (NotSupportedException)
        {
            return Result<bool>.Failure("SMTP-сервер не поддерживает выбранный способ защиты соединения или авторизации.");
        }
        catch (Exception exception)
        {
            return Result<bool>.Failure(
                $"Ошибка соединения с SMTP-сервером. Не удалось подтвердить отправку письма. {exception.Message}");
        }
    }
}
