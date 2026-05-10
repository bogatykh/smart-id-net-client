using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using SK.SmartId.Common;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId;

/// <seealso cref="ee.sk.smartid.DeviceLinkAuthenticationSessionRequestBuilderTest" />
public class DeviceLinkAuthenticationSessionRequestBuilderTest
{
    private static readonly Regex Base64Pattern = new("^[A-Za-z0-9+/]+={0,2}$", RegexOptions.Compiled);

    public static IEnumerable<object[]> CertificateLevelPairs()
    {
        yield return new object[] { AuthenticationCertificateLevel.ADVANCED, "ADVANCED" };
        yield return new object[] { AuthenticationCertificateLevel.QUALIFIED, "QUALIFIED" };
        // Java also tests explicit null expecting null certificateLevel in payload; DeviceLinkAuthenticationSessionRequestBuilder
        // uses non-nullable AuthenticationCertificateLevel with default QUALIFIED — not representable here.
    }

    public static IEnumerable<object?[]> RelyingPartyUuidInvalid() => new object?[] { null, "" }.Select(x => new[] { x });
    public static IEnumerable<object?[]> RelyingPartyNameInvalid() => new object?[] { null, "" }.Select(x => new[] { x });
    public static IEnumerable<object?[]> RpChallengeInvalid() => new object?[] { null, "" }.Select(x => new[] { x });
    public static IEnumerable<object?[]> InteractionsEmpty() => new object?[] { null, new List<DeviceLinkInteraction>() }.Select(x => new[] { x });

    public static IEnumerable<object?[]> SessionIdInvalid() => new object?[] { null, "" }.Select(x => new[] { x });
    public static IEnumerable<object?[]> SessionTokenInvalid() => new object?[] { null, "" }.Select(x => new[] { x });
    public static IEnumerable<object?[]> SessionSecretInvalid() => new object?[] { null, "" }.Select(x => new[] { x });
    /// <remarks>Java also rejected <c>about:blank</c>; .NET treats it as non-blank (see deviceLinkBase validators).</remarks>
    public static IEnumerable<object?[]> DeviceLinkBaseInvalid() => new object?[] { null, "", "   " }.Select(x => new[] { x });

    private static string GenerateBase64String(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    private static DeviceLinkSessionResponse ToDeviceLinkAuthenticationResponse() => new()
    {
        SessionID = "00000000-0000-0000-0000-000000000000",
        SessionToken = GenerateBase64String("sessionToken"),
        SessionSecret = GenerateBase64String("sessionSecret"),
        DeviceLinkBase = "https://example.com/callback"
    };

    private DeviceLinkAuthenticationSessionRequestBuilder ToBaseDeviceLinkRequestBuilder(Mock<ISmartIdConnector> connector) =>
        new DeviceLinkAuthenticationSessionRequestBuilder(connector.Object)
            .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000")
            .WithRelyingPartyName("DEMO")
            .WithRpChallenge(GenerateBase64String("".PadRight(32, 'a')))
            .WithHashAlgorithm(SmartIdHashAlgorithm.SHA3_512)
            .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Log into internet banking system") });

    [Fact]
    public async Task InitAuthenticationSession_AnonymousAuthentication_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        var builder = ToBaseDeviceLinkRequestBuilder(connector);

        await builder.InitAsync();

        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default), Times.Once);
        var cap = connector.Invocations[^1].Arguments[0] as DeviceLinkAuthenticationSessionRequest;
        AssertAuthenticationSessionRequest(cap);
    }

    [Fact]
    public async Task InitAuthenticationSession_WithDocumentNumber_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), It.IsAny<string>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        var builder = ToBaseDeviceLinkRequestBuilder(connector).WithDocumentNumber("PNOEE-48010010101-MOCK-Q");

        await builder.InitAsync();

        connector.Verify(c => c.InitDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), "PNOEE-48010010101-MOCK-Q", default), Times.Once);
    }

    [Fact]
    public async Task InitAuthenticationSession_WithSemanticsIdentifier_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        var builder = ToBaseDeviceLinkRequestBuilder(connector)
            .WithSemanticsIdentifier(new SemanticsIdentifier("PNOEE-48010010101"));

        await builder.InitAsync();

        connector.Verify(c => c.InitDeviceLinkAuthenticationAsync(
            It.IsAny<DeviceLinkAuthenticationSessionRequest>(),
            It.Is<SemanticsIdentifier>(s => s.Identifier == "PNOEE-48010010101"),
            default), Times.Once);
    }

    [Theory]
    [MemberData(nameof(CertificateLevelPairs))]
    public async Task InitAuthenticationSession_CertificateLevel_Ok(AuthenticationCertificateLevel level, string expectedValue)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector).WithCertificateLevel(level).InitAsync();
        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.Is<DeviceLinkAuthenticationSessionRequest>(r =>
            r.CertificateLevel == expectedValue), default), Times.Once);
    }

    [Fact]
    public async Task InitAuthenticationSession_SignatureAlgorithm_Ok_RSASSA_PSS()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector)
            .WithSignatureAlgorithm(AuthenticationSignatureAlgorithm.RSASSA_PSS)
            .InitAsync();
        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.Is<DeviceLinkAuthenticationSessionRequest>(r =>
            r.SignatureProtocolParameters.SignatureAlgorithm == "rsassa-pss"
            && Base64Pattern.IsMatch(r.SignatureProtocolParameters.RpChallenge)), default), Times.Once);
    }

    [Fact]
    public async Task InitAuthenticationSession_IpQueryingNotUsed_RequestPropertiesNull()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector).InitAsync();
        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.Is<DeviceLinkAuthenticationSessionRequest>(r =>
            r.RequestProperties == null), default), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InitAuthenticationSession_IpQueryingRequired_Ok(bool ipRequested)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector).WithShareMdClientIpAddress(ipRequested).InitAsync();
        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(
            It.Is<DeviceLinkAuthenticationSessionRequest>(r =>
                r.RequestProperties != null && r.RequestProperties.ShareMdClientIpAddress == ipRequested
                && Base64Pattern.IsMatch(r.SignatureProtocolParameters.RpChallenge)), default), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task InitAuthenticationSession_Capabilities_EmptyVariants_Ok(string? capabilities)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector).WithCapabilities(capabilities!).InitAsync();
        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.Is<DeviceLinkAuthenticationSessionRequest>(r =>
            r.Capabilities != null && r.Capabilities.Count == 0), default), Times.Once);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitAuthenticationSession_Capabilities_Ok(string[] capabilities, HashSet<string> expected)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector).WithCapabilities(capabilities).InitAsync();
        var inv = connector.Invocations[^1];
        var req = (DeviceLinkAuthenticationSessionRequest)inv.Arguments[0]!;
        Assert.Equal(expected.OrderBy(x => x), req.Capabilities.OrderBy(x => x));
    }

    [Fact]
    public async Task InitAuthenticationSession_InitialCallbackUrlIsValid_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        await ToBaseDeviceLinkRequestBuilder(connector).WithInitialCallbackUrl("https://example.com/callback").InitAsync();
        connector.Verify(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.Is<DeviceLinkAuthenticationSessionRequest>(r =>
            r.InitialCallbackUrl == "https://example.com/callback"), default), Times.Once);
    }

    [Theory]
    [MemberData(nameof(RelyingPartyUuidInvalid))]
    public async Task InitAuthenticationSession_RelyingPartyUuidIsEmpty_Throws(string? uuid)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithRelyingPartyUUID(uuid!).InitAsync());
        Assert.Equal("Value for 'relyingPartyUUID' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(RelyingPartyNameInvalid))]
    public async Task InitAuthenticationSession_RelyingPartyNameIsEmpty_Throws(string? name)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithRelyingPartyName(name!).InitAsync());
        Assert.Equal("Value for 'relyingPartyName' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(RpChallengeInvalid))]
    public async Task InitAuthenticationSession_RpChallengeIsEmpty_Throws(string? challenge)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithRpChallenge(challenge!).InitAsync());
        Assert.Equal("Value for 'rpChallenge' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.InvalidRpChallengeCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitAuthenticationSession_RpChallengeIsInvalid_Throws(string challenge, string expectedMessage)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithRpChallenge(challenge).InitAsync());
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task InitAuthenticationSession_SignatureAlgorithmIsSetInvalid_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector)
                .WithSignatureAlgorithm((AuthenticationSignatureAlgorithm)999)
                .InitAsync());
        Assert.Equal("Value for 'signatureAlgorithm' must be set", ex.Message);
    }

    [Theory]
    [MemberData(nameof(InteractionsEmpty))]
    public async Task InitAuthenticationSession_InteractionsIsEmpty_Throws(List<DeviceLinkInteraction>? interactions)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithInteractions(interactions!).InitAsync());
        Assert.Equal("Value for 'interactions' cannot be empty", ex.Message);
    }

    [Fact]
    public async Task InitAuthenticationSession_InteractionsContainsNullOnly_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithInteractions(new List<DeviceLinkInteraction?> { null }!).InitAsync());
        Assert.Equal("Value for 'interactions' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.DuplicateDeviceLinkInteractionsCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitAuthenticationSession_DuplicateInteractions_Throws(List<DeviceLinkInteraction> dups)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithInteractions(dups).InitAsync());
        Assert.Equal("Value for 'interactions' cannot contain duplicate types", ex.Message);
    }

    [Theory]
    [InlineData("http://example.com", "Value for 'initialCallbackUrl' must match pattern ^https://[^|]+$ and must not contain unencoded vertical bars")]
    [InlineData("https://example.com|test", "Value for 'initialCallbackUrl' must match pattern ^https://[^|]+$ and must not contain unencoded vertical bars")]
    [InlineData("ftp://example.com", "Value for 'initialCallbackUrl' must match pattern ^https://[^|]+$ and must not contain unencoded vertical bars")]
    public async Task InitAuthenticationSession_InitialCallbackUrlIsInvalid_Throws(string url, string expectedMessage)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithInitialCallbackUrl(url).InitAsync());
        Assert.Equal(expectedMessage, ex.Message);
    }

    [Fact]
    public async Task InitAuthenticationSession_HashAlgorithmInvalid_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector).WithHashAlgorithm((SmartIdHashAlgorithm)999).InitAsync());
        Assert.Equal("Value for 'hashAlgorithm' must be set", ex.Message);
    }

    [Fact]
    public async Task InitAuthenticationSession_BothSemanticsIdentifierAndDocumentNumberSet_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            ToBaseDeviceLinkRequestBuilder(connector)
                .WithDocumentNumber("PNOEE-48010010101-MOCK-Q")
                .WithSemanticsIdentifier(new SemanticsIdentifier("PNOEE-48010010101"))
                .InitAsync());
        Assert.Equal("Only one of 'semanticsIdentifier' or 'documentNumber' may be set", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionIdInvalid))]
    public async Task InitAuthenticationSession_SessionIdMissingInResponse_Throws(string? sid)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse
            {
                SessionID = sid,
                SessionToken = GenerateBase64String("sessionToken"),
                SessionSecret = GenerateBase64String("sessionSecret"),
                DeviceLinkBase = "https://example.com/callback"
            });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => ToBaseDeviceLinkRequestBuilder(connector).InitAsync());
        Assert.Equal("Device link authentication session initialisation response field 'sessionID' is missing or empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionTokenInvalid))]
    public async Task InitAuthenticationSession_SessionTokenMissingInResponse_Throws(string? tok)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse
            {
                SessionID = "00000000-0000-0000-0000-000000000000",
                SessionToken = tok,
                SessionSecret = GenerateBase64String("sessionSecret"),
                DeviceLinkBase = "https://example.com/callback"
            });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => ToBaseDeviceLinkRequestBuilder(connector).InitAsync());
        Assert.Equal("Device link authentication session initialisation response field 'sessionToken' is missing or empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(SessionSecretInvalid))]
    public async Task InitAuthenticationSession_SessionSecretMissingInResponse_Throws(string? secret)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse
            {
                SessionID = "00000000-0000-0000-0000-000000000000",
                SessionToken = GenerateBase64String("sessionToken"),
                SessionSecret = secret,
                DeviceLinkBase = "https://example.com/callback"
            });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => ToBaseDeviceLinkRequestBuilder(connector).InitAsync());
        Assert.Equal("Device link authentication session initialisation response field 'sessionSecret' is missing or empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(DeviceLinkBaseInvalid))]
    public async Task InitAuthenticationSession_DeviceLinkBaseMissingOrBlank_Throws(string? baseUri)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse
            {
                SessionID = "00000000-0000-0000-0000-000000000000",
                SessionToken = GenerateBase64String("sessionToken"),
                SessionSecret = GenerateBase64String("sessionSecret"),
                DeviceLinkBase = baseUri
            });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => ToBaseDeviceLinkRequestBuilder(connector).InitAsync());
        Assert.Equal("Device link authentication session initialisation response field 'deviceLinkBase' is missing or empty", ex.Message);
    }

    [Fact]
    public async Task GetAuthenticationSessionRequest_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(c => c.InitAnonymousDeviceLinkAuthenticationAsync(It.IsAny<DeviceLinkAuthenticationSessionRequest>(), default))
            .ReturnsAsync(ToDeviceLinkAuthenticationResponse());
        var builder = ToBaseDeviceLinkRequestBuilder(connector);
        await builder.InitAsync();
        var req = builder.GetAuthenticationSessionRequest();
        AssertAuthenticationSessionRequest(req);
    }

    [Fact]
    public void GetAuthenticationSessionRequest_AuthenticationNotInitialized_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var builder = ToBaseDeviceLinkRequestBuilder(connector);
        var ex = Assert.Throws<SmartIdClientException>(() => builder.GetAuthenticationSessionRequest());
        Assert.Equal("Device link authentication session has not been initialized yet", ex.Message);
    }

    private static void AssertAuthenticationSessionRequest(DeviceLinkAuthenticationSessionRequest request)
    {
        Assert.Equal("00000000-0000-0000-0000-000000000000", request.RelyingPartyUUID);
        Assert.Equal("DEMO", request.RelyingPartyName);
        Assert.Equal("QUALIFIED", request.CertificateLevel);
        Assert.Equal(SignatureProtocol.ACSP_V2, request.SignatureProtocol);
        Assert.NotNull(request.SignatureProtocolParameters);
        Assert.NotNull(request.SignatureProtocolParameters.RpChallenge);
        Assert.Equal("rsassa-pss", request.SignatureProtocolParameters.SignatureAlgorithm);
        Assert.NotNull(request.Interactions);
        Assert.True(Base64Pattern.IsMatch(request.SignatureProtocolParameters.RpChallenge));

        byte[] decoded = Convert.FromBase64String(request.Interactions);
        var ix = JArray.Parse(Encoding.UTF8.GetString(decoded));
        var jo = Assert.IsType<JObject>(ix[0]);
        Assert.Equal(InteractionFlow.DISPLAY_TEXT_AND_PIN.Code, jo["type"]?.Value<string>());
    }
}
