using System;
using System.Collections.Generic;
using System.Text;

namespace Setre.Models.Models
{
    public class SignInResponseModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
