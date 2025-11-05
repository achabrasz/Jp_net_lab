module Library

open System

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

type Library =
    new : path: string -> Library
    member GetPatron : id: int -> Czytelnik option
    member GetLoanHistory : id: int -> (string * bool) list
    member IsPatronForLongerThan : id: int * days: int -> bool
    member AddFine : id: int * amountPLN: decimal<PLN> -> unit
    member ConvertCurrency : symbol: string * value: decimal -> Async<decimal>
    member CheckFineLimit : id: int * usdToPln: decimal * eurToPln: decimal -> bool
    member PromoteIfEligible : id: int * minBooks: int * minDays: int -> unit
