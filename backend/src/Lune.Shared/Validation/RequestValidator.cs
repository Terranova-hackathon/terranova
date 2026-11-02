using System.ComponentModel.DataAnnotations;

namespace Lune.Shared.Validation;

public static class RequestValidator
{

    /// <summary>
    /// Validates the given request object and returns whether it is valid.
    /// Will populate a dictionary with validation errors like this:
    /// {
    ///   "Email": ["The Email field is required."],
    ///   "Password": ["The field Password must be a string or array type with a minimum length of '8'."]
    /// }
    /// <returns></returns>
    public static bool TryValidate<T>(T req, out Dictionary<string, string[]> errors) where T : notnull
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(req, new ValidationContext(req), results, validateAllProperties: true);

        errors = results
            .SelectMany(r => (r.MemberNames.Any() ? r.MemberNames : [string.Empty])
                .Select(member => (member, message: r.ErrorMessage ?? "Invalid value.")))
            .GroupBy(x => x.member)
            .ToDictionary(g => g.Key, g => g.Select(x => x.message).ToArray());

        return errors.Count == 0;
    }
}