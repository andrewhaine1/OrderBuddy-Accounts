using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

public static class CertExtensions
{
    private static void AddCertificateFromStore(this IIdentityServerBuilder builder,
        IConfiguration options)
    {
        var subjectName = options.GetValue<string>("SubjectName");

        var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        var certificates = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);

        if (certificates.Count > 0)
        {
            builder.AddSigningCredential(certificates[0]);
        }
    }
}