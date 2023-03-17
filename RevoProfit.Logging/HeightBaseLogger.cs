using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace RevoProfit.Logging;

public class HeightBaseLogger : ILogger
{
    private const string ApiToken = "39f423db-383a-4b1b-98ee-21ed0109279c";
    private const string ApiUrl = "https://uk.api.8base.com/cle1px21j00ja08ma5itvda3s";

    public async void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
#if DEBUG
        await Task.CompletedTask;
#else
        await LogToApi(logLevel, eventId, state);
#endif
    }

    private static async Task LogToApi<TState>(LogLevel logLevel, EventId eventId, TState state)
    {
        var graphql = new
        {
            query = $@"mutation logCreate {{
    logCreate(data: {{ logEvent: {eventId}, logLevel: ""{logLevel}"", logMessage: ""{state}"" }}) {{ id }}
}}",
            variables = "{}",
        };

        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiUrl),
            DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", ApiToken) },
        };

        await httpClient.PostAsync(string.Empty,
            new StringContent(JsonSerializer.Serialize(graphql), Encoding.UTF8, "application/json"));
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        throw new NotImplementedException();
    }
}