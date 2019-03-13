using System;
using System.Collections.Generic;
using System.Linq;
using EntityDbMock;

namespace ODataClient.Synchronizer.Countries
{
    public class CountriesSynchronizer : ODataClientSynchronizer
    {
        protected override string ODataClientMethodName { get; } = "GetCountries";

        private Dictionary<string, CountryModel> parsedData = new Dictionary<string, CountryModel>();

        protected override void LoadAndParseODataClientMethod()
        {
            //OData Countries do not support Reporting period name
            IQueryable<AmdService.Model.Country> response = ODataClientClient.Countries
                .Where(x => x.IsActive)
                .Where(x => x.IsIso);
            //OData V4 do not allow for deep expand
            IQueryable<AmdService.Model.Region> deepRegionResponse = ODataClientClient.Regions;
            //=====================================
            if (response == null || deepRegionResponse == null)
            {
                throw new Exception(SynchronizerResource.ErrServiceResponseIsMissing);
            }
            foreach (var line in response)
            {
                CountryModel parsedModel = MapFromAmdModel(line, deepRegionResponse);
                if (parsedModel.IsValid)
                {
                    parsedData.Add(parsedModel.Code, parsedModel);
                }
            }
        }

        private CountryModel MapFromAmdModel(AmdService.Model.Country amdModel, IQueryable<AmdService.Model.Region> deepRegionResponse)
        {
            //Country model implementation

            CountryModel model = new CountryModel();
            model.Code = amdModel.Code;
            model.Name = amdModel.Name;
            //OData V4 do not allow for deep expand so we have to do it in this way
            IQueryable<AmdService.Model.Region> region = deepRegionResponse
                .Where(x => x.IsActive)
                ;
            //======================================
            model.RegionCode = (region?.Where(x => x.Code == model.Code).FirstOrDefault())?.ParentCode;
            model.IsDeleted = (true & amdModel.DeleteDate != null);
            model.ReplacedBy = amdModel.MergeToCode;

            return model;
        }

        protected override void UpdateDatabase(IEntityDb db)
        {
            HashSet<string> updated = new HashSet<string>();
            IList<LVCountry> dbCountries = db.LVCountries.ToList();
            IList<LVRegion> dbRegions = db.LVRegions.ToList();

            //update existing application codes
            foreach (var dbCountry in dbCountries)
            {
                CountryModel updateCountries;
                if (parsedData.TryGetValue(dbCountry.Code, out updateCountries))
                {
                    int? regionId = null;
                    if (dbRegions.Where(dbr => dbr.Code == updateCountries.RegionCode).Any())
                    {
                        regionId = dbRegions.Where(dbr => dbr.Code == updateCountries.RegionCode).First().RegionID;
                    }

                    dbCountry.CountryName = updateCountries.Name;
                    dbCountry.IsDeleted = updateCountries.IsDeleted;
                    if (regionId != null)
                    {
                        dbCountry.RegionID = regionId;
                    }
                    updated.Add(dbCountry.Code);
                }
                else
                {
                    dbCountry.IsDeleted = true;
                }
            }
            //new applications:
            foreach (var newCountry in parsedData.Where(a => !updated.Contains(a.Value.Code)))
            {
                int? regionId = null;
                if (dbRegions.Where(dbr => dbr.Code == newCountry.Value.RegionCode).Any())
                {
                    regionId = dbRegions.Where(dbr => dbr.Code == newCountry.Value.RegionCode).First().RegionID;
                }

                dbCountries.Add(db.LVCountries.Add(new LVCountry()
                {
                    Code = newCountry.Value.Code,
                    CountryName = newCountry.Value.Name,
                    RegionID = regionId,
                    IsDeleted = newCountry.Value.IsDeleted
                }));
            }
            //update replaceby value:
            foreach (var updatedCountry in parsedData.Where(a => !string.IsNullOrEmpty(a.Value.ReplacedBy)))
            {
                var oldAc = dbCountries.Where(ac => ac.Code == updatedCountry.Value.Code).FirstOrDefault();
                var newAc = dbCountries.Where(ac => ac.Code == updatedCountry.Value.ReplacedBy).FirstOrDefault();

                oldAc.IsDeleted = true;
                oldAc.ReplacedBy = newAc.CodeID; //set replaced by
            }
        }
    }
}