using Model.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblActivityLogsService
    {
        private readonly tblActivityLogsRepository _activityLogsRepository = new tblActivityLogsRepository();

        public async Task<IEnumerable<tblActivityLogs>> GetAll()
        {
            return await _activityLogsRepository.GetAll();
        }

        public async Task<tblActivityLogs> GetById(int id)
        {
            return await _activityLogsRepository.GetById(id);
        }

        public async Task<tblActivityLogs> DeleteById(int id)
        {
            return await _activityLogsRepository.DeleteById(id);
        }
    }
}
