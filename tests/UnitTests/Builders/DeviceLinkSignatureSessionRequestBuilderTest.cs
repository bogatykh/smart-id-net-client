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

/// <seealso cref="ee.sk.smartid.DeviceLinkSignatureSessionRequestBuilderTest" />
public class DeviceLinkSignatureSessionRequestBuilderTest
{
    private static readonly SemanticsIdentifier SemanticsId =
        new(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111");

    private static DeviceLinkSignatureSessionRequestBuilder BaseBuilder(Mock<ISmartIdConnector> c) =>
        new DeviceLinkSignatureSessionRequestBuilder(c.Object)
            .WithRelyingPartyUUID("test-relying-party-uuid")
            .WithRelyingPartyName("DEMO")
            .WithSemanticsIdentifier(SemanticsId)
            .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Please sign the document") })
            .WithSignableData(new SignableData("Test data"u8.ToArray()));

    private static DeviceLinkSessionResponse MockSignatureSessionResponse() => new()
    {
        SessionID = "test-session-id",
        SessionToken = "test-session-token",
        SessionSecret = "test-session-secret",
        DeviceLinkBase = "https://example.com/device-link"
    };

    public static IEnumerable<object?[]> CertificateLevelPairs()
    {
        yield return new object?[] { null, null };
        yield return new object[] { CertificateLevel.ADVANCED, "ADVANCED" };
        yield return new object[] { CertificateLevel.QUALIFIED, "QUALIFIED" };
    }

    public static IEnumerable<object?[]> RelyingPartyInvalidDoc() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    public static IEnumerable<object?[]> NullableInteractions()
    {
        yield return new object?[] { null };
        yield return new object[] { new List<DeviceLinkInteraction>() };
    }

    public static IEnumerable<object?[]> NullableSessionPieces() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    /// <remarks>Excludes values like <c>about:blank</c> that Java rejected but .NET accepts (non-whitespace).</remarks>
    public static IEnumerable<object?[]> DeviceLinkBadBase() =>
        new[] { new object?[] { null }, new object?[] { "" }, new object[] { "   " } };

    private static SignableHash Hash(string utf8, SmartIdHashAlgorithm algo) => new SignableHash
    {
        HashInBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(utf8)),
        HashAlgorithm = algo
    };

