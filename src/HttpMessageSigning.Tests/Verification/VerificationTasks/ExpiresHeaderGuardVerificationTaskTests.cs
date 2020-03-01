using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    public class ExpiresHeaderGuardVerificationTaskTests {
        private readonly ExpiresHeaderGuardVerificationTask _sut;

        public ExpiresHeaderGuardVerificationTaskTests() {
            _sut = new ExpiresHeaderGuardVerificationTask();
        }

        public class Verify : ExpiresHeaderGuardVerificationTaskTests {
            private readonly Client _client;
            private readonly Func<HttpRequestForSigning, Signature, Client, Task<Exception>> _method;
            private readonly Signature _signature;
            private readonly HttpRequestForSigning _signedRequest;

            public Verify() {
                _signature = (Signature)TestModels.Signature.Clone();
                _signature.Algorithm = "hmac-sha256";
                _signature.Headers = _signature.Headers.Concat(new[] {HeaderName.PredefinedHeaderNames.Expires}).ToArray();
                _signedRequest = (HttpRequestForSigning)TestModels.Request.Clone();
                _signedRequest.Headers.Add(HeaderName.PredefinedHeaderNames.Expires, _signature.Expires.Value.ToUnixTimeSeconds().ToString());
                _client = new Client(TestModels.Client.Id, TestModels.Client.Name, new CustomSignatureAlgorithm("hs2019"));
                _method = (request, signature, client) => _sut.Verify(request, signature, client);
            }

            [Theory]
            [InlineData("RSA")]
            [InlineData("HMAC")]
            [InlineData("ECDSA")]
            public async Task WhenSignatureIncludesExpiresHeader_ButItShouldNot_ReturnsSignatureVerificationException(string algorithm) {
                var client = new Client(_client.Id, _client.Name, new CustomSignatureAlgorithm(algorithm));
                _signature.Algorithm = algorithm + "-sha256";

                var actual = await _method(_signedRequest, _signature, client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenSignatureDoesNotSpecifyAnExpirationTime_ReturnsSignatureVerificationException() {
                _signature.Expires = null;

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenExpiresHeaderIsNotAValidTimestamp_ReturnsSignatureVerificationException() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Expires] = "{Nonsense}";

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenExpiresHeaderDoesNotMatchSignatureExpiration_ReturnsSignatureVerificationException() {
                _signedRequest.Headers[HeaderName.PredefinedHeaderNames.Expires] = (long.Parse(_signedRequest.Headers[HeaderName.PredefinedHeaderNames.Expires]) + 1).ToString();

                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().NotBeNull().And.BeAssignableTo<SignatureVerificationException>();
            }

            [Fact]
            public async Task WhenExpiresHeaderMatchesSignatureExpiration_ReturnsNull() {
                var actual = await _method(_signedRequest, _signature, _client);

                actual.Should().BeNull();
            }
        }
    }
}