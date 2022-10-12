namespace Min.Common;

public class DisposalTest
{
    [Fact]
    public void DisposableActionTest()
    {
        var a = 0;
        var disposal = new DisposeAction(() => a++);
        disposal.Dispose();
        
        a.ShouldBe(1);
    }

    [Fact]
    public void DisposableActionParallelTest()
    {
        var a = 0;
        var disposal = new DisposeAction(() => a++);
        Parallel.For(1, 10, _ =>
        {
            disposal.Dispose();
        });
        
        a.ShouldBe(1);
    }
}
