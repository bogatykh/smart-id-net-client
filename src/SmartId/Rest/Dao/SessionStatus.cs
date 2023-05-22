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

using System.Text.Json.Serialization;

namespace SK.SmartId.Rest.Dao
{
    public class SessionStatus
    {
        [JsonPropertyName("state")]
        public virtual string State { get; set; }

        [JsonPropertyName("result")]
        public SessionResult Result { get; set; }

        [JsonPropertyName("cert")]
        public SessionCertificate Cert { get; set; }

        [JsonPropertyName("signature")]
        public SessionSignature Signature { get; set; }

        [JsonPropertyName("ignoredProperties")]
        public string[] IgnoredProperties { get; set; } = { };

        [JsonPropertyName("interactionFlowUsed")]
        public string InteractionFlowUsed { get; set; }

  /**
   * IP-address of the device running the App.
   *
   * Present only if withShareMdClientIpAddress() was specified with the request
   * Also, the RelyingParty must be subscribed for the service.
   * Also, the data must be available (e.g. not present in case state is TIMEOUT).
   * @see <a href="https://github.com/SK-EID/smart-id-documentation#238-mobile-device-ip-sharing">Mobile Device IP sharing</a>
   *
   * @return IP address of the device running Smart-ID app (or null if not returned)
   */
        [JsonPropertyName("deviceIpAddress")]
        public string DeviceIpAddress { get; set; }
    }
}