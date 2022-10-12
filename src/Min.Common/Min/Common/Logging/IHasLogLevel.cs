using Microsoft.Extensions.Logging;

namespace Min.Common.Logging;

public interface IHasLogLevel
{
    LogLevel LogLevel { get; set; }
}
