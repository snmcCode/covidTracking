using FrontEnd.Models;
using MasjidTracker.FrontEnd.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FrontEnd.Interfaces
{
    public interface IUserService
    {
        Task<Visitor> GetUser(string url, string targetResource);
        Task<Visitor> GetUsers(string url, string targetResource);
        Task<Guid?> RegisterUser(string url, Visitor visitor, string targetResource);
        Task<string> RequestCode(string url, SMSRequestModel requestModel, string targetResource);
        Task<VisitorPhoneNumberInfo> VerifyCode(string url, SMSRequestModel requestModel, string targetResource);
    }
}