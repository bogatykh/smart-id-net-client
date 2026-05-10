/*-
 * #%L
 * Smart ID sample Java client
 * %%
 * Shared theory data ported from Java test ArgumentProviders:
 * CapabilitiesArgumentProvider, DuplicateDeviceLinkInteractionsProvider,
 * DuplicateNotificationInteractionArgumentProvider, InvalidRpChallengeArgumentProvider.
 */

using System.Collections.Generic;
using SK.SmartId.Common;

namespace SK.SmartId;

/// <summary>Shared MemberData ported from Java ArgumentProvider classes.</summary>
public static class SessionRequestBuilderTestData
{
    public static IEnumerable<object[]> CapabilitiesCases()
    {
        yield return new object[] { new[] { "capability1", "capability2" }, new HashSet<string> { "capability1", "capability2" } };
        yield return new object[] { new[] { "capability1" }, new HashSet<string> { "capability1" } };
        yield return new object[] { new[] { "capability1", "capability1" }, new HashSet<string> { "capability1" } };
        yield return new object[] { new[] { "capability1", null }, new HashSet<string> { "capability1" } };
        yield return new object[] { new[] { null, "capability1" }, new HashSet<string> { "capability1" } };
        yield return new object[] { new[] { "", "capability1" }, new HashSet<string> { "capability1" } };
        yield return new object[] { new[] { " ", "capability1" }, new HashSet<string> { "capability1" } };
    }

    public static IEnumerable<object[]> DuplicateDeviceLinkInteractionsCases()
    {
        var interaction1 = DeviceLinkInteraction.DisplayTextAndPin("Enter your PIN.");
        var interaction2 = DeviceLinkInteraction.DisplayTextAndPin("Enter your PIN.");
        yield return new object[] { new List<DeviceLinkInteraction> { interaction1, interaction1 } };
        yield return new object[] { new List<DeviceLinkInteraction> { interaction1, interaction2 } };
    }

    public static IEnumerable<object[]> DuplicateNotificationInteractionsCases()
    {
        yield return new object[]
        {
            new List<NotificationInteraction>
            {
                NotificationInteraction.DisplayTextAndPin("Enter your PIN."),
                NotificationInteraction.DisplayTextAndPin("Enter your PIN.")
            }
        };
        yield return new object[]
        {
            new List<NotificationInteraction>
            {
                NotificationInteraction.DisplayTextAndPin("Provide your PIN"),
                NotificationInteraction.DisplayTextAndPin("Enter your PIN.")
            }
        };
    }

    public static IEnumerable<object[]> InvalidRpChallengeCases()
    {
        // .NET validates valid Base64 first, then length; "invalid value" decodes and fails length (< 44).
        yield return new object[] { "invalid value", "Value for 'rpChallenge' must have length between 44 and 88 characters" };
        yield return new object[]
        {
            SafeBase64("a".PadRight(30, 'a')),
            "Value for 'rpChallenge' must have length between 44 and 88 characters"
        };
        yield return new object[]
        {
            SafeBase64("a".PadRight(67, 'a')),
            "Value for 'rpChallenge' must have length between 44 and 88 characters"
        };
    }

    private static string SafeBase64(string utf8Source) =>
        System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(utf8Source));

    public static IEnumerable<object[]> AllSmartIdHashAlgorithms()
    {
        foreach (SmartIdHashAlgorithm alg in System.Enum.GetValues(typeof(SmartIdHashAlgorithm)))
        {
            yield return new object[] { alg };
        }
    }

    public static IEnumerable<object[]> AllSigningSignatureAlgorithms()
    {
        foreach (SigningSignatureAlgorithm alg in System.Enum.GetValues(typeof(SigningSignatureAlgorithm)))
        {
            yield return new object[] { alg };
        }
    }

    public static IEnumerable<object[]> AllCertificateLevels()
    {
        // QUALIFIED and QSCD share the same underlying value — Enum.GetValues yields both names and duplicates xUnit case IDs.
        yield return new object[] { CertificateLevel.ADVANCED };
        yield return new object[] { CertificateLevel.QUALIFIED };
        yield return new object[] { CertificateLevel.QSCD };
    }

    /// <summary>Valid nonce values: null, 1-char, max length per API.</summary>
    public static IEnumerable<object?[]> ValidNonceLengths()
    {
        yield return new object?[] { null };
        yield return new object[] { "a" };
        yield return new object[] { "a".PadRight(30, 'a') };
    }

    /// <summary>Invalid nonces for notification certificate choice (message matches .NET builders).</summary>
    public static IEnumerable<object[]> InvalidNotificationCertChoiceNonces()
    {
        yield return new object[] { "", "Value for 'nonce' length must be between 1 and 30 characters" };
        yield return new object[] { "a".PadRight(31, 'a'), "Value for 'nonce' length must be between 1 and 30 characters" };
    }
}
