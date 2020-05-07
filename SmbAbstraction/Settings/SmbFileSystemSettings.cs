using System;
using System.Collections.Generic;
using System.Text;

namespace SmbAbstraction
{
    public class SmbFileSystemSettings : ISmbFileSystemSettings
    {
        public SmbFileSystemSettings()
        {
            ClientSessionTimeout = 45;
        }

        /// <summary>
        /// Timeout (in seconds) for client to wait before determining the connection to the share is lost. Default: 45
        /// </summary>
        public int ClientSessionTimeout { get; set; }
    }
}
