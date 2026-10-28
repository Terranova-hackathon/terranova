using MockApi.Auth;

namespace MockApi.Shared;

public static class RequireAuthExtensions
{
    private const string UserIdKey = "mock.userId";

    // Returns 401 with an empty body unless the cookie maps to an existing user.
    public static TBuilder RequireSession<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(async (ctx, next) =>
        {
            var http = ctx.HttpContext;
            var userId = http.RequestServices.GetRequiredService<SessionStore>().GetUserId(http);
            if (userId is null || http.RequestServices.GetRequiredService<UserStore>().FindById(userId) is null)
                return Problems.Unauthorized();
            http.Items[UserIdKey] = userId;
            return await next(ctx);
        });

    public static string CurrentUserId(this HttpContext http) => (string)http.Items[UserIdKey]!;
}
