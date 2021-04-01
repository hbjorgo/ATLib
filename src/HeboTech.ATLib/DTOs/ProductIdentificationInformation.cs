namespace HeboTech.ATLib.DTOs
{
    public class ProductIdentificationInformation
    {
        public ProductIdentificationInformation(string information)
        {
            Information = information.TrimEnd('\r', '\n');
        }

        public string Information { get; }

        public override string ToString()
        {
            return Information;
        }
    }
}
