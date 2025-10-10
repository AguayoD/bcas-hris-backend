using Model.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblDepartmentService
    {
        private readonly tblDepartmentRepository _departmentRepository = new tblDepartmentRepository();


        public async Task<IEnumerable<tblDepartment>> GetAll()
        {
            return await _departmentRepository.GetAll();
        }

        public async Task<tblDepartment> GetById(int id)
        {
            return await _departmentRepository.GetById(id);
        }

        public async Task<tblDepartment> Insert(tblDepartment department)
        {
            return await _departmentRepository.Insert(department);
        }

        public async Task<tblDepartment> Update(tblDepartment department)
        {
            return await _departmentRepository.Update(department);
        }

        public async Task<tblDepartment> DeleteById(int id)
        {
            return await _departmentRepository.DeleteById(id);
        }
    }
}
