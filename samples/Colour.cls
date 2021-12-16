public class ColourDTO
{
    public static ColourDTO FromJSON(string jsonString)
    {
        return (ColourDTO)JSON.Deserialize(jsonString, ColourDTO.Class);
    }

    public string HexValue;
    public string Name;
}
