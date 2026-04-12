using System.Text.Encodings.Web;
using System.Text.Json;
using Shared.SharedKernel;

namespace Framework.Exceptions;

public class ValidationException: Exception
{
    protected internal ValidationException(Error[] errors)
        : base(JsonSerializer.Serialize(errors, new JsonSerializerOptions
            { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
    {
    }
}