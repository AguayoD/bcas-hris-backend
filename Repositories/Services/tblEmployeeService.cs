using Models.Models;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
            try
            {
                Console.WriteLine($"=== tblEmployeeService.Update ===");
                Console.WriteLine($"EmployeeID: {tblemployee.EmployeeID}");
                Console.WriteLine($"Employee data to update: {JsonSerializer.Serialize(tblemployee)}");

                var result = await _tblemployeesRepository.Update(tblemployee);

                Console.WriteLine($"Update result: {(result != null ? "Success" : "Null result")}");
                if (result != null)
                {
                    Console.WriteLine($"Updated employee: {JsonSerializer.Serialize(result)}");
                }
                else
                {
                    Console.WriteLine($"WARNING: Update returned null!");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in tblEmployeeService.Update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<tblEmployees> DeleteById(int id)
        {
            return await _tblemployeesRepository.DeleteById(id);
        }
    }
}