﻿using API.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<RolesController> _logger;

        public RolesController(
            AppDbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<RolesController> logger
            )
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string name)
        {
            var roleExists = await _roleManager.RoleExistsAsync(name);

            if(roleExists)
            {
                return BadRequest(new { Error = "Role already exists" });
            }

            var roleResult = await _roleManager.CreateAsync(new IdentityRole { Name = name });

            if (!roleResult.Succeeded)
            {
                _logger.LogError($"Role {name} created successfully");
                return BadRequest(new { Message = $"Role {name} was not created" });
            }

            _logger.LogInformation($"Role {name} created successfully");
            return Ok(new { Message = $"Role {name} created successfully" });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        [Route("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole(string userEmail, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user is null)
            {
                return BadRequest(new { Error = "User does not exist" });
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if(!roleExists) 
            {
                return BadRequest(new { Error = "Role does not exist" });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if(!result.Succeeded) 
            {
                return BadRequest(new { Error = "something wrong happened" });
            }

            return Ok(new { Message = "User added to role successfully" });
        }
    }
}