using System;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace proto_azure_pgp
{
    class Program
    {
        private static ClientAssertionCertificate Cert { get; set; }
        private static KeyVaultClient Client { get; set; } 
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            certStore.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                        X509FindType.FindByThumbprint,
                                        // Replace below with your certificate's thumbprint
                                        Environment.GetEnvironmentVariable("WEBSITE_LOAD_CERTIFICATES"),
                                        false);
            // Get the first cert with the thumbprint
            Console.WriteLine($"cert collection count: {certCollection.Count}");
            if (certCollection.Count > 0)
            {
                X509Certificate2 rawCert = certCollection[0];
                // Use certificate
                Console.WriteLine($"friendly name: {rawCert.FriendlyName}");
                Console.WriteLine($"subject: {rawCert.Subject}");
                var appId = Environment.GetEnvironmentVariable("APPLICATION_ID");
                Cert = Cert ?? new ClientAssertionCertificate(appId, rawCert);

                var vaultKeyPrefix = Environment.GetEnvironmentVariable("VAULT_KEY_PREFIX");
                var vaultUrl = Environment.GetEnvironmentVariable("VAULT_URL");
                Client = Client ?? new KeyVaultClient(GetAccessTokenAsync);

                var secret = new SecretBundle();
                secret = await Client.GetSecretAsync(vaultUrl, $"{vaultKeyPrefix}-public");
                Regex.Unescape(secret.Value);
                Console.WriteLine("public fetched");
                secret = await Client.GetSecretAsync(vaultUrl, $"{vaultKeyPrefix}-private");
                Regex.Unescape(secret.Value);
                Console.WriteLine("private fetched");
                secret = await Client.GetSecretAsync(vaultUrl, $"{vaultKeyPrefix}-pass");
                Regex.Unescape(secret.Value);
                Console.WriteLine("passphrase fetched");
            }
            else
            {
                Console.WriteLine("no certs fetched");
            }
            certStore.Close();

            Console.WriteLine("it's over");
        }

        private static async Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, Cert);

            return result.AccessToken;
        }
    }
}
