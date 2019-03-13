using System.Collections.Generic;
using EntityDbMock;

namespace ODataClient.Synchronizer.CompanyLocations
{
    public class CompanyLocationModel
    {
        public string LocationId { get; set; }

        public string Name { get; set; }

        public string AbacusCode { get; set; }

        public string AddressId { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string State { get; set; }

        public string LocationType { get; set; }

        public bool CountryHq { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string LocalLanguageName { get; set; }

        public string LocalName { get; set; }

        public string MailingAddress { get; set; }

        public string Comment { get; set; }

        public bool IsDeleted { get; set; }

        public string ReplacedBy { get; set; }
    }

    class CompanyLocations : ICompanyLocations
    {
        public string LocationId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string CompanyCode { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string AddressId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string LocationType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool CountryHq { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string Phone { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string Fax { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string LocalLanguageName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string LocalName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string MailingAddress { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string Comment { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public bool Deleted { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string SuccessorId { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string City { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string CompanyName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string Street { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string ZipCode { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string State { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void Add(ICompanyLocations entity)
        {
            throw new System.NotImplementedException();
        }

        public void SaveChanges()
        {
            throw new System.NotImplementedException();
        }

        public List<ICompanyLocations> ToList()
        {
            throw new System.NotImplementedException();
        }
    }
}