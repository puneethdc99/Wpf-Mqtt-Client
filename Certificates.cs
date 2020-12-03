namespace WpfMqttClient
{
    using System.Collections;
    using System.Configuration;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using WpfClient.Properties;

    public static class Certificates
    {
        public static X509Certificate GetCertificate(string subjectName)
        {
            X509Certificate FindCertificate(StoreLocation location)
            {
                X509Store store = new X509Store(location);
                store.Open(OpenFlags.OpenExistingOnly);
                IEnumerable certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
                return certs.OfType<X509Certificate>().FirstOrDefault();
            }

            return FindCertificate(StoreLocation.CurrentUser) ?? FindCertificate(StoreLocation.LocalMachine);
        }

#pragma warning disable SA1404 // Code analysis suppression must have justification
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
#pragma warning restore SA1404 // Code analysis suppression must have justification
        public static bool IsCertificateEnabled()
        {
            try
            {
                var val = Settings.Default.UseTLS;
                var isEnabled = ConfigurationManager.AppSettings.Get("UseTLS");
                if (!string.IsNullOrEmpty(isEnabled))
                    return bool.Parse(isEnabled);
                else
                    return false;
            }
            catch { return false; }
        }
    }
}
