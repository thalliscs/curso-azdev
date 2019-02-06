using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiCurso.Controllers
{
    [Route("api/machine")]
    [ApiController]
    public class MachineController : ControllerBase
    {
        public IActionResult GetMachineName()
        {
            var envValue = Environment.GetEnvironmentVariable("TIPO_BANCO");
            return Ok(new { nome = Environment.MachineName, data = DateTime.UtcNow, banco = envValue });
        }
    }
}