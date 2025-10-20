using System.Data;

namespace Lab3;

public class CatCalc : ICalculator
{
    public string Eval(string a, string b)
    {
        return a+b;
    }
}