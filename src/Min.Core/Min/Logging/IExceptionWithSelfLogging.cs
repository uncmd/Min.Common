using Microsoft.Extensions.Logging;

namespace Min.Logging;

public interface IExceptionWithSelfLogging
{
    void Log(ILogger logger);
}
