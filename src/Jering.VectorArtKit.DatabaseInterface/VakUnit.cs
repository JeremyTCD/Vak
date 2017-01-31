using System;
using Jering.Accounts.DatabaseInterface;
using System.Collections.Generic;

namespace Jering.VectorArtKit.DatabaseInterface
{
    public class VakUnit 
    {
        public VakUnit()
        {
            VakUnitTags = new HashSet<VakUnitTag>();
        }

        public int VakUnitId { get; set; }

        public string Name { get; set; }

        public int AccountId { get; set; }

        public byte[] RowVersion { get; set; }

        public VakAccount VakAccount { get; set; }

        public virtual ICollection<VakUnitTag> VakUnitTags { get; set; }
    }
}
