using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Shared;

namespace DirectoryService.Application.Exceptions;

public class FailureException: Exception
{
    protected internal FailureException(Error[] errors)
        : base(JsonSerializer.Serialize(errors, new JsonSerializerOptions
            { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
    {
    }
}