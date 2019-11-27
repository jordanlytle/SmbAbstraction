using SmbLibraryStd;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;

namespace System.IO.Abstractions.SMB
{
    public static class NTStatusExtensions
    {
        public static void HandleStatus(this NTStatus status)
        {
            switch (status)
            {
                case (NTStatus.STATUS_NOT_SUPPORTED):
                    throw new NotSupportedException();
                case (NTStatus.STATUS_NOT_IMPLEMENTED):
                    throw new NotImplementedException();
                case (NTStatus.STATUS_INVALID_HANDLE):
                    throw new ArgumentException("Invalid Handle.");
                case (NTStatus.STATUS_INVALID_INFO_CLASS):
                    throw new ArgumentException("Invalid Information Class.");
                case (NTStatus.STATUS_INVALID_PARAMETER):
                    throw new ArgumentException("Invalid Parameter.");
                case (NTStatus.STATUS_NO_SUCH_FILE):
                    throw new FileNotFoundException();
                case (NTStatus.STATUS_CANNOT_DELETE):
                    throw new IOException("Cannot delete.");
                case (NTStatus.STATUS_DIRECTORY_NOT_EMPTY):
                    throw new IOException("The directory trying to be deleted is not empty.");
                case (NTStatus.STATUS_OBJECT_PATH_INVALID):
                case (NTStatus.STATUS_OBJECT_PATH_SYNTAX_BAD):
                    throw new IOException("The path is invalid.");
                case (NTStatus.STATUS_OBJECT_PATH_NOT_FOUND):
                    throw new IOException("The path is not found.");
                case (NTStatus.STATUS_INVALID_SMB):
                case (NTStatus.STATUS_INVALID_DEVICE_REQUEST):
                case (NTStatus.STATUS_NO_SUCH_DEVICE):
                    throw new DriveNotFoundException();
                case (NTStatus.STATUS_BAD_NETWORK_NAME):
                    throw new Exception("The network name cannot be found.");
                case (NTStatus.STATUS_NETWORK_NAME_DELETED):
                    throw new DriveNotFoundException("Network name has been deleted");
                case (NTStatus.STATUS_FILE_IS_A_DIRECTORY):
                    throw new IOException("The file is a directory.");
                case (NTStatus.STATUS_END_OF_FILE):
                    throw new IOException("End of file");
                case (NTStatus.STATUS_DISK_FULL):
                    throw new IOException("Disk is full.");
                case (NTStatus.STATUS_ACCESS_DENIED):
                case (NTStatus.STATUS_INVALID_LOGON_HOURS):
                case (NTStatus.STATUS_INVALID_WORKSTATION):
                case (NTStatus.STATUS_LOGON_TYPE_NOT_GRANTED):
                case (NTStatus.STATUS_OS2_INVALID_ACCESS):
                    throw new UnauthorizedAccessException();
                case (NTStatus.STATUS_ACCOUNT_EXPIRED):
                case (NTStatus.STATUS_ACCOUNT_DISABLED):
                case (NTStatus.STATUS_ACCOUNT_LOCKED_OUT):
                case (NTStatus.STATUS_ACCOUNT_RESTRICTION):
                case (NTStatus.STATUS_LOGON_FAILURE):
                case (NTStatus.STATUS_PASSWORD_MUST_CHANGE):
                case (NTStatus.STATUS_PASSWORD_EXPIRED):
                case (NTStatus.SEC_E_INVALID_TOKEN):
                    throw new AuthenticationException();
                case (NTStatus.STATUS_BUFFER_OVERFLOW):
                    throw new InternalBufferOverflowException();
                case (NTStatus.STATUS_SUCCESS):
                case (NTStatus.STATUS_PENDING):
                case (NTStatus.STATUS_DELETE_PENDING):
                case (NTStatus.STATUS_CANCELLED):
                    break;
                default:
                    break;
            }
        }
    }
}