    [Fact]
    public async Task InitSignatureSession_WithSemanticsIdentifier_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), SemanticsId, default))
            .ReturnsAsync(MockSignatureSessionResponse());
        Assert.NotNull(await BaseBuilder(connector).InitAsync());
        connector.Verify(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), SemanticsId, default), Times.Once);
    }

    [Fact]
    public async Task InitSignatureSession_WithDocumentNumber_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        const string doc = "PNOEE-31111111111-MOCK-Q";
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), doc, default))
            .ReturnsAsync(MockSignatureSessionResponse());
        var builder = new DeviceLinkSignatureSessionRequestBuilder(connector.Object)
            .WithRelyingPartyUUID("test-relying-party-uuid")
            .WithRelyingPartyName("DEMO")
            .WithDocumentNumber(doc)
            .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("Please sign the document") })
            .WithSignableData(new SignableData("Test data"u8.ToArray()));
        await builder.InitAsync();
        connector.Verify(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), doc, default), Times.Once);
    }

    [Theory]
    [MemberData(nameof(CertificateLevelPairs))]
    public async Task InitSignatureSession_WithCertificateLevel(CertificateLevel? level, string? expectedValue)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        var b = level.HasValue ? BaseBuilder(connector).WithCertificateLevel(level.Value) : BaseBuilder(connector);
        await b.InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(expectedValue, req.CertificateLevel);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.ValidNonceLengths), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitSignatureSession_WithNonce_Ok(string? nonce)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        var b = nonce == null ? BaseBuilder(connector).WithNonce(null!) : BaseBuilder(connector).WithNonce(nonce);
        await b.InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(nonce, req.Nonce);
    }

    [Fact]
    public async Task InitSignatureSession_WithRequestProperties_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithShareMdClientIpAddress(true).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.NotNull(req.RequestProperties);
        Assert.True(req.RequestProperties.ShareMdClientIpAddress);
    }

    [Fact]
    public async Task InitSignatureSession_WithSignatureAlgorithm_RSASSA_PSS_SetsAlgorithm()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithSignatureAlgorithm(SigningSignatureAlgorithm.RSASSA_PSS).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal("rsassa-pss", req.SignatureProtocolParameters.SignatureAlgorithm);
    }

    [Theory]
    [InlineData(SigningSignatureAlgorithm.SHA256_WITH_RSA_ENCRYPTION)]
    [InlineData(SigningSignatureAlgorithm.SHA384_WITH_RSA_ENCRYPTION)]
    [InlineData(SigningSignatureAlgorithm.SHA512_WITH_RSA_ENCRYPTION)]
    public async Task InitSignatureSession_WithLegacyRsaAlgorithm_OmitsSignatureAlgorithmParameters(SigningSignatureAlgorithm algo)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithSignatureAlgorithm(algo).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(algo.GetAlgorithmName(), req.SignatureProtocolParameters.SignatureAlgorithm);
        Assert.Null(req.SignatureProtocolParameters.SignatureAlgorithmParameters);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSmartIdHashAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitSignatureSession_WithSignableHash(SmartIdHashAlgorithm hashAlgorithm)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithSignableData(null!).WithSignableHash(Hash("Test hash", hashAlgorithm)).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("Test hash")), req.SignatureProtocolParameters.Digest);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSmartIdHashAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitSignatureSession_WithSignableData(SmartIdHashAlgorithm hashAlgorithm)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        var signableData = new SignableData(Encoding.UTF8.GetBytes("Test hash"), hashAlgorithm);
        await BaseBuilder(connector).WithSignableData(signableData).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        var expectedDigest = Convert.ToBase64String(DigestCalculator.CalculateDigest(Encoding.UTF8.GetBytes("Test hash"), hashAlgorithm));
        Assert.Equal(expectedDigest, req.SignatureProtocolParameters.Digest);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task InitSignatureSession_WithCapabilitiesSetToEmpty_Ok(string? capabilities)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithCapabilities(capabilities!).InitAsync();
        Assert.Equal(0, ((DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!).Capabilities.Count);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitSignatureSession_WithCapabilities_Ok(string[] capabilities, HashSet<string> expected)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithCapabilities(capabilities).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.True(expected.SetEquals(req.Capabilities));
    }

    [Fact]
    public async Task InitSignatureSession_DefaultSigningAlgorithm_RSASSA_PSS()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(SigningSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(), req.SignatureProtocolParameters.SignatureAlgorithm);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSigningSignatureAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitSignatureSession_WithSignatureAlgorithm_Ok(SigningSignatureAlgorithm signatureAlgorithm)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        await BaseBuilder(connector).WithSignatureAlgorithm(signatureAlgorithm).InitAsync();
        var req = (DeviceLinkSignatureSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.Equal(signatureAlgorithm.GetAlgorithmName(), req.SignatureProtocolParameters.SignatureAlgorithm);
        if (signatureAlgorithm.IsLegacyRsa())
            Assert.Null(req.SignatureProtocolParameters.SignatureAlgorithmParameters);
        else
        {
            Assert.NotNull(req.SignatureProtocolParameters.SignatureAlgorithmParameters);
            Assert.Equal(SmartIdHashAlgorithm.SHA_512.GetApiAlgorithmName(), req.SignatureProtocolParameters.SignatureAlgorithmParameters.HashAlgorithm);
        }
    }

    [Fact]
    public async Task GetSignatureSessionRequest_Ok()
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(MockSignatureSessionResponse());
        var b = BaseBuilder(connector);
        await b.InitAsync();
        var req = b.GetSignatureSessionRequest();
        Assert.Equal("test-relying-party-uuid", req.RelyingPartyUUID);
        Assert.Equal("DEMO", req.RelyingPartyName);
        Assert.Equal(SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(), req.SignatureProtocol);
        Assert.NotNull(req.SignatureProtocolParameters);
        Assert.NotNull(req.Interactions);
    }

    [Fact]
    public void GetSignatureSessionRequest_SessionNotStarted_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = Assert.Throws<SmartIdClientException>(() => BaseBuilder(connector).GetSignatureSessionRequest());
        Assert.Equal("Signature session has not been initiated yet", ex.Message);
    }

    [Theory]
    [MemberData(nameof(RelyingPartyInvalidDoc))]
    public async Task ErrorCases_MissingDocumentNumberAndSemanticsIdentifier(string? doc)
    {
        var connector = new Mock<ISmartIdConnector>();
        var b = new DeviceLinkSignatureSessionRequestBuilder(connector.Object)
            .WithRelyingPartyUUID("test-relying-party-uuid")
            .WithRelyingPartyName("DEMO")
            .WithDocumentNumber(doc!)
            .WithInteractions(new List<DeviceLinkInteraction> { DeviceLinkInteraction.DisplayTextAndPin("x") })
            .WithSignableData(new SignableData("Test data"u8.ToArray()));
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => b.InitAsync());
        Assert.Contains("Either 'documentNumber' or 'semanticsIdentifier' must be set", ex.Message);
    }

    [Fact]
    public async Task ErrorCases_SignatureAlgorithmInvalid_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithSignatureAlgorithm((SigningSignatureAlgorithm)999).InitAsync());
        Assert.Equal("Value for 'signatureAlgorithm' must be set", ex.Message);
    }

    [Fact]
    public void ErrorCases_SignableData_InvalidHashAlgorithm_Throws()
    {
        var ex = Assert.Throws<SmartIdRequestSetupException>(() => new SignableData("Test data"u8.ToArray(), (SmartIdHashAlgorithm)999));
        Assert.Equal("Parameter 'hashAlgorithm' must be set", ex.Message);
    }

    [Fact]
    public void ErrorCases_SignableHash_InvalidHashAlgorithmOnValidate_Throws()
    {
        var sh = new SignableHash
        {
            HashInBase64 = Convert.ToBase64String("Test data"u8.ToArray()),
            HashAlgorithm = (SmartIdHashAlgorithm)999
        };
        var ex = Assert.Throws<SmartIdRequestSetupException>(() => sh.Validate());
        Assert.Equal("Parameter 'hashAlgorithm' must be set", ex.Message);
    }

    [Fact]
    public async Task ErrorCases_NoDigestInput_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithSignableData(null!).WithSignableHash(null!).InitAsync());
        Assert.Equal("Value for 'digestInput' must be set with either SignableData or SignableHash", ex.Message);
    }

    [Fact]
    public void ErrorCases_HashAfterData_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        Assert.Throws<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithSignableHash(Hash("Test data", SmartIdHashAlgorithm.SHA_512)));
    }

    [Fact]
    public void ErrorCases_DataAfterHash_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        Assert.Throws<SmartIdRequestSetupException>(() =>
            new DeviceLinkSignatureSessionRequestBuilder(connector.Object)
                .WithRelyingPartyUUID("test-relying-party-uuid")
                .WithRelyingPartyName("DEMO")
                .WithSemanticsIdentifier(SemanticsId)
                .WithSignableHash(Hash("Test data", SmartIdHashAlgorithm.SHA_512))
                .WithSignableData(new SignableData("Test data"u8.ToArray())));
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com|test")]
    [InlineData("ftp://example.com")]
    public async Task ErrorCases_InvalidInitialCallbackUrl_Throws(string url)
    {
        var connector = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => BaseBuilder(connector).WithInitialCallbackUrl(url).InitAsync());
    }

    [Theory]
    [MemberData(nameof(NullableInteractions))]
    public async Task ErrorCases_EmptyInteractions_Throws(IReadOnlyList<DeviceLinkInteraction>? ix)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithInteractions(ix!).InitAsync());
        Assert.Equal("Value for 'interactions' cannot be empty", ex.Message);
    }

    [Fact]
    public async Task ErrorCases_InteractionsWithNullOnly_Throws()
    {
        var connector = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithInteractions(new List<DeviceLinkInteraction> { null! }).InitAsync());
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.DuplicateDeviceLinkInteractionsCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task ErrorCases_DuplicateInteractions_Throws(List<DeviceLinkInteraction> dups)
    {
        var connector = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithInteractions(dups).InitAsync());
        Assert.Equal("Value for 'interactions' cannot contain duplicate types", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ErrorCases_InvalidRelyingPartyUuid_Throws(string? uuid)
    {
        var connector = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithRelyingPartyUUID(uuid!).InitAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ErrorCases_InvalidRelyingPartyName_Throws(string? name)
    {
        var connector = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            BaseBuilder(connector).WithRelyingPartyName(name!).InitAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123456789012345678901")]
    public async Task ErrorCases_InvalidNonce_Throws(string nonce)
    {
        var connector = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => BaseBuilder(connector).WithNonce(nonce).InitAsync());
    }

    [Theory]
    [MemberData(nameof(NullableSessionPieces))]
    public async Task ResponseValidation_MissingSessionId_Throws(string? sid)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = sid, SessionToken = "tok", SessionSecret = "sec", DeviceLinkBase = "https://x" });
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => BaseBuilder(connector).InitAsync());
    }

    [Theory]
    [MemberData(nameof(NullableSessionPieces))]
    public async Task ResponseValidation_MissingSessionToken_Throws(string? tok)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = "id", SessionToken = tok, SessionSecret = "sec", DeviceLinkBase = "https://x" });
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => BaseBuilder(connector).InitAsync());
    }

    [Theory]
    [MemberData(nameof(NullableSessionPieces))]
    public async Task ResponseValidation_MissingSessionSecret_Throws(string? sec)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = "id", SessionToken = "tok", SessionSecret = sec, DeviceLinkBase = "https://x" });
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => BaseBuilder(connector).InitAsync());
    }

    [Theory]
    [MemberData(nameof(DeviceLinkBadBase))]
    public async Task ResponseValidation_InvalidDeviceLinkBase_Throws(string? dlb)
    {
        var connector = new Mock<ISmartIdConnector>();
        connector.Setup(x => x.InitDeviceLinkSignatureAsync(It.IsAny<DeviceLinkSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = "id", SessionToken = "tok", SessionSecret = "sec", DeviceLinkBase = dlb });
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => BaseBuilder(connector).InitAsync());
    }
}
