using System.Security.Cryptography.X509Certificates;

public class CertificateValidationService
{
    public bool ValidateCertificate(X509Certificate2 clientCertificate)
    {
        //TODO: In production code use key vault to read this
        var cert = new X509Certificate2("dev_cert.pfx","1234");

        if(clientCertificate.Thumbprint == cert.Thumbprint)
        {
            return true;
        }

        return false;
    }
}
