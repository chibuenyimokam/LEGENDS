using Newtonsoft.Json;
using System.Globalization;

namespace LegendPay.Models.WalletStation.Response
{
    public class GetTransactionListResponse
    {
        public ResponseHeader ResponseHeader { get; set; }
        public Pagination Pagination { get; set; }
        public List<TransactionDetailsList> TransactionDetailsList { get; set; } = new();
    }
        public class Pagination
        {
            public required int CurrentPage { get; set; }
            public required int TotalCount { get; set; }
            public required int TotalPages { get; set; }
            public required bool HasPrevious { get; set; }
            public required bool HasNext { get; set; }
        }
        public class TransactionDetailsList 
        {
            public required string TransactionType { get; set; }
            public required string TransactionId { get; set; }
            public required string SessionId { get; set; }
            public required decimal Amount { get; set; }
            public required decimal Balance { get; set; }
            public required string Description { get; set; }
        public required string TranType { get; set; }
        [JsonProperty("date")]
        public DateTime Date { get; internal set; }
    }
    
    
}
