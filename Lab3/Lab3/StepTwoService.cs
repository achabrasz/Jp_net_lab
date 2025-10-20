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
        // Step two logic
    }
}