using System.Text.Json;
using FluentValidation.Results;
using Shared.SharedKernel;

namespace Core.Validation;

public static class ValidationExtensions
{
    public static Errors ToListError(this ValidationResult validationResult)
    {
        var validationErrors = validationResult.Errors;

        var errors = from validationError in validationErrors
            let errorMessage = validationError.ErrorMessage
            let error = JsonSerializer.Deserialize<Error>(errorMessage)
            select error;

        return new Errors(errors);
    }
}