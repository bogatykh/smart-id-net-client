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

/// <seealso cref="ee.sk.smartid.NotificationAuthenticationSessionRequestBuilderTest" />
public class NotificationAuthenticationSessionRequestBuilderTest
{
    private static string B64(string s) => Convert.ToBase64String(Encoding.UTF8.GetBytes(s));

    private static NotificationAuthenticationSessionRequestBuilder Base(Mock<ISmartIdConnector> c) =>
        new NotificationAuthenticationSessionRequestBuilder(c.Object)
            .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000")
            .WithRelyingPartyName("DEMO")
            .WithRpChallenge(B64("".PadRight(32, 'a')))
            .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Verify the code") })
            .WithDocumentNumber("PNOEE-1234567890-MOCK-Q");

    private static NotificationAuthenticationSessionResponse OkResponse() =>
        new() { SessionID = "00000000-0000-0000-0000-000000000000" };

    public static IEnumerable<object?[]> NullableString() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    public static IEnumerable<object?[]> NullableInteractionList()
    {
        yield return new object?[] { null };
        yield return new object[] { new List<NotificationInteraction>() };
    }

    public static IEnumerable<object?[]> NullableSessionId() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    /// <summary>Certificate level cases matching Java ADVANCED / QUALIFIED; Java null omitted (explicit null unsupported on WithCertificateLevel).</summary>
    public static IEnumerable<object?[]> AuthenticationCertLevels()
    {
        yield return new object[] { AuthenticationCertificateLevel.ADVANCED, "ADVANCED" };
        yield return new object[] { AuthenticationCertificateLevel.QUALIFIED, "QUALIFIED" };
    }

    private static NotificationAuthenticationSessionRequestBuilder BaseNoCertLevel(Mock<ISmartIdConnector> c) =>
        new NotificationAuthenticationSessionRequestBuilder(c.Object)
            .WithRelyingPartyUUID("00000000-0000-4000-8000-000000000001")
            .WithRelyingPartyName("DEMO2")
            .WithRpChallenge(B64("".PadRight(32, 'a')))
            .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Verify") })
            .WithDocumentNumber("PNOEE-1234567890-MOCK-Q");

    [Fact]
    public async Task InitAuthenticationSession_WithDocumentNumber_Ok()
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await Base(c).InitAsync();
        var req = (NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!;
        AssertAuthenticationRequest(req);
    }

