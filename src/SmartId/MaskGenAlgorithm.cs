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
    /// Mask generation algorithm in session RSASSA-PSS parameters (Java <c>ee.sk.smartid.signature.MaskGenAlgorithm</c>).
    /// </summary>
    public readonly struct MaskGenAlgorithm : IEquatable<MaskGenAlgorithm>
    {
        private readonly byte _id;

        private MaskGenAlgorithm(byte id)
        {
            _id = id;
        }

        /// <summary>Smart-ID API <c>id-mgf1</c>; Java crypto name <c>MGF1</c>.</summary>
        public static MaskGenAlgorithm IdMgf1 => new MaskGenAlgorithm(1);

        /// <summary>Value sent in JSON <c>signatureAlgorithmParameters.maskGenAlgorithm.algorithm</c>.</summary>
        public string GetAlgorithmName()
        {
            return _id == 1 ? "id-mgf1" : throw new InvalidOperationException();
        }

        /// <summary>Name used when talking to .NET / BouncyCastle MGF APIs.</summary>
        public string GetMgfName()
        {
            return _id == 1 ? "MGF1" : throw new InvalidOperationException();
        }

        /// <summary>Parses the Smart-ID API string (Java <c>MaskGenAlgorithm.fromString</c>).</summary>
        public static bool TryParse(string apiAlgorithmName, out MaskGenAlgorithm value)
        {
            if (string.Equals(apiAlgorithmName, IdMgf1.GetAlgorithmName(), StringComparison.Ordinal))
            {
                value = IdMgf1;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// True for supported API id or legacy <c>MGF1</c> (verification and signing paths may use either).
        /// </summary>
        public static bool IsSupportedApiOrLegacyMgfName(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            return string.Equals(s, IdMgf1.GetAlgorithmName(), StringComparison.Ordinal)
                || string.Equals(s, IdMgf1.GetMgfName(), StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(MaskGenAlgorithm other) => _id == other._id;

        public override bool Equals(object obj) => obj is MaskGenAlgorithm other && Equals(other);

        public override int GetHashCode() => _id.GetHashCode();

        public static bool operator ==(MaskGenAlgorithm left, MaskGenAlgorithm right) => left.Equals(right);

        public static bool operator !=(MaskGenAlgorithm left, MaskGenAlgorithm right) => !left.Equals(right);
    }
}
