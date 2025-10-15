using Models.Models;
using Repositories.Repositories;

namespace Repositories.Service
{
    public class tblEmploymentStatusService
    {
        private readonly tblEmploymentStatusRepository _repository;

        public tblEmploymentStatusService()
        {
            _repository = new tblEmploymentStatusRepository();
        }

        public async Task<IEnumerable<tblEmploymentStatus>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<tblEmploymentStatus?> GetById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task<tblEmploymentStatus?> Insert(tblEmploymentStatus employmentStatus)
        {
            return await _repository.Insert(employmentStatus);
        }

        public async Task<tblEmploymentStatus?> Update(tblEmploymentStatus employmentStatus)
        {
            return await _repository.Update(employmentStatus);
        }

        public async Task<tblEmploymentStatus?> DeleteById(int id)
        {
            return await _repository.DeleteById(id);
        }
    }
}