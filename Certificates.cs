// ============================================================================
// This document contains confidential and proprietary information of ABB
// and may be protected by patents, trademarks, copyrights, trade secrets,
// and/or other relevant state, federal, and foreign laws.  Its receipt or
// possession does not convey any rights to reproduce, disclose its contents,
// or to manufacture, use or sell anything contained herein.  Forwarding,
// reproducing, disclosing or using without specific written authorization of
// ABB is strictly forbidden.
// ============================================================================
// <copyright file="Certificates.cs" company="ABB Ltd">
//   Copyright © ABB Ltd 2017. All rights reserved.
// </copyright>
// ============================================================================

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
