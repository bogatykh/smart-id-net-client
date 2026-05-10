/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
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

using System;
using SK.SmartId.Exceptions.Permanent;

namespace SK.SmartId
{
    internal static class HashTypeConversions
    {
        public static SmartIdHashAlgorithm FromHashType(HashType hashType)
        {
            if (hashType == HashType.SHA256)
            {
                return SmartIdHashAlgorithm.SHA_256;
            }
            if (hashType == HashType.SHA384)
            {
                return SmartIdHashAlgorithm.SHA_384;
            }
            if (hashType == HashType.SHA512)
            {
                return SmartIdHashAlgorithm.SHA_512;
            }
            throw new ArgumentOutOfRangeException(nameof(hashType));
        }

        public static HashType ToHashType(SmartIdHashAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case SmartIdHashAlgorithm.SHA_256:
                    return HashType.SHA256;
                case SmartIdHashAlgorithm.SHA_384:
                    return HashType.SHA384;
                case SmartIdHashAlgorithm.SHA_512:
                    return HashType.SHA512;
                default:
                    throw new SmartIdClientException(
                        "HashType is only defined for SHA-256, SHA-384, and SHA-512. Use SmartIdHashAlgorithm on SignableData/SignableHash for SHA-3.");
            }
        }
    }
}
