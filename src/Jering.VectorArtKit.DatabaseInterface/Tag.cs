using System;
using Jering.Accounts.DatabaseInterface;
using System.Collections.Generic;

namespace Jering.VectorArtKit.DatabaseInterface
{
    public class Tag
    {
        public Tag()
        {
            VakUnitTags = new HashSet<VakUnitTag>();
        }

        public string Value { get; set; }
        public int TagId { get; set; }

        public byte[] RowVersion { get; set; }

        public virtual ICollection<VakUnitTag> VakUnitTags { get; set; }
    }
}
