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

/// <seealso cref="ee.sk.smartid.NotificationSignatureSessionRequestBuilderTest" />
public class NotificationSignatureSessionRequestBuilderTest
{
    private static readonly string RpUuid = "00000000-0000-4000-8000-000000000000";
    private static readonly SemanticsIdentifier Semantics =
        new(SemanticsIdentifier.IdentityType.PNO, SemanticsIdentifier.CountryCode.EE, "31111111111");

    private const string DocumentNumber = "PNOEE-31111111111";

    private static NotificationSignatureSessionResponse OkResponse(VerificationCode? vc = null) => new()
    {
        SessionID = "00000000-0000-0000-0000-000000000000",
        Vc = vc ?? new VerificationCode { Type = VerificationCodeType.NUMERIC4.GetValue(), Value = "4927" }
    };

    private static NotificationSignatureSessionRequestBuilder SemanticsBase(Mock<ISmartIdConnector> m) =>
        new NotificationSignatureSessionRequestBuilder(m.Object)
            .WithRelyingPartyUUID(RpUuid).WithRelyingPartyName("DEMO")
            .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Sign?") })
            .WithSignableData(new SignableData("Test data"u8.ToArray())).WithSemanticsIdentifier(Semantics);

    private static NotificationSignatureSessionRequestBuilder DocBase(Mock<ISmartIdConnector> m) =>
        new NotificationSignatureSessionRequestBuilder(m.Object)
            .WithRelyingPartyUUID(RpUuid).WithRelyingPartyName("DEMO")
            .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Sign?") })
            .WithSignableData(new SignableData("Test data"u8.ToArray())).WithDocumentNumber(DocumentNumber);

    private static SignableHash PrecomputedHashSha512() =>
        new SignableHash
        {
            HashInBase64 = Convert.ToBase64String(DigestCalculator.CalculateDigest("Test data"u8.ToArray(), SmartIdHashAlgorithm.SHA_512))
        };

    public static IEnumerable<object?[]> RpBad() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    public static IEnumerable<object?[]> NullableIx()
    {
        yield return new object?[] { null };
        yield return new object[] { new List<NotificationInteraction>() };
    }

    public static IEnumerable<object?[]> NullableSid() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    private static NotificationSignatureSessionResponse Resp(VerificationCode? vc, string sid = "00000000-0000-0000-0000-000000000000") =>
        new() { SessionID = sid, Vc = vc! };

    [Fact]
    public async Task Init_WithSemanticsIdentifier_Ok()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), Semantics, default)).ReturnsAsync(OkResponse());
        await SemanticsBase(m).InitAsync();
        Assert.Equal(SignatureProtocol.RAW_DIGEST_SIGNATURE.ToString(),
            ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocol);
    }

    [Fact]
    public async Task Init_WithDocumentNumber_Ok()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), DocumentNumber, default)).ReturnsAsync(OkResponse());
        await DocBase(m).InitAsync();
        m.Verify(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), DocumentNumber, default), Times.Once);
    }

    /// <summary>ADVANCED, QUALIFIED, QSCD plus unspecified — <see cref="CertificateLevel.QSCD"/> and <see cref="CertificateLevel.QUALIFIED"/> share value 2, so enum <c>ToString()</c> resolves to QUALIFIED on the wire.</summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData("ADVANCED", "ADVANCED")]
    [InlineData("QUALIFIED", "QUALIFIED")]
    [InlineData("QSCD", "QUALIFIED")]
    public async Task Init_CertificateLevel(string? levelName, string? expected)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        var b = levelName == null ? SemanticsBase(m) : SemanticsBase(m).WithCertificateLevel(Enum.Parse<CertificateLevel>(levelName));
        await b.InitAsync();
        Assert.Equal(expected, ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).CertificateLevel);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.ValidNonceLengths), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_WithNonce(string? nonce)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        var b = nonce == null ? SemanticsBase(m).WithNonce(null!) : SemanticsBase(m).WithNonce(nonce);
        await b.InitAsync();
        Assert.Equal(nonce, ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).Nonce);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Init_WithRequestProperties(bool shareIp)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        await SemanticsBase(m).WithShareMdClientIpAddress(shareIp).InitAsync();
        var r = ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).RequestProperties;
        Assert.NotNull(r);
        Assert.Equal(shareIp, r.ShareMdClientIpAddress);
    }

    [Fact]
    public async Task Init_SignableHash_DefaultSha512_HashParameters()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        var h = new SignableHash { HashInBase64 = Convert.ToBase64String("Test data"u8.ToArray()) };
        await SemanticsBase(m).WithSignableData(null!).WithSignableHash(h).InitAsync();
        var p = ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocolParameters;
        Assert.Equal(SmartIdHashAlgorithm.SHA_512.GetApiAlgorithmName(), p.SignatureAlgorithmParameters!.HashAlgorithm);
        Assert.Equal(SigningSignatureAlgorithm.RSASSA_PSS.GetAlgorithmName(), p.SignatureAlgorithm);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSmartIdHashAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_SignableHash_AllHashAlgorithms(SmartIdHashAlgorithm algo)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        var h = new SignableHash { HashInBase64 = Convert.ToBase64String("Test hash"u8.ToArray()), HashAlgorithm = algo };
        await SemanticsBase(m).WithSignableData(null!).WithSignableHash(h).InitAsync();
        var p = ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocolParameters;
        Assert.Equal(Convert.ToBase64String("Test hash"u8.ToArray()), p.Digest);
        Assert.Equal(algo.GetApiAlgorithmName(), p.SignatureAlgorithmParameters!.HashAlgorithm);
    }

    [Fact]
    public async Task Init_SignableData_DefaultSha512()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        await SemanticsBase(m).InitAsync();
        var p = ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocolParameters;
        Assert.Equal(SmartIdHashAlgorithm.SHA_512.GetApiAlgorithmName(), p.SignatureAlgorithmParameters!.HashAlgorithm);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSmartIdHashAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_SignableData_AllHashAlgorithms(SmartIdHashAlgorithm algo)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        var sd = new SignableData("Test data"u8.ToArray(), algo);
        await SemanticsBase(m).WithSignableData(sd).InitAsync();
        var p = ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocolParameters;
        Assert.Equal(sd.GetDigestInBase64(), p.Digest);
        Assert.Equal(algo.GetApiAlgorithmName(), p.SignatureAlgorithmParameters!.HashAlgorithm);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_Capabilities(string[] caps, HashSet<string> expected)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        await SemanticsBase(m).WithCapabilities(caps).InitAsync();
        Assert.True(expected.SetEquals(((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).Capabilities));
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.AllSigningSignatureAlgorithms), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_SignatureAlgorithms_All(SigningSignatureAlgorithm algo)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse());
        await SemanticsBase(m).WithSignatureAlgorithm(algo).InitAsync();
        var p = ((NotificationSignatureSessionRequest)m.Invocations[^1].Arguments[0]!).SignatureProtocolParameters;
        Assert.Equal(algo.GetAlgorithmName(), p.SignatureAlgorithm);
        if (algo.IsLegacyRsa()) Assert.Null(p.SignatureAlgorithmParameters);
        else
        {
            Assert.NotNull(p.SignatureAlgorithmParameters);
            Assert.Equal(SmartIdHashAlgorithm.SHA_512.GetApiAlgorithmName(), p.SignatureAlgorithmParameters.HashAlgorithm);
        }
    }

    [Theory]
    [MemberData(nameof(RpBad))]
    public async Task Err_MissingRpUuid(string? v) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => SemanticsBase(new Mock<ISmartIdConnector>()).WithRelyingPartyUUID(v!).InitAsync());

    [Theory]
    [MemberData(nameof(RpBad))]
    public async Task Err_MissingRpName(string? v) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => SemanticsBase(new Mock<ISmartIdConnector>()).WithRelyingPartyName(v!).InitAsync());

    [Fact]
    public async Task Err_BothIdentifierFields_Throws() =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            DocBase(new Mock<ISmartIdConnector>()).WithSemanticsIdentifier(Semantics).InitAsync());

    [Fact]
    public async Task Err_NoIdentifierFields_Throws() =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            new NotificationSignatureSessionRequestBuilder(new Mock<ISmartIdConnector>().Object)
                .WithRelyingPartyUUID(RpUuid).WithRelyingPartyName("DEMO")
                .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Sign?") })
                .WithSignableData(new SignableData("Test data"u8.ToArray())).InitAsync());

    [Fact]
    public async Task Err_InvalidSignatureAlgo_Throws() =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            SemanticsBase(new Mock<ISmartIdConnector>()).WithSignatureAlgorithm((SigningSignatureAlgorithm)999).InitAsync());

    [Fact]
    public async Task Err_NoDigest_Throws() =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            SemanticsBase(new Mock<ISmartIdConnector>()).WithSignableData(null!).WithSignableHash(null!).InitAsync());

    [Fact]
    public void Err_DataThenHash_LineBuild_Throws() =>
        Assert.Throws<SmartIdRequestSetupException>(() =>
            new NotificationSignatureSessionRequestBuilder(new Mock<ISmartIdConnector>().Object)
                .WithRelyingPartyUUID(RpUuid).WithRelyingPartyName("DEMO")
                .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Sign?") })
                .WithSignableData(new SignableData("Test data"u8.ToArray())).WithSignableHash(PrecomputedHashSha512())
                .WithSemanticsIdentifier(Semantics));

    [Fact]
    public void Err_HashThenData_LineBuild_Throws() =>
        Assert.Throws<SmartIdRequestSetupException>(() =>
            new NotificationSignatureSessionRequestBuilder(new Mock<ISmartIdConnector>().Object)
                .WithRelyingPartyUUID(RpUuid).WithRelyingPartyName("DEMO")
                .WithInteractions(new List<NotificationInteraction> { NotificationInteraction.DisplayTextAndPin("Sign?") })
                .WithSignableHash(PrecomputedHashSha512()).WithSignableData(new SignableData("Test data"u8.ToArray()))
                .WithSemanticsIdentifier(Semantics));

    [Theory]
    [MemberData(nameof(NullableIx))]
    public async Task Err_InteractionsMissing(IReadOnlyList<NotificationInteraction>? ix) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            SemanticsBase(new Mock<ISmartIdConnector>()).WithInteractions(ix!).InitAsync());

    [Fact]
    public async Task Err_InteractionsNullElement_Throws() =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            SemanticsBase(new Mock<ISmartIdConnector>()).WithInteractions(new List<NotificationInteraction> { null! }).InitAsync());

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.DuplicateNotificationInteractionsCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Err_DuplicateInteractions(List<NotificationInteraction> list) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            SemanticsBase(new Mock<ISmartIdConnector>()).WithInteractions(list).InitAsync());

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123456789012345678901")]
    public async Task Err_InvalidNonce(string nonce) =>
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() =>
            SemanticsBase(new Mock<ISmartIdConnector>()).WithNonce(nonce).InitAsync());

    [Theory]
    [MemberData(nameof(NullableSid))]
    public async Task Resp_MissingSessionId(string? sid)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Resp(new VerificationCode { Type = VerificationCodeType.NUMERIC4.GetValue(), Value = "4927" }, sid!));
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => SemanticsBase(m).InitAsync());
    }

    [Fact]
    public async Task Resp_MissingVerificationCodeWrapper_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(new NotificationSignatureSessionResponse { SessionID = "00000000-0000-0000-0000-000000000000", Vc = null });
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => SemanticsBase(m).InitAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Resp_VcTypeMissing(string? t)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse(new VerificationCode { Type = t!, Value = "4927" }));
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => SemanticsBase(m).InitAsync());
    }

    [Fact]
    public async Task Resp_UnsupportedVcType_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse(new VerificationCode { Type = "unsupportedType", Value = null }));
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => SemanticsBase(m).InitAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Resp_VcValueMissing(string? v)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse(new VerificationCode { Type = VerificationCodeType.NUMERIC4.GetValue(), Value = v! }));
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => SemanticsBase(m).InitAsync());
    }

    [Fact]
    public async Task Resp_VcPatternInvalid_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.InitNotificationSignatureAsync(It.IsAny<NotificationSignatureSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(OkResponse(new VerificationCode { Type = VerificationCodeType.NUMERIC4.GetValue(), Value = "aaaaaa" }));
        await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => SemanticsBase(m).InitAsync());
    }
}
