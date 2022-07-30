/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2022 SK ID Solutions AS
 * %%
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 * #L%
 */

using SK.SmartId.Exceptions;
using System;
using System.Diagnostics;
using System.Globalization;

namespace SK.SmartId.Util
{
    public static class NationalIdentityNumberUtil
    {
        private const string DATE_FORMAT_YYYY_MM_DD = "yyyyMMdd";

        /// <summary>
        /// Detect date-of-birth from a Baltic national identification number if possible or return null.
        /// <para>This method always returns the value for all Estonian and Lithuanian national identification numbers.</para>
        /// <para>It also works for older Latvian personal codes but Latvian personal codes issued after July 1st 2017 (starting with "32") do not carry date-of-birth.</para>
        /// <para>For non-Baltic countries (countries other than Estonia, Latvia or Lithuania) it always returns null (even if it would be possible to deduce date of birth from national identity number).</para>
        /// <para>Newer (but not all) Smart-ID certificates have date-of-birth on a separate attribute. It is recommended to use that value if present</para>
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
                    return null;
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
                return DateTime.ParseExact(birthDate, DATE_FORMAT_YYYY_MM_DD, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new UnprocessableSmartIdResponseException("Could not parse birthdate from nationalIdentityNumber=" + eeOrLtNationalIdentityNumber, e);
            }
        }

        public static DateTime? ParseLvDateOfBirth(string lvNationalIdentityNumber)
        {
            string birthDay = lvNationalIdentityNumber.Substring(0, 2);
            if ("32".Equals(birthDay))
            {
                Debug.WriteLine("Person has newer type of Latvian ID-code that does not carry birthdate info");
                return null;
            }

            string birthMonth = lvNationalIdentityNumber.Substring(2, 2);
            string birthYearTwoDigit = lvNationalIdentityNumber.Substring(4, 2);
            string century = lvNationalIdentityNumber.Substring(7, 1);
            string birthDateYyyyMmDd;

            switch (century)
            {
                case "0":
                    birthDateYyyyMmDd = "18" + (birthYearTwoDigit + birthMonth + birthDay);
                    break;
                case "1":
                    birthDateYyyyMmDd = "19" + (birthYearTwoDigit + birthMonth + birthDay);
                    break;
                case "2":
                    birthDateYyyyMmDd = "20" + (birthYearTwoDigit + birthMonth + birthDay);
                    break;
                default:
                    throw new UnprocessableSmartIdResponseException("Invalid personal code: " + lvNationalIdentityNumber);
            }

            try
            {
                return DateTime.ParseExact(birthDateYyyyMmDd, DATE_FORMAT_YYYY_MM_DD, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new UnprocessableSmartIdResponseException("Unable get birthdate from Latvian personal code " + lvNationalIdentityNumber, e);
            }
        }
    }
}
