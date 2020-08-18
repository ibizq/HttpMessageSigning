using System;
using System.Security.Claims;

namespace Dalion.HttpMessageSigning.Verification {
    /// <summary>
    ///     Represents a successful signature verification.
    /// </summary>
    public class RequestSignatureVerificationResultSuccess : RequestSignatureVerificationResult {
        /// <summary>
        ///     Creates a new instance of this class.
        /// </summary>
        /// <param name="client">The client for which the verification happened.</param>
        /// <param name="signature">The signature for which verification was attempted.</param>
        /// <param name="principal">The principal that represents the verified signature.</param>
        public RequestSignatureVerificationResultSuccess(
            Client client,
            Signature signature,
            ClaimsPrincipal principal) : base(client, signature) {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (signature == null) throw new ArgumentNullException(nameof(signature));
            Principal = principal ?? throw new ArgumentNullException(nameof(principal));
        }

        /// <summary>
        ///     Gets the principal that represents the verified signature.
        /// </summary>
        public ClaimsPrincipal Principal { get; }

        /// <summary>
        ///     Gets a value indicating whether the signature was successfully verified.
        /// </summary>
        public override bool IsSuccess => true;
    }
}