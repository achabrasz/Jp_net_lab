namespace Lab3;

public class Worker2 (ICalculator calculator)
{
    private ICalculator calculator;
    
    public void SetCalculator(ICalculator calculator)
    {
        this.calculator = calculator;
    }
    
    public string Work(string a, string b)
    {
        return (calculator.Eval(a, b));
    }
}