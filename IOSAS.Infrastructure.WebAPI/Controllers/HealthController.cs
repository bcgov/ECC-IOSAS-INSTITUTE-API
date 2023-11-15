using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IOSAS.Infrastructure.WebAPI.Controllers
{
    /// <summary>
    /// Controller is used to indicate the health/availability of the service. 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        // GET: api/Health to check availability of the API
        [HttpGet]
        public ActionResult Get()
        {
            return Ok();
        }
    }
}
