using System.Web.Http;
using ScrapingResult.Services;

namespace ScrapingResult.Controllers
{
    public class EzyController : ApiController
    {
        public IHttpActionResult Get()
        {
            var database = 1;
            var matches = BetResult.GetBetResult(database);
            return Ok(matches);
        }
    }
}
