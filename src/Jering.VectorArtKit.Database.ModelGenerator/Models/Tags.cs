using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class Tags
    {
        public Tags()
        {
            VakUnitTags = new HashSet<VakUnitTags>();
        }

        public string Value { get; set; }
        public int TagId { get; set; }

        public virtual ICollection<VakUnitTags> VakUnitTags { get; set; }
    }
}
