/*-
 * #%L
 * Smart ID .NET client
 * %%
 * Copyright (C) 2018 - 2026 SK ID Solutions AS
 * #L%
 */

using System;

namespace SK.SmartId
{
    /// <summary>
    /// RSASSA-PSS trailer field from session status (Java <c>ee.sk.smartid.signature.TrailerField</c>).
    /// </summary>
    public readonly struct TrailerField : IEquatable<TrailerField>
    {
        private readonly byte _id;

        private TrailerField(byte id)
        {
            _id = id;
        }

        /// <summary>Hexadecimal <c>0xbc</c>, PSS trailer-field value <c>1</c>.</summary>
        public static TrailerField BC => new TrailerField(1);

        public string GetValue()
        {
            return _id == 1 ? "0xbc" : throw new InvalidOperationException();
        }

        public int GetPssSpecValue()
        {
            return _id == 1 ? 1 : throw new InvalidOperationException();
        }

        /// <summary>Parses API string (Java <c>TrailerField.fromString</c>).</summary>
        public static bool TryParse(string value, out TrailerField field)
        {
            if (string.Equals(value, BC.GetValue(), StringComparison.Ordinal))
            {
                field = BC;
                return true;
            }

            field = default;
            return false;
        }

        public bool Equals(TrailerField other) => _id == other._id;

        public override bool Equals(object obj) => obj is TrailerField other && Equals(other);

        public override int GetHashCode() => _id.GetHashCode();

        public static bool operator ==(TrailerField left, TrailerField right) => left.Equals(right);

        public static bool operator !=(TrailerField left, TrailerField right) => !left.Equals(right);
    }
}
