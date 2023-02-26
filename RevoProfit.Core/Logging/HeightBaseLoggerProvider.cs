using Microsoft.Extensions.Logging;

namespace RevoProfit.Core.Logging;

public class HeightBaseLoggerProvider : ILoggerProvider
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new HeightBaseLogger();
    }
}