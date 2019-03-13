namespace ODataClient.Synchronizer.Countries
{
    public class CountryModel
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string RegionCode { get; set; }

        public bool IsDeleted { get; set; }

        public string ReplacedBy { get; set; }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(this.Code);
            }
        }
    }
}