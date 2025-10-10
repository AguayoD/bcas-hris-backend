using Model.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblPositionsService
    {
        private readonly tblPositionsRepository _positionsRepository = new tblPositionsRepository();


        public async Task<IEnumerable<tblPositions>> GetAll()
        {
            return await _positionsRepository.GetAll();
        }

        public async Task<tblPositions> GetById(int id)
        {
            return await _positionsRepository.GetById(id);
        }

        public async Task<tblPositions> Insert(tblPositions positions)
        {
            return await _positionsRepository.Insert(positions);
        }

        public async Task<tblPositions> Update(tblPositions positions)
        {
            return await _positionsRepository.Update(positions);
        }

        public async Task<tblPositions> DeleteById(int id)
        {
            return await _positionsRepository.DeleteById(id);
        }
    }
}
