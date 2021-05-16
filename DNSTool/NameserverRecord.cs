using System.Net;
using DnsClient;

namespace DNSTool
{
    public class NameserverRecord : NameServer
    {
        public NameserverRecord(IPAddress ipAddress, string domain) : base(ipAddress)
        {
            Domain = domain;
        }

        public string Domain { get; set; }
    }
}