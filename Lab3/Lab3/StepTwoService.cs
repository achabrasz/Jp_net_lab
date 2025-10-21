namespace Lab3;

public class StepTwoService
{
    private readonly ITransactionContext _context;

    public StepTwoService(ITransactionContext context)
    {
        _context = context;
    }

    public void Execute()
    {
        Console.WriteLine($"Step 2: Processing in transaction {_context.TransactionId}");
    }
}