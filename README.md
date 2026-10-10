# Курсы валют ЦБ РФ

Десктопное приложение для загрузки курсов валют с сайта ЦБ РФ. Пользователь выбирает дату, приложение загружает курсы на эту дату и на предыдущий день, считает изменение, формирует PDF-отчёт и отправляет его на почту.

Написано на C# (.NET 9), интерфейс на Avalonia.

## Что умеет

- загрузка курсов на выбранную дату (не позже сегодняшней) и на предыдущий доступный день;
- таблица с кодом валюты, названием, курсом за 1 единицу и изменением в процентах;
- поиск по коду или названию валюты;
- топ-3 выросших и топ-3 упавших валют;
- если на выбранную дату ЦБ не устанавливал курс (выходные, праздники), приложение показывает, на какую дату курс на самом деле;
- формирование PDF-отчёта и отправка его по почте;
- сообщения об ошибках вместо падения приложения.

## Структура решения

Решение состоит из двух проектов.

`CentralBankRate.Core` содержит всю логику:

- `Services/Api` - запрос к сайту ЦБ РФ;
- `Services/Parser` - разбор XML и проверка данных;
- `Services/Currency` - основной сервис, который собирает всё вместе: загрузка, расчёт изменений, формирование и отправка отчёта;
- `Services/Report` - генерация PDF;
- `Services/Sender` - отправка письма по SMTP;
- `Configurations` - чтение config.json;
- `Models` - модели данных и `Result<T>`;
- `Formatting` - форматирование курсов и процентов, общее для UI и PDF.

`CentralBankRate.UI` содержит только интерфейс (MVVM). Логики в нём нет, ViewModel вызывает методы Core и показывает результат. Зависимости подключаются через Microsoft.Extensions.DependencyInjection в `App.axaml.cs`.

Для UI достаточно одного интерфейса из Core:

```csharp
public interface ICurrencyReportService
{
    Task<Result<ReportData>> LoadReport(DateOnly date, CancellationToken cancellationToken = default);

    Task<Result<bool>> SendReport(ReportData data, string email, CancellationToken cancellationToken = default);
}
```

`LoadReport` загружает и обрабатывает курсы, `SendReport` формирует PDF и отправляет письмо. Настройки SMTP читаются через `ConfigLoad.Load()`.

Сервисы Core не выбрасывают исключения наружу. Ожидаемые ошибки (нет сети, плохой XML, ошибка SMTP и т.д.) перехватываются внутри и возвращаются в `Result<T>` с текстом, который можно сразу показать пользователю.

## Как считаются данные

Курсы берутся из `https://cbr.ru/scripts/XML_daily.asp`. Ответ приходит в windows-1251, для этого подключён пакет System.Text.Encoding.CodePages.

Числа в XML записаны через запятую, поэтому разбираются с культурой ru-RU. Курс за одну единицу берётся из поля `VunitRate`, то есть он уже поделён на номинал.

Предыдущий день отсчитывается от даты из атрибута `Date` в `ValCurs`, а не от даты в календаре. В выходные ЦБ отдаёт последний действующий курс, и если считать от календаря, то в воскресенье суббота сравнивалась бы сама с собой.

Изменение в процентах считается как `(курс / предыдущий курс - 1) * 100`. Валюты сопоставляются по ID. Если валюты не было в предыдущий день, вместо изменения выводится прочерк. Среднее изменение считается по всем валютам, у которых есть изменение.

## PDF и письмо

В PDF есть заголовок, дата курсов, таблица всех валют с изменениями и блок «Итоги дня»: топ-3 роста, топ-3 падения и среднее изменение по всем валютам.

Письмо отправляется с темой «Курсы валют ЦБ РФ на дд.мм.гггг». В тексте письма краткая сводка с топ-3 роста и падения, PDF прикладывается вложением.

## Обработка ошибок

Приложение показывает понятное сообщение в следующих случаях:

- нет интернета, сайт ЦБ недоступен или не отвечает (таймаут 30 секунд);
- сайт вернул ошибку, пустой ответ или некорректный XML;
- неправильный адрес получателя;
- ошибка SMTP (неверный логин или пароль, проблемы с TLS, сервер отклонил письмо, таймаут);
- нет файла config.json или в нём ошибка. В этом случае предупреждение появляется сразу при запуске, курсы загружать можно, но отправка не работает.

## Настройка

Нужен .NET SDK 9.0 или новее. Работает на Windows, macOS и Linux.

Настройки почты хранятся в `CentralBankRate.Core/config.json`. Этот файл в `.gitignore`, в репозитории лежит только шаблон `config.example.json`.

Скопируйте шаблон.

macOS / Linux:

```bash
cp CentralBankRate.Core/config.example.json CentralBankRate.Core/config.json
```

Windows (PowerShell):

```powershell
Copy-Item CentralBankRate.Core\config.example.json CentralBankRate.Core\config.json
```

Заполните поля:

```json
{
  "Host": "smtp.gmail.com",
  "Port": 587,
  "UseSslOnConnect": false,
  "Username": "your-account@gmail.com",
  "Password": "пароль приложения"
}
```

- `Host` и `Port` - адрес и порт SMTP-сервера;
- `UseSslOnConnect` - `true` для порта 465, `false` для порта 587 (STARTTLS);
- `Username` - логин, с этого же адреса отправляется письмо;
- `Password` - пароль. Для Gmail, Яндекса и Mail.ru нужен пароль приложения, обычный пароль от почты не подойдёт.

Для Gmail: `smtp.gmail.com`, порт 587, `UseSslOnConnect: false`.
Для Яндекса: `smtp.yandex.ru`, порт 465, `UseSslOnConnect: true`.
Для Mail.ru: `smtp.mail.ru`, порт 465, `UseSslOnConnect: true`.

При сборке config.json копируется в папку с exe-файлом UI. Если поменяли конфиг после сборки, проект нужно пересобрать (`dotnet run` делает это сам).

## Запуск

```bash
git clone https://github.com/voodeex/CentralBankRate.git
```

```bash
cd CentralBankRate
```

После создания config.json:

```bash
dotnet run --project CentralBankRate.UI
```

Также можно открыть `CentralBankRate.sln` в Rider или Visual Studio и запустить проект `CentralBankRate.UI`.

Порядок работы в приложении: выбрать дату, нажать «Загрузить данные», ввести адрес почты и нажать «Отправить отчёт».

## Библиотеки

Core:

- QuestPDF - генерация PDF;
- MailKit - отправка почты;
- System.Text.Encoding.CodePages - кодировка windows-1251;
- HttpClient, XmlSerializer, System.Text.Json из стандартной библиотеки.

UI:

- Avalonia 12 (Desktop, Themes.Fluent, Controls.DataGrid, Fonts.Inter);
- CommunityToolkit.Mvvm;
- Microsoft.Extensions.DependencyInjection и Microsoft.Extensions.Hosting.

## Дополнительные задания

Дополнительные задания не выполнялись.
