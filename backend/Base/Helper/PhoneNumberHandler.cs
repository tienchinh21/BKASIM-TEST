namespace MiniAppGIBA.Base.Helpers
{
    public class PhoneNumberHandler
    {
        public static string FixFormatPhoneNumber(string phoneNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length > 12 && phoneNumber.Length < 9)
                {
                    return "";
                }

                if (phoneNumber[0] == '0')
                {
                    phoneNumber = "84" + phoneNumber.Substring(1, phoneNumber.Length - 1);
                }
                if (phoneNumber[0] != '0' && (phoneNumber.Count() == 9 || phoneNumber.Count() == 10))
                {
                    phoneNumber = "84" + phoneNumber;
                }
                string kq = phoneNumber;
                string dauso = phoneNumber.Substring(0, 5);
                string cuoiso = phoneNumber.Substring(5);
                //mobi
                if (dauso == "84120") kq = "8470" + cuoiso;
                else if (dauso == "84121") kq = "8479" + cuoiso;
                else if (dauso == "84122") kq = "8477" + cuoiso;
                else if (dauso == "84126") kq = "8476" + cuoiso;
                else if (dauso == "84128") kq = "8478" + cuoiso;
                //vina
                else if (dauso == "84123") kq = "8483" + cuoiso;
                else if (dauso == "84124") kq = "8484" + cuoiso;
                else if (dauso == "84125") kq = "8485" + cuoiso;
                else if (dauso == "84127") kq = "8481" + cuoiso;
                else if (dauso == "84129") kq = "8482" + cuoiso;
                //viettel
                else if (dauso == "84162") kq = "8432" + cuoiso;
                else if (dauso == "84163") kq = "8433" + cuoiso;
                else if (dauso == "84164") kq = "8434" + cuoiso;
                else if (dauso == "84165") kq = "8435" + cuoiso;
                else if (dauso == "84166") kq = "8436" + cuoiso;
                else if (dauso == "84167") kq = "8437" + cuoiso;
                else if (dauso == "84168") kq = "8438" + cuoiso;
                else if (dauso == "84169") kq = "8439" + cuoiso;
                //VNM
                else if (dauso == "84186") kq = "8456" + cuoiso;
                else if (dauso == "84188") kq = "8458" + cuoiso;
                //GTEL
                else if (dauso == "84199") kq = "8459" + cuoiso;

                return kq;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static byte GetIdTelco(string sdt)
        {
            try
            {
                if (string.IsNullOrEmpty(sdt))
                {
                    return 0;
                }
                if (sdt.Length < 8)
                {
                    return 0;
                }
                sdt = FixFormatPhoneNumber(sdt);
                byte kq = 0;
                int len = sdt.Length;
                string PhonePrefix = sdt.Substring(0, 4);
                string PhonePrefixKorean = sdt.Substring(0, 3);
                if (len == 11)
                {
                    if (PhonePrefix == "8489") kq = 1;
                    else if (PhonePrefix == "8490") kq = 1;
                    else if (PhonePrefix == "8493") kq = 1;
                    else if (PhonePrefix == "8470") kq = 1;
                    else if (PhonePrefix == "8479") kq = 1;
                    else if (PhonePrefix == "8477") kq = 1;
                    else if (PhonePrefix == "8476") kq = 1;
                    else if (PhonePrefix == "8478") kq = 1;
                    else if (PhonePrefix == "8491") kq = 2;
                    else if (PhonePrefix == "8494") kq = 2;
                    else if (PhonePrefix == "8488") kq = 2;
                    else if (PhonePrefix == "8483") kq = 2;
                    else if (PhonePrefix == "8484") kq = 2;
                    else if (PhonePrefix == "8485") kq = 2;
                    else if (PhonePrefix == "8481") kq = 2;
                    else if (PhonePrefix == "8482") kq = 2;
                    else if (PhonePrefix == "8497") kq = 3;
                    else if (PhonePrefix == "8498") kq = 3;
                    else if (PhonePrefix == "8496") kq = 3;
                    else if (PhonePrefix == "8486") kq = 3;
                    else if (PhonePrefix == "8432") kq = 3;
                    else if (PhonePrefix == "8433") kq = 3;
                    else if (PhonePrefix == "8434") kq = 3;
                    else if (PhonePrefix == "8435") kq = 3;
                    else if (PhonePrefix == "8436") kq = 3;
                    else if (PhonePrefix == "8437") kq = 3;
                    else if (PhonePrefix == "8438") kq = 3;
                    else if (PhonePrefix == "8439") kq = 3;
                    else if (PhonePrefix == "8492") kq = 12;
                    else if (PhonePrefix == "8452") kq = 12;
                    else if (PhonePrefix == "8456") kq = 12;
                    else if (PhonePrefix == "8458") kq = 12;
                    else if (PhonePrefix == "8499") kq = 11;
                    else if (PhonePrefix == "8459") kq = 11;
                    else if (PhonePrefixKorean == "010") kq = 6;
                    else if (PhonePrefixKorean == "011") kq = 6;
                    else if (PhonePrefixKorean == "013") kq = 6;
                    else if (PhonePrefix == "8487") kq = 14;
                    else if (PhonePrefix == "8455") kq = 16;
                }
                else if (len == 12)
                {
                    PhonePrefix = sdt.Substring(0, 5);
                    if (PhonePrefix == "84126") kq = 1;
                    else if (PhonePrefix == "84128") kq = 1;
                    else if (PhonePrefix == "84120") kq = 1;
                    else if (PhonePrefix == "84121") kq = 1;
                    else if (PhonePrefix == "84122") kq = 1;
                    else if (PhonePrefix == "84123") kq = 2;
                    else if (PhonePrefix == "84124") kq = 2;
                    else if (PhonePrefix == "84125") kq = 2;
                    else if (PhonePrefix == "84127") kq = 2;
                    else if (PhonePrefix == "84129") kq = 2;
                    else if (PhonePrefix == "84168") kq = 3;
                    else if (PhonePrefix == "84169") kq = 3;
                    else if (PhonePrefix == "84166") kq = 3;
                    else if (PhonePrefix == "84165") kq = 3;
                    else if (PhonePrefix == "84167") kq = 3;
                    else if (PhonePrefix == "84164") kq = 3;
                    else if (PhonePrefix == "84163") kq = 3;
                    else if (PhonePrefix == "84162") kq = 3;
                    else if (PhonePrefix == "84186") kq = 12;
                    else if (PhonePrefix == "84186") kq = 12;
                    else if (PhonePrefix == "84188") kq = 12;
                    else if (PhonePrefix == "84199") kq = 11;
                }
                return kq;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        // Method to validate if the keyword is a valid phone number
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            // Example regex for validating phone numbers (you can adjust based on your phone number format)
            string pattern = @"^\+?\d{10,15}$"; // This is a simple pattern for international phone numbers
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern);
        }

    }
}
