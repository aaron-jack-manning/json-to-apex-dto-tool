# JSON to Apex DTO Tool

This tool converts JSON to a data transfer object for Salesforce Apex.

## Example

For example, the following JSON is converted to the following class.

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

More examples are provided in the `usage` file.

## Usage

The easiest way to use this library is to download the repo and open `Program.fs`. Then change the `jsonString` to be the JSON that you wish to convert, `folder` to the location you want the `.cls` file to be placed in and the `className` to be the name of the top level class.

## Implementation Details

The provided output will translate all objects within the main object to internal classes at the top level of the main data transfer object. A `FromJSON` static method is created to parse JSON into an instance of the corresponding class. The top level of JSON must be a list of objects or a single object.

If the provided JSON includes an array of JSON objects, the class name will be the property name followed by the word ***Single***. For example:

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

In general, it is best to avoid `null` values in the JSON schema as the type cannot be inferred. This is handled by assuming all `null` values to be an empty object so that fields can be manually added later. If you wish for this behaviour to change, please see the `swapNulls` function in `JSONProcessor.fs`. The `jsonValidator` function which is called later has a case to catch any nulls that aren't handled in `swapNulls`.

## Known Problems

The following are known problems with the existing implementation that I have come across in my usage. I do plan on fixing these at some point, but they are documented here in the meantime for convenience, and as specifications for when I go to fix them.

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

- If a property is named according to a language keyword, such as `public` or `private`, the resulting DTO will not be valid Apex due to the variable being given that same name. Shout out to the Raisely API for this annoying one :P. My current plan for this is to give the properties that have the same name as any language keyword a new name (with `Property` appended to the end or something) and to do a find and replace within the static method that deserializes. This would look something like this `jsonString.ReplaceAll('"public"(\\s?|\\s+):', '"publicProperty":')`. This currently what I do manually but will implement a fix in the generator at some point.

## Links

The JSON parser used within this project was implemented using [FParsec](https://github.com/stephan-tolksdorf/fparsec).
