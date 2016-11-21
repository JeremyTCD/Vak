using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.VectorArtKit.WebApi.ResponseModels.Account
{
    public class TwoFactorLogInResponseModel
    {
        public string Username { get; set; }
        public bool IsPersistent { get; set; }
    }
}
