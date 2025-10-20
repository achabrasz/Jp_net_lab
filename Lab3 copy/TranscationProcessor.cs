namespace Lab3;

public class TransactionProcessor
{
    private readonly StepOneService _stepOne;
    private readonly StepTwoService _stepTwo;

    public TransactionProcessor(StepOneService stepOne, StepTwoService stepTwo)
    {
        _stepOne = stepOne;
        _stepTwo = stepTwo;
    }

    public void Process()
    {
        _stepOne.Execute();
        _stepTwo.Execute();
    }
}