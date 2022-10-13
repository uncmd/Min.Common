using System.Runtime.Serialization;

namespace Min;

public class MinCommonException : Exception
{
    public MinCommonException()
    {

    }

    public MinCommonException(string message)
        : base(message)
    {

    }

    public MinCommonException(string message, Exception innerException)
        : base(message, innerException)
    {

    }

    public MinCommonException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
    {

    }
}
