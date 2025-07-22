using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using Microsoft.Extensions.Configuration;

namespace MigratedJobPortalAPI.Utils
{
    public class JobUtils
    {
        private static IMongoCollection<Job> _jobCollection;

        public JobUtils(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
            _jobCollection = database.GetCollection<Job>("Jobs");
        }

        public static async Task ChangeApplicantCount(string jobId, string action = "inc")
        {
            try
            {
                var objectId = new ObjectId(jobId);
                var incrementValue = action == "dec" ? -1 : 1;

                var update = Builders<Job>.Update.Inc(j => j.ApplicantCount, incrementValue);
                var result = await _jobCollection.UpdateOneAsync(j => j.Id == objectId.ToString(), update); // FIXED

                if (result.ModifiedCount == 0)
                {
                    Console.WriteLine($"[ChangeApplicantCount] No job found with ID: {jobId}");
                }
                else
                {
                    Console.WriteLine($"[ChangeApplicantCount] Successfully {(action == "dec" ? "decremented" : "incremented")} applicantCount for job {jobId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChangeApplicantCount] Error: {ex.Message}");
            }
        }

        public static async Task ChangeVacancyCount(string jobId, string action = "inc")
        {
            try
            {
                var objectId = new ObjectId(jobId);
                var incrementValue = action == "dec" ? -1 : 1;

                var update = Builders<Job>.Update.Inc(j => j.Vacancies, incrementValue);
                var result = await _jobCollection.UpdateOneAsync(j => j.Id == objectId.ToString(), update); // FIXED

                if (result.ModifiedCount == 0)
                {
                    Console.WriteLine($"[ChangeVacancyCount] No job found with ID: {jobId}");
                }
                else
                {
                    Console.WriteLine($"[ChangeVacancyCount] Successfully {(action == "dec" ? "decremented" : "incremented")} vacancies for job {jobId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChangeVacancyCount] Error: {ex.Message}");
            }
        }
    }
}
