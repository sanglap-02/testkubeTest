using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.POIFS.Crypt;
using SharedLibrary.Framework.Logger.Interface;

namespace SharedLibrary.main.auto.framework.utilities
{
    public class EncodingDecoding
    {
        private readonly ILogging _logging;
        public EncodingDecoding(ILogging logging) => _logging = logging;
        
        public string GetEncodedStringValue(string stringToEncode)
        {
            _logging.LogInformation("EncodingDecoding - GetEncodedStringValue");
            
            byte[] encodedString = Encoding.UTF8.GetBytes(stringToEncode);
            return Convert.ToBase64String(encodedString);
        }
        
        public string GetDecodedStringValue(string stringToDecode)
        {
            _logging.LogInformation("EncodingDecoding - GetDecodedStringValue");
            
            byte[] decodedString = Convert.FromBase64String(stringToDecode);
            return Encoding.UTF8.GetString(decodedString);
        }
    }
}
