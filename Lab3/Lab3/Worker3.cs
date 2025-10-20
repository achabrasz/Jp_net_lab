namespace Lab3;

public class Worker3
{
    public ICalculator calculator;
    public string Work(string a, string b)
    {
        return (calculator.Eval(a, b));
    }
}