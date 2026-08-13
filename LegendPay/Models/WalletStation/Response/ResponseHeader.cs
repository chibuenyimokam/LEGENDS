using LegendPay.Models.Data.Response_Table;
using System.ComponentModel.DataAnnotations;

namespace LegendPay.Models.WalletStation.Response
{
    public class ResponseHeader
    {
        public required string ResponseCode { get; set; }
        public required string ResponseMessage { get; set; }

        //public bool IsSuccessful => ResponseCode == "00";
    }
}