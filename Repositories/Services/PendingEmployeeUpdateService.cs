// [file name]: PendingEmployeeUpdateService.cs
using Models.Models;
using Repositories.Repositories;
using System.Text.Json;


namespace Repositories.Service
{
    public class PendingEmployeeUpdateService
    {
        private readonly tblPendingEmployeeUpdateRepository _pendingUpdateRepository;

        public PendingEmployeeUpdateService()
        {
            _pendingUpdateRepository = new tblPendingEmployeeUpdateRepository();
        }

        public async Task<IEnumerable<tblPendingEmployeeUpdate>> GetAllAsync()
        {
            return await _pendingUpdateRepository.GetAllAsync();
        }

        public async Task<tblPendingEmployeeUpdate> GetByIdAsync(int id)
        {
            return await _pendingUpdateRepository.GetByIdAsync(id);
        }

        public async Task<tblPendingEmployeeUpdate> SubmitUpdateAsync(int employeeId, Dictionary<string, object> updateData, Dictionary<string, object> originalData)
        {
            var pendingUpdate = new tblPendingEmployeeUpdate
            {
                EmployeeID = employeeId,
                UpdateData = JsonSerializer.Serialize(updateData),
                OriginalData = JsonSerializer.Serialize(originalData),
                Status = "pending",
                SubmittedAt = DateTime.Now
            };

            return await _pendingUpdateRepository.InsertAsync(pendingUpdate);
        }

        public async Task<tblPendingEmployeeUpdate> ApproveUpdateAsync(int pendingUpdateId, int reviewedBy, string reviewerName, string comments = null)
        {
            var pendingUpdate = await _pendingUpdateRepository.GetByIdAsync(pendingUpdateId);
            if (pendingUpdate == null)
                throw new ArgumentException($"Pending update with ID {pendingUpdateId} not found");

            pendingUpdate.Status = "approved";
            pendingUpdate.ReviewedAt = DateTime.Now;
            pendingUpdate.ReviewedBy = reviewedBy;
            pendingUpdate.ReviewerName = reviewerName;
            pendingUpdate.Comments = comments;

            return await _pendingUpdateRepository.UpdateAsync(pendingUpdate);
        }

        public async Task<tblPendingEmployeeUpdate> RejectUpdateAsync(int pendingUpdateId, int reviewedBy, string reviewerName, string comments = null)
        {
            var pendingUpdate = await _pendingUpdateRepository.GetByIdAsync(pendingUpdateId);
            if (pendingUpdate == null)
                throw new ArgumentException($"Pending update with ID {pendingUpdateId} not found");

            pendingUpdate.Status = "rejected";
            pendingUpdate.ReviewedAt = DateTime.Now;
            pendingUpdate.ReviewedBy = reviewedBy;
            pendingUpdate.ReviewerName = reviewerName;
            pendingUpdate.Comments = comments;

            return await _pendingUpdateRepository.UpdateAsync(pendingUpdate);
        }

        public async Task<IEnumerable<tblPendingEmployeeUpdate>> GetPendingUpdatesAsync()
        {
            return await _pendingUpdateRepository.GetByStatusAsync("pending");
        }

        public async Task<IEnumerable<tblPendingEmployeeUpdate>> GetEmployeeHistoryAsync(int employeeId)
        {
            return await _pendingUpdateRepository.GetByEmployeeIdAsync(employeeId);
        }

        public async Task CleanupOldUpdatesAsync()
        {
            await _pendingUpdateRepository.DeleteOldUpdatesAsync();
        }
    }
}