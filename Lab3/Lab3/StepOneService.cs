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
        // Step one logic
    }
}