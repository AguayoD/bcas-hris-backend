using Model.Models;
using Models.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblEmployeeService
    {
        private readonly tblEmployeesRepository _tblemployeesRepository = new tblEmployeesRepository();


        public async Task<IEnumerable<tblEmployees>> GetAll()
        {
            return await _tblemployeesRepository.GetAll();
        }

        public async Task<tblEmployees> GetById(int id)
        {
            return await _tblemployeesRepository.GetById(id);
        }

        public async Task<tblEmployees> Insert(tblEmployees tblemployee)
        {
            return await _tblemployeesRepository.Insert(tblemployee);
        }

        public async Task<tblEmployees> Update(tblEmployees tblemployee)
        {
            return await _tblemployeesRepository.Update(tblemployee);
        }

        public async Task<tblEmployees> DeleteById(int id)
        {
            return await _tblemployeesRepository.DeleteById(id);
        }
    }
}
