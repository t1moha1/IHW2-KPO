using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MyApp.Controllers
{
    public class Data
    {
        public string? Value { get; set;}
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private static readonly List<string> _items = new();

        // GET api/items
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            if (_items.Count == 0)
                return Ok(new[] { "No items available yet." });

            return Ok(_items);
        }

        // POST api/items
        [HttpPost]
        public ActionResult<string> Post([FromBody] Data data_obj)
        {
            string value = data_obj.Value;    

            if (string.IsNullOrWhiteSpace(value))
                return BadRequest("Please provide a non-empty value.");

            _items.Add(value);
            return Ok($"Item \"{value}\" added. There are now {_items.Count} item(s).");
        }
    }
}
