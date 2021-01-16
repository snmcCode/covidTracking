using MasjidTracker.FrontEnd.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FrontEnd.Interfaces
{
    public interface IEventsService
    {
        Task<List<EventModel>> GetEvents(string url, string targetResource);
        Task<int> RegisterInEvent(string url, string targetResource, string jsonBody);
        Task<string> UnregisterFromEvent(string url, string targetResource, string jsonBody);
        Task<string> UpdateEvent(string url, string targetResource, string jsonBody);
    }
}