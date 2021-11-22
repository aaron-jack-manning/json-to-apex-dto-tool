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
    printfn "%A" (jsonToApexDto "Donation" jsonString)

    0