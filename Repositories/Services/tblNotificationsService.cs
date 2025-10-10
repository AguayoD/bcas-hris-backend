using Model.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Service
{
    public class tblNotificationsService
    {
        private readonly tblNotificationsRepository _notificationsRepository = new tblNotificationsRepository();


        public async Task<IEnumerable<tblNotifications>> GetAll()
        {
            return await _notificationsRepository.GetAll();
        }

        public async Task<tblNotifications> GetById(int id)
        {
            return await _notificationsRepository.GetById(id);
        }

        public async Task<tblNotifications> Insert(tblNotifications notifications)
        {
            return await _notificationsRepository.Insert(notifications);
        }

        public async Task<tblNotifications> Update(tblNotifications notifications)
        {
            return await _notificationsRepository.Update(notifications);
        }

        public async Task<tblNotifications> DeleteById(int id)
        {
            return await _notificationsRepository.DeleteById(id);
        }
    }
}
