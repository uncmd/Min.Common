using Microsoft.Extensions.Logging;

namespace Min.Common.Logging;

public interface IExceptionWithSelfLogging
{
    void Log(ILogger logger);
}
