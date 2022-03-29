using System.Security.Cryptography.X509Certificates;
using UnityEngine.Networking;

public class CustomCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // accept all certificates if SSL is disabled
        if (!ConfigController.Instance.GetBoolValue(ConfigController.SSL_ENABLED))
            return true;

        X509Certificate2 remoteCertificate = new X509Certificate2(certificateData);
        string pk = remoteCertificate.GetPublicKeyString();

        if (pk == null)
            return false;

        string password = ConfigController.Instance.GetStringValue(ConfigController.SSL_KEYSTORE_PASSWORD);
        X509Certificate2 cert =
            new X509Certificate2(ConfigController.Instance.GetStringValue(ConfigController.SSL_KEYSTORE_PATH),
                password);
        return pk.Equals(cert.GetPublicKeyString());
    }
}