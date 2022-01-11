open ApexDTO

open System.IO

[<EntryPoint>]
let main argv =

    let jsonString =
        """
        [
            {
                "Name": "Green",
                "HexValue": "#66ff66"
            },
            {
                "Name": "Red",
                "HexValue": "#ff0066"
            },
            {
                "Name": "Blue",
                "HexValue": "#0000ff"
            }
        ]
        """

    let className = "Colour"
    let folder = @"C:\Users\username\Downloads\"
    let apexClassResult = jsonToApexDto className jsonString

    match apexClassResult with
    | Ok apexClass -> File.WriteAllText(folder + className + ".cls", apexClass)
    | Error message -> printfn "%s" message
    
    0