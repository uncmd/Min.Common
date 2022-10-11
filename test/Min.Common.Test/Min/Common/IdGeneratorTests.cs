using Min.Common.Helpers;

namespace Min.Common;

public class IdGeneratorTests
{
    [Fact]
    public void GuidIdTest()
    {
        var id = GuidIdGenerator.Instance.NewId();

        id.ShouldNotBe(GuidIdGenerator.Instance.NewId());
    }

    [Fact]
    public void SequentialGuidIdTest()
    {
        var idGenerator = new SequentialGuidIdGenerator(SequentialGuidType.SequentialAsString);
        var id = idGenerator.NewId();
        
        id.ShouldNotBe(idGenerator.NewId());
    }

    [Fact]
    public void ObjectIdTest()
    {
        var idGenerator = new ObjectIdGenerator();
        var id = idGenerator.NewId();
        
        id.ShouldNotBe(idGenerator.NewId());
        string.CompareOrdinal(id, idGenerator.NewId()).ShouldBeLessThan(0);
    }

    [Fact]
    public void SnowflakeIdTest()
    {
        Should.Throw<NotImplementedException>(() => new SnowflakeIdGenerator().NewId());
    }
}
