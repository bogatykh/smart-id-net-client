using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserActions;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId;

/// <seealso cref="ee.sk.smartid.DeviceLinkCertificateChoiceSessionRequestBuilderTest" />
public class DeviceLinkCertificateChoiceSessionRequestBuilderTest
{
    private Mock<ISmartIdConnector> connector = null!;
    private DeviceLinkCertificateChoiceSessionRequestBuilder builderService = null!;

    public DeviceLinkCertificateChoiceSessionRequestBuilderTest()
    {
        connector = new Mock<ISmartIdConnector>();
        builderService = new DeviceLinkCertificateChoiceSessionRequestBuilder(connector.Object)
            .WithRelyingPartyUUID("test-relying-party-uuid")
            .WithRelyingPartyName("DEMO")
            .WithCertificateLevel(CertificateLevel.QUALIFIED)
            .WithNonce("1234567890")
            .WithInitialCallbackUrl("https://example.com/callback");
    }

    private static DeviceLinkSessionResponse MockCertificateChoiceResponse() => new()
    {
        SessionID = "test-session-id",
        SessionToken = "test-session-token",
        SessionSecret = "test-session-secret",
        DeviceLinkBase = "https://example.com/device-link"
    };

    [Fact]
    public async Task InitiateCertificateChoice_Ok()
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(MockCertificateChoiceResponse());

        DeviceLinkSessionResponse result = await builderService.InitAsync();

        Assert.NotNull(result);
        Assert.Equal("test-session-id", result.SessionID);
        Assert.Equal("test-session-token", result.SessionToken);
        Assert.Equal("test-session-secret", result.SessionSecret);
        Assert.Equal("https://example.com/device-link", result.DeviceLinkBase);
        connector.Verify(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task InitiateCertificateChoice_NullRequestProperties_WithShareMdClientIpFalse()
    {
        builderService.WithShareMdClientIpAddress(false);
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(MockCertificateChoiceResponse());

        DeviceLinkSessionResponse result = await builderService.InitAsync();

        Assert.NotNull(result);
        Assert.Equal("test-session-id", result.SessionID);
        connector.Verify(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task InitiateCertificateChoice_MissingCertificateLevel_Ok()
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(MockCertificateChoiceResponse());
        var b = new DeviceLinkCertificateChoiceSessionRequestBuilder(connector.Object)
            .WithRelyingPartyUUID("test-relying-party-uuid")
            .WithRelyingPartyName("DEMO")
            .WithNonce("1234567890")
            .WithInitialCallbackUrl("https://example.com/callback");
        Assert.NotNull(await b.InitAsync());
        connector.Verify(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default), Times.Once);
    }

    [Theory]
    [MemberData(nameof(SessionRequestBuilderTestData.CapabilitiesCases), MemberType = typeof(SessionRequestBuilderTestData))]
    public async Task InitiateCertificateChoice_WithValidCapabilities_Ok(string[] capabilities, HashSet<string> expected)
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(MockCertificateChoiceResponse());
        await builderService.WithCapabilities(capabilities).InitAsync();
        var req = (DeviceLinkCertificateChoiceSessionRequest)connector.Invocations[^1].Arguments[0]!;
        Assert.True(expected.SetEquals(req.Capabilities ?? new HashSet<string>()));
    }

    public static IEnumerable<object?[]> NullableSessionFields() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    public static IEnumerable<object?[]> NullableDeviceLinkUris()
    {
        yield return new object?[] { null };
        yield return new object[] { "" };
        yield return new object[] { " " };
        // Java also includes about:blank; .NET accepts non-whitespace URIs without validating scheme.
    }

    [Theory]
    [MemberData(nameof(NullableSessionFields))]
    public async Task ErrorCases_InitiateCertificateChoice_WhenSessionIdIsNullOrEmpty_Throws(string? sessionId)
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse
            {
                SessionID = sessionId,
                SessionToken = "test-session-token",
                SessionSecret = "test-session-secret",
                DeviceLinkBase = "https://example.com/device-link"
            });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => builderService.InitAsync());
        Assert.Equal("Device link certificate choice session initialisation response field 'sessionID' is missing or empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableSessionFields))]
    public async Task ErrorCases_InitiateCertificateChoice_WhenSessionTokenIsNullOrEmpty_Throws(string? tok)
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = "test-session-id", SessionToken = tok, SessionSecret = "secret", DeviceLinkBase = "https://x" });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => builderService.InitAsync());
        Assert.Equal("Device link certificate choice session initialisation response field 'sessionToken' is missing or empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableSessionFields))]
    public async Task ErrorCases_InitiateCertificateChoice_WhenSessionSecretIsNullOrEmpty_Throws(string? sec)
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = "id", SessionToken = "tok", SessionSecret = sec, DeviceLinkBase = "https://x" });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => builderService.InitAsync());
        Assert.Equal("Device link certificate choice session initialisation response field 'sessionSecret' is missing or empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(NullableDeviceLinkUris))]
    public async Task ErrorCases_InitiateCertificateChoice_WhenDeviceLinkBaseIsNullOrEmpty_Throws(string? uriStr)
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(new DeviceLinkSessionResponse { SessionID = "id", SessionToken = "tok", SessionSecret = "sec", DeviceLinkBase = uriStr });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => builderService.InitAsync());
        Assert.Equal("Device link certificate choice session initialisation response field 'deviceLinkBase' is missing or empty", ex.Message);
    }

    [Fact]
    public async Task ErrorCases_InitiateCertificateChoice_UserAccountNotFound()
    {
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ThrowsAsync(new UserAccountNotFoundException());
        await Assert.ThrowsAsync<UserAccountNotFoundException>(() => builderService.InitAsync());
    }

    [Fact]
    public async Task ErrorCases_InitiateCertificateChoice_MissingRelyingPartyUUID()
    {
        builderService.WithRelyingPartyUUID(null!);
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => builderService.InitAsync());
        Assert.Equal("Value for 'relyingPartyUUID' cannot be empty", ex.Message);
    }

    [Fact]
    public async Task ErrorCases_InitiateCertificateChoice_MissingRelyingPartyName()
    {
        builderService.WithRelyingPartyName(null!);
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => builderService.InitAsync());
        Assert.Equal("Value for 'relyingPartyName' cannot be empty", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890123456789012345678901")]
    public async Task ErrorCases_InitiateCertificateChoice_NonceWithInvalidLength_Throws(string invalidNonce)
    {
        builderService.WithNonce(invalidNonce);
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => builderService.InitAsync());
        Assert.Equal("Value for 'nonce' must have length between 1 and 30 characters", ex.Message);
    }

    [Fact]
    public async Task ErrorCases_InitiateCertificateChoice_WithoutInitialCallbackUrl_Ok()
    {
        builderService.WithInitialCallbackUrl(null!);
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(MockCertificateChoiceResponse());
        Assert.NotNull(await builderService.InitAsync());
        connector.Verify(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task ErrorCases_InitiateCertificateChoice_NullNonce_Ok()
    {
        builderService.WithNonce(null!);
        connector.Setup(c => c.InitDeviceLinkCertificateChoiceAsync(It.IsAny<DeviceLinkCertificateChoiceSessionRequest>(), default))
            .ReturnsAsync(MockCertificateChoiceResponse());
        Assert.NotNull(await builderService.InitAsync());
    }

    [Theory]
    [InlineData("http://example.com")]
    [InlineData("https://example.com|test")]
    [InlineData("ftp://example.com")]
    public async Task ErrorCases_InitCertificateChoice_InitialCallbackUrlIsInvalid_Throws(string url)
    {
        var b = new DeviceLinkCertificateChoiceSessionRequestBuilder(connector.Object)
            .WithRelyingPartyUUID("00000000-0000-0000-0000-000000000000")
            .WithRelyingPartyName("DEMO")
            .WithNonce("123456")
            .WithInitialCallbackUrl(url);
        var ex = await Assert.ThrowsAsync<SmartIdRequestSetupException>(() => b.InitAsync());
        Assert.Equal("Value for 'initialCallbackUrl' must match pattern ^https://[^|]+$ and must not contain unencoded vertical bars", ex.Message);
    }
}
