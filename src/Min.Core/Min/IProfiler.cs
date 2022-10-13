using System.Diagnostics;

namespace Min;

public interface IProfiler
{
    /// <summary>
    /// 开始或恢复测量间隔的运行时间
    /// </summary>
    void Start();

    /// <summary>
    /// 停止测量间隔的运行时间
    /// </summary>
    void Stop();

    /// <summary>
    /// 停止时间间隔测量，将运行时间重置为零，并开始测量运行时间
    /// </summary>
    void Restart();

    /// <summary>
    /// 获取由当前实例测量的总运行时间
    /// </summary>
    TimeSpan Elapsed { get; }
}

public class StopwatchProfiler : IProfiler
{
    private readonly Stopwatch _stopwatch;

    public StopwatchProfiler()
    {
        _stopwatch = new Stopwatch();
    }

    public void Start()
    {
        _stopwatch.Start();
    }

    public void Stop()
    {
        _stopwatch.Stop();
    }

    public void Restart()
    {
        _stopwatch.Restart();
    }

    public TimeSpan Elapsed => _stopwatch.Elapsed;
}
