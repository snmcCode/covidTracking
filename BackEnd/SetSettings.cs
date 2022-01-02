using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Common.Models;
using Common.Resources;
using Common.Utilities;
using Common.Utilities.Exceptions;

namespace BackEnd
{
    public class SetSettings
    {

        private readonly IConfiguration config;

        public SetSettings(IConfiguration config)
        {
            this.config = config;
        }

        [FunctionName("SetSettings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "setting")] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            throw new NotImplementedException("Not implemented yet");
            
        }
    }
}

