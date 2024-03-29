/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 SK ID Solutions AS
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
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    public class SmartIdAuthenticationResponse
    {
        public byte[] SignatureValue
        {
            get
            {
                try
                {
                    return Convert.FromBase64String(SignatureValueInBase64);
                }
                catch (FormatException)
                {
                    throw new UnprocessableSmartIdResponseException("Failed to parse signature value in base64. Probably incorrectly encoded base64 string: '" + SignatureValueInBase64);
                }
            }
        }

        public string EndResult { get; set; }

        public string SignatureValueInBase64 { get; set; }

        public string AlgorithmName { get; set; }

        public X509Certificate2 Certificate { get; set; }

        public string CertificateLevel { get; set; }

        public string SignedHashInBase64 { get; set; }

        public HashType HashType { get; set; }

        public string RequestedCertificateLevel { get; set; }

        public string DocumentNumber { get; set; }

        public string InteractionFlowUsed { get; set; }

        /// <summary>
        /// IP address of the device running the App.
        /// <para>
        /// IP address of the device running Smart-id app (or null if not returned)
        /// </para>
        /// </summary>
        /// <remarks>Present only for subscribed RPs and when available (e.g. not present in case state is TIMEOUT).</remarks>
        public string DeviceIpAddress { get; set; }
    }
}