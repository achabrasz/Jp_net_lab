module Library

open System
open System.Net.Http
open System.Threading.Tasks

type StockData = { Date: string; Open: float; Close: float }

type StockAnalyzer() =

    let httpClient = new HttpClient()  // Changed from 'static let' to 'let'

    member _.LoadPrices (ticker: string) =
        let url = $"https://stooq.pl/q/d/l/?s={ticker}&i=d"
        let csv = httpClient.GetStringAsync(url).Result
        csv.Split('\n')
        |> Array.skip 1
        |> Array.choose (fun line ->
            let parts = line.Split(',')
            if parts.Length >= 5 then
                Some { Date = parts[0]; Open = float parts[1]; Close = float parts[4] }
            else None)
        |> Array.toList

    member _.LoadPricesAsync (ticker: string) =
        async {
            let url = $"https://stooq.pl/q/d/l/?s={ticker}&i=d"
            let! csv = httpClient.GetStringAsync(url) |> Async.AwaitTask
            return
                csv.Split('\n')
                |> Array.skip 1
                |> Array.choose (fun line ->
                    let parts = line.Split(',')
                    if parts.Length >= 5 then
                        Some { Date = parts[0]; Open = float parts[1]; Close = float parts[4] }
                    else None)
                |> Array.toList
        }

    member this.GetReturn (ticker: string) =
        let data = this.LoadPrices ticker
        if data.Length > 1 then
            let first = data |> List.head
            let last = data |> List.last
            (last.Close - first.Open) / first.Open * 100.0
        else 0.0

    member this.GetVolatility (ticker: string) =
        let data = this.LoadPrices ticker
        if data.Length > 1 then
            let closes = data |> List.map (fun x -> x.Close)
            let avg = closes |> List.average
            let variance = closes |> List.averageBy (fun x -> (x - avg) ** 2.0)
            sqrt variance
        else 0.0
