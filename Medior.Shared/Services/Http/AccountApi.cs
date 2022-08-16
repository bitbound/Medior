using Medior.Shared.Auth;
using Medior.Shared.Entities;
using Medior.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Services.Http
{
    public interface IAccountApi
    {
        HttpClient Client { get; }

        Task<Result<UserAccount>> CreateAccount(UserAccount account);
        Task<Result<UserAccount>> UpdatePublicKey(UserAccount account);
    }

    public class AccountApi : IAccountApi
    {
        private readonly HttpClient _client;
        private readonly ILogger<AccountApi> _logger;

        public AccountApi(
            HttpClient client, 
            ILogger<AccountApi> logger)
        {
            _client = client;
            _logger = logger;
        }

        public HttpClient Client => _client;

        public async Task<Result<UserAccount>> CreateAccount(UserAccount account)
        {
            try
            {
                var response = await _client.PostAsJsonAsync($"/api/Account", account);
                response.EnsureSuccessStatusCode();
                var newAccount = await response.Content.ReadFromJsonAsync<UserAccount>();
                if (newAccount is null)
                {
                    return Result.Fail<UserAccount>("Response was empty.");
                }
                return Result.Ok(newAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating account.");
                return Result.Fail<UserAccount>(ex);
            }
        }

        public async Task<Result<UserAccount>> UpdatePublicKey(UserAccount account)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"/api/Account", account);
                response.EnsureSuccessStatusCode();
                var newAccount = await response.Content.ReadFromJsonAsync<UserAccount>();
                if (newAccount is null)
                {
                    return Result.Fail<UserAccount>("Response was empty.");
                }
                return Result.Ok(newAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating public key.");
                return Result.Fail<UserAccount>(ex);
            }
        }
    }
}
