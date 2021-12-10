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
    let apexClass = jsonToApexDto className jsonString


    File.WriteAllText(folder + className + ".cls", apexClass)

    0