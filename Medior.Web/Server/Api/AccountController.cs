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
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly AppDb _appDb;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDb appDb, ILogger<AccountController> logger)
        {
            _appDb = appDb;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<UserAccount>> Create(UserAccount account)
        {
            using var scope = _logger.BeginScope(nameof(Create));

            if (account.Id != Guid.Empty)
            {
                _logger.LogWarning("Attempted to create new account with populated Id: {guid}", account.Id);
                return BadRequest();
            }

            if (!Regex.IsMatch(account.Username, "^((\\w|-)* {0,1}([a-zA-Z0-9])+)+$"))
            {
                _logger.LogWarning("Username is invalid: {username}", account.Username);
                return BadRequest();
            }

            if (await _appDb.UserAccounts.AnyAsync(x => x.Username == account.Username))
            {
                _logger.LogWarning("Username already exists: {username}", account.Username);
                return BadRequest();
            }

            _appDb.UserAccounts.Add(account);
            await _appDb.SaveChangesAsync();
            return account;
        }
    }
}
