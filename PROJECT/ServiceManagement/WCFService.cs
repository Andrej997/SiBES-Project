﻿using Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManagement
{
    public class WCFService : IWCFContract
    {
        //Dictionary<imeKorisnika, Dictionary<brojServisa, njegovEndpoint>>
        //private static Dictionary<string, Dictionary<int, string>> servisi = new Dictionary<string, Dictionary<int, string>>();
        //Dictionary<port+protokol, serviceHost>
        private static Dictionary<string, ServiceHost> servisi = new Dictionary<string, ServiceHost>();

        
        public string Connect()
        {
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            Console.WriteLine("Name: {0}", identity.Name);
            Console.WriteLine("IsAuthenticated {0}", identity.IsAuthenticated);
            Console.WriteLine("AuthenticationType {0}", identity.AuthenticationType);

            WindowsIdentity winIdentity = identity as WindowsIdentity;
            Console.WriteLine("Security Identifier (SID) {0}", winIdentity.User); // ovo ne moze preko IIentity id=nterfejsa jer je Windows-specific
            
            foreach (IdentityReference group in winIdentity.Groups)
            {
                SecurityIdentifier sid = (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier));
                var name = sid.Translate(typeof(NTAccount));
                Console.WriteLine("{0}", name.ToString());
            }
            return Program.secretKey;           
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public PovratnaVrijednost OpenApp(byte[] encrypted)
        {
            OpenAppData decryted = AesAlg.Decrypt(encrypted, Program.secretKey);

            /*if(BlackList logic)
            {
                string pov = WCFServiceAudit.factory.ConnectS();
                if(pov == "DOS")
                    return PovratnaVrijednost.DOS;
                return PovratnaVrijednost.NEMADOZ;;
            }*/


            if (servisi.ContainsKey(string.Format("{0}", decryted.Port)))
            {
                return PovratnaVrijednost.VECOTV;
            }

            ServiceHost host = new ServiceHost(typeof(WCFService));

            if(decryted.Protokol == "UDP")
            {
                UdpBinding binding = new UdpBinding();
                string addr = String.Format("soap.udp://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }
            else if(decryted.Protokol == "HTTP")
            {
                NetHttpBinding binding = new NetHttpBinding();
                string addr = String.Format("http://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }
            else
            {
                NetTcpBinding binding = new NetTcpBinding();
                string addr = String.Format("net.tcp://localhost:{0}/{1}", decryted.Port, decryted.ImeMasine);
                host.AddServiceEndpoint(typeof(IWCFContract), binding, addr);
            }

            string key = String.Format("{0}", decryted.Port);
            servisi.Add(key, host);
            servisi[key].Open();
            return PovratnaVrijednost.USPJEH;
        }

        [PrincipalPermission(SecurityAction.Demand, Role = "RunService")]
        public PovratnaVrijednost CloseApp(byte[] encrypted)
        {
            /*if(BlackList logic)
            {
                string pov = WCFServiceAudit.factory.ConnectS();
                if(pov == "DOS")
                    return PovratnaVrijednost.DOS;
                return PovratnaVrijednost.NEMADOZ;
            }*/

            OpenAppData decryted = AesAlg.Decrypt(encrypted, Program.secretKey);
            string key = string.Format("{0}", decryted.Port);
            if (servisi.ContainsKey(key))
            {
                servisi[key].Close();
                return PovratnaVrijednost.USPJEH;
            }

            
            return PovratnaVrijednost.NIJEOTV;
        }
    }



}
