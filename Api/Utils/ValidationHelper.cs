using FluentValidation;

namespace Api.Utils;

public static class ValidationHelper
{
    public static async Task<Dictionary<string, string[]>?> ValidateRequestAsync<T>(T request, IValidator<T> validator)
    {
        var result = await validator.ValidateAsync(request);

        return result.IsValid
            ? null
            : result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
    }
}