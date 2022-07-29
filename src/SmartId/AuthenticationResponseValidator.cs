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
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>
    /// Class used to validate the authentication
    /// </summary>
    public class AuthenticationResponseValidator
    {
        private readonly List<X509Certificate2> trustedCACertificates = new List<X509Certificate2>();

        /// <summary>
        /// Constructs a new <see cref="AuthenticationResponseValidator"/>.
        /// </summary>
        /// <exception cref="Exceptions.Permanent.SmartIdClientException">when there was an error initializing trusted CA certificates</exception>
        /// <remarks>The constructed instance is initialized with default trusted CA certificates.</remarks>
        public AuthenticationResponseValidator()
        {
            InitializeTrustedCACertificatesFromKeyStore();
        }

        /// <summary>
        /// Constructs a new <see cref="AuthenticationResponseValidator"/>.
        /// </summary>
        /// <remarks>The constructed instance is initialized passed in certificates.</remarks>
        /// <exception cref="Exceptions.Permanent.SmartIdClientException">when there was an error initializing trusted CA certificates</exception>
        /// <param name="trustedCertificates">List of certificates to trust</param>
        public AuthenticationResponseValidator(X509Certificate2[] trustedCertificates)
        {
            trustedCACertificates.AddRange(trustedCertificates);
        }

        /// <summary>
        /// Validates the authentication response and returns the its result
        /// <para>Performs following validations:</para>
        /// <list type="bullet">
        ///     <item>"result.endResult" has the value "OK"</item>
        ///     <item>"signature.value" is the valid signature over the same "hash", which was submitted by the RP.</item>
        ///     <item>"signature.value" is the valid signature, verifiable with the public key inside the certificate of the user, given in the field "cert.value"</item>
        ///     <item>The person's certificate given in the "cert.value" is valid (not expired, signed by trusted CA and with correct (i.e. the same as in response structure, greater than or equal to that in the original request) level).</item>
        /// </list>
        /// </summary>
        /// <param name="authenticationResponse">authentication response to be validated</param>
        /// <returns>authentication result</returns>
        public AuthenticationIdentity Validate(SmartIdAuthenticationResponse authenticationResponse)
        {
            ValidateAuthenticationResponse(authenticationResponse);
            AuthenticationIdentity identity = ConstructAuthenticationIdentity(authenticationResponse.Certificate);
            if (!VerifyResponseEndResult(authenticationResponse))
            {
                throw new UnprocessableSmartIdResponseException("Smart-ID API returned end result code '" + authenticationResponse.EndResult + "'");
            }
            if (!VerifySignature(authenticationResponse))
            {
                throw new UnprocessableSmartIdResponseException("Failed to verify validity of signature returned by Smart-ID");
            }
            if (!VerifyCertificateExpiry(authenticationResponse.Certificate))
            {
                throw new UnprocessableSmartIdResponseException("Signer's certificate has expired");
            }
            if (!IsCertificateTrusted(authenticationResponse.Certificate))
            {
                throw new UnprocessableSmartIdResponseException("Signer's certificate is not trusted");
            }
            if (!VerifyCertificateLevel(authenticationResponse))
            {
                throw new CertificateLevelMismatchException();
            }
            return identity;
        }

        /// <summary>
        /// Gets the list of trusted CA certificates
        /// <para>
        /// Authenticating person's certificate has to be issued by
        /// one of the trusted CA certificates.Otherwise the person's
        /// authentication is deemed untrusted and therefore not valid.
        /// </para>
        /// </summary>
        /// <returns>list of trusted CA certificates</returns>
        public IReadOnlyList<X509Certificate2> GetTrustedCACertificates()
        {
            return trustedCACertificates.AsReadOnly();
        }

        /// <summary>
        /// Adds a certificate to the list of trusted CA certificates
        /// <para>
        /// Authenticating person's certificate has to be issued by
        /// one of the trusted CA certificates. Otherwise the person's
        /// authentication is deemed untrusted and therefore not valid.
        /// </para>
        /// </summary>
        /// <param name="certificate">certificate trusted CA certificate</param>
        public void AddTrustedCACertificate(X509Certificate2 certificate)
        {
            trustedCACertificates.Add(certificate);
        }

        /// <summary>
        /// Constructs a certificate from the byte array and
        /// adds it into the list of trusted CA certificates
        /// <para>
        /// Authenticating person's certificate has to be issued by
        /// one of the trusted CA certificates. Otherwise the person's
        /// authentication is deemed untrusted and therefore not valid.
        /// </para>
        /// </summary>
        /// <exception cref="CertificateException">when there was an error constructing the certificate from bytes</exception>
        /// <param name="certificateBytes">certificateBytes trusted CA certificate</param>
        public void AddTrustedCACertificate(byte[] certificateBytes)
        {
            X509Certificate2 caCertificate = new X509Certificate2(certificateBytes);
            AddTrustedCACertificate(caCertificate);
        }

        /// <summary>
        /// Constructs a certificate from the file and adds it into the list of trusted CA certificates
        /// <para>Authenticating person's certificate has to be issued by one of the trusted CA certificates. Otherwise the person's authentication is deemed untrusted and therefore not valid.</para>
        /// </summary>
        /// <param name="certificateFile">trusted CA certificate</param>
        /// <exception cref="CertificateException">when there is an error constructing the certificate from the bytes of the file</exception>
        public void AddTrustedCACertificate(string certificateFile)
        {
            AddTrustedCACertificate(File.ReadAllBytes(certificateFile));
        }

        /// <summary>
        /// Clears the list of trusted CA certificates
        /// </summary>
        /// <remarks>
        /// PS! When clearing the trusted CA certificates
        /// make sure it is not left empty. In that case
        /// there is impossible to verify the trust of the
        /// authenticating person.
        /// </remarks>
        public void ClearTrustedCACertificates()
        {
            trustedCACertificates.Clear();
        }

        private void InitializeTrustedCACertificatesFromKeyStore()
        {
            var assembly = GetType().Assembly;

            var resources = new HashSet<string>
            {
                "SK.SmartId.Resources.EID-SK_2016.pem.crt",
                "SK.SmartId.Resources.NQ-SK_2016.pem.crt",
                "SK.SmartId.Resources.TEST_of_EID-SK_2016.pem.crt",
                "SK.SmartId.Resources.TEST_of_NQ-SK_2016.pem.crt"
            };

            foreach (var resourceName in resources)
            {
                using (Stream resource = assembly.GetManifestResourceStream(resourceName))
                {
                    byte[] buffer = new byte[resource.Length];
                    int r, offset = 0;
                    while ((r = resource.Read(buffer, offset, buffer.Length - offset)) > 0)
                        offset += r;

                    var certificate = new X509Certificate2(buffer);
                    AddTrustedCACertificate(certificate);
                }
            }
        }

        private void ValidateAuthenticationResponse(SmartIdAuthenticationResponse authenticationResponse)
        {
            if (authenticationResponse.Certificate == null)
            {
                throw new UnprocessableSmartIdResponseException("Certificate is not present in the authentication response");
            }
            if (string.IsNullOrEmpty(authenticationResponse.SignatureValueInBase64))
            {
                throw new UnprocessableSmartIdResponseException("Signature is not present in the authentication response");
            }
            if (authenticationResponse.HashType == null)
            {
                throw new UnprocessableSmartIdResponseException("Hash type is not present in the authentication response");
            }
        }

        private bool VerifyResponseEndResult(SmartIdAuthenticationResponse authenticationResponse)
        {
            return "OK".Equals(authenticationResponse.EndResult, StringComparison.OrdinalIgnoreCase);
        }

        private bool VerifySignature(SmartIdAuthenticationResponse authenticationResponse)
        {
            try
            {
                var signersPublicKey = authenticationResponse.Certificate.GetRSAPublicKey();

                byte[] signedHash = Convert.FromBase64String(authenticationResponse.SignedHashInBase64);

                return signersPublicKey.VerifyHash(signedHash, authenticationResponse.SignatureValue, authenticationResponse.HashType.AlgorithmName, RSASignaturePadding.Pkcs1);
            }
            catch (CryptographicException e)
            {
                throw new UnprocessableSmartIdResponseException("Signature verification failed", e);
            }
        }

        private bool VerifyCertificateExpiry(X509Certificate2 certificate)
        {
            return !(certificate.NotAfter < DateTime.UtcNow);
        }

        private bool IsCertificateTrusted(X509Certificate2 certificate)
        {
            X509Chain verify = new X509Chain();

            verify.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            verify.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            foreach (X509Certificate2 trustedCACertificate in trustedCACertificates)
            {
                verify.ChainPolicy.ExtraStore.Add(trustedCACertificate);
            }

            if (verify.Build(certificate))
            {
                foreach (var chainElement in verify.ChainElements)
                {
                    if (string.Equals(chainElement.Certificate.Thumbprint, certificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    foreach (var trustedCert in verify.ChainPolicy.ExtraStore)
                    {
                        if (string.Equals(trustedCert.Thumbprint, chainElement.Certificate.Thumbprint, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool VerifyCertificateLevel(SmartIdAuthenticationResponse authenticationResponse)
        {
            CertificateLevel certLevel = new CertificateLevel(authenticationResponse.CertificateLevel);
            string requestedCertificateLevel = authenticationResponse.RequestedCertificateLevel;
            return string.IsNullOrEmpty(requestedCertificateLevel) || certLevel.IsEqualOrAbove(requestedCertificateLevel);
        }

        public static AuthenticationIdentity ConstructAuthenticationIdentity(X509Certificate2 certificate)
        {
            AuthenticationIdentity identity = new AuthenticationIdentity(certificate);

            string[] elements = certificate.SubjectName.Decode(X500DistinguishedNameFlags.UseNewLines | X500DistinguishedNameFlags.DoNotUseQuotes).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var element in elements)
            {
                string[] keyValue = element.Trim().Split('=');

                if (keyValue.Length != 2)
                {
                    continue;
                }

                if (keyValue[0].Equals("GIVENNAME", StringComparison.OrdinalIgnoreCase) || keyValue[0].Equals("GN", StringComparison.OrdinalIgnoreCase) || keyValue[0].Equals("G", StringComparison.OrdinalIgnoreCase))
                {
                    identity.GivenName = keyValue[1];
                }
                else if (keyValue[0].Equals("SURNAME", StringComparison.OrdinalIgnoreCase) || keyValue[0].Equals("SN", StringComparison.OrdinalIgnoreCase))
                {
                    identity.Surname = keyValue[1];
                }
                else if (keyValue[0].Equals("SERIALNUMBER", StringComparison.OrdinalIgnoreCase))
                {
                    identity.IdentityNumber = keyValue[1].Split(new char[] { '-' }, 2)[1];
                }
                else if (keyValue[0].Equals("C", StringComparison.OrdinalIgnoreCase))
                {
                    identity.Country = keyValue[1];
                }
            }

            identity.DateOfBirth = GetDateOfBirth(identity);

            return identity;
        }

        public static DateTime? GetDateOfBirth(AuthenticationIdentity identity)
        {
            return CertificateAttributeUtil.GetDateOfBirth(identity.AuthCertificate) ??
                NationalIdentityNumberUtil.GetDateOfBirth(identity);
        }
    }
}
