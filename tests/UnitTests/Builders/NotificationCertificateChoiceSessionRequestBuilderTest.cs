using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId;

/// <seealso cref="ee.sk.smartid.NotificationCertificateChoiceSessionRequestBuilderTest" />
public class NotificationCertificateChoiceSessionRequestBuilderTest
{
    private static readonly string RpUuid = "00000000-0000-4000-8000-000000000000";
    private static readonly SemanticsIdentifier Semantics = new("PNOEE-48010010101");

    private NotificationCertificateChoiceSessionRequestBuilder Base(Mock<ISmartIdConnector> m) =>
        new NotificationCertificateChoiceSessionRequestBuilder(m.Object)
            .WithRelyingPartyUUID(RpUuid)
            .WithRelyingPartyName("DEMO")
            .WithSemanticsIdentifier(Semantics);

    private static NotificationCertificateChoiceSessionResponse Ok() =>
        new() { SessionID = "00000000-0000-0000-0000-000000000000" };

    public static IEnumerable<object?[]> NullableSid() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    [Fact]
    public async Task Init_WithSemanticsIdentifier_Ok()
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Ok());
        await Base(c).InitAsync();
        Assert.Equal("PNOEE-48010010101", ((SemanticsIdentifier)c.Invocations[^1].Arguments[1]!).Identifier);
    }

    /// <summary>ADVANCED, QUALIFIED, QSCD plus unspecified (null expected on wire). Uses enum names — xUnit loses <see cref="CertificateLevel.QSCD"/> when resolving duplicate-valued enums from theory payloads. <see cref="CertificateLevel.QSCD"/> and <see cref="CertificateLevel.QUALIFIED"/> share value 2, so serialization uses QUALIFIED.</summary>
    [Theory]
    [InlineData(null, null)]
    [InlineData("ADVANCED", "ADVANCED")]
    [InlineData("QUALIFIED", "QUALIFIED")]
    [InlineData("QSCD", "QUALIFIED")]
    public async Task Init_CertificateLevel(string? levelName, string? expected)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Ok());
        var b = levelName == null ? Base(c) : Base(c).WithCertificateLevel(Enum.Parse<CertificateLevel>(levelName));
        await b.InitAsync();
        Assert.Equal(expected, ((NotificationCertificateChoiceSessionRequest)c.Invocations[^1].Arguments[0]!).CertificateLevel);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.ValidNonceLengths), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_Nonce(string? nonce)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Ok());
        var b = nonce == null ? Base(c).WithNonce(null!) : Base(c).WithNonce(nonce);
        await b.InitAsync();
        Assert.Equal(nonce, ((NotificationCertificateChoiceSessionRequest)c.Invocations[^1].Arguments[0]!).Nonce);
    }

    [Fact]
    public async Task Init_IpNotUsed_RequestPropertiesNull()
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Ok());
        await Base(c).InitAsync();
        Assert.Null(((NotificationCertificateChoiceSessionRequest)c.Invocations[^1].Arguments[0]!).RequestProperties);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Init_ShareIp(bool shareIp)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Ok());
        await Base(c).WithShareMdClientIpAddress(shareIp).InitAsync();
        var req = (NotificationCertificateChoiceSessionRequest)c.Invocations[^1].Arguments[0]!;
        Assert.NotNull(req.RequestProperties);
        Assert.Equal(shareIp, req.RequestProperties.ShareMdClientIpAddress);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Init_Capabilities(string[] caps, HashSet<string> expected)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(Ok());
        await Base(c).WithCapabilities(caps).InitAsync();
        Assert.True(expected.SetEquals(((NotificationCertificateChoiceSessionRequest)c.Invocations[^1].Arguments[0]!).Capabilities));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_RelyingPartyUuid(string? v)
    {
        var c = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithRelyingPartyUUID(v!).InitAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_RelyingPartyName(string? v)
    {
        var c = new Mock<ISmartIdConnector>();
        await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithRelyingPartyName(v!).InitAsync());
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.InvalidNotificationCertChoiceNonces), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task Validate_Nonce(string nonce, string msg)
    {
        var c = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => Base(c).WithNonce(nonce).InitAsync());
        Assert.Equal(msg, ex.Message);
    }

    [Fact]
    public async Task Validate_SemanticsMissing_Throws()
    {
        var c = new Mock<ISmartIdConnector>();
        var b = new NotificationCertificateChoiceSessionRequestBuilder(c.Object)
            .WithRelyingPartyUUID(RpUuid)
            .WithRelyingPartyName("DEMO");
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => b.InitAsync());
        Assert.Equal("Value for 'semanticIdentifier' must be set", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableSid))]
    public async Task Response_MissingSessionId(string? sid)
    {
        var c = new Mock<ISmartIdConnector>();
        c.Setup(x => x.InitNotificationCertificateChoiceAsync(It.IsAny<NotificationCertificateChoiceSessionRequest>(), It.IsAny<SemanticsIdentifier>(), default))
            .ReturnsAsync(new NotificationCertificateChoiceSessionResponse { SessionID = sid });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Base(c).InitAsync());
        Assert.Equal("Notification-based certificate choice response field 'sessionID' is missing or empty", ex.Message);
    }
}
