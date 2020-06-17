using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using BackEnd.Models;
using BackEnd.Utilities;

namespace BackEnd
{
    public static class RegisterVisitor
    {
        [FunctionName("RegisterVisitor")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation(requestBody);

            Visitor visitor = null;
            DatabaseManager databaseManager = null;
            bool success = true;
            try
            {
                visitor = JsonConvert.DeserializeObject<Visitor>(requestBody);
                databaseManager = new DatabaseManager(visitor, log);
                databaseManager.CreateVisitor();
                log.LogInformation(
                    $"\nVisitor\n" +
                    $"RegistrationOrg: {visitor.RegistrationOrg}\n" +
                    $"FirstName: {visitor.FirstName}\n" +
                    $"LastName: {visitor.LastName}\n" +
                    $"EmailAddress: {visitor.EmailAddress}\n" +
                    $"PhoneNumber: {visitor.PhoneNumber}\n" +
                    $"Address: {visitor.Address}\n" +
                    $"FamilyID: {visitor.FamilyID}\n" +
                    $"IsMale: {visitor.IsMale}\n"
                    );
            }

            catch (Exception e)
            {
                log.LogError(e.Message);
                success = false;
            }   

            return success
                ? (ActionResult)new OkObjectResult($"Successfully added visitor with ID: {databaseManager.GetVisitorId()}")
                : new BadRequestObjectResult("Failed to add visitor");
        }
    }
}
