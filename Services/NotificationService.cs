using MongoDB.Driver;
using MigratedJobPortalAPI.Models;
using System;
using System.Threading.Tasks;

namespace MigratedJobPortalAPI.Services
{
    public class NotificationService
    {
        private readonly IMongoCollection<Notification> _notificationCollection;

        public NotificationService(IMongoDatabase database)
        {
            _notificationCollection = database.GetCollection<Notification>("notifications");
        }

        public async Task AddNotification(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message
            };

            try
            {
                await _notificationCollection.InsertOneAsync(notification);
                Console.WriteLine($"[NotificationService] Notification sent to user {userId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Failed to send notification to user {userId}: {ex.Message}");
            }
        }
    }
}
