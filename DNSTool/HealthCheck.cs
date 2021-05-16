using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace DNSTool
{
    public class HealthCheck
    {
        public HealthCheck(DomainName domain)
        {
            DomainName = domain;
            Date = DateTime.UtcNow;
        }

        private DateTime Date { get; set; }

        private DomainName DomainName { get; set; }

        public string[] DnsRecords { get; set; }

        public Dictionary<string, object> GetHealthCheck()
        {
            var dnsReport = new DnsReport(DomainName);
            var result = dnsReport.GetDnsRecords().Result;

            return result;
        }

        private static bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            Console.WriteLine("Subject: {0}", certificate.Subject);
            Console.WriteLine("Effective date: {0}", certificate.GetEffectiveDateString());
            Console.WriteLine("Expiration date: {0}", certificate.GetExpirationDateString());
            Console.WriteLine("Issuer: {0}", certificate.Issuer);
            Console.WriteLine("Key algorithm: {0}", certificate.GetKeyAlgorithm());
            Console.WriteLine("Certificate hash: {0}", certificate.GetCertHashString());
            Console.WriteLine("Public key: {0}", certificate.GetPublicKeyString());
            Console.WriteLine("Serial number: {0}", certificate.GetSerialNumberString());
            Console.WriteLine("SSL policy errors: {0}", errors);
            return errors == SslPolicyErrors.None;
        }

        private static void VerifySsl(string domain)
        {
            var request = (HttpWebRequest) WebRequest.Create(@"https://" + domain);
            request.ServerCertificateValidationCallback += OnCertificateValidation;
            try
            {
                var response = (HttpWebResponse) request.GetResponse();
                Console.WriteLine("Response status: {0} ({1})", response.StatusCode, response.StatusDescription);
            }
            catch (WebException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }
    }
}