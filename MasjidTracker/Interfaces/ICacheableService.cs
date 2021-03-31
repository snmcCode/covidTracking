using Common.Models;
using MasjidTracker.FrontEnd.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrontEnd.Interfaces
{
    public interface ICacheableService
    {
        Task<string> GetSetting(string url, string domain, string key, string targetResource, Setting mysetting);
        Task<List<Organization>> GetOrgs(string url, string targetResource);
        Task<List<StatusModel>> GetStatuses(string url, string targetResource);
    }
}