    [Fact]
    public async Task InitAuthenticationSession_WithSemanticsIdentifier_Ok()
    {
        var c = new Mock<ISmartIdConnector>();
        var sem = new SemanticsIdentifier("PNOEE-48010010101");
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        await new NotificationAuthenticationSessionRequestBuilder(c.Object)
            .WithRelyingPartyUUID("00000000-0000-4000-8000-000000000002")
            .WithRelyingPartyName("DEMO")
            .WithRpChallenge(B64("".PadRight(32, 'a')))
            .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Verify the code") })
            .WithDocumentNumber(null!)
            .WithSemanticsIdentifier(sem)
            .InitAsync();
        Assert.Equal("PNOEE-48010010101", ((SemanticsIdentifier)c.Invocations[^1].Arguments[1]!).Identifier);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InitAuthenticationSession_ShareMdClientIp(bool shareIp)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await Base(c).WithShareMdClientIpAddress(shareIp).InitAsync();
        var req = (NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!;
        Assert.NotNull(req.RequestProperties);
        Assert.Equal(shareIp, req.RequestProperties.ShareMdClientIpAddress);
    }

    [Theory]
    [MemberData(nameof(AuthenticationCertLevels))]
    public async Task InitAuthenticationSession_CertificateLevel(AuthenticationCertificateLevel level, string api)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await BaseNoCertLevel(c).WithCertificateLevel(level).InitAsync();
        Assert.Equal(api, ((NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!).CertificateLevel);
    }

    [Fact]
    public async Task InitAuthenticationSession_CertificateLevelUnspecified_IsNullOnWire()
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await BaseNoCertLevel(c).InitAsync();
        Assert.Null(((NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!).CertificateLevel);
    }

    [Fact]
    public async Task InitAuthenticationSession_SignatureAlgorithm_RSASSA_PSS()
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await Base(c).WithSignatureAlgorithm(AuthenticationSignatureAlgorithm.RSASSA_PSS).InitAsync();
        var req = (NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!;
        Assert.Equal("rsassa-pss", req.SignatureProtocolParameters.SignatureAlgorithm);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSmartIdHashAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitAuthenticationSession_HashAlgorithm_Ok(SmartIdHashAlgorithm algo)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await Base(connector).WithHashAlgorithm(algo).InitAsync();
        var req = (NotificationAuthenticationSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(algo.GetApiAlgorithmName(), req.SignatureProtocolParameters.SignatureAlgorithmParameters!.HashAlgorithm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task InitAuthenticationSession_EmptyCapabilities_Ok(string? cap)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        var response = await Base(c).WithCapabilities(cap!).InitAsync();
        Assert.Equal("00000000-0000-0000-0000-000000000000", response.SessionID);
        Assert.Equal(0, ((NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!).Capabilities.Count);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitAuthenticationSession_Capabilities_Ok(string[] capabilities, HashSet<string> expected)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        await Base(c).WithCapabilities(capabilities).InitAsync();
        var req = (NotificationAuthenticationSessionRequest)c.Invocations[^1].Arguments[0]!;
        Assert.True(expected.SetEquals(req.Capabilities));
    }

    [Theory]
    [MemberData(nameof(NullableString))]
    public async Task Validate_RelyingPartyUuid_Empty(string? v)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithRelyingPartyUUID(v!).InitAsync());
        Assert.Equal("Value for 'relyingPartyUUID' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableString))]
    public async Task Validate_RelyingPartyName_Empty(string? v)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithRelyingPartyName(v!).InitAsync());
        Assert.Equal("Value for 'relyingPartyName' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableString))]
    public async Task Validate_RpChallenge_Empty(string? v)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithRpChallenge(v!).InitAsync());
        Assert.Equal("Value for 'rpChallenge' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.InvalidRpChallengeCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Validate_InvalidRpChallenge_Messages(string ch, string expected)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithRpChallenge(ch).InitAsync());
        Assert.Equal(expected, ex.Message);
    }

    [Fact]
    public async Task Validate_SignatureAlgorithmInvalid_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(c).WithSignatureAlgorithm((AuthenticationSignatureAlgorithm)999).InitAsync());
        Assert.Equal("Value for 'signatureAlgorithm' must be set", ex.Message);
    }

    [Fact]
    public async Task Validate_HashAlgorithmInvalid_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(c).WithHashAlgorithm((SmartIdHashAlgorithm)999).InitAsync());
        Assert.Equal("Value for 'hashAlgorithm' must be set", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableInteractionList))]
    public async Task Validate_InteractionsEmpty_Throws(List<NotificationInteraction>? ix)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithInteractions(ix!).InitAsync());
        Assert.Equal("Value for 'interactions' cannot be empty", ex.Message);
    }

    [Fact]
    public async Task Validate_InteractionsNullElement_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(c).WithInteractions(new List<NotificationInteraction> { null! }).InitAsync());
        Assert.Equal("Value for 'interactions' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.DuplicateNotificationInteractionsCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Validate_DuplicateInteractions_Throws(List<NotificationInteraction> list)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(c).WithInteractions(list).InitAsync());
        Assert.Equal("Value for 'interactions' cannot contain duplicate types", ex.Message);
    }

    [Fact]
    public async Task Validate_NoDocumentOrSemantics_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var b = new NotificationAuthenticationSessionRequestBuilder(c.Object)
            .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000")
            .WithRelyingPartyName("DEMO")
            .WithRpChallenge(B64("".PadRight(32, 'a')))
            .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("x") })
            .WithDocumentNumber(null!);
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => b.InitAsync());
        Assert.Equal("Either 'documentNumber' or 'semanticsIdentifier' must be set", ex.Message);
    }

    [Fact]
    public async Task Validate_BothDocumentAndSemantics_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            Base(c).WithSemanticsIdentifier(new SemanticsIdentifier("PNOEE-48010010101")).InitAsync());
        Assert.Equal("Only one of 'semanticsIdentifier' or 'documentNumber' may be set", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableSessionId))]
    public async Task Response_MissingSessionId_Throws(string? sid)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(new NotificationAuthenticationSessionResponse { SessionID = sid });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Base(c).InitAsync());
        Assert.Equal("Notification-based authentication session initialisation response field 'sessionID' is missing or empty", ex.Message);
    }

    [Fact]
    public async Task GetAuthenticationSessionRequest_Ok()
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationAuthenticationAsync(It.IsAny<NotificationAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(OkResponse());
        var b = Base(c);
        await b.InitAsync();
        AssertAuthenticationRequest(b.GetAuthenticationSessionRequest());
    }

    [Fact]
    public void GetAuthenticationSessionRequest_NotInitialized_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = Assert.Throws<SmartIdClientException>(() => Base(c).GetAuthenticationSessionRequest());
        Assert.Equal("Notification-based authentication session has not been initialized yet", ex.Message);
    }

    private static void AssertAuthenticationRequest(NotificationAuthenticationSessionRequest request)
    {
        Assert.Equal("00000000-0000-0000-0000-000000000000", request.RelyingPartyUUID);
        Assert.Equal("DEMO", request.RelyingPartyName);
        Assert.Equal(SignatureProtocol.ACSP_V2.ToString(), request.SignatureProtocol);
        Assert.NotNull(request.SignatureProtocolParameters);
        Assert.Equal("rsassa-pss", request.SignatureProtocolParameters.SignatureAlgorithm);
        Assert.NotNull(request.Interactions);
    }
}
