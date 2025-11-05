module Library

open System
open System.IO
open System.Net
open System.Text.Json

[<Measure>] type PLN
[<Measure>] type EUR
[<Measure>] type USD

type ContactInfo =
    | EmailOnly of string
    | PostOnly of string
    | EmailAndPost of string * string

type DaneKontaktowe = {
    FirstName: string
    LastName: string
    BirthDate: string
    LibraryCard: string
    Email: string option
    ContactInfo: ContactInfo
}

type AccountStatus = Standard | Premium

type Czytelnik = {
    Id: int
    Contact: DaneKontaktowe option
    DepositUSD: decimal<USD>
    JoinDate: DateTime
    mutable Status: AccountStatus
    mutable FinesPLN: decimal<PLN>
}

type LoanRecord = { ISBN: string; Returned: bool }
type LoanHistory = { PatronId: int; Loans: LoanRecord list }

let private loadJson<'T> path =
    let json = File.ReadAllText(path)
    let options = System.Text.Json.JsonSerializerOptions()
    options.Converters.Add(System.Text.Json.Serialization.JsonFSharpConverter())
    JsonSerializer.Deserialize<'T>(json, options)

let private getExchangeRate (url: string) =
    async {
        try
            let! html = (new WebClient()).DownloadStringTaskAsync(url) |> Async.AwaitTask
            let pattern = @">(\d+,\d+)<"
            let matches = System.Text.RegularExpressions.Regex.Matches(html, pattern)
            if matches.Count > 0 then
                let rateStr = matches.[0].Groups.[1].Value.Replace(",", ".")
                return Decimal.Parse(rateStr)
            else
                printfn "Nie znaleziono kursu, używam domyślnej wartości 4.0"
                return 4.0M
        with
        | ex ->
            printfn $"Błąd pobierania kursu: {ex.Message}"
            return 4.0M
    }

type Library(path: string) =
    let patrons : Czytelnik list = 
        let raw = loadJson<{| Id: int; DepositUSD: decimal; JoinDate: DateTime; Status: AccountStatus; FinesPLN: decimal; Contact: DaneKontaktowe option |}[]>(path)
        raw |> Array.map (fun p -> 
            { Id = p.Id
              DepositUSD = p.DepositUSD * 1.0M<USD>
              JoinDate = p.JoinDate
              Status = p.Status
              FinesPLN = p.FinesPLN * 1.0M<PLN>
              Contact = p.Contact }
        ) |> Array.toList
    
    member this.GetPatron(id: int) : Czytelnik option =
        patrons |> List.tryFind (fun p -> p.Id = id)
    
    member this.GetLoanHistory(id: int) : (string * bool) list =
        let historyPath = $"history/{id}.json"
        if File.Exists(historyPath) then
            let history = loadJson<LoanHistory>(historyPath)
            history.Loans |> List.map (fun l -> l.ISBN, l.Returned)
        else []
    
    member this.IsPatronForLongerThan(id: int, days: int) : bool =
        match this.GetPatron(id) with
        | Some p -> (DateTime.Now - p.JoinDate).TotalDays > float days
        | None -> false
    
    member this.AddFine(id: int, amountPLN: decimal<PLN>) : unit =
        match this.GetPatron(id) with
        | Some p -> p.FinesPLN <- p.FinesPLN + amountPLN
        | None -> ()
    
    member this.ConvertCurrency(symbol: string, value: decimal) : Async<decimal> =
        async {
            let! rate =
                match symbol with
                | "EUR" -> getExchangeRate "https://stooq.pl/q/?s=eurpln"
                | "USD" -> getExchangeRate "https://stooq.pl/q/?s=usdpln"
                | _ -> async.Return 1.0M
            return value * rate
        }
    
    member this.CheckFineLimit(id: int, usdToPln: decimal, eurToPln: decimal) : bool =
        match this.GetPatron(id) with
        | Some p ->
            let depositValueUSD = p.DepositUSD / 1.0M<USD>  // strips unit, gives plain decimal
            let depositInPln = depositValueUSD * usdToPln * 1.0M<PLN>
            p.FinesPLN < depositInPln
        | None -> false

    
    member this.PromoteIfEligible(id: int, minBooks: int, minDays: int) : unit =
        match this.GetPatron(id) with
        | Some p ->
            let history = this.GetLoanHistory(id)
            let booksCount = history.Length
            let longEnough = this.IsPatronForLongerThan(id, minDays)
            if booksCount > minBooks && longEnough && p.Status = Standard then
                p.Status <- Premium
                printfn $"Czytelnik {id} awansowany do Premium."
            elif p.Status = Premium then
                printfn $"Czytelnik {id} już jest Premium."
            else
                printfn $"Czytelnik {id} nie spełnia kryteriów awansu."
        | None -> 
            printfn "Nie znaleziono czytelnika."
