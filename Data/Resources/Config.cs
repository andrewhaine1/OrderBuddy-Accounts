using IdentityServer4.Models;
using System.Collections.Generic;

namespace Ord.Accounts.Data.Resources
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("orderbuddyapi", "Order Buddy API")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // Ionic client
                new Client
                {
                    ClientName = "Order Buddy Password Client",
                    ClientId = "orderbuddy_password",
                    ClientSecrets = { new Secret("7baeb4e4".Sha256()) },
                    Enabled = true,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AccessTokenLifetime = 7776000,
                    RequireConsent = false,
                    AllowAccessTokensViaBrowser = true,
                    AllowedCorsOrigins = 
                    { 
                        "http://localhost",
                        "http://localhost:5000",
                        "https://accounts.orderbuddy.co.za", 
                        "https://backend.orderbuddy.co.za"
                    },
                    AllowedScopes = { "openid", "profile", "orderbuddyapi" }
                }
            };
        }
    }
}

