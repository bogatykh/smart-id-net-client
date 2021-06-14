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

using System;

namespace SK.SmartId.Rest.Dao
{
    public class SessionStatusRequest
    {
        public SessionStatusRequest(string sessionId)
        {
            SessionId = sessionId;
        }

        public string SessionId { get; }

        /// <summary>
        /// Request long poll timeout value.If not provided, a default is used.
        /// <para>
        /// This parameter is used for a long poll method, meaning the request method might not return until a timeout expires
        /// set by this parameter.
        /// </para>
        /// Caller can tune the request parameters inside the bounds set by service operator.
        /// </summary>
        public TimeSpan? ResponseSocketOpenTime { get; set; }

        public bool IsResponseSocketOpenTimeSet()
        {
            return ResponseSocketOpenTime.HasValue && ResponseSocketOpenTime.Value.TotalMilliseconds > 0;
        }
    }
}