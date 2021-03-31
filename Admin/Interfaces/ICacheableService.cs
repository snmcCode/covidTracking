using common.Models;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace Admin.Interfaces
{
    public interface ICacheableService
    {
         Task<List<StatusInfo>> GetStatuses(string url, string targetResource);
    }
}