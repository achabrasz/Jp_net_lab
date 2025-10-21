using Autofac;

namespace Lab3;

public class TransactionProcessor
{
    private readonly ILifetimeScope _scope;

    public TransactionProcessor(ILifetimeScope scope)
    {
        _scope = scope;
    }

    public void Process()
    {
        using (var transactionScope = _scope.BeginLifetimeScope("transaction"))
        {
            var stepOne = transactionScope.Resolve<StepOneService>();
            var stepTwo = transactionScope.Resolve<StepTwoService>();
            stepOne.Execute();
            stepTwo.Execute();
        }
    }
}