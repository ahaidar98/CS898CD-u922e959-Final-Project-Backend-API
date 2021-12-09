using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BabAl_SalamWebAPI.Models;

namespace BabAl_SalamWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public UserController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            //return await _context.Users.ToListAsync();
            var activeUsers = await _context.Users
                .Select(x => x.IsActive ? UserToDTO(x) : null)
                .ToListAsync();

            activeUsers.RemoveAll(item => item == null);

            return activeUsers;
        }

        // GET: api/User/5
        /*[HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }*/

        // PUT: api/User/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<ActionResult<ResponseResult>> PutUser([FromBody] UserDTO userDTO)
        {
            ResponseResult responseObj;

            var user = _context.Users.SingleOrDefault(e => e.Email == userDTO.Email); ;
            if(user == null)
            {
                return NotFound();
            }

            user.Status = userDTO.Status;
            user.IsActive = userDTO.IsActive;

            var success = await _context.SaveChangesAsync() > 0;

            if(success)
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"Successfully edited.",
                    MessageStanding = "green",
                    Data = new UserDataInformation
                    {
                        Users = GetUsers().Result.Value
                    }
                };
            } else
            {
                responseObj = new ResponseResult
                {
                    ResponseMessage = $"Edit Faile.",
                    MessageStanding = "red",
                };
            }

            return responseObj;
        }

        // POST: api/User
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /*[HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }*/

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private static UserDTO UserToDTO(User user) =>
            new UserDTO
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Id = user.Id,
                Status = user.Status,
                IsActive = user.IsActive,
                Location = user.Location
    };
    }
}
