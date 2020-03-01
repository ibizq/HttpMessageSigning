using System;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    /// Represents a request signature verification failure.
    /// </summary>
    public class RequestSignatureVerificationResultFailure : RequestSignatureVerificationResult {
        public RequestSignatureVerificationResultFailure(
            Client client, 
            Signature signature,
            SignatureVerificationException signatureVerificationException) : base(client, signature) {
            SignatureVerificationException = signatureVerificationException ?? throw new ArgumentNullException(nameof(signatureVerificationException));
        }

        /// <summary>
        /// Gets the exception that caused the verification failure.
        /// </summary>
        public SignatureVerificationException SignatureVerificationException { get; }
        
        /// <summary>
        /// Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public override bool IsSuccess => false;
    }
}