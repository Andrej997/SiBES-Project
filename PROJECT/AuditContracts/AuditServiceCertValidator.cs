﻿using System;
using System.IdentityModel.Selectors;
using System.Security.Cryptography.X509Certificates;

namespace AuditContracts
{
    public class AuditServiceCertValidator : X509CertificateValidator
    {
        /// <summary>
        /// Implementation of a custom certificate validation on the service side.
        /// Service should consider certificate valid if its issuer is the same as the issuer of the service.
        /// If validation fails, throw an exception with an adequate message.
        /// </summary>
        /// <param name="certificate"> certificate to be validate </param>
        public override void Validate(X509Certificate2 certificate)
        {
            /// This will take service's certificate from storage
            X509Certificate2 srvCert = AuditCertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, "wcfservice");

            try
            {
                if (!certificate.Issuer.Equals(srvCert.Issuer))
                {
                    throw new Exception("Certificate is not from the valid issuer.");
                }

                Console.WriteLine("Certificate valid");
            }
            catch(Exception e)
            {
                Console.WriteLine("Cert validation error: " + e.Message);
            }
            
        }
    }
}
