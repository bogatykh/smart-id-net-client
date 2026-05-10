using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SK.SmartId.Common;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId;

/// <seealso cref="ee.sk.smartid.LinkedNotificationSignatureSessionRequestBuilderTest" />
public class LinkedNotificationSignatureSessionRequestBuilderTest
{
    private const string DocumentNumber = "PNOEE-12345678901-MOCK-Q";

    private static LinkedNotificationSignatureSessionRequestBuilder Base(Mock<ISmartIdConnector> m) =>
        new LinkedNotificationSignatureSessionRequestBuilder(m.Object)
            .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000")
            .WithRelyingPartyName("DEMO")
            .WithDocumentNumber(DocumentNumber)
            .WithSignableData(new SignableData("Test data"u8.ToArray()))
            .WithLinkedSessionID("10000000-0000-0000-0000-000000000000")
            .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Sign?") });

    private static LinkedSignatureSessionResponse Ok() =>
        new() { SessionID = "20000000-0000-0000-0000-000000000000" };

    public static IEnumerable<object?[]> RpInvalid() => new[] { new object?[] { null }, new object?[] { "" } };
    public static IEnumerable<object?[]> EmptyIx() { yield return new object?[] { null }; yield return new object[] { new List<DeviceLinkInteraction>() }; }

    [Fact]
    public async Task InitSignatureSession_Ok()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitLinkedNotificationSignatureAsync(It.IsAny<LinkedSignatureSessionRequest>(), DocumentNumber, default))
            .ReturnsAsync(Ok());
        Assert.Equal("20000000-0000-0000-0000-000000000000", (await Base(m).InitAsync()).SessionID);
    }

    /// <summary>Enum passed by name — <see cref="CertificateLevel.QSCD"/> and <see cref="CertificateLevel.QUALIFIED"/> share value 2, so theory payloads collide if <see cref="CertificateLevel"/> is serialized as int.</summary>
    [Theory]
    [InlineData(nameof(CertificateLevel.ADVANCED))]
    [InlineData(nameof(CertificateLevel.QUALIFIED))]
    [InlineData(nameof(CertificateLevel.QSCD))]
    public async Task InitSignatureSession_CertificateLevels(string levelName)
    {
        var level = Enum.Parse<CertificateLevel>(levelName);
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitLinkedNotificationSignatureAsync(It.IsAny<LinkedSignatureSessionRequest>(), DocumentNumber, default))
            .ReturnsAsync(Ok());
        await new LinkedNotificationSignatureSessionRequestBuilder(m.Object)
            .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000").WithRelyingPartyName("DEMO")
            .WithCertificateLevel(level).WithDocumentNumber(DocumentNumber)
            .WithSignableData(new SignableData("Test data"u8.ToArray()))
            .WithLinkedSessionID("10000000-0000-0000-0000-000000000000")
            .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Sign?") }).InitAsync();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Init_CapabilitiesEmpty(string? caps)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitLinkedNotificationSignatureAsync(It.IsAny<LinkedSignatureSessionRequest>(), DocumentNumber, default))
            .ReturnsAsync(Ok());
        await Base(m).WithCapabilities(caps!).InitAsync();
        Assert.Equal(0, ((LinkedSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).Capabilities.Count);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_Capabilities(string[] caps, HashSet<string> expected)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitLinkedNotificationSignatureAsync(It.IsAny<LinkedSignatureSessionRequest>(), DocumentNumber, default))
            .ReturnsAsync(Ok());
        await Base(m).WithCapabilities(caps).InitAsync();
        Assert.True(expected.SetEquals(((LinkedSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).Capabilities));
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSigningSignatureAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_SignatureAlgorithms(SigningSignatureAlgorithm algo)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitLinkedNotificationSignatureAsync(It.IsAny<LinkedSignatureSessionRequest>(), DocumentNumber, default))
            .ReturnsAsync(Ok());
        await Base(m).WithSignatureAlgorithm(algo).InitAsync();
        var p = ((LinkedSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocolParameters;
        Assert.Equal(algo.GetAlgorithmName(), p.SignatureAlgorithm);
        if (algo.IsLegacyRsa()) Assert.Null(p.SignatureAlgorithmParameters);
        else
        {
            Assert.NotNull(p.SignatureAlgorithmParameters);
            Assert.Equal(SmartIdHashAlgorithm.SHA_512.GetApiAlgorithmName(), p.SignatureAlgorithmParameters.HashAlgorithm);
        }
    }

    [Theory]
    [MemberData(nameof(RpInvalid))]
    public async Task Validate_RpUuid(string? v) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(new Mock<ISmartIdConnector>()).WithRelyingPartyUUID(v!).InitAsync());

    [Theory]
    [MemberData(nameof(RpInvalid))]
    public async Task Validate_RpName(string? v) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(new Mock<ISmartIdConnector>()).WithRelyingPartyName(v!).InitAsync());

    [Theory]
    [MemberData(nameof(RpInvalid))]
    public async Task Validate_DocumentNumber(string? v) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(new Mock<ISmartIdConnector>()).WithDocumentNumber(v!).InitAsync());

    [Fact]
    public async Task Validate_NoDigestInput_Throws()
    {
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(new Mock<ISmartIdConnector>()).WithSignableData(null!).WithSignableHash(null!).InitAsync());
        Assert.Contains("digestInput", ex.Message);
    }

    [Fact]
    public void Validate_HashAfterData_Line_DuringBuild_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        var ex = Assert.Throws<SmartIdRequestSetupException>(() =>
            Base(m).WithSignableHash(new SignableHash
            {
                HashInBase64 = Convert.ToBase64String(DigestCalculator.CalculateDigest("Test data"u8.ToArray(), SmartIdHashAlgorithm.SHA_512))
            }));
        Assert.Contains("SignableData", ex.Message);
    }

    [Fact]
    public void Validate_DataAfterHash_Line_DuringBuild_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        Assert.Throws<SmartIdRequestSetupException>(() =>
            new LinkedNotificationSignatureSessionRequestBuilder(m.Object)
                .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000").WithRelyingPartyName("DEMO").WithDocumentNumber(DocumentNumber)
                .WithSignableHash(new SignableHash
                {
                    HashInBase64 = Convert.ToBase64String(DigestCalculator.CalculateDigest("Test data"u8.ToArray(), SmartIdHashAlgorithm.SHA_512))
                }).WithSignableData(new SignableData("Test data"u8.ToArray())));
    }

    [Fact]
    public async Task Validate_SignatureAlgorithmInvalid_Throws() =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(new Mock<ISmartIdConnector>()).WithSignatureAlgorithm((SigningSignatureAlgorithm)999).InitAsync());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_LinkedSessionId(string? id) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(new Mock<ISmartIdConnector>()).WithLinkedSessionID(id!).InitAsync());

    [Theory]
    [InlineData("1234567890123456789012345678901")]
    [InlineData("")]
    public async Task Validate_NonceLength(string nonce) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(new Mock<ISmartIdConnector>()).WithNonce(nonce).InitAsync());

    [Theory]
    [MemberData(nameof(EmptyIx))]
    public async Task Validate_InteractionsMissing(IReadOnlyList<DeviceLinkInteraction>? ix) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(new Mock<ISmartIdConnector>()).WithInteractions(ix!).InitAsync());

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.DuplicateDeviceLinkInteractionsCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Validate_DuplicateInteractions(List<DeviceLinkInteraction> dups) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(new Mock<ISmartIdConnector>()).WithInteractions(dups).InitAsync());

    [Fact]
    public async Task Response_MissingSessionId_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitLinkedNotificationSignatureAsync(It.IsAny<LinkedSignatureSessionRequest>(), DocumentNumber, default))
            .ReturnsAsync(new LinkedSignatureSessionResponse { SessionID = null });
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Base(m).InitAsync());
    }
}
