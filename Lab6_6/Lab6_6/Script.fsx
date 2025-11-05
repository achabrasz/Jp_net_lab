#r "nuget: FSharp.Collections.ParallelSeq"
#r "nuget: XPlot.Plotly"

open System
open System.Net.Http
open System.Diagnostics
open FSharp.Collections.ParallelSeq
open XPlot.Plotly

// ========================
// I. PODSTAWY F#
// ========================

let y = 0
let nums = [1.0; 2.0; 3.0]
let sqr x = x * x

let sumOfSquaresI n =
    let mutable acc = 0.0
    for i in 1 .. n do
        acc <- acc + float (i * i)
    acc

let sumOfSquaresF n =
    [1 .. n]
    |> List.map (fun x -> x * x)
    |> List.sum

let sumOfSquaresInline = [1 .. 10] |> List.map (fun x -> x * x) |> List.sum

printfn $"sumOfSquaresI 10 = {sumOfSquaresI 10}"
printfn $"sumOfSquaresF 10 = {sumOfSquaresF 10}"
printfn $"sumOfSquaresInline = {sumOfSquaresInline}"

// ========================
// II. RÓWNOLEGŁOŚĆ i ASYNC
// ========================

// Użycie int64, żeby uniknąć overflow
let sumOfSquaresP n =
    [|1 .. n|]
    |> PSeq.map (fun x -> int64 x * int64 x)
    |> PSeq.sum

printfn $"sumOfSquaresP 1000000 = {sumOfSquaresP 1000000}"

// Funkcje asynchroniczne z HttpClient
let httpClient = new HttpClient()

let loadPrices (ticker: string) =
    let url = $"https://stooq.pl/q/d/l/?s={ticker}&i=d"
    httpClient.GetStringAsync(url).Result

let loadPricesAsync ticker =
    async {
        let url = $"https://stooq.pl/q/d/l/?s={ticker}&i=d"
        let! html = httpClient.GetStringAsync(url) |> Async.AwaitTask
        return html
    }

let tickers = ["MSFT"; "AAPL"; "GOOG"; "C"; "ORCL"; "EBAY"]

let measureTime name f =
    let sw = Stopwatch.StartNew()
    let r = f()
    sw.Stop()
    printfn $"{name} wykonano w {sw.ElapsedMilliseconds} ms"
    r

// Sekwencyjnie
measureTime "Sekwencyjnie" (fun () ->
    tickers
    |> List.iter (fun t ->
        let html = loadPrices t
        printfn $"{t}: {html.Length} znaków"))

// Równolegle
measureTime "Równolegle" (fun () ->
    tickers
    |> List.map loadPricesAsync
    |> Async.Parallel
    |> Async.RunSynchronously
    |> Array.iteri (fun i html -> printfn $"{tickers[i]}: {html.Length} znaków"))
