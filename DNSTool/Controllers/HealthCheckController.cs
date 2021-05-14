using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using DnsClient;

namespace DNSTool.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    
    public class QueryController : Controller
    {
        [HttpGet]
        public string GetRecords()
        {
            string[] records = {"A", "AAAA", "CNAME"};
            var result = DoLookup();
            return result.Answers.ARecords().FirstOrDefault()?.Address.ToString();


        }
        
        private static IDnsQueryResponse DoLookup()
        {
            var lookup = new LookupClient();
            return lookup.Query("google.com", QueryType.A);
            
        }
    }
}