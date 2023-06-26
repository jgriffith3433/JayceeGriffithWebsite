using System.Globalization;
using System.Net;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace ContainerNinja.Core.Walmart
{
    public class WalmartWebClient : WebClient
    {
        private readonly string _key;
        private readonly string _password;

        public WalmartWebClient()
        {
            _key = Environment.GetEnvironmentVariable("WalmartServiceApiKey").Replace("\\n", "\n");
            _password = Environment.GetEnvironmentVariable("WalmartServiceApiKeyPassword");
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }

        /// <summary>
        /// Begining of time according to Java world.
        /// </summary>
        private readonly DateTime JanuaryFirst1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// The calculated signature
        /// </summary>
        /// <returns>
        /// The calculated signature
        /// </returns>
        public string GetWalmartSignature(string consumerId, string timeStamp, string version)
        {
            // Append values into string for signing
            var message = consumerId + "\n" + timeStamp + "\n" + version + "\n";

            RsaKeyParameters rsaKeyParameter;
            try
            {
                StringReader stringReader = new StringReader(_key);
                PemReader pemReader = new PemReader(stringReader, new PasswordFinder(_password));
                RsaPrivateCrtKeyParameters keyParams = (RsaPrivateCrtKeyParameters)pemReader.ReadObject();

                rsaKeyParameter = keyParams;
            }
            catch (Exception)
            {
                throw new Exception("Unable to load private key");
            }

            var signer = SignerUtilities.GetSigner("SHA256withRSA");
            signer.Init(true, rsaKeyParameter);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            signer.BlockUpdate(messageBytes, 0, messageBytes.Length);
            var signed = signer.GenerateSignature();
            var hashed = Convert.ToBase64String(signed);
            return hashed;
        }

        /// <summary>
        /// Get the TimeStamp as a string equivalent to Java System.currentTimeMillis
        /// </summary>
        /// <returns>
        /// Generated sign string
        /// </returns>
        public string GetTimestampInJavaMillis()
        {
            var millis = (DateTime.UtcNow - JanuaryFirst1970).TotalMilliseconds;
            return Convert.ToString(Math.Round(millis), CultureInfo.InvariantCulture);
        }

        private class PasswordFinder : IPasswordFinder
        {
            private string password;
            public PasswordFinder(string pwd) => password = pwd;
            public char[] GetPassword() => password.ToCharArray();
        }
    }
}
