using Models.Models;
using Repositories.Repositories;

namespace Repositories.Service
{
    public class tblEducationalAttainmentService
    {
        private readonly tblEducationalAttainmentRepository _repository;

        public tblEducationalAttainmentService()
        {
            _repository = new tblEducationalAttainmentRepository();
        }

        public async Task<IEnumerable<tblEducationalAttainment>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<tblEducationalAttainment?> GetById(int id)
        {
            return await _repository.GetById(id);
        }

        public async Task<tblEducationalAttainment?> Insert(tblEducationalAttainment educationalAttainment)
        {
            return await _repository.Insert(educationalAttainment);
        }

        public async Task<tblEducationalAttainment?> Update(tblEducationalAttainment educationalAttainment)
        {
            return await _repository.Update(educationalAttainment);
        }

        public async Task<tblEducationalAttainment?> DeleteById(int id)
        {
            return await _repository.DeleteById(id);
        }
    }
}