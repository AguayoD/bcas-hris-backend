using Model.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblContractsService
    {
        private readonly tblContractsRepository _contractsRepository = new tblContractsRepository();


        public async Task<IEnumerable<tblContracts>> GetAll()
        {
            return await _contractsRepository.GetAll();
        }

        public async Task<tblContracts> GetById(int id)
        {
            return await _contractsRepository.GetById(id);
        }

        public async Task<tblContracts> Insert(tblContracts contracts)
        {
            return await _contractsRepository.Insert(contracts);
        }

        public async Task<tblContracts> Update(tblContracts contracts)
        {
            return await _contractsRepository.Update(contracts);
        }

        public async Task<tblContracts> DeleteById(int id)
        {
            return await _contractsRepository.DeleteById(id);
        }
    }
}
