using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using Setre.Common;
using Setre.Models.Models;
using Setre.UI.AuthProvider;
using Setre.UI.Services.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Setre.UI.Services.Implementation
{
    public class AuthenticationHttpService : IAuthenticationHttpService
    {
        private readonly HttpClient _client;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationHttpService(HttpClient client, AuthenticationStateProvider authStateProvider, ILocalStorageService localStorage)
        {
            _client = client;
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<Result<SignInResponseModel>> Login(SignInModel model)
        {
            var content = JsonConvert.SerializeObject(model);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var authResult = await _client.PostAsync("account/signin", bodyContent);
            var authContent = await authResult.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SignInResponseModel>(authContent);
            if (!authResult.IsSuccessStatusCode)
                return new Result<SignInResponseModel>(false, ResultConstant.InvalidAuthentication);

            await _localStorage.SetItemAsync("authToken", result.Token);
            await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
            return new Result<SignInResponseModel>(true, "Welcome");   
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _client.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<Result<IEnumerable<string>>> RegisterUser(SignUpModel model)
        {
            var content = JsonConvert.SerializeObject(model);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var registrationResult = await _client.PostAsync("account/signup", bodyContent);
            var registrationContent = await registrationResult.Content.ReadAsStringAsync();
            if (!registrationResult.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<IEnumerable<string>>(registrationContent);
                return new Result<IEnumerable<string>>(false,ResultConstant.RecordCreateNotSuccessfully,result);
            }
            return new Result<IEnumerable<string>>(true,ResultConstant.RecordCreateSuccessfully);
        }

        public async Task<string> RefreshToken()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
            var tokenDto = JsonConvert.SerializeObject(new RefreshTokenDto { Token = token, RefreshToken = refreshToken });
            var bodyContent = new StringContent(tokenDto, Encoding.UTF8, "application/json");
            var refreshResult = await _client.PostAsync("token/refresh", bodyContent);
            var refreshContent = await refreshResult.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<SignInResponseModel>(refreshContent);
            if (!refreshResult.IsSuccessStatusCode)
                throw new ApplicationException("Something went wrong during the refresh token action");
            await _localStorage.SetItemAsync("authToken", result.Token);
            await _localStorage.SetItemAsync("refreshToken", result.RefreshToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
            return result.Token;
        }

        public async Task<Result<string>> GetUserIdbyName(string userName)
        {
            var content = JsonConvert.SerializeObject(userName);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");
            var registrationResult = await _client.PostAsync("account/getuserid", bodyContent);
            var registrationContent = await registrationResult.Content.ReadAsStringAsync();
            if (registrationResult.IsSuccessStatusCode)
            {
               
                return new Result<string>(false, ResultConstant.IdNotNull,registrationContent);
            }
            return new Result<string>(true, "User Not Found");
        }
    }
}
