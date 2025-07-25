using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using BCrypt.Net;

namespace MigratedJobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserEmployerController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Employer> _employers;
        private readonly IMongoCollection<Application> _applications;
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IMongoCollection<Job> _jobs;

        public UserEmployerController(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
            _employers = database.GetCollection<Employer>("employers");
            _applications = database.GetCollection<Application>("applications");
            _notifications = database.GetCollection<Notification>("notifications");
            _jobs = database.GetCollection<Job>("jobs");
        }

        [HttpPost("user")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Name, email, and password are required");

            var emailExists = await _users.Find(u => u.Email == user.Email).AnyAsync()
                            || await _employers.Find(e => e.Email == user.Email).AnyAsync();

            if (emailExists)
                return Conflict("An account with this email already exists");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            await _users.InsertOneAsync(user);
            user.Password = null;
            return Created("", user);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserData(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return NotFound("User not found");

            user.Password = null;
            return Ok(user);
        }

        [HttpPut("user/{userId}")]
        public async Task<IActionResult> UpdateUserData(string userId, [FromBody] User updatedUser)
        {
            if (updatedUser.Password != null)
                updatedUser.Password = BCrypt.Net.BCrypt.HashPassword(updatedUser.Password);

            var updateDef = Builders<User>.Update
                .Set(u => u.Name, updatedUser.Name)
                .Set(u => u.Email, updatedUser.Email)
                .Set(u => u.Password, updatedUser.Password);

            var result = await _users.UpdateOneAsync(u => u.Id == userId, updateDef);
            if (result.ModifiedCount == 0) return NotFound("User not found");

            updatedUser.Password = null;
            return Ok(updatedUser);
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            await _applications.DeleteManyAsync(a => a.UserId == userId);
            await _notifications.DeleteManyAsync(n => n.UserId == userId);
            var result = await _users.DeleteOneAsync(u => u.Id == userId);

            if (result.DeletedCount == 0)
                return NotFound("User not found");

            return Ok("User and related data deleted successfully");
        }

        [HttpPost("employer")]
        public async Task<IActionResult> AddEmployer([FromBody] Employer employer)
        {
            if (string.IsNullOrEmpty(employer.Name) || string.IsNullOrEmpty(employer.Email) || string.IsNullOrEmpty(employer.Password))
                return BadRequest("Name, email, password, and company name are required");

            var emailExists = await _employers.Find(e => e.Email == employer.Email).AnyAsync()
                            || await _users.Find(u => u.Email == employer.Email).AnyAsync();

            if (emailExists)
                return Conflict("An account with this email already exists");

            employer.Password = BCrypt.Net.BCrypt.HashPassword(employer.Password);
            await _employers.InsertOneAsync(employer);
            employer.Password = null;
            return Created("", employer);
        }

        [HttpGet("employer/{employerId}")]
        public async Task<IActionResult> GetEmployerData(string employerId)
        {
            var employer = await _employers.Find(e => e.Id == employerId).FirstOrDefaultAsync();
            if (employer == null) return NotFound("Employer not found");

            employer.Password = null;
            return Ok(employer);
        }

        [HttpPut("employer/{employerId}")]
        public async Task<IActionResult> UpdateEmployerData(string employerId, [FromBody] Employer updatedEmployer)
        {
            if (updatedEmployer.Password != null)
                updatedEmployer.Password = BCrypt.Net.BCrypt.HashPassword(updatedEmployer.Password);

            var updateDef = Builders<Employer>.Update
                .Set(e => e.Name, updatedEmployer.Name)
                .Set(e => e.Email, updatedEmployer.Email)
                .Set(e => e.Company, updatedEmployer.Company)
                .Set(e => e.Password, updatedEmployer.Password);

            var result = await _employers.UpdateOneAsync(e => e.Id == employerId, updateDef);
            if (result.ModifiedCount == 0) return NotFound("Employer not found");

            updatedEmployer.Password = null;
            return Ok(updatedEmployer);
        }

        [HttpDelete("employer/{employerId}")]
        public async Task<IActionResult> DeleteEmployer(string employerId)
        {
            await _jobs.DeleteManyAsync(j => j.EmployerId == employerId);
            var result = await _employers.DeleteOneAsync(e => e.Id == employerId);

            if (result.DeletedCount == 0)
                return NotFound("Employer not found");

            return Ok("Employer and related jobs deleted successfully");
        }
    }
}
