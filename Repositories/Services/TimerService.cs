// Repositories/Services/TimerService.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Repositories.Services
{
    public class TimerService
    {
        private readonly ContractNotificationService _notificationService;
        private Timer _timer;

        public TimerService(ContractNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Start()
        {
            // Start immediately, then run every 24 hours
            _timer = new Timer(async _ => await CheckContracts(), null, TimeSpan.Zero, TimeSpan.FromHours(24));
            Console.WriteLine("Contract notification timer started - will run every 24 hours");
        }

        public void Stop()
        {
            _timer?.Dispose();
            Console.WriteLine("Contract notification timer stopped");
        }

        private async Task CheckContracts()
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now}] Checking for expiring contracts...");
                await _notificationService.CheckAndSendContractNotifications();
                Console.WriteLine($"[{DateTime.Now}] Contract expiration check completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Error in contract check: {ex.Message}");
            }
        }
    }
}