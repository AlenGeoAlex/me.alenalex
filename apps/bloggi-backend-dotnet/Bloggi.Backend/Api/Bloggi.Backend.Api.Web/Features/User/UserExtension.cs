using Bloggi.Backend.Api.Web.Features.User.Services;

namespace Bloggi.Backend.Api.Web.Features.User;

public static class UserExtension
{
    public static void AddUserModule(
        this IServiceCollection services,
        IConfigurationManager configurationManager
    )
    {
        services.AddScoped<IUserModule, UserModule>();
        services.AddScoped<UserService>();
        services.AddScoped<AuthService>();
    }
}