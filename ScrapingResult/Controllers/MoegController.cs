using System.Web.Http;
using ScrapingResult.Services;

namespace ScrapingResult.Controllers
{
    public class MoegController : ApiController
    {
        public IHttpActionResult Get()
        {
            var database = 2;
            var matches = BetResult.GetBetResult(database);
            return Ok(matches);
        }
    }


}
