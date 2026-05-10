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

using System.Collections.Generic;
using System.Text.Json.Serialization;
using SK.SmartId;

namespace SK.SmartId.Rest.Dao
{
    public class DeviceLinkAuthenticationSessionRequest
    {
        [JsonPropertyName("relyingPartyUUID")]
        public string RelyingPartyUUID { get; set; }

        [JsonPropertyName("relyingPartyName")]
        public string RelyingPartyName { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("certificateLevel")]
        public string CertificateLevel { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("signatureProtocol")]
        public SignatureProtocol SignatureProtocol { get; set; }

        [JsonPropertyName("signatureProtocolParameters")]
        public AcspV2SignatureProtocolParameters SignatureProtocolParameters { get; set; }

        [JsonPropertyName("interactions")]
        public string Interactions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("requestProperties")]
        public RequestProperties RequestProperties { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("capabilities")]
        public HashSet<string> Capabilities { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("initialCallbackUrl")]
        public string InitialCallbackUrl { get; set; }
    }
}
