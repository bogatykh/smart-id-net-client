/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SK.SmartId
{
    /// <summary>RSASSA-PSS and RSASSA-PKCS#1 v1.5 signature verification (Java <c>SignatureValueValidatorImpl</c>).</summary>
    public sealed class SignatureValueValidatorImpl : ISignatureValueValidator
    {
        public void Validate(
            byte[] signatureValue,
            byte[] payload,
            X509Certificate2 certificate,
            ISignatureFactory signatureFactory)
        {
            if (signatureValue == null)
            {
                throw new SmartIdClientException("Parameter 'signatureValue' is not provided");
            }
            if (payload == null)
            {
                throw new SmartIdClientException("Parameter 'payload' is not provided");
            }
            if (certificate == null)
            {
                throw new SmartIdClientException("Parameter 'certificate' is not provided");
            }
            if (signatureFactory == null)
            {
                throw new SmartIdClientException("Parameter 'signatureFactory' is not provided");
            }
            try
            {
                if (!signatureFactory.VerifySignature(certificate, payload, signatureValue))
                {
                    throw new UnprocessableSmartIdResponseException(
                        "Provided signature value does not match the calculated signature value");
                }
            }
            catch (UnprocessableSmartIdResponseException)
            {
                throw;
            }
            catch (CryptographicException ex)
            {
                throw new UnprocessableSmartIdResponseException("Signature value validation failed", ex);
            }
            catch (Exception ex)
            {
                throw new UnprocessableSmartIdResponseException("Signature value validation failed", ex);
            }
        }
    }
}
