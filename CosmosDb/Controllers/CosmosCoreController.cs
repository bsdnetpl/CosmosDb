using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Drawing;

namespace CosmosDb.Controllers
{
    public record Employee (string id, string name, string department, string address);
    [Route("api/[controller]")]
    [ApiController]
    public class CosmosCoreController : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult>Poost(Employee employee)
        {
            var cont = await GetContainer();
            await cont.CreateItemAsync(employee);
            return Accepted();
        }
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string id, [FromQuery] string partitionKey)
        {
            var cont = await GetContainer();
            var emp = await cont.ReadItemAsync<Employee>(id, new PartitionKey(partitionKey));
            return Ok(emp);
        }
        [HttpPut]
        public async Task<IActionResult> Put(Employee employee)
        {
            var container = await GetContainer();
            await container.UpsertItemAsync(employee);
            return Ok(employee);
        }
        [HttpGet("read")]
        public async Task <IActionResult> Query()
        {
            var sqlQuery = "SELECT * FROM  Employees e WHERE e.department = 'IT'";
            var employee =  GetEmployees(sqlQuery);
            return Ok(employee);

        }
        [HttpGet("read-linq")]
        public async Task<IActionResult> QueryLinq()
        {
            var container = await GetContainer();
            var emp = container.GetItemLinqQueryable<Employee>(true)
                .Where(e => e.department == "IT")
                .ToList();
            return Ok(emp);
        }

        private async  IAsyncEnumerable<Employee> GetEmployees(string sqlQery)
        {
            var container = await GetContainer();
            var empFilter = container.GetItemQueryIterator<Employee>(sqlQery);
            while(empFilter.HasMoreResults)
            {
                var response = await empFilter.ReadNextAsync();
                foreach(var employee in response)
                {
                    yield return employee;
                }
            }

        }
        private async Task<Container> GetContainer()
        {
            var ConnectionString = "AccountEndpoint=https://simple1ccne.documents.azure.com:443/;AccountKey=Fyh7kYGMfcWqj7zvBpQGSvHPNsLzfNdOHfrGLIg2bfwFZJ8G2nicVrsEdbwN8A4sV8zzNf9EVWWjACDbkqe0MA==;";
            CosmosClient CosmosClient = new CosmosClient(ConnectionString);
            Database database = await CosmosClient.CreateDatabaseIfNotExistsAsync("Employees");
            return await database.CreateContainerIfNotExistsAsync("Employees", "/department", 400);
        }
  

    }
}
