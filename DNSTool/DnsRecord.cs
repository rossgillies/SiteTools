namespace DNSTool
{
    public class DnsRecord
    {
        public DnsRecord(int ttl, string raw)
        {
            Ttl = ttl;
            Raw = raw;
        }

        public int Ttl { get; set; }

        public string Raw { get; set; }
    }
    
    public class AddressRecord : DnsRecord
    {
        public AddressRecord(string domain, string address, int ttl, string raw) : base(ttl, raw)
        {
            Domain = domain;
            Address = address;
        }

        public string Domain { get; set; }
        public string Address { get; set; }
    }
    
    public class MxRecord : DnsRecord
    {
        public MxRecord(string exchange, int priority, int ttl, string raw) : base(ttl, raw)
        {
            Exchange = exchange;
            Priority = priority;
        }

        public string Exchange { get; set; }

        public int Priority { get; set; }
    }

    public class CnameRecord : DnsRecord
    {
        public CnameRecord(string canonical, int ttl, string raw) : base(ttl, raw)
        {
            Canonical = canonical;
        }

        public string Canonical { get; set; }
    }

    public class TxtRecord : DnsRecord
    {
        public TxtRecord(string text, int ttl, string raw) : base(ttl, raw)
        {
            Text = text;
        }

        public string Text { get; set; }
    }
}