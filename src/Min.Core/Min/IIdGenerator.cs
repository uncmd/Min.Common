using Min.Helpers;

namespace Min;

/// <summary>
/// Id生成器
/// </summary>
public interface IIdGenerator
{
    /// <summary>
    /// 生成一个新的id
    /// </summary>
    /// <returns>new id</returns>
    string NewId();
}

/// <summary>
/// 基于Guid的Id生成器
/// </summary>
public sealed class GuidIdGenerator : IIdGenerator
{
    public static readonly GuidIdGenerator Instance = new();

    public string NewId() => Guid.NewGuid().ToString("N");
}

/// <summary>
/// 基于有序Guid的Id生成器
/// </summary>
public sealed class SequentialGuidIdGenerator : IIdGenerator
{
    private readonly SequentialGuidType _sequentialGuidType;

    public SequentialGuidIdGenerator(SequentialGuidType sequentialGuidType)
    {
        _sequentialGuidType = sequentialGuidType;
    }

    public string NewId() => SequentialGuidGenerator.Create(_sequentialGuidType).ToString("N");
}

/// <summary>
/// 基于ObjectId的Id生成器
/// </summary>
public sealed class ObjectIdGenerator : IIdGenerator
{
    public string NewId() => ObjectId.GenerateNewStringId();
}

/// <summary>
/// 基于雪花算法的Id生成器
/// </summary>
public sealed class SnowflakeIdGenerator : IIdGenerator
{
    public string NewId()
    {
        throw new NotImplementedException();
    }
}
