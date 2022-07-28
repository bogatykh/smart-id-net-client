using SK.SmartId.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;

namespace SK.SmartId.Util
{
    public static class NationalIdentityNumberUtil
    {
        private const string DATE_FORMAT = "yyyyMMdd";

        /// <summary>
        /// Detect date-of-birth from national identification number if possible or return null.
        /// <para>This method always returns the value for all Estonian and Lithuanian national identification numbers.</para>
        /// <para>It also works for older Latvian personal codes but Latvian personal codes issued after July 1st 2017 (starting with "32") do not carry date-of-birth.</para>
        /// <para>Some (but not all) Smart-ID certificates have date-of-birth on a separate attribute. It is recommended to use that value if present</para>
        /// <see cref="CertificateAttributeUtil.GetDateOfBirth(System.Security.Cryptography.X509Certificates.X509Certificate2)"/>
        /// </summary>
        /// <param name="authenticationIdentity">Authentication identity</param>
        /// <returns>DateOfBirth or null if it cannot be detected from personal code</returns>
        public static DateTime? GetDateOfBirth(AuthenticationIdentity authenticationIdentity)
        {
            string identityNumber = authenticationIdentity.IdentityNumber;

            switch (authenticationIdentity.Country.ToUpperInvariant())
            {
                case "EE":
                case "LT":
                    return ParseEeLtDateOfBirth(identityNumber);
                case "LV":
                    return ParseLvDateOfBirth(identityNumber);
                default:
                    throw new UnprocessableSmartIdResponseException("Unknown country: " + authenticationIdentity.Country);
            }
        }

        public static DateTime ParseEeLtDateOfBirth(string eeOrLtNationalIdentityNumber)
        {
            string birthDate = eeOrLtNationalIdentityNumber.Substring(1, 6);

            switch (eeOrLtNationalIdentityNumber.Substring(0, 1))
            {
                case "1":
                case "2":
                    birthDate = "18" + birthDate;
                    break;
                case "3":
                case "4":
                    birthDate = "19" + birthDate;
                    break;
                case "5":
                case "6":
                    birthDate = "20" + birthDate;
                    break;
                default:
                    throw new UnprocessableSmartIdResponseException("Invalid personal code " + eeOrLtNationalIdentityNumber);
            }

            try
            {
                return DateTime.ParseExact(birthDate, DATE_FORMAT, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new UnprocessableSmartIdResponseException("Could not parse birthdate from nationalIdentityNumber=" + eeOrLtNationalIdentityNumber, e);
            }
        }

        public static DateTime? ParseLvDateOfBirth(String lvNationalIdentityNumber)
        {
            if ("32".Equals(lvNationalIdentityNumber.Substring(0, 2)))
            {
                Debug.WriteLine("Person has newer type of Latvian ID-code that does not carry birthdate info");
                return null;
            }

            string birthDate = lvNationalIdentityNumber.Substring(4, 2) + lvNationalIdentityNumber.Substring(0, 4);

            switch (lvNationalIdentityNumber.Substring(7, 1))
            {
                case "0":
                    birthDate = "18" + birthDate;
                    break;
                case "1":
                    birthDate = "19" + birthDate;
                    break;
                case "2":
                    birthDate = "20" + birthDate;
                    break;
                default:
                    throw new UnprocessableSmartIdResponseException("Invalid personal code: " + lvNationalIdentityNumber);
            }

            try
            {
                return DateTime.ParseExact(birthDate, DATE_FORMAT, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new UnprocessableSmartIdResponseException("Unable get birthdate from Latvian personal code " + lvNationalIdentityNumber, e);
            }
        }
    }
}
