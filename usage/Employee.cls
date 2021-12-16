public class EmployeeDTO
{
    public static EmployeeDTO FromJSON(string jsonString)
    {
        return (EmployeeDTO)JSON.Deserialize(jsonString, EmployeeDTO.Class);
    }

    public string Name;
    public List<string> WorkingDays;
}
