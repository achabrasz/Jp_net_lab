module Library

type StockData = { Date: string; Open: float; Close: float }

type StockAnalyzer =
    new : unit -> StockAnalyzer
    member LoadPrices : string -> StockData list
    member GetReturn : string -> float
    member GetVolatility : string -> float
    member LoadPricesAsync : string -> Async<StockData list>
