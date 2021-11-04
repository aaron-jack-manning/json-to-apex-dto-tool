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

If the provided JSON includes an array of JSON objects, the class name will be the property name followed by the word ***Singular***. For example:

```
"ContactNumbers":
[
    { "Type": "Home", "PhoneNumber": 7312627627 },
    { "Type": "Mobile", "PhoneNumber": 3445725540 }
]
```

will be converted into the class:

```
class ContactNumbersSingleDTO
{
    public long PhoneNumber;
    public string Type;
}
```

with the instance in the higher level class of:

```
public List<ContactNumbersSingleDTO> ContactNumbers
```

The name of the top level class is provided as input to the `jsonToApexDto` function, which is the only function that should be called when trying to achieve the above output (as opposed to modifying the implementation or extracting specific functions for other uses).

More examples are provided in the `examples` file, corresponding to the examples in the `Program.fs` file.

The source code for the JSON parser can be found at `JSONParser` while the code to construct the Apex DTO is available in `ApexDTO`.


## Known Problems (and Planned Fixes)

The following are current problems with the existing implementation.

- If nested classes have the same name then two classes of the same name will be created. For example:

```
{
    "data":
    {
        "property1":"value",
        "data":
        {
            "property2":"value"
        }
    }
}
```

will created two classes named `DataDTO`.

- If a property is named according to a language keyword, such as public or private, the resulting DTO will not be valid Apex due to the variable being given that same name. This makes strict deserialization hard in general, and should be avoided from the start, but there is currently no explicit handling for this case. Shout out to the Raisely API for this annoying one :P. My current plan for this is to give the properties that have the same name as any language keyword a new name (with property appended to the end or something) and to do a find and replace within the static method that deserializes. This would look something like this `jsonString.ReplaceAll('"public"(\\s?|\\s+):', '"publicProperty":')`.
