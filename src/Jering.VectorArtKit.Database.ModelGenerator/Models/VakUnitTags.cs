using System;
using System.Collections.Generic;

namespace Jering.VectorArtKit.Database.ModelGenerator.Models
{
    public partial class VakUnitTags
    {
        public int VakUnitId { get; set; }
        public int TagId { get; set; }

        public virtual Tags Tag { get; set; }
        public virtual VakUnits VakUnit { get; set; }
    }
}
