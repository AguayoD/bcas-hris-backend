using Model.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblBackupRecordsService
    {
        private readonly tblBackupRecordsRepository _tblBackupRecordsRepository = new tblBackupRecordsRepository();


        public async Task<IEnumerable<tblBackupRecords>> GetAll()
        {
            return await _tblBackupRecordsRepository.GetAll();
        }

        public async Task<tblBackupRecords> GetById(int id)
        {
            return await _tblBackupRecordsRepository.GetById(id);
        }

        public async Task<tblBackupRecords> Insert(tblBackupRecords backupRecords)
        {
            return await _tblBackupRecordsRepository.Insert(backupRecords);
        }

        public async Task<tblBackupRecords> Update(tblBackupRecords backupRecords)
        {
            return await _tblBackupRecordsRepository.Update(backupRecords);
        }

        public async Task<tblBackupRecords> DeleteById(int id)
        {
            return await _tblBackupRecordsRepository.DeleteById(id);
        }

        public async Task Insert(tblBackupRecordsService tblBackupRecordsService)
        {
            throw new NotImplementedException();
        }
    }
}
