using System.Text.Encodings.Web;
using System.Text.Json;
using Shared;

namespace DirectoryService.Application.Exceptions;

public class ConflictException: Exception
{
    protected internal ConflictException(Error[] errors)
        : base(JsonSerializer.Serialize(errors, new JsonSerializerOptions
            { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
    {
    }
}