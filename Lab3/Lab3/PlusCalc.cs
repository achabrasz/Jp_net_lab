namespace Lab3;

public class PlusCalc : ICalculator
{
    public string Eval(string a, string b)
    {
        int numA = int.Parse(a);
        int numB = int.Parse(b);
        return (numA + numB).ToString();
    }
}