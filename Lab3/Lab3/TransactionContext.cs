namespace Lab3;

public class TransactionContext : ITransactionContext
{
    public Guid TransactionId { get; }

    public TransactionContext()
    {
        TransactionId = Guid.NewGuid();
    }
}