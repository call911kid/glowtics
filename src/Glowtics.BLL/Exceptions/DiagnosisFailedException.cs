using System;

namespace Glowtics.BLL.Exceptions
{
    public class DiagnosisFailedException : ExternalServiceException
    {
        public DiagnosisFailedException() : base("Diagnosis failed.") { }
        public DiagnosisFailedException(string message) : base(message) { }
        public DiagnosisFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
