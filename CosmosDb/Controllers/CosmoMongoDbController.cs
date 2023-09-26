using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace CosmosDb.Controllers
{
    public record Product(string Id, string Category, string Name, int Quantity, bool Sales);

    [Route("api/[controller]")]
    [ApiController]
    public class CosmoMongoDbController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Product product)
        {
            var products = GetCollection();
            products.InsertOne(product);
            return Accepted();

        }
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var products = GetCollection();
           var product = (await products.FindAsync(p => p.Id == id)).First();
            return Ok(product);
        }

        [HttpGet("query-linq")]
        public async Task<IActionResult> GetByQueryLinq()
        {
            var products = GetCollection();
            var result = products.AsQueryable()
                .Where(p => p.Category == "IT")
                .OrderByDescending(p => p.Id)
                .ToList();
            return Ok(result);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var products = GetCollection();
            await products.DeleteOneAsync(p => p.Id == id);
            return NoContent();

        }
        private IMongoCollection<Product>GetCollection()
        {
            var ConnectionString = "mongodb://wecosmosmdb2:41TNlGbhQRScNRPy5jUHsH5cYL2X5S9Ou05OgYfTeqSJ5jfA4Iwwkdm2CQSLLSm164AtTF38Taa7ACDbuXJoIw==@wecosmosmdb2.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@wecosmosmdb2@";
            MongoClient client = new MongoClient(ConnectionString);
            IMongoDatabase mongoDatabase = client.GetDatabase("ProductDB");
            return mongoDatabase.GetCollection<Product>("Products");
        }

    }
}
