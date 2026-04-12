using System.Text.Encodings.Web;
using System.Text.Json;
using Shared.SharedKernel;

namespace Framework.Exceptions;

public class NotFoundException: Exception
{
    protected internal NotFoundException(Error[] errors)
        : base(JsonSerializer.Serialize(errors, new JsonSerializerOptions
            { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
    {
    }
}