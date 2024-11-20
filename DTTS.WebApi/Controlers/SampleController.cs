using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DTTS.WebApi.Controlers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    [EnableCors("_myAllowVNPTDTTSOrigins")]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        [Route("GetId")]
        public ActionResult GetId(string name)
        {
            try
            {
                return RedirectToAction(string.Empty);
            }
            catch
            {
                return null;
            }
        }
    }
}
