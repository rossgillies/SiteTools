using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;

namespace DNSTool
{
    public class DnsReport
    {
        private static readonly LookupClient DnsClient = new(
            new LookupClientOptions(NameServer.Cloudflare)
            {
                UseCache = false,
                EnableAuditTrail = false,
                Recursion = true,
                Retries = 3,
                Timeout = TimeSpan.FromSeconds(3)
            }
        );

        private static readonly List<QueryType> QueryTypes = new()
        {
            QueryType.A,
            QueryType.AAAA,
            QueryType.CNAME,
            QueryType.MX,
            QueryType.TXT,
            QueryType.NS,
            QueryType.SOA,
            QueryType.PTR,
            QueryType.CAA,
            QueryType.SRV
        };

        //TODO add remaining root servers
        private static readonly List<string> RootNameservers = new()
        {
            "a.root-servers.net",
            "k.root-servers.net",
            "l.root-servers.net"
        };

        public DnsReport(DomainName domain)
        {
            DomainName = domain;
            Date = DateTime.UtcNow;
        }

        private DateTime Date { get; }

        private DomainName DomainName { get; }


        public async Task<Dictionary<string, object>> GetDnsRecords()
        {
            var responses = new ConcurrentDictionary<QueryType, List<DnsRecord>>();

            var rootNameservers = FetchRootNameservers();
            var tldNameservers = FetchTldNameservers(rootNameservers, DomainName.GetTld());
            var domainNameservers = FetchDomainNameservers(tldNameservers, DomainName.Domain);

            var queryNameserver = new List<NameserverRecord>()
            {
                domainNameservers[new Random().Next(domainNameservers.Count)]
            };

            var tasks = QueryTypes.Select(async recordType =>
            {
                var response = await FetchDnsRecord(domainNameservers, DomainName, recordType);
                responses.TryAdd(recordType, response);
            });

            await Task.WhenAll(tasks);

            var result = new Dictionary<string, object>
            {
                {"query", DomainName.Domain},
                {"nameserver", queryNameserver.First().Domain},
                {"authoritativeNameservers", domainNameservers.Select(ns => ns.Domain).Distinct().ToList()},
                {"time", Date},
                {"records", responses}
            };

            return result;
        }

        private List<NameserverRecord> FetchRootNameservers()
        {
            var record = DnsClient.Query(RootNameservers[new Random().Next(RootNameservers.Count)], QueryType.A).Answers
                .ARecords().First();
            return new List<NameserverRecord>
            {
                new(record.Address, record.DomainName.Value)
            };
        }

        private static List<NameserverRecord> FetchTldNameservers(List<NameserverRecord> parentNameservers,
            string domain)
        {
            if (parentNameservers.Count < 1) return null;

            var query = DnsClient.QueryServer(parentNameservers, domain, QueryType.A);

            var childNameservers = new List<NameserverRecord>();
            query.Additionals.ARecords().ToList().ForEach(nameserver =>
            {
                childNameservers.Add(new NameserverRecord(nameserver.Address, nameserver.DomainName.Value));
            });
            return childNameservers;
        }

        private static List<NameserverRecord> FetchDomainNameservers(List<NameserverRecord> parentNameservers,
            string domain)
        {
            if (parentNameservers.Count < 1) return null;

            var query = DnsClient.QueryServer(parentNameservers, domain, QueryType.A);

            var childNameservers = new List<NameserverRecord>();
            query.Authorities.NsRecords().ToList().ForEach(nameserver =>
            {
                DnsClient.Query(nameserver.NSDName.Value, QueryType.A).Answers.AddressRecords().ToList().ForEach(record =>
                    {
                        childNameservers.Add(new NameserverRecord(record.Address, nameserver.NSDName.Value));
                    });
            });
            return childNameservers;
        }

        private static async Task<List<DnsRecord>> FetchDnsRecord(List<NameserverRecord> nameservers, DomainName domain,
            QueryType queryType)
        {
            var response = await DnsClient.QueryServerAsync(nameservers, domain.Domain, queryType);
            var records = new List<DnsRecord>();
            switch (queryType)
            {
                case QueryType.A:
                    response.Answers.ARecords().ToList().ForEach(item =>
                        records.Add(new AddressRecord(item.DomainName.ToString(), item.Address.ToString(),
                            item.TimeToLive, item.ToString())));
                    break;
                case QueryType.AAAA:
                    response.Answers.AaaaRecords().ToList().ForEach(item =>
                        records.Add(new AddressRecord(item.DomainName.ToString(), item.Address.ToString(),
                            item.InitialTimeToLive, item.ToString())));
                    break;

                case QueryType.CNAME:
                    response.Answers.CnameRecords().ToList().ForEach(item =>
                        records.Add(new CnameRecord(item.CanonicalName.ToString(), item.TimeToLive, item.ToString())));
                    break;

                case QueryType.MX:
                    response.Answers.MxRecords().ToList().ForEach(item =>
                        records.Add(new MxRecord(item.Exchange.ToString(), item.Preference, item.InitialTimeToLive,
                            item.ToString())));
                    break;

                case QueryType.TXT:
                    response.Answers.TxtRecords().ToList().ForEach(item =>
                        records.Add(new TxtRecord(item.EscapedText.First().ToString(), item.InitialTimeToLive,
                            item.ToString())));
                    break;
            }

            return records;
        }
    }
}