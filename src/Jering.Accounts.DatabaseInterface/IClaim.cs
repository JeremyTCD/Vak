namespace Jering.Accounts.DatabaseInterface
{
    public interface IClaim
    {
        string Type { get; set; }
        string Value { get; set; }
    }
}
