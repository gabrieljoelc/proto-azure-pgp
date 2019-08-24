using System;
using System.Security.Cryptography.X509Certificates;

namespace proto_azure_pgp
{
    class Program
    {
        static void Main(string[] args)
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
            if (certCollection.Count > 0)
            {
                X509Certificate2 cert = certCollection[0];
                // Use certificate
                Console.WriteLine(cert.FriendlyName);
            }
            else
            {
                Console.WriteLine("no certs fetched");
            }
            certStore.Close();

            Console.WriteLine("it's over");
        }
    }
}
