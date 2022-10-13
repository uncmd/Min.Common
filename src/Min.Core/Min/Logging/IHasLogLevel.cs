using Microsoft.Extensions.Logging;

namespace Min.Logging;

public interface IHasLogLevel
{
    LogLevel LogLevel { get; set; }
}
