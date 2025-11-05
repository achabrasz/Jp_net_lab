Imports Library
Imports Microsoft.FSharp.Control

Module Program
    Sub Main()
        Dim analyzer = New StockAnalyzer()
        Dim tickers = {"msft.us", "aapl.us", "goog.us", "orcl.us", "ebay.us", "c.us"}

        Console.WriteLine("=== SYNCHRONICZNIE ===")
        For Each t In tickers
            Dim ret = analyzer.GetReturn(t)
            Dim vol = analyzer.GetVolatility(t)
            Console.WriteLine($"{t}: Zwrot = {ret:F2}%  Zmienność = {vol:F2}")
        Next

        Console.WriteLine()
        Console.WriteLine("=== ASYNCHRONICZNIE ===")
        Dim tasks = tickers.Select(Function(t)
            Return FSharpAsync.StartAsTask(analyzer.LoadPricesAsync(t), Nothing, Nothing)
        End Function).ToArray()

        Dim results = Threading.Tasks.Task.WhenAll(tasks).Result

        For i = 0 To tickers.Length - 1
            Console.WriteLine($"{tickers(i)}: {results(i).Count} rekordów")
        Next

        Console.WriteLine("Gotowe!")
        Console.ReadLine()
    End Sub
End Module
