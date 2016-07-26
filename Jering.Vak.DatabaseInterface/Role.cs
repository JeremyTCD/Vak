using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jering.Vak.DatabaseInterface
{
    public class Role
    {
        /// <summary>
        /// Default constructor 
        /// </summary>
        public Role()
        {
        }

        public int RoleId { get; set; }

        public string Name { get; set; }
    }
}
