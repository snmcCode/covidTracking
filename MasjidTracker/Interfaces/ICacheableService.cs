using Common.Models;
using MasjidTracker.FrontEnd.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FrontEnd.Interfaces
{
    public interface ICacheableService
    {
        Task<string> GetSetting(string url, string domain, string key, string targetResource, Setting mysetting);
    }
}
