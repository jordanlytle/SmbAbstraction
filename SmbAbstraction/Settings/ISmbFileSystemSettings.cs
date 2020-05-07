using System;
using System.Collections.Generic;
using System.Text;

namespace SmbAbstraction
{
    public interface ISmbFileSystemSettings
    {
        /// <summary>
        /// Timeout (in seconds) for client to wait before determining the connection to the share is lost. Default: 45
        /// </summary>
        int ClientSessionTimeout { get; set; }
    }
}
