using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;
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

        [HttpGet]
        public IActionResult GetException()
        {
            throw new AppException(ExceptionCode.Generic, "This is a sample exception for testing purposes.");
        }

        [HttpPost]
        public IActionResult PostSample([FromBody] CreateOrUpdateRequest<SampleDto> request)
        {
            return this.Ok(request);
        }
    }
}