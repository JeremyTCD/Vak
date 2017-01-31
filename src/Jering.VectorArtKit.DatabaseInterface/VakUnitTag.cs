namespace Jering.VectorArtKit.DatabaseInterface
{
    public class VakUnitTag
    {
        public int VakUnitId { get; set; }
        public int TagId { get; set; }
        public Tag Tag { get; set; }
        public VakUnit VakUnit { get; set; }
    }
}
