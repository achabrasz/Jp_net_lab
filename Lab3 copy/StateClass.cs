namespace Lab3;

public class StateCalc : ICalculator
{
    private int _counter;

    public StateCalc(int start)
    {
        _counter = start;
    }

    public string Eval(string a, string b)
    {
        _counter++;
        return a + b + _counter;
    }
}