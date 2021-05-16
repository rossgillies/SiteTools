using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DNSTool.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthCheckController : Controller
    {
        [HttpGet("{domain}")]
        public object GetRecords(string domain)
        {
            var domainName = DomainName.DomainNameFactory(domain);
            if (domainName == null)
                return UnprocessableEntity();

            var healthCheck = new HealthCheck(domainName);
            var result = healthCheck.GetHealthCheck();
            return JsonConvert.SerializeObject(result);
        }
    }
}