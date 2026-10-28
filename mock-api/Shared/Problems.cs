namespace MockApi.Shared;

public static class Problems
{
    private const string BadRequestType = "https://tools.ietf.org/html/rfc9110#section-15.5.1";

    public static IResult Validation(Dictionary<string, string[]> errors) =>
        Results.Json(new
        {
            type = BadRequestType,
            title = "One or more validation errors occurred.",
            status = 400,
            errors,
            traceId = Guid.NewGuid().ToString("N")
        }, statusCode: 400);

    public static IResult Validation(string field, params string[] messages) =>
        Validation(new Dictionary<string, string[]> { [field] = messages });

    public static IResult Unauthorized() => Results.StatusCode(401);

    public static IResult Problem(int status, string title, string? detail = null) =>
        Results.Json(detail is null ? new { title, status } : (object)new { title, status, detail }, statusCode: status);

    public static IResult ProviderUnavailable() =>
        Problem(503, "Game data is currently unavailable.", "The RAWG API key is not configured (Rawg__ApiKey).");

    // Lets the frontend dev force the 503 state with ?mock503=1.
    public static bool Wants503(HttpRequest request) => request.Query["mock503"] == "1";
}
