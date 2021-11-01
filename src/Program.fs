open ApexDTO

[<EntryPoint>]
let main argv =

    let jsonString =
        """
        {
            "FirstName": "John",
            "LastName": "Smith",
            "Gender": "male",
            "Age": 24,
            "Address":
            {
                "HouseNumber": 12,
                "Street": "Generic Street",
                "City": "Austin",
                "State": "Texas",
                "PostalCode": "78701"
            },
            "ContactNumbers":
            [
                { "Type": "Home", "PhoneNumber": 7312627627 },
                { "Type": "Mobile", "PhoneNumber": 3445725540 }
            ]
        }
        """
    printfn "%A" (jsonToApexDto "Person" jsonString)
    printfn "----------------------------------------------"
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
    printfn "%A" (jsonToApexDto "Colour" jsonString)
    printfn "----------------------------------------------"
    let jsonString =
        """
        {
            "Name": "Jane Doe",
            "WorkingDays": ["Monday", "Wednesday", "Thursday", "Friday"]
        }
        """
    printfn "%A" (jsonToApexDto "Employee" jsonString)

    0