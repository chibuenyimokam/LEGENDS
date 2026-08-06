//using System.Security.Cryptography;
//using System.Text;
//using LegendPay.Services.Configuration;
//using Microsoft.Extensions.Options;

//namespace LegendPay.Services.Transaction
//{
//    /// <summary>
//    /// Generates the X-Signature header required by CoralPay's Customer Enquiry
//    /// and Vend Value endpoints. Uses SHA256withRSA, matching the sample in the
//    /// VAS integration document.
//    /// </summary>
//    public class VasSignatureService
//    {
//        private readonly VasSettings _settings;

//        public VasSignatureService(IOptions<VasSettings> settings)
//        {
//            _settings = settings.Value;
//        }

//        private string EncodedBasicAuth =>
//            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.Password}"));

//        /// <summary>
//        /// stringToSign = Base64(username:password) + customerId + billerId + institutionId
//        /// NOTE: the doc's request body for customer-lookup uses billerSlug, but the signature
//        /// formula documents billerId. Pass whichever value CoralPay confirms works in sandbox.
//        /// </summary>
//        public string GenerateCustomerLookupSignature(string customerId, string billerId)
//        {
//            var stringToSign = EncodedBasicAuth + customerId + billerId + _settings.InstitutionId;
//            return Sign(stringToSign);
//        }

//        /// <summary>
//        /// stringToSign = Base64(username:password) + paymentReference + customerId + amount + billerId + institutionId
//        /// </summary>
//        public string GenerateVendValueSignature(string paymentReference, string customerId, string amount, string billerId)
//        {
//            var stringToSign = EncodedBasicAuth + paymentReference + customerId + amount + billerId + _settings.InstitutionId;
//            return Sign(stringToSign);
//        }

//        private string Sign(string stringToSign)
//        {
//            if (string.IsNullOrWhiteSpace(_settings.PrivateKeyPem))
//            {
//                throw new InvalidOperationException(
//                    "Vas:PrivateKeyPem is not configured. CoralPay must supply this RSA private key during VAS onboarding.");
//            }

//            using var rsa = RSA.Create();
//            rsa.ImportFromPem(_settings.PrivateKeyPem.ToCharArray());

//            var dataBytes = Encoding.UTF8.GetBytes(stringToSign);
//            var signedBytes = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

//            return Convert.ToBase64String(signedBytes);
//        }
//    }
//}