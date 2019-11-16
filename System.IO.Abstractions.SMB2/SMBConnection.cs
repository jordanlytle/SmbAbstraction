using System;
using System.Collections.Generic;
using System.Net;
using SmbLibraryStd;
using SmbLibraryStd.Client;

namespace System.IO.Abstractions.SMB
{
    public class SMBConnection : IDisposable
    {
        private static Dictionary<IPAddress, SMBConnection> instances = new Dictionary<IPAddress, SMBConnection>();

        private readonly IPAddress _address;
        private readonly SMBTransportType _transport;
        private readonly ISMBCredential _credential;
        private long _referenceCount { get; set; }
        private bool _isDesposed { get; set; }

        public readonly ISMBClient SMBClient;

        private SMBConnection(ISMBClientFactory smbClientFactory, IPAddress address, SMBTransportType transport, ISMBCredential credential)
        {
            SMBClient = smbClientFactory.CreateClient();
            _address = address;
            _transport = transport;
            _credential = credential;
            _referenceCount = 1;
        }

        private void Connect()
        {
            var succeded = SMBClient.Connect(_address, _transport);
            if(!succeded)
            {
                throw new IOException($"Unable to connect to SMB share.");
            }
            var status = SMBClient.Login(_credential.Domain, _credential.UserName, _credential.Password);
            if(status != NTStatus.STATUS_SUCCESS)
            {
                throw new IOException($"Unable to login to SMB share. Status was: {status}");
            }
        }

        public static SMBConnection CreateSMBConnection(ISMBClientFactory smbClientFactory, IPAddress address, SMBTransportType transport, ISMBCredential credential)
        {
            if(instances.ContainsKey(address))
            {
                var instance = instances[address];
                instance._referenceCount += 1;
                return instance;
            }
            else
            {
                var instance = new SMBConnection(smbClientFactory, address, transport, credential);
                instance.Connect();
                instances.Add(address, instance);
                return instance;
            }
        }

        public void Dispose()
        {
            if (_isDesposed)
            {
                return;
            }

            if (_referenceCount == 1)
            {
                try
                {
                    SMBClient.Logoff(); //Once you logout OR disconnect you can't log back in for some reason. TODO come back to this and try to debug more.
                    SMBClient.Disconnect();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    instances.Remove(_address);
                    _isDesposed = true;
                }
            }
            else
            {
                _referenceCount -= 1;
            }
        }
    }
}
