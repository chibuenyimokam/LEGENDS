namespace LegendPay.Models.BillerOne.Request
{
    public class AirtimeCreditRequest
    {
        public string BillerName { get; set; }
        public string BillerId { get; set; }
        public decimal Amount { get; set; }
        public string Channel { get; set; }
        public string Category { get; set; }
        public string CustomerReferece { get; set; } //Phone number
        public string PaymentMethod { get; set; } //Wallet
    }
}
