using Medior.Shared.Entities;
using Medior.Web.Server.Auth;
using Medior.Web.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Medior.Web.Server.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(DigitalSignatureFilterAttribute))]
    public class AccountController : ControllerBase
    {
        private readonly AppDb _appDb;

        public AccountController(AppDb appDb)
        {
            _appDb = appDb;
        }

        [HttpPost]
        public async Task<ActionResult<UserAccount>> Create(UserAccount account)
        {
            if (account.Id != Guid.Empty ||
                !Regex.IsMatch(account.Username, "^((\\w|-)* {0,1}([a-zA-Z0-9])+)+$") ||
                await _appDb.UserAccounts.AnyAsync(x => x.Username == account.Username))
            {
                return BadRequest();
            }

            _appDb.UserAccounts.Add(account);
            await _appDb.SaveChangesAsync();
            return account;
        }
    }
}
