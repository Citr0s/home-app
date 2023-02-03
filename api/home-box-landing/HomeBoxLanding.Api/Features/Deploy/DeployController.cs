using System.Net;
using HomeBoxLanding.Api.Core.Shell;
using HomeBoxLanding.Api.Features.Deploy.Types;
using Microsoft.AspNetCore.Mvc;

namespace HomeBoxLanding.Api.Features.Deploy
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class DeployController : Controller
    {
        private readonly DeployService _deployService;

        public DeployController()
        {
            _deployService = new DeployService(new ShellService(), new DeployRepository());
        }
        
        [HttpPost("")]
        //[GithubAuth]
        public ActionResult Get([FromBody]GithubBuildRequest request)
        {
            var deployResponse = _deployService.Deploy(request);

            if (deployResponse.HasError)
                return StatusCode((int)HttpStatusCode.BadRequest, deployResponse.Error.UserMessage);
            
            return Ok("A-OK");
        }
        
        [HttpGet("")]
        //[Administator]
        //[Authentication]
        public ActionResult Get()
        {
            return Ok(_deployService.GetAllDeploys());
        }
    }
}