﻿using System;
using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dalion.HttpMessageSigning.Signing {
    public class CompositionTests : IDisposable {
        private readonly ServiceProvider _provider;
        private readonly RSACryptoServiceProvider _rsa;

        public CompositionTests() {
            _rsa = new RSACryptoServiceProvider();
            var services = new ServiceCollection();
            services
                .AddHttpMessageSigning("unit-test-app", settings => { settings.SignatureAlgorithm = SignatureAlgorithm.CreateForSigning("s3cr3t"); })
                .AddHMACHttpMessageSigning("unit-test-app-hmac", "s3cr3t")
                .AddRSAHttpMessageSigning("unit-test-app-rsa", _rsa);
            _provider = services.BuildServiceProvider();
        }

        public void Dispose() {
            _provider?.Dispose();
            _rsa?.Dispose();
        }

        [Theory]
        [InlineData(typeof(IRequestSignerFactory))]
        public void CanResolveType(Type requestedType) {
            object actualInstance = null;
            Action act = () => actualInstance = _provider.GetRequiredService(requestedType);
            act.Should().NotThrow();
            actualInstance.Should().NotBeNull();
            actualInstance.Should().BeAssignableTo(requestedType);
        }

        [Theory]
        [InlineData("unit-test-app")]
        [InlineData("unit-test-app-hmac")]
        [InlineData("unit-test-app-rsa")]
        public void CanResolveRegisteredSigner(string keyId) {
            object actualSigner = null;
            Action act = () => actualSigner = _provider.GetRequiredService<IRequestSignerFactory>().CreateFor(keyId);
            act.Should().NotThrow();
            actualSigner.Should().NotBeNull();
            actualSigner.Should().BeAssignableTo<IRequestSigner>();
        }

        [Theory]
        [InlineData("something-not-registered")]
        public void CannotResolveUnregisteredKeyId(string keyId) {
            Action act = () => _provider.GetRequiredService<IRequestSignerFactory>().CreateFor(keyId);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}