//// Controllers/ApplicationController.cs
//using Microsoft.AspNetCore.Mvc;
//using MongoDB.Driver;
//using MigratedJobPortalAPI.Models;

//[ApiController]
//[Route("api/[controller]")]
//public class ApplicationController : ControllerBase
//{
//    private readonly IMongoCollection<Application> _applications;
//    private readonly IMongoCollection<Job> _jobs;
//    private readonly IMongoCollection<User> _users;

//    public ApplicationController(IMongoDatabase db)
//    {
//        _applications = db.GetCollection<Application>("Applications");
//        _jobs = db.GetCollection<Job>("Jobs");
//        _users = db.GetCollection<User>("Users");
//    }

//    [HttpPost("apply")]
//    public async Task<IActionResult> ApplyForJob([FromBody] ApplyRequest request)
//    {
//        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.JobId))
//            return BadRequest(new { message = "User ID and Job ID are required." });

//        // Validate ObjectId format if needed

//        var user = await _users.Find(u => u.Id == request.UserId).FirstOrDefaultAsync();
//        var job = await _jobs.Find(j => j.Id == request.JobId).FirstOrDefaultAsync();

//        if (user == null)
//            return NotFound(new { message = "User not found." });
//        if (job == null)
//            return NotFound(new { message = "Job not found." });

//        var existing = await _applications.Find(a => a.UserId == request.UserId && a.JobId == request.JobId).FirstOrDefaultAsync();
//        if (existing != null)
//            return Conflict(new { message = "You have already applied for this job." });

//        var application = new Application
//        {
//            UserId = request.UserId,
//            JobId = request.JobId,
//            Employer = job.EmployerName,
//            EmployerId = job.EmployerId,
//            Status = "Applied"
//        };

//        await _applications.InsertOneAsync(application);
//        // TODO: changeApplicantCount(job.Id);

//        return Created("", new { message = "Application submitted successfully.", application });
//    }

//    [HttpGet("user/{userId}/applied")]
//    public async Task<IActionResult> GetUserAppliedJobs(string userId, int page = 1, int limit = 5)
//    {
//        // Validate ObjectId format if needed

//        var totalApplications = await _applications.CountDocumentsAsync(a => a.UserId == userId);
//        var applications = await _applications.Find(a => a.UserId == userId)
//            .Skip((page - 1) * limit)
//            .Limit(limit)
//            .ToListAsync();

//        // TODO: Populate job details if needed

//        return Ok(new
//        {
//            currentPage = page,
//            totalPages = (int)Math.Ceiling((double)totalApplications / limit),
//            totalApplications,
//            jobs = applications
//        });
//    }

//    [HttpPut("{applicationId}/status")]
//    public async Task<IActionResult> UpdateApplicationStatus(string applicationId, [FromBody] StatusRequest request)
//    {
//        var validStatuses = new[] { "In Progress", "Accepted", "Rejected" };
//        if (!validStatuses.Contains(request.Status))
//            return BadRequest(new { message = "Invalid status value." });

//        var application = await _applications.Find(a => a.Id == applicationId).FirstOrDefaultAsync();
//        if (application == null)
//            return NotFound(new { message = "Application not found." });

//        if (application.Status == request.Status)
//            return Ok(new { message = "Status is already up to date.", application });

//        // TODO: Handle vacancy logic as in Node.js

//        application.Status = request.Status;
//        await _applications.ReplaceOneAsync(a => a.Id == applicationId, application);

//        // TODO: addNotification(application.UserId, ...);

//        return Ok(new { message = "Application status updated and user notified.", application });
//    }

//    [HttpDelete("{applicationId}")]
//    public async Task<IActionResult> RevokeApplication(string applicationId)
//    {
//        var application = await _applications.Find(a => a.Id == applicationId).FirstOrDefaultAsync();
//        if (application == null)
//            return NotFound(new { message = "Application not found" });

//        // TODO: changeApplicantCount(application.JobId, 'dec');
//        // TODO: If accepted, changeVacancyCount(application.JobId, 'inc');

//        await _applications.DeleteOneAsync(a => a.Id == applicationId);

//        return Ok(new { message = "Application revoked successfully" });
//    }
//}

//// Request DTOs
//public class ApplyRequest
//{
//    public string UserId { get; set; }
//    public string JobId { get; set; }
//}

//public class StatusRequest
//{
//    public string Status { get; set; }
//}