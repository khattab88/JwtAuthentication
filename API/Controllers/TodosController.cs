using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "User"
        )]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TodosController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/<TodosController>
        [HttpGet]
        public IActionResult Get()
        {
            var todos = _db.Todos.ToList();

            return Ok(todos);
        }

        // GET api/<TodosController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var todo = _db.Todos.FirstOrDefault(t => t.Id == id);
            return Ok(todo);
        }

        // POST api/<TodosController>
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            _db.Todos.Add(todo);
            _db.SaveChanges();

            return Created("", todo);
        }

        // PUT api/<TodosController>/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Todo todo)
        {
            if(id != todo.Id) return BadRequest(todo.Id);

            var existing = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (existing is null) return NotFound();

            existing.Title = todo.Title;
            existing.Completed = todo.Completed;

            _db.SaveChanges();

            return NoContent();
        }

        // DELETE api/<TodosController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var existing = _db.Todos.FirstOrDefault(t => t.Id == id);
            if (existing is null) return NotFound();

            _db.Todos.Remove(existing);
            _db.SaveChanges();

            return NoContent();
        }
    }
}
