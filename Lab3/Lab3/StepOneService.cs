namespace Lab3;

public class StepOneService
{
    private readonly ITransactionContext _context;

    public StepOneService(ITransactionContext context)
    {
        _context = context;
    }

    public void Execute()
    {
        Console.WriteLine($"Step 1: Processing in transaction {_context.TransactionId}");
    }
}