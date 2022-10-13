namespace Min;

public class ProfilerTests
{
    [Theory]
    [InlineData(500)]
    [InlineData(1000)]
    [InlineData(2000)]
    public void StopWatchProfileTest(int delay)
    {
        var profiler = new StopwatchProfiler();
        profiler.Start();

        Thread.Sleep(delay * 2);
        profiler.Stop();

        profiler.Elapsed.TotalMilliseconds.ShouldBeGreaterThan(delay);
    }
}
