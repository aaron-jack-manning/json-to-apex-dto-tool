public class PersonDTO
{
    public static PersonDTO FromJSON(string jsonString)
    {
        return (PersonDTO)JSON.Deserialize(jsonString, PersonDTO.Class);
    }

    class AddressDTO
    {
        public string City;
        public long HouseNumber;
        public string PostalCode;
        public string State;
        public string Street;
    }
    class [UNNAMEDCLASS0]DTO
    {
        public long PhoneNumber;
        public string Type;
    }

    public AddressDTO Address;
    public long Age;
    public List<[UNNAMEDCLASS0]DTO> ContactNumbers;
    public string FirstName;
    public string Gender;
    public string LastName;
}
