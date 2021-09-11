using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SharpC2.Services
{
    public class CertificateService
    {
        private string _acceptedThumbprint;

        public bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            var thumbprint = certificate.GetCertHashString();

            if (thumbprint is not null && thumbprint.Equals(_acceptedThumbprint))
                return true;

            Console.WriteLine();
            Console.WriteLine("Server Certificate");
            Console.WriteLine("------------------");
            Console.WriteLine();
            Console.WriteLine(certificate.ToString());

            Console.Write("accept? [y/N] > ");
            var accept = Console.ReadLine();

            if (string.IsNullOrEmpty(accept)) return false;
            if (!accept.Equals("Y", StringComparison.OrdinalIgnoreCase)) return false;

            _acceptedThumbprint = thumbprint;
            return true;
        }
    }
}