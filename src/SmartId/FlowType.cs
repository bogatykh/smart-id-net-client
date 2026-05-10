/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Copyright (C) 2018 - 2025 SK ID Solutions AS
 * #L%
 */

using System;

namespace SK.SmartId
{
    /// <summary>
    /// Flow the user completed the session with (Java <c>FlowType</c>).
    /// </summary>
    public enum FlowType
    {
        QR,
        Web2App,
        App2App,
        Notification
    }

    public static class FlowTypeExtensions
    {
        public static string GetApiValue(this FlowType flowType)
        {
            switch (flowType)
            {
                case FlowType.QR:
                    return "QR";
                case FlowType.Web2App:
                    return "Web2App";
                case FlowType.App2App:
                    return "App2App";
                case FlowType.Notification:
                    return "Notification";
                default:
                    throw new ArgumentOutOfRangeException(nameof(flowType), flowType, null);
            }
        }

        public static bool IsSupportedApiValue(string flowType)
        {
            return TryParse(flowType, out _);
        }

        public static FlowType ParseApiValue(string flowType)
        {
            if (!TryParse(flowType, out var result))
            {
                throw new ArgumentException("Invalid flowType value: " + flowType, nameof(flowType));
            }
            return result;
        }

        public static bool TryParse(string flowType, out FlowType result)
        {
            result = default;
            if (string.IsNullOrEmpty(flowType))
            {
                return false;
            }
            foreach (FlowType f in Enum.GetValues(typeof(FlowType)))
            {
                if (string.Equals(f.GetApiValue(), flowType, StringComparison.Ordinal))
                {
                    result = f;
                    return true;
                }
            }
            return false;
        }
    }
}
