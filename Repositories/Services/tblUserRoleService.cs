using Model.Models;
using Models.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Services
{
    class tblUserRoleService
    {
        private readonly tblUserRoleRepository _RoleRepository = new tblUserRoleRepository();


        public async Task<IEnumerable<tblRoles>> GetAll()
        {
            return await _RoleRepository.GetAll();
        }

        public async Task<tblRoles> GetById(int id)
        {
            return await _RoleRepository.GetById(id);
        }

        public async Task<tblRoles> Insert(tblRoles roles)
        {
            return await _RoleRepository.Insert(roles);
        }

        public async Task<tblRoles> Update(tblRoles roles)
        {
            return await _RoleRepository.Update(roles);
        }

        public async Task<tblRoles> DeleteById(int id)
        {
            return await _RoleRepository.DeleteById(id);
        }
    }
}
