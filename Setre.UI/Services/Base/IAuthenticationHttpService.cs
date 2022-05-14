using Setre.Common;
using Setre.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Setre.UI.Services.Base
{
    public interface IAuthenticationHttpService
    {
        Task<Result<IEnumerable<string>>> RegisterUser(SignUpModel model);
        Task<Result<SignInResponseModel>> Login(SignInModel model);
        Task Logout();
        Task<string> RefreshToken();
        Task<Result<string>> GetUserIdbyName(string userName);
    }
}
