using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using PgpCore;

namespace proto_azure_pgp
{
    class Program
    {
        private static ClientAssertionCertificate Cert { get; set; }
        private static KeyVaultClient Client { get; set; } 
        private static PGP _pgp = new PGP();

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
                var publicKey = Regex.Unescape(secret.Value);
                Console.WriteLine($"public fetched");
                secret = await Client.GetSecretAsync(vaultUrl, $"{vaultKeyPrefix}-private");
                var privateKey = Regex.Unescape(secret.Value);
                Console.WriteLine($"private fetched");
                secret = await Client.GetSecretAsync(vaultUrl, $"{vaultKeyPrefix}-pass");
                var PassPhrase = Regex.Unescape(secret.Value);
                Console.WriteLine($"passphrase fetched");

                try
                {
                    Console.WriteLine("Opening file streams");

                    /*
                     * Replace this PGP file with one you have encrypted with your private key already.
                     * It is unlikely that this key will work.
                     */
                    var inputStream = File.Open("./samplepgpfile.txt.txt.pgp", FileMode.Open, FileAccess.ReadWrite);
                    var outStream = new MemoryStream();

                    Console.WriteLine("Creating Private Key Stream");
                    var privateKeyStream = new MemoryStream(Encoding.UTF8.GetBytes(privateKey));
                    PassPhrase = PassPhrase == "0" ? null : PassPhrase;

                    Console.WriteLine("decrypting");
                    await _pgp.DecryptStreamAsync(inputStream, outStream, privateKeyStream, PassPhrase);

                    Console.WriteLine("Decrypted");
                    using (var outputStream = File.Open("./output.txt", FileMode.Create, FileAccess.Write))
                        outStream.WriteTo(outputStream);
                    outStream.Dispose();
                    Console.WriteLine("File created");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error");
                    Console.WriteLine(ex.Message); //A Message of No key found would indicate that we have the wrong key.
                }
                
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
