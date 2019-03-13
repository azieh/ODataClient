using System;
using System.Collections.Generic;
using System.Linq;
using EntityDbMock;

namespace ODataClient.Synchronizer.CompanyLocations
{
    public class CompanyLocationSynchronizer : ODataClientSynchronizer
    {
        protected override string ODataClientMethodName { get; } = "GetCompanyLocations";

        private static IQueryable<AmdService.Model.CompanyLocation> ExecQuery
        {
            get
            {
                //OData needs to build two separate query
                if (!string.IsNullOrEmpty(ReportingPeriod))
                {
                    return ODataClientClient.CompanyLocations
                    .Expand("Address")
                    .Expand("Company")
                    .Expand("AddressType")
                    .Where(x => x.IsActive)
                    .Where(x => x.ReportingPeriodName == ReportingPeriod);
                }
                return ODataClientClient.CompanyLocations
                    .Expand("Address")
                    .Expand("Company")
                    .Expand("AddressType")
                    .Where(x => x.IsActive);
            }
        }

        private Dictionary<string, CompanyLocationModel> parsedData = new Dictionary<string, CompanyLocationModel>();

        protected override void LoadAndParseODataClientMethod()
        {
            IQueryable<AmdService.Model.CompanyLocation> response = ExecQuery;
            
            //OData V4 do not allow for deep expand
            Microsoft.OData.Client.DataServiceQuery<AmdService.Model.State> deepStatesResponse = ODataClientClient.States;
            //=====================================
            if (response == null || deepStatesResponse == null)
            {
                throw new Exception(SynchronizerResource.ErrServiceResponseIsMissing);
            }
            foreach (var line in response)
            {
                CompanyLocationModel parsedModel = MapFromAmdModel(line, deepStatesResponse);

                parsedData.Add(parsedModel.LocationId, parsedModel);
            }
        }

        private CompanyLocationModel MapFromAmdModel(AmdService.Model.CompanyLocation amdModel, Microsoft.OData.Client.DataServiceQuery<AmdService.Model.State> deepStatesResponse)
        {
            //CompanyLocation model implementation
            CompanyLocationModel model = new CompanyLocationModel();

            model.LocationId = amdModel.Id;
            model.AbacusCode = amdModel.Company?.AbacusCode;
            model.AddressId = amdModel.AddressId;
            model.LocationType = amdModel.AddressType?.Name;
            model.CountryHq = amdModel.CountryHeadQuarter;
            model.Phone = amdModel.Phone;
            model.Fax = amdModel.Fax;
            model.LocalName = amdModel.Company?.LegalName;
            model.MailingAddress = amdModel.MailingAddress;
            model.LocalLanguageName = amdModel.LocalLanguageName;
            model.Comment = amdModel.Comment;
            model.IsDeleted = (true & amdModel.DeleteDate != null);
            model.ReplacedBy = amdModel.MergeToId;
            model.City = amdModel.Address?.City;
            model.Street = string.Join(" "
                , amdModel.Address?.LocalLanguageAddressPart1
                , amdModel.Address?.LocalLanguageAddressPart2
                , amdModel.Address?.LocalLanguageAddressPart3);
            model.ZipCode = amdModel.Address?.ZipCode;
            model.Name = amdModel.Company?.ShortName;
            //OData V4 do not allow for deep expand so we have to do it in this way
            AmdService.Model.State state = deepStatesResponse
                .Where(x => x.Id == amdModel.Address.StateId)
                .FirstOrDefault();
            //======================================
            model.State = state?.Name;

            return model;
        }

        protected override void UpdateDatabase(IEntityDb db)
        {
            HashSet<string> updated = new HashSet<string>();
            var dbCountryLocations = db.CompanyLocations.ToList();

            //update existing application codes
            foreach (var dbCountryLocation in dbCountryLocations)
            {
                CompanyLocationModel updateCountries;
                if (parsedData.TryGetValue(dbCountryLocation.LocationId, out updateCountries))
                {
                    dbCountryLocation.CompanyCode = updateCountries.AbacusCode;
                    dbCountryLocation.AddressId = updateCountries.AddressId;
                    dbCountryLocation.LocationType = updateCountries.LocationType;
                    dbCountryLocation.CountryHq = updateCountries.CountryHq;
                    dbCountryLocation.Phone = updateCountries.Phone;
                    dbCountryLocation.Fax = updateCountries.Fax;
                    dbCountryLocation.LocalLanguageName = updateCountries.LocalLanguageName;
                    dbCountryLocation.LocalName = updateCountries.LocalName;
                    dbCountryLocation.MailingAddress = updateCountries.MailingAddress;
                    dbCountryLocation.Comment = updateCountries.Comment;
                    dbCountryLocation.Deleted = updateCountries.IsDeleted;
                    dbCountryLocation.SuccessorId = updateCountries.ReplacedBy;
                    dbCountryLocation.City = updateCountries.City;
                    dbCountryLocation.CompanyName = updateCountries.Name;
                    dbCountryLocation.Street = updateCountries.Street;
                    dbCountryLocation.ZipCode = updateCountries.ZipCode;
                    dbCountryLocation.State = updateCountries.State;

                    updated.Add(dbCountryLocation.LocationId);
                }
                else
                {
                    dbCountryLocation.Deleted = true;
                }
            }
            db.SaveChanges();
            //new applications:
            foreach (var newApplication in parsedData.Where(a => !updated.Contains(a.Value.LocationId)))
            {
                db.CompanyLocations.Add(new CompanyLocations()
                {
                    LocationId = newApplication.Value.LocationId,
                    CompanyCode = newApplication.Value.AbacusCode,
                    AddressId = newApplication.Value.AddressId,
                    LocationType = newApplication.Value.LocationType,
                    CountryHq = newApplication.Value.CountryHq,
                    Phone = newApplication.Value.Phone,
                    Fax = newApplication.Value.Fax,
                    LocalLanguageName = newApplication.Value.LocalLanguageName,
                    LocalName = newApplication.Value.LocalName,
                    MailingAddress = newApplication.Value.MailingAddress,
                    Comment = newApplication.Value.Comment,
                    Deleted = newApplication.Value.IsDeleted,
                    SuccessorId = newApplication.Value.ReplacedBy,
                    City = newApplication.Value.City,
                    CompanyName = newApplication.Value.Name,
                    Street = newApplication.Value.Street,
                    ZipCode = newApplication.Value.ZipCode,
                    State = newApplication.Value.State
                });
            }
            //update replaceby value:
            foreach (var updatedRegion in parsedData.Where(a => !string.IsNullOrEmpty(a.Value.ReplacedBy)))
            {
                var oldAc = dbCountryLocations.Where(ac => ac.LocationId == updatedRegion.Value.LocationId).FirstOrDefault();
                var newAc = dbCountryLocations.Where(ac => ac.LocationId == updatedRegion.Value.ReplacedBy).FirstOrDefault();

                oldAc.Deleted = true;
                oldAc.SuccessorId = newAc.LocationId; //set replaced by
            }
        }
    }
}