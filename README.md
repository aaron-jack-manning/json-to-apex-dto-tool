# JSON to Apex DTO Tool

This tool converts JSON to a data transfer object for Salesforce Apex. An example is shown below:

```
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
```

```
public class ColourDTO
{
    public static ColourDTO FromJSON(string jsonString)
    {
        return (ColourDTO)JSON.Deserialize(jsonString, ColourDTO.Class);
    }

    public string HexValue;
    public string Name;
}
```

The provided output will translate all objects within the main object to internal classes at the top level of the main data transfer object. A `FromJSON` static method is created to parse JSON into an instance of the corresponding class. The top level of JSON must be a list of objects or a single object.

If the provided JSON includes an array of JSON objects, the class will be marked as unnamed and the user will need to fill in the name of that class.

The name of the top level class is provided as input to the `jsonToApexDto` function, which is the only function that should be called when trying to achieve the above output (as opposed to modifying the implementation or extracting specific functions for other uses).

More examples are provided in the `examples` file, corresponding to the examples in the `Program.fs` file.

The source code for the JSON parser can be found at `JSONParser` while the code to construct the Apex DTO is available in `ApexDTO`.
