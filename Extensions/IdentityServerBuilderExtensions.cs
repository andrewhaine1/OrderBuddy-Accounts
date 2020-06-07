using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;


namespace Ord.Accounts.Extensions
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder LoadSigningCredentialFrom(this IIdentityServerBuilder builder, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                builder.AddSigningCredential(new X509Certificate2(path));
            }
            else
            {
                builder.AddDeveloperSigningCredential();
            }

            return builder;
        }
    }
}
