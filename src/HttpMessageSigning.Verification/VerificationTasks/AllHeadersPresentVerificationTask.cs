using System.Linq;

namespace Dalion.HttpMessageSigning.Verification.VerificationTasks {
    internal class AllHeadersPresentVerificationTask : VerificationTask {
        public override SignatureVerificationFailure VerifySync(HttpRequestForSigning signedRequest, Signature signature, Client client) {
            if (!signature.Headers.Contains(HeaderName.PredefinedHeaderNames.RequestTarget)) {
                return SignatureVerificationFailure.HeaderMissing($"The {HeaderName.PredefinedHeaderNames.RequestTarget} header is required to be included in the signature.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeDateHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Date)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Date} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeCreatedHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Created)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Created} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }
            
            if (client.SignatureAlgorithm.ShouldIncludeExpiresHeader() && !signature.Headers.Contains(HeaderName.PredefinedHeaderNames.Expires)) {
                return SignatureVerificationFailure.HeaderMissing($"The inclusion of the {HeaderName.PredefinedHeaderNames.Expires} header is required when using security algorithm '{client.SignatureAlgorithm.Name}'.");
            }

            foreach (var headerName in signature.Headers) {
                var isPresent = false;
                
                switch (headerName) {
                    case string str when str == HeaderName.PredefinedHeaderNames.Expires:
                        isPresent = signedRequest.Headers.Contains(headerName.ToSanitizedHttpHeaderName());
                        break;
                    case string str when str == HeaderName.PredefinedHeaderNames.Created:
                        isPresent = signedRequest.Headers.Contains(headerName.ToSanitizedHttpHeaderName());
                        break;
                    case string str when str == HeaderName.PredefinedHeaderNames.RequestTarget:
                        isPresent = true;
                        break;
                    default:
                        isPresent = signedRequest.Headers.Contains(headerName);
                        break;
                }
                
                if (!isPresent) {
                    return SignatureVerificationFailure.HeaderMissing($"The request header {headerName} is missing, but it is required to validate the signature.");
                }
            }
            
            return null;
        }
    }
}