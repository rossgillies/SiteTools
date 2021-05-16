using System;

namespace DNSTool
{
    public class DomainName
    {
        private DomainName(string domain)
        {
            Domain = new Uri(domain).Host;
        }

        public string Domain { get; set; }

        public static DomainName DomainNameFactory(string domain)
        {
            if (!IsValidDomain(domain))
                throw new ArgumentException("Invalid domain.");

            if (!domain.Contains(Uri.SchemeDelimiter))
                domain = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, domain);

            return new DomainName(domain);
        }

        private static bool IsValidDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return false;
            return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
        }

        public string GetTld()
        {
            return Domain.Substring(Domain.LastIndexOf('.') + 1);
        }

        public string GetUri()
        {
            return "https://" + Domain;
        }
    }
}