using AppProject.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General
{
    [Route("api/general/[controller]/[action]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetSample()
        {
            return this.Ok("SampleController is working!");
        }

        [HttpGet]
        public IActionResult GetCultureSample()
        {
            return this.Ok(StringResource.GetStringByKey("Sample_Message_Text"));
        }
    }
}