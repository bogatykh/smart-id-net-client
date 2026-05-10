using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SK.SmartId.Exceptions;
using SK.SmartId.Exceptions.Permanent;
using SK.SmartId.Exceptions.UserAccounts;
using SK.SmartId.Rest;
using SK.SmartId.Rest.Dao;
using Xunit;

namespace SK.SmartId;

/// <seealso cref="ee.sk.smartid.CertificateByDocumentNumberRequestBuilderTest" />
public class CertificateByDocumentNumberRequestBuilderTest
{
    private const string CertificateBase64 =
        "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";

    private const string DocumentNumber = "PNOEE-1234567890-MOCK-Q";
    private const string RpUuid = "00000000-0000-0000-0000-000000000000";
    private const string RpName = "DEMO";

    private static CertificateResponse Ok(string certValue, string level) =>
        new() { State = CertificateState.OK.ToString(), Cert = new CertificateInfo { Value = certValue, CertificateLevel = level } };

    public static IEnumerable<object?[]> EmptyString() =>
        new[] { new object?[] { null }, new object?[] { "" } };

    private static Mock<ISmartIdConnector> MockGet(CertificateResponse r)
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.GetCertificateByDocumentNumberAsync(DocumentNumber, It.IsAny<CertificateByDocumentNumberRequest>(), default))
            .ReturnsAsync(r);
        return m;
    }

    private static CertificateByDocumentNumberRequestBuilder Valid(Mock<ISmartIdConnector> m) =>
        new CertificateByDocumentNumberRequestBuilder(m.Object)
            .WithDocumentNumber(DocumentNumber).WithRelyingPartyUUID(RpUuid).WithRelyingPartyName(RpName);

    [Fact]
    public async Task GetCertificateByDocumentNumber_Ok()
    {
        var m = MockGet(Ok(CertificateBase64, CertificateLevel.QUALIFIED.ToString()));
        var result = await new CertificateByDocumentNumberRequestBuilder(m.Object)
            .WithDocumentNumber(DocumentNumber).WithRelyingPartyUUID(RpUuid).WithRelyingPartyName(RpName)
            .WithCertificateLevel(CertificateLevel.QUALIFIED).GetCertificateByDocumentNumberAsync();
        Assert.NotNull(result.Certificate);
        Assert.Equal(CertificateLevel.QUALIFIED, result.CertificateLevel);
        string subject = result.Certificate.Subject;
        Assert.True(subject.Contains("TESTNUMBER", StringComparison.Ordinal) || subject.Contains("DEMO", StringComparison.Ordinal), subject);
        m.Verify(x => x.GetCertificateByDocumentNumberAsync(DocumentNumber, It.Is<CertificateByDocumentNumberRequest>(r =>
            r.RelyingPartyUUID == RpUuid && r.RelyingPartyName == RpName && r.CertificateLevel == "QUALIFIED"), default), Times.Once);
    }

    [Theory]
    [MemberData(nameof(EmptyString))]
    public async Task Validate_DocumentNumber_Empty(string? dn)
    {
        var m = new Mock<ISmartIdConnector>();
        var ex = await Assert.ThrowsAsync<SmartIdClientException>(() =>
            new CertificateByDocumentNumberRequestBuilder(m.Object).WithDocumentNumber(dn!).WithRelyingPartyUUID(RpUuid).WithRelyingPartyName(RpName).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Value for 'documentNumber' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(EmptyString))]
    public async Task Validate_RelyingPartyUuid_Empty(string? v)
    {
        var m = MockGet(Ok(CertificateBase64, CertificateLevel.QUALIFIED.ToString()));
        var ex = await Assert.ThrowsAsync<SmartIdClientException>(() =>
            new CertificateByDocumentNumberRequestBuilder(m.Object).WithDocumentNumber(DocumentNumber).WithRelyingPartyUUID(v!).WithRelyingPartyName(RpName).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Value for 'relyingPartyUUID' cannot be empty", ex.Message);
    }

    [Theory]
    [MemberData(nameof(EmptyString))]
    public async Task Validate_RelyingPartyName_Empty(string? v)
    {
        var m = MockGet(Ok(CertificateBase64, CertificateLevel.QUALIFIED.ToString()));
        var ex = await Assert.ThrowsAsync<SmartIdClientException>(() =>
            new CertificateByDocumentNumberRequestBuilder(m.Object).WithDocumentNumber(DocumentNumber).WithRelyingPartyUUID(RpUuid).WithRelyingPartyName(v!).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Value for 'relyingPartyName' cannot be empty", ex.Message);
    }

    [Fact]
    public async Task Response_Null_Throws()
    {
        var m = new Mock<ISmartIdConnector>();
        m.Setup(x => x.GetCertificateByDocumentNumberAsync(DocumentNumber, It.IsAny<CertificateByDocumentNumberRequest>(), default))
            .ReturnsAsync((CertificateResponse)null!);
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response is not provided", ex.Message);
    }

    [Fact]
    public async Task Response_StateMissing_Throws()
    {
        var m = MockGet(new CertificateResponse { State = null, Cert = null });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'state' is missing", ex.Message);
    }

    [Fact]
    public async Task Response_StateInvalid_Throws()
    {
        var m = MockGet(new CertificateResponse { State = "invalid", Cert = null });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'state' has unsupported value", ex.Message);
    }

    [Fact]
    public async Task Response_StateDocumentUnusable_Throws()
    {
        var m = MockGet(new CertificateResponse { State = CertificateState.DOCUMENT_UNUSABLE.ToString(), Cert = null });
        await Assert.ThrowsAsync<DocumentUnusableException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
    }

    [Fact]
    public async Task Response_CertFieldMissing_Throws()
    {
        var m = MockGet(new CertificateResponse { State = CertificateState.OK.ToString(), Cert = null });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'cert' is missing", ex.Message);
    }

    [Fact]
    public async Task Response_CertificateLevelMissing_Throws()
    {
        var m = MockGet(new CertificateResponse { State = CertificateState.OK.ToString(), Cert = new CertificateInfo { Value = CertificateBase64, CertificateLevel = null } });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'cert.certificateLevel' is missing", ex.Message);
    }

    [Fact]
    public async Task Response_CertificateLevelInvalid_Throws()
    {
        var m = MockGet(Ok(CertificateBase64, "invalid"));
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'cert.certificateLevel' has unsupported value", ex.Message);
    }

    [Fact]
    public async Task Response_LevelLowerThanRequested_Throws()
    {
        var m = MockGet(Ok(CertificateBase64, CertificateLevel.ADVANCED.ToString()));
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() =>
            new CertificateByDocumentNumberRequestBuilder(m.Object)
                .WithDocumentNumber(DocumentNumber).WithRelyingPartyUUID(RpUuid).WithRelyingPartyName(RpName)
                .WithCertificateLevel(CertificateLevel.QUALIFIED).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate has lower level than requested", ex.Message);
    }

    [Fact]
    public async Task Response_CertValueMissing_Throws()
    {
        var m = MockGet(new CertificateResponse { State = CertificateState.OK.ToString(), Cert = new CertificateInfo { Value = null, CertificateLevel = CertificateLevel.QUALIFIED.ToString() } });
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'cert.value' is missing", ex.Message);
    }

    [Fact]
    public async Task Response_CertValueInvalidBase64_Throws()
    {
        var m = MockGet(Ok("NOT@BASE64!", CertificateLevel.QUALIFIED.ToString()));
        var ex = await Assert.ThrowsAsync<UnprocessableSmartIdResponseException>(() => Valid(m).GetCertificateByDocumentNumberAsync());
        Assert.Equal("Queried certificate response field 'cert.value' does not have Base64-encoded value", ex.Message);
    }
}
