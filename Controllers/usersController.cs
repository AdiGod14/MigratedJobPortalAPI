using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using BCrypt.Net;
using System;
using System.Threading.Tasks;

namespace MigratedJobPortalAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersAndEmployersController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Employer> _employers;
        private readonly IMongoCollection<Application> _applications;
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IMongoCollection<Job> _jobs;

        private const int SALT_ROUNDS = 10;

        public UsersAndEmployersController(MongoDbContext context)
        {
            _users = context.Users;
            _employers = context.Employers;
            _applications = context.Applications;
            _notifications = context.Notifications;
            _jobs = context.Jobs;
        }

        // ----------------- USER SECTION -----------------

        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
                return BadRequest(new { message = "Name, email, and password are required" });

            var existingUser = await _users.Find(u => u.Email == user.Email).FirstOrDefaultAsync();
            var existingEmployer = existingUser == null
                ? await _employers.Find(e => e.Email == user.Email).FirstOrDefaultAsync()
                : null;

            if (existingUser != null || existingEmployer != null)
                return Conflict(new { message = "An account with this email already exists" });

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, SALT_ROUNDS);
            await _users.InsertOneAsync(user);

            user.Password = null; // Don't return password
            return CreatedAtAction(nameof(GetUserData), new { userId = user.Id }, new { message = "User added successfully", user });
        }

        [HttpGet("getUser/{userId}")]
        public async Task<IActionResult> GetUserData(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required" });

            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.Password = null;
            return Ok(new { message = "User data retrieved successfully", user });
        }

        [HttpPut("updateUser/{userId}")]
        public async Task<IActionResult> UpdateUserData(string userId, [FromBody] User updatedData)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required" });

            if (!string.IsNullOrEmpty(updatedData.Password))
                updatedData.Password = BCrypt.Net.BCrypt.HashPassword(updatedData.Password, SALT_ROUNDS);

            var result = await _users.FindOneAndReplaceAsync(u => u.Id == userId, updatedData, new FindOneAndReplaceOptions<User> { ReturnDocument = ReturnDocument.After });

            if (result == null)
                return NotFound(new { message = "User not found" });

            result.Password = null;
            return Ok(new { message = "User updated successfully", user = result });
        }

        [HttpDelete("deleteUser/{userId}")]
        public async Task<IActionResult> DeleteUserData(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID is required" });

            var appDeleteResult = await _applications.DeleteManyAsync(a => a.UserId == userId);
            var notificationDeleteResult = await _notifications.DeleteManyAsync(n => n.UserId == userId);
            var userDeleteResult = await _users.DeleteOneAsync(u => u.Id == userId);

            if (userDeleteResult.DeletedCount == 0)
                return NotFound(new { message = "Failed to delete user" });

            return Ok(new
            {
                message = "User and associated applications deleted successfully",
                deletedApplications = appDeleteResult.DeletedCount,
                deletedNotifications = notificationDeleteResult.DeletedCount
            });
        }

        // ----------------- EMPLOYER SECTION -----------------

        [HttpPost("addEmployer")]
        public async Task<IActionResult> AddEmployer([FromBody] Employer employer)
        {
            if (string.IsNullOrEmpty(employer.Name) || string.IsNullOrEmpty(employer.Email) || string.IsNullOrEmpty(employer.Password))
                return BadRequest(new { message = "Name, email, and password are required" });

            var existingEmployer = await _employers.Find(e => e.Email == employer.Email).FirstOrDefaultAsync();
            var existingUser = existingEmployer == null
                ? await _users.Find(u => u.Email == employer.Email).FirstOrDefaultAsync()
                : null;

            if (existingEmployer != null || existingUser != null)
                return Conflict(new { message = $"An account already exists with this {employer.Email}" });

            employer.Password = BCrypt.Net.BCrypt.HashPassword(employer.Password, SALT_ROUNDS);
            await _employers.InsertOneAsync(employer);

            employer.Password = null;
            return CreatedAtAction(nameof(GetEmployerData), new { employerId = employer.Id }, new { message = "Employer added successfully", employer });
        }

        [HttpGet("getEmployer/{employerId}")]
        public async Task<IActionResult> GetEmployerData(string employerId)
        {
            if (string.IsNullOrEmpty(employerId))
                return BadRequest(new { message = "Employer ID is required" });

            var employer = await _employers.Find(e => e.Id == employerId).FirstOrDefaultAsync();
            if (employer == null)
                return NotFound(new { message = "Employer not found" });

            employer.Password = null;
            return Ok(new { message = "Employer data retrieved successfully", employer });
        }

        [HttpPut("updateEmployer/{employerId}")]
        public async Task<IActionResult> UpdateEmployerData(string employerId, [FromBody] Employer updatedData)
        {
            if (string.IsNullOrEmpty(employerId))
                return BadRequest(new { message = "Employer ID is required" });

            if (!string.IsNullOrEmpty(updatedData.Password))
                updatedData.Password = BCrypt.Net.BCrypt.HashPassword(updatedData.Password, SALT_ROUNDS);

            var result = await _employers.FindOneAndReplaceAsync(e => e.Id == employerId, updatedData, new FindOneAndReplaceOptions<Employer> { ReturnDocument = ReturnDocument.After });

            if (result == null)
                return NotFound(new { message = "Employer not found" });

            result.Password = null;
            return Ok(new { message = "Employer updated successfully", employer = result });
        }

        [HttpDelete("deleteEmployer/{employerId}")]
        public async Task<IActionResult> DeleteEmployerData(string employerId)
        {
            if (string.IsNullOrEmpty(employerId))
                return BadRequest(new { message = "Employer ID is required" });

            var jobDeleteResult = await _jobs.DeleteManyAsync(j => j.EmployerId == employerId);
            var employerDeleteResult = await _employers.DeleteOneAsync(e => e.Id == employerId);

            if (employerDeleteResult.DeletedCount == 0)
                return NotFound(new { message = "Failed to delete employer" });

            return Ok(new
            {
                message = "Employer and associated jobs deleted successfully",
                deletedJobs = jobDeleteResult.DeletedCount
            });
        }
    }
}
