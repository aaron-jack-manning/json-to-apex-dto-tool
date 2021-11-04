public class DonationDTO
{
    public static DonationDTO FromJSON(string jsonString)
    {
        return (DonationDTO)JSON.Deserialize(jsonString, DonationDTO.Class);
    }

    public class AddressDTO
    {
        public string City;
        public long HouseNumber;
        public string PostalCode;
        public string State;
        public string Street;
    }
    class ContactNumbersSingle]DTO
    {
        public long PhoneNumber;
        public string Type;
    }

    public AddressDTO Address;
    public long Age;
    public List<ContactNumbersSingleDTO> ContactNumbers;
    public string FirstName;
    public string Gender;
    public string LastName;
}
