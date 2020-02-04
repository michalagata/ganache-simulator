namespace AnubisWorks.GanacheSimulator.Domain.Enums
{
    public enum TransactionStatus
    {
        Fail = 0,
        Done = 1
    }

    public enum TransactionDetails
    {
        Successfully = 1,
        Error = 2,
        InsufficientFunds = 3
    }
}