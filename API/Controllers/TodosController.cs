using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private static List<Todo> todos = new List<Todo>()
        {
            new Todo{ Id = 1, Title = "todo 1", Completed = true },
            new Todo{ Id = 2, Title = "todo 2", Completed = false },
            new Todo{ Id = 3, Title = "todo 3", Completed = false },
        };

        // GET: api/<TodosController>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(todos);
        }

        // GET api/<TodosController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var todo = todos.FirstOrDefault(t => t.Id == id);
            return Ok(todo);
        }

        // POST api/<TodosController>
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            todo.Id = todos.Max(t => t.Id) + 1;
            todo.Completed = false;

            todos.Add(todo);
            return Created("", todo);
        }

        // PUT api/<TodosController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Todo todo)
        {
            if(id != todo.Id) return BadRequest(todo.Id);

            var existing = todos.FirstOrDefault(t => t.Id == id);
            if (existing is null) return NotFound();

            existing.Title = todo.Title;
            existing.Completed = todo.Completed;

            return NoContent();
        }

        // DELETE api/<TodosController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = todos.FirstOrDefault(t => t.Id == id);
            if (existing is null) return NotFound();

            todos.Remove(existing);

            return NoContent();
        }
    }
}
