/*-
 * #%L
 * Smart ID .NET client tests — port of Java <c>ee.sk.smartid.signature.SignatureValueValidatorImplTest</c>.
 * #L%
 */

using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SK.SmartId
{
    public sealed class SignatureValueValidatorImplTest
    {
        private const string CertB64 =
            "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";

        private static readonly byte[] Payload = Convert.FromBase64String("ZGF0YVRvU2lnbg==");

        private static readonly IReadOnlyDictionary<SigningSignatureAlgorithm, byte[]> SignatureValueMap =
            new Dictionary<SigningSignatureAlgorithm, byte[]>
            {
                [SigningSignatureAlgorithm.RSASSA_PSS] = Convert.FromBase64String(
                    "iBgRsurf7Hc0uEUYAnRR3IfQ8JXNvYM/Ft9TvWtQvnatwzLLDDukhpPCRPPP5CtNrPvKVLSLSJ9GRMz1w6lHqrlooWJm74sQaD0o6QAl5UrdaJe5ez9zn/jBPh0p+E5Z1TC9bYFTibyRHGX1W6LFTP+/wDS0eGLURnc82Q0CxQjjqzlpOQXBFqPYLmCFn8ps2AAzzKPIKLimebkuOYzYWGFT25qaiWN8ZHif8RysEYex07eTmsaLl/ZeO5DURZtJaaIFFQURcD7XIgg1iy4jns3Q3T+E5lfdPMwocn3CZgZNyx0gsoiJF0A3LRjN9lb5MS3SCN697i8EFZxsOXg/IJRWjwp2afeRgJunwFK/STkeVVDwdOAgSJ0DY4/BjasFWRuBSdrkTPPjzAFvJa00DRofS1GMaayjcVgw4H7QAvAXoden/QTLUMIy8nJSfVTfAV42dbfBLrlkenL2fWfyR6gr1s8Lkh9beFM+5AYjfwNlsuy1iY6riqVtcFDl8glpE0bxo706W143RPRxfo2VxJkk+hZtcSaunMkFCjFxxzHUOgh2Jmtz7fmWmWtknnTcc6uBhB2w00y4lLrQ80c80TMO8K6uivZU2WQ7PDxvJ0uOJXDXJItd36Aa7wwIV/XPefkDrCBRyUdpKKbgjSO3S0cbRhnvVzSDy8t95wFheXKg8uJ8bbU/f2ZpFpCN+dk+HTgSNffh9dpCV4FgjDhMYE1Oz7Ennf/gLWW5FePg7K9Xd1d9T5DfBb/YGVvxOOoleQr0DTbarCwGaUnyTSatgcSMrtSrQlq0nh1p3RKrJF8qluqNqdDvhFLMEEUHt66Zkg/JEiT/k3+PYMk43E2TsIaqTOrs+AiTvZRuJlwhXKAUSApG6U76UdsWF3Ui7pGBRlwHDbJtq4qsAhUiEqSqfFnsGeIMxvUT9pv7aasStv13G7RzehGtxoOqkae8ajy7prmFgRl0Om8xwcHWXRGlMFTppeagsjG5ilKYkj7RpWo2NOXCwsOx5KXQxu3xcv1Y"),
                [SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION] = Convert.FromBase64String(
                    "cRZanVdkMHJAuBYKpmmwT4TowxqibpwpFR//XJo3Cx71XN55XlMeTrrhjrNs/IYr2y5plRFD9n8OkrvkQIK/dSzAhnnwVirMFmJv/NjxXkb1HisKCS1XMEcNtpW17k5N6PU8yOD6jLZqcMmGCIjO/PQQZfIwKXJCOC1YC0oAmaBDvVMpT+j4jaxoGhdzpjvf5vaueam94K9DmMRkJaoCxo93WGa04dZgYOz62GnFp991bOQv95TasH48fUkVCju5zSuzD2Cf8jaxd3Zc8V8JfsUEmAiumsyTv2wNyjrTZ3TVh3W7oOYXLI7GLb6XQFVQG7TBwnPnD4wf5SvLuRgCUtXVUjYLHt9tQz20AO7tEAxnX3FcmS4r759aiqFjX/xreCmIQ96zTnsF3pMsHgPhnnPCiXHRRWQHPU1vil19LYTciYrjRouuO2IKV9Etx/5Ire3YUXQM81yDfeWfPFEj4s8DRtywYNW9Ditv2MECUJTP07h4DbKpNpJFHfh6etI43YM755949DkFwmN6ZyIvZ32jgclV0ZpPsAvB0VquH8Odc8OpHYOXMBvHXr/Kd2TywTNA88HFgesBhWSbKcKt756cvFbUX/+Eji/DxjJ6DTmS8jl9q+dPvcIAzNcDEcmxTGcwS+qjD6HzoMv0I3+pZ4v3ZzwwDeahfhAqxMr3sqceCqzqS1fG+KTpeXJw+vADnVcCh+oOKPfLf9iQStOheX2c+cBqjEGWuoCVrMY+3+MaQ0NHEjG5UA7M+m9Oj80Jr6ODSSFLi4KM6NZ1yT/vOh4b0klIET0LSVm3HXACvmVc+l7IC2toVNoAsaZ83zQ2O5R2dtwNc/6p8TCOrlfvxnx3MM8R8HDET1csyqnnOAiZi0JToMJkr3F/0N8gFgAb/BJs7onETvCLuv+8SCaTTJD5XdcUno6BrjRho9vcbOnjpKWE6KbsOZu/rYwWkeqeMEzgHryD0MkYLUhqfNHuIKw2zJgP5T0apsKASoJjSLzTd0TFFNkT34MNx7YMEOHF"),
                [SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION] = Convert.FromBase64String(
                    "XHDnLzHGds19lU4zdwWILXGOoFEE9h1OCZ3McSJE0osEWkR7v05FCsLTylSx/UdExxgqdychsBP6vlRyrkx5aZM64xletCgMbd+AQn3+nGxkCFSWzo/szv/8fB0OpJ5o1Ks5BpgvONrfRVhjvVCYxH9d1tjNvJYNMsxVYvpkz4Njox0IefOHwJWj9VoL13S/g5h0MBUNPjKlec3evCbZ4n0eesQySY6dQSWL8WhMdnXp9UIU0pEROEhO2Y+iHnjXa1T7FKcdzh/uhHaAipJnufBJ2Q0xpjlV3GPpYMhvk/qL2Rdh71bJKlPDaKCdZEgOglK/++qHmuxMOfMRimGT57QGVcVMcQXf/iBzoSIkucZu4rwPhQdVJpVSpBydO8rVcz1orSi3WCYHcQIMmdn3MSSK3nIxxpx/DLBChVNoVyGrCKLEDktb5ZCRS71RZZBrIW2reMi9l/jQGdZ5YAdQ1hx29LhANtxFPL+8imbBHqbOCkGHGLf+5d9ibmLWPBfnwdoBuN9pb5UH3gdf6/YWp3XObOoY2919ZwtWqBYqY0rum5a6cGuVia+52y8z+GJql/IE49sLispI9B0gm1tWY1mH7YP+6mzxweNBY+O4L49X23YHdNq9uTgYkkmUFu4CQqFDpuzqMh8KabgnZxop4IwXyZSKlAFFiM7GQgLKI9MRM69e/xjMMBjBjzILgusDdnStRs3VcRWW5zpbeUwqnEiV+stZweDSuLJsy4BfAEeYOzVVSu1O94zIKZJJRl+sW9W0lbfdQd0jjKzOXebTLegud+RzU4Agoh4zLa0mGS4pMiNJfye4Lt8po7JCLsqR4RU46UFnyt/R7eTU8Zk01nN8F1O+qyo2FIyU2drOxybhE2YsOdgBiAox04IlR1XmHFenaytFn+6Gn951gZPah+1OrCWiO+z63jzdAfNMkWk5vhaqr81jYKYAYc8d5/TTEd4kkNtK5aaLoiwh7ZAG5nu4Y7dciGt2YJurINdG9la3Z9cJA8DJAkjyrNK9Uz9p"),
                [SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION] = Convert.FromBase64String(
                    "IgbB+cIDO2jTFuzWWowjN2PZlGm1PFLLSDv2K6eEfnSt1DPHDeKl8nQMituwuNOqh6HpG6TZhmHvOxebxx9FCSNyradw4rYNX/tSDEuIc4dBZQGLHJqDbI+1Yz1mHOEZE1GpJb8LbHF34/1z98s05isTQ8kwiFkzipSL1Qm3G/o6O018yEsCD4YxsDL8YUBdgUzF1T+NhMOppDy5rVge+gblGY2WfFV+Q0i3dPuqllqpa7k31QEBzAzeOPzUQqC78J9Rax9AaR1vKrYS5RR9U9hxCCQUROK6Ead/1igYnDfwd8xXe+RMSel46KQkBrH9tWUw5DpQuzDbyuCw/2HvK68/jZwJjAeAPoiRgE2DLuQJkzTGMDE6JREe/nJ+LpOSEJxX+eyKp4Ia1+J317j/cK+CApdbAK1molV1Cb4N264tT+KZmG24ERDfMDfXKzLVJUF+AIfJbQdbxxUZ51D+NQX03gj6yjVoIQ0rAdFbRarqtW1AAPAqNfcx9hvYTfD52fzPk3IHd2WMcronmTLU21RCnu0V072DXsKBOnQyBmL3UE4udVQBZxOsip3u2y/qGN8fVFzL5J0+xHbDIixHWo7b5QCR+ZZSUZmoVA/SMqumRGVMcUTaRwxPOih32+eXOElZRoGJo80Km8EHJPpo1Fvsw13I9NyHKRudIFDVJz+iJTZRHrVaWAxUdf5qJZI1FknQXX3jUmrcCMFdjAlGpVz2LxCPz7rHJh6oBg4+eWdP4TdnHuG6Hzh4QaAKEvHnmWHNHSKNYYTCzkOpRMVgQd2w9ymkVHgu8K0oBwKcMu43Pb9u/lxWRUgwt3qf6wa336vrE70A17xJZa2fWCncnCIl9ilXKH1FxHrWvhK7rH1YgfLgYQHXcO5PGDEemMiPmzKnGJDhP11BaNEhnI9ogWCr/nahwDL7nBpgYP9SMN3EY6pxZzP9II6fGWA9VNtGJg4fif6pPVUutLV5El+cf04rWChBMis+5cSgKyKNAPBHJFHiASDOEaPG4fpgaiCT")
            };

        private static readonly X509Certificate2 Cert = CertificateParser.ParseX509Certificate(CertB64);

        private readonly ISignatureValueValidator validator = new SignatureValueValidatorImpl();

        private static RsaSsaPssParameters ToRsaSsaPssParameters()
        {
            return new RsaSsaPssParameters
            {
                DigestHashAlgorithm = SmartIdHashAlgorithm.SHA_512,
                MaskGenAlgorithm = "id-mgf1",
                MaskHashAlgorithm = SmartIdHashAlgorithm.SHA_512,
                SaltLength = SmartIdHashAlgorithm.SHA_512.GetDigestOctetLength(),
                TrailerField = "0xbc"
            };
        }

        [Fact]
        public void Validate_withRsaSsaPssSignatureFactory_ok()
        {
            var factory = new RsaSsaPssSignatureFactory(ToRsaSsaPssParameters());
            validator.Validate(SignatureValueMap[SigningSignatureAlgorithm.RSASSA_PSS], Payload, Cert, factory);
        }

        [Theory]
        [InlineData(SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION)]
        public void Validate_withRsaSsaPkcs1SignatureFactory_ok(SigningSignatureAlgorithm signatureAlgorithm)
        {
            var factory = new RsaSsaPkcs1SignatureFactory(signatureAlgorithm);
            validator.Validate(SignatureValueMap[signatureAlgorithm], Payload, Cert, factory);
        }

        public static IEnumerable<object[]> EmptyInputCases()
        {
            yield return new object[] { null, null, null, null, "signatureValue" };
            yield return new object[] { Array.Empty<byte>(), null, null, null, "payload" };
            yield return new object[] { Array.Empty<byte>(), Array.Empty<byte>(), null, null, "certificate" };
            yield return new object[] { Array.Empty<byte>(), Array.Empty<byte>(), Cert, null, "signatureFactory" };
        }

        [Theory]
        [MemberData(nameof(EmptyInputCases))]
        public void Validate_inputNotProvided_throwException(
            byte[] signatureValue,
            byte[] payload,
            X509Certificate2 certificate,
            ISignatureFactory signatureFactory,
            string errorParameter)
        {
            var ex = Assert.Throws<SmartIdClientException>(() =>
                validator.Validate(signatureValue, payload, certificate, signatureFactory));
            Assert.Equal("Parameter '" + errorParameter + "' is not provided", ex.Message);
        }

        [Fact]
        public void Validate_withRsaSsaPssSignatureFactory_invalidSignature_throwException()
        {
            var factory = new RsaSsaPssSignatureFactory(ToRsaSsaPssParameters());
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                validator.Validate(
                    System.Text.Encoding.UTF8.GetBytes("invalidValue"),
                    Payload,
                    Cert,
                    factory));
            // BouncyCastle PSS returns false for this short payload; Java JCA often throws GeneralSecurityException instead.
            Assert.Equal("Provided signature value does not match the calculated signature value", ex.Message);
        }

        [Theory]
        [InlineData(SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION)]
        public void Validate_withRsaSsaPkcs1SignatureFactory_invalidSignature_throwException(SigningSignatureAlgorithm algorithm)
        {
            var factory = new RsaSsaPkcs1SignatureFactory(algorithm);
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                validator.Validate(
                    System.Text.Encoding.UTF8.GetBytes("invalidSignature"),
                    Payload,
                    Cert,
                    factory));
            Assert.Equal("Provided signature value does not match the calculated signature value", ex.Message);
        }

        [Fact]
        public void Validate_withRsaSsaPssSignatureFactory_payloadDoesNotMatch_throwException()
        {
            var factory = new RsaSsaPssSignatureFactory(ToRsaSsaPssParameters());
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                validator.Validate(
                    SignatureValueMap[SigningSignatureAlgorithm.RSASSA_PSS],
                    System.Text.Encoding.UTF8.GetBytes("payloadThatDoesNotMatch"),
                    Cert,
                    factory));
            // BouncyCastle may throw during PSS decode for wrong message length; surfaced as validation failure (Java returns false from verify).
            Assert.Equal("Signature value validation failed", ex.Message);
        }

        [Theory]
        [InlineData(SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION)]
        [InlineData(SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION)]
        public void Validate_withRsaSsaPkcs1SignatureFactory_payloadDoesNotMatch_throwException(
            SigningSignatureAlgorithm signatureAlgorithm)
        {
            var factory = new RsaSsaPkcs1SignatureFactory(signatureAlgorithm);
            var ex = Assert.Throws<UnprocessableSmartIdResponseException>(() =>
                validator.Validate(
                    SignatureValueMap[signatureAlgorithm],
                    System.Text.Encoding.UTF8.GetBytes("payloadThatDoesNotMatch"),
                    Cert,
                    factory));
            Assert.Equal("Provided signature value does not match the calculated signature value", ex.Message);
        }
    }
}
