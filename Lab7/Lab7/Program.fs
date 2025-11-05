module Program

open System
open Library

[<EntryPoint>]
let main _ =
    let lib = Library("readers.json")

    let rec menu () =
        printfn "\n--- System Biblioteczny ---"
        printfn "1. Pokaż dane czytelnika"
        printfn "2. Dodaj karę (PLN)"
        printfn "3. Sprawdź limit kaucji"
        printfn "4. Awansuj czytelnika"
        printfn "0. Wyjście"
        printf "Wybór: "
        match Console.ReadLine() with
        | "1" ->
            printf "Podaj ID: "
            let id = int (Console.ReadLine())
            match lib.GetPatron(id) with
            | Some p ->
                match p.Contact with
                | Some c ->
                    let email = defaultArg c.Email "brak"
                    printfn $"{c.FirstName} {c.LastName}, Email: {email}"
                | None -> printfn "Brak danych kontaktowych"
            | None -> printfn "Nie znaleziono"
            menu()
        | "2" ->
            printf "Podaj ID: "
            let id = int (Console.ReadLine())
            printf "Podaj kwotę PLN: "
            let kwota = decimal (Console.ReadLine()) * 1.0M<PLN>
            lib.AddFine(id, kwota)
            printfn "Dodano karę."
            menu()
        | "3" ->
            printf "Podaj ID: "
            let id = int (Console.ReadLine())
            let usdToPln = Async.RunSynchronously(lib.ConvertCurrency("USD", 1.0M))
            let ok = lib.CheckFineLimit(id, usdToPln, 4.5M)
            printfn (if ok then "Nie przekroczono limitu." else "Limit przekroczony!")
            menu()
        | "4" ->
            printf "Podaj ID: "
            let id = int (Console.ReadLine())
            lib.PromoteIfEligible(id, 2, 30)
            menu()
        | "0" -> ()
        | _ -> menu()
    menu()
    0
