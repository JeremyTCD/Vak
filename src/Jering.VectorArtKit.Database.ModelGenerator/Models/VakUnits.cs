using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class VakUnits
    {
        public VakUnits()
        {
            VakUnitTags = new HashSet<VakUnitTags>();
        }

        public int VakUnitId { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }

        public virtual ICollection<VakUnitTags> VakUnitTags { get; set; }
        public virtual Accounts Account { get; set; }
    }
}
