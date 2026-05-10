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
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace SK.SmartId
{
    public static class DigestCalculator
    {
        public static byte[] CalculateDigest(byte[] dataToDigest, HashType hashType)
        {
            if (hashType == null)
            {
                throw new UnprocessableSmartIdResponseException("Problem with digest calculation.");
            }
            return CalculateDigest(dataToDigest, HashTypeConversions.FromHashType(hashType));
        }

        public static byte[] CalculateDigest(byte[] dataToDigest, SmartIdHashAlgorithm algorithm)
        {
            try
            {
                switch (algorithm)
                {
                    case SmartIdHashAlgorithm.SHA_256:
                    case SmartIdHashAlgorithm.SHA_384:
                    case SmartIdHashAlgorithm.SHA_512:
                        using (var hashAlg = HashAlgorithm.Create(MapToSystemName(algorithm)))
                        {
                            return hashAlg.ComputeHash(dataToDigest);
                        }
                    case SmartIdHashAlgorithm.SHA3_256:
                        return Sha3Digest(dataToDigest, 256);
                    case SmartIdHashAlgorithm.SHA3_384:
                        return Sha3Digest(dataToDigest, 384);
                    case SmartIdHashAlgorithm.SHA3_512:
                        return Sha3Digest(dataToDigest, 512);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
                }
            }
            catch (Exception e) when (!(e is ArgumentOutOfRangeException))
            {
                throw new UnprocessableSmartIdResponseException("Problem with digest calculation. " + e);
            }
        }

        private static string MapToSystemName(SmartIdHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SmartIdHashAlgorithm.SHA_256:
                    return HashType.SHA256.AlgorithmName.Name;
                case SmartIdHashAlgorithm.SHA_384:
                    return HashType.SHA384.AlgorithmName.Name;
                case SmartIdHashAlgorithm.SHA_512:
                    return HashType.SHA512.AlgorithmName.Name;
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm));
            }
        }

        private static byte[] Sha3Digest(byte[] data, int bitLength)
        {
            var digest = new Sha3Digest(bitLength);
            digest.BlockUpdate(data, 0, data.Length);
            var output = new byte[digest.GetDigestSize()];
            digest.DoFinal(output, 0);
            return output;
        }
    }
}