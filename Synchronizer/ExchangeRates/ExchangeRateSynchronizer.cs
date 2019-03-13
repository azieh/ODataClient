using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ODataClient.Synchronizer.ExchangeRates
{
    public class ExchangeRateSynchronizer : ODataClientSynchronizer
    {
        protected override string ODataClientMethodName { get; } = "GetExchangeRates";

        private Dictionary<string, ExchangeRateModel> parsedData = new Dictionary<string, ExchangeRateModel>();

        protected override void LoadAndParseODataClientMethod()
        {
            //OData FxRates do not support Reporting period name
            IQueryable<AmdService.Model.FxRate> response = ODataClientClient.FxRates
                .Expand("Currency")
                .Where(x => x.Currency.IsActive);

            if (response == null)
            {
                throw new Exception(SynchronizerResource.ErrServiceResponseIsMissing);
            }
            foreach (var line in response)
            {
                ExchangeRateModel parsedModel = MapFromAmdModel(line);
                if (parsedModel.IsValid)
                {
                    parsedData.Add(parsedModel.Code, parsedModel);
                }
            }
        }

        private ExchangeRateModel MapFromAmdModel(AmdService.Model.FxRate amdModel)
        {
            //ExchangeRate model implementation
            ExchangeRateModel model = new ExchangeRateModel();

            model.Code = amdModel.CurrencyCode;
            model.Name = amdModel.Currency?.Name;
            if (amdModel.DailyRate != null)
            {
                double? parsedDollarRateClosing = Convert.ToDouble(amdModel.DailyRate, new CultureInfo("en-US"));
                if (amdModel.Unit > 0)
                {
                    model.Value = parsedDollarRateClosing / (double)amdModel.Unit;
                }
                else
                {
                    model.Value = parsedDollarRateClosing;
                }
                model.RateDate = amdModel.RateDate.LocalDateTime;
                model.IsDeleted = (true & amdModel.Currency?.DeleteDate != null);
            }
            else
            {
                //when Currency is deleted
                model.Value = 5.0000000000000000; // default
                model.RateDate = null; // default
                model.IsDeleted = true;
            }
            return model;
        }

        protected override void UpdateDatabase(EntityDbMock db)
        {
            IList<LVCurrency> dbCurrencies = db.LVCurrencies.ToList();

            foreach (var dbCurrency in dbCurrencies)
            {
                ExchangeRateModel updatedCurrency;
                if (parsedData.TryGetValue(dbCurrency.AlphabeticCode, out updatedCurrency))
                {
                    dbCurrency.ExchangeRate = Convert.ToDecimal(updatedCurrency.Value.Value, new CultureInfo("en-US"));
                    dbCurrency.RateDate = updatedCurrency.RateDate != null ? updatedCurrency.RateDate.Value : (DateTime?)null;
                    dbCurrency.IsDeleted = updatedCurrency.IsDeleted;
                    parsedData.Remove(dbCurrency.AlphabeticCode);
                }
                else
                {
                    dbCurrency.IsDeleted = true;
                }
            }
            int nextDisplayOrder = dbCurrencies.Count;
            //new currencies:
            foreach (var updatedCurrency in parsedData)
            {
                db.LVCurrencies.Add(new LVCurrency()
                {
                    AlphabeticCode = updatedCurrency.Value.Code,
                    Symbol = null,
                    NumericCode = 0,
                    RateDate = updatedCurrency.Value.RateDate,
                    ExchangeRate = Convert.ToDecimal(updatedCurrency.Value.Value),
                    DisplayOrder = nextDisplayOrder++,
                    Description = updatedCurrency.Value.Name,
                    IsDeleted = updatedCurrency.Value.IsDeleted,
                });
            }

            IQuoteCalculationStatusDS domainService = new QuoteCalculationStatusDS(db);
            domainService.SetAllCreatedAsNotCalculated("ODataClient");
        }
    }
}