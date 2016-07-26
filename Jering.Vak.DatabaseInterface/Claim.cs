using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Vak.DatabaseInterface
{
    public class Claim : System.Security.Claims.Claim
    {
        public Claim(int claimId, string type, string value) : base (type, value)
        {
            ClaimId = claimId;
        }

        public int ClaimId { get; }
    }
}
