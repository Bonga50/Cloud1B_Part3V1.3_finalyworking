using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Tables;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Primitives;
using System.Net;
using System.Net.Http;
using System.Collections.ObjectModel;

namespace Cloud1B_Part3V1._3
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue("outqueue"), StorageAccount("azurewebjobsstorage")] ICollector<string> msg,
            [Table("outputTable"), StorageAccount("azurewebjobsstorage")] IAsyncCollector<Person> msg1,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            string[] IDs = {
                "1234567891011"
               ,"2468101214181"
               ,"3691215182124"
            };
            string[] names = { "Berry Benson", "Jerry Summer", "Rick Sanchez" };
            string[] Vacinne = { "Pfizer", "Moderna", "BioNTech" };
            string[] VacinneCenter = { "Clicks", "Bros Clinic", "Dischem" };
            string responseMessage = null;
            string ID = req.Query["ID"];
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            ID = ID ?? data?.ID;
            if (ID == null)
            {
                responseMessage = "This HTTP triggered function executed successfully. Pass a ID in the query string or in the request body for a personalized response.";
            }
            else if (ID != null)
            {
                for (int i = 0; i < IDs.Length; i++)
                {
                    if (ID == IDs[i])
                    {

                        responseMessage = $"This HTTP triggered function executed successfully." +
                            $" Hello {names[i]}\nID : {IDs[i]}\nVaccine : {Vacinne[i]}\nVacinne Center : {VacinneCenter[i]}";

                        //// INSERT DATA IN AZURE TABLE
                        var obj = new Person()
                        {
                            PartitionKey = Guid.NewGuid().ToString(), // Must be unique
                            RowKey = Guid.NewGuid().ToString(),
                            Id = IDs[i],
                            Name = names[i],
                            Vaccince = Vacinne[i],
                            VacinneCenter = VacinneCenter[i]
                        };
                        msg.Add(responseMessage);
                        await msg1.AddAsync(obj);
                        i = IDs.Length;
                    }
                    else
                    {
                        responseMessage = $"This HTTP triggered function executed successfully.{ID} is an Invalid ID";
                    }
                }
            }
            

            return new OkObjectResult(responseMessage);
        }
    }
}
public class Person : TableEntity
{
    
    public string Name { get; set; }
    public string Id { get; set; }
    public string Vaccince { get; set; }
    public string VacinneCenter { get; set; }

}