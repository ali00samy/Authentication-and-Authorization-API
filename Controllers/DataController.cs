using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Authantication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        [Authorize(Policy = ("RequireManagers"))]
        [HttpGet]
        [Route("Get")]
        public ActionResult Get()
        {
            return Ok(new
            {
                Name = "Ali"
            });
        }

        [Authorize(Policy = ("Adults"))]
        [HttpGet]
        [Route("violent-content")]
        public ActionResult GetViolentContent()
        {
            return Ok(new
            {
                Name = "+18 content"
            });
        }
    }
}
