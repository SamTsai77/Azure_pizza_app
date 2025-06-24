using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using CosmosWebApi.Models;

namespace CosmosWebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly Container _container;

    public MenuController(Container container)
    {
        _container = container;
    }

    [HttpPost]
    public async Task<IActionResult> AddMenu([FromBody] MenuItem menuItem)
    {
        var response = await _container.CreateItemAsync(menuItem, new PartitionKey(menuItem.category));
        return CreatedAtAction(nameof(GetMenu), new { id = menuItem.id }, response.Resource);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateItem(string id, [FromBody] MenuItem menuItem)
    {

        if (id != menuItem.id)
            return BadRequest("ID mismatch");

        try
        {
            // Replace the item entirely (overwrite)
            var response = await _container.ReplaceItemAsync(
                menuItem,
                id,
                new PartitionKey(menuItem.category)  // Or use the correct partition key field
            );

            return Ok(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound($"Item with id '{id}' not found.");
        }

    }

    [HttpGet("{id}/{category}")]
    public async Task<IActionResult> GetMenu(string id, String category)
    {
        try
        {
            var response = await _container.ReadItemAsync<MenuItem>(id, new PartitionKey(category));
            return Ok(response.Resource);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IEnumerable<MenuItem>> GetAll()
    {
        var query = _container.GetItemQueryIterator<MenuItem>("SELECT * FROM c");
        var results = new List<MenuItem>();
        while (query.HasMoreResults)
        {
            var page = await query.ReadNextAsync();
            results.AddRange(page);
        }
        return results;
    }
}
