using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public sealed record ExchangeRate
    {
        /// <summary>
        /// Database primary key field
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The three letter ISO code for the Exchange Rate, e.g. USD
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// The conversion rate of this currency from the base currency
        /// </summary>
        public decimal Value { get; set; }

        public DateOnly Date { get; set; }

        public DateOnly LastModifiedDate { get; set; }

        /// <summary>
        /// Creates a new instance of the ExchangeRate class
        /// </summary>
        public ExchangeRate()
        {
            CurrencyCode = string.Empty;
            Value = 1.0m;
        }
    }
}
