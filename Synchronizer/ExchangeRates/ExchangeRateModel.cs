using System;

namespace ODataClient.Synchronizer.ExchangeRates
{
    public class ExchangeRateModel
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public double? Value { get; set; }

        public DateTime? RateDate { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsValid
        {
            get { return !string.IsNullOrEmpty(this.Name) && !string.IsNullOrEmpty(this.Code) && this.Value.HasValue && this.RateDate.HasValue; }
        }
    }
}