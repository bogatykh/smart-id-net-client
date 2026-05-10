/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest.Dao;

namespace SK.SmartId.Common
{
    public static class InteractionUtil
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static string EncodeToBase64(IReadOnlyList<Interaction> interactions)
        {
            try
            {
                string json = JsonSerializer.Serialize(interactions, JsonOptions);
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception ex)
            {
                throw new SmartIdClientException("Unable to encode interactions to Base64", ex);
            }
        }

        public static string CalculateDigest(string interactions)
        {
            byte[] digest = DigestCalculator.CalculateDigest(Encoding.UTF8.GetBytes(interactions), SmartIdHashAlgorithm.SHA_256);
            return Convert.ToBase64String(digest);
        }

        public static bool IsEmpty(IReadOnlyList<ISmartIdInteraction> interactions)
        {
            return interactions == null || !interactions.Where(i => i != null).Any();
        }
    }
}
