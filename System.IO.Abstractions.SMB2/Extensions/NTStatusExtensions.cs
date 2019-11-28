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
                //ERRDOS Class 0x01
                case (NTStatus.STATUS_NOT_IMPLEMENTED):
                    throw new NotImplementedException($"{status.ToString()}: {ERRBadFunc}");
                case (NTStatus.STATUS_INVALID_DEVICE_REQUEST):
                    throw new InvalidOperationException($"{status.ToString()}: {ERRBadFunc}");
                case (NTStatus.STATUS_NO_SUCH_FILE):
                case (NTStatus.STATUS_NO_SUCH_DEVICE):
                case (NTStatus.STATUS_OBJECT_NAME_NOT_FOUND):
                    throw new FileNotFoundException($"{status.ToString()}: {ERRBadFile}");
                case (NTStatus.STATUS_OBJECT_PATH_INVALID):
                case (NTStatus.STATUS_OBJECT_PATH_NOT_FOUND):
                case (NTStatus.STATUS_OBJECT_PATH_SYNTAX_BAD):
                    throw new DirectoryNotFoundException($"{status.ToString()}: {ERRBadPath}");
                case (NTStatus.STATUS_TOO_MANY_OPENED_FILES):
                    throw new FileNotFoundException($"{status.ToString()}: {ERRNoFids}");
                case (NTStatus.STATUS_ACCESS_DENIED):
                case (NTStatus.STATUS_DELETE_PENDING):
                case (NTStatus.STATUS_PRIVILEGE_NOT_HELD):
                case (NTStatus.STATUS_LOGON_FAILURE):
                case (NTStatus.STATUS_FILE_IS_A_DIRECTORY):
                case (NTStatus.STATUS_CANNOT_DELETE):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {ERRNoAccess}");
                case (NTStatus.STATUS_SMB_BAD_FID):
                case (NTStatus.STATUS_INVALID_HANDLE):
                case (NTStatus.STATUS_FILE_CLOSED):
                    throw new ArgumentException($"{status.ToString()}: {ERRBadFid}");
                case (NTStatus.STATUS_INSUFF_SERVER_RESOURCES):
                    throw new OutOfMemoryException($"{status.ToString()}:{ERRNoMem}");
                case (NTStatus.STATUS_OS2_INVALID_ACCESS):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {ERRBadAccess}");
                case (NTStatus.STATUS_DATA_ERROR):
                    throw new InvalidDataException($"{status.ToString()}: {ERRBadData}");
                case (NTStatus.STATUS_DIRECTORY_NOT_EMPTY):
                    throw new IOException($"{status.ToString()}: {ERRRemCd}");
                case (NTStatus.STATUS_NO_MORE_FILES):
                    throw new IOException($"{status.ToString()}: {ERRNoFiles}");
                case (NTStatus.STATUS_END_OF_FILE):
                    throw new IOException($"{status.ToString()}: {ERREof}");
                case (NTStatus.STATUS_NOT_SUPPORTED):
                    throw new NotSupportedException($"{status.ToString()}: {ERRUnsup}");
                case (NTStatus.STATUS_OBJECT_NAME_COLLISION):
                    throw new IOException($"{status.ToString()}: {ERRFileExists}");
                case (NTStatus.STATUS_INVALID_PARAMETER):
                    throw new ArgumentException($"{status.ToString()}: {ERRInvalidParam}");
                case (NTStatus.STATUS_OS2_INVALID_LEVEL):
                    throw new UnsupportedInformationLevelException($"{status.ToString()}: {ERRUnknownLevel}");
                case (NTStatus.STATUS_RANGE_NOT_LOCKED):
                    throw new AccessViolationException($"{status.ToString()}: {ERROR_NOT_LOCKED}");
                case (NTStatus.STATUS_OS2_NO_MORE_SIDS):
                    throw new InvalidOperationException($"{status.ToString()}: {ERROR_NO_MORE_SEARCH_HANDLES}");
                case (NTStatus.STATUS_INVALID_INFO_CLASS):
                    throw new UnsupportedInformationLevelException($"{status.ToString()}: {ERRBadPipe}");
                case (NTStatus.STATUS_BUFFER_OVERFLOW):
                case (NTStatus.STATUS_MORE_PROCESSING_REQUIRED):
                    throw new InvalidOperationException($"{status.ToString()}: {ERRMoreData}");
                case (NTStatus.STATUS_NOTIFY_ENUM_DIR):
                    throw new AccessViolationException($"{status.ToString()}: {ERR_NOTIFY_ENUM_DIR}");

                //ERRSRV Class 0x02
                case (NTStatus.STATUS_INVALID_SMB):
                    throw new ArgumentException($"{status.ToString()}: {ERRInvSmb}"); //Is there a better exception for this?
                case (NTStatus.STATUS_NETWORK_NAME_DELETED):
                case (NTStatus.STATUS_SMB_BAD_TID):
                    throw new ArgumentException($"{status.ToString()}: {ERRInvTid}");
                case (NTStatus.STATUS_BAD_NETWORK_NAME):
                    throw new ArgumentException($"{status.ToString()}: {ERRInvNetName}");
                case (NTStatus.STATUS_SMB_BAD_COMMAND):
                    throw new NotImplementedException($"{status.ToString()}: {ERRBadCmd}");
                case (NTStatus.STATUS_TOO_MANY_SESSIONS):
                    throw new ApplicationException($"{status.ToString()}: {ERRTooManyUids}");
                case (NTStatus.STATUS_ACCOUNT_DISABLED):
                case (NTStatus.STATUS_ACCOUNT_EXPIRED):
                    throw new AuthenticationException($"{status.ToString()}: {ERRAccountExpired}");
                case (NTStatus.STATUS_INVALID_WORKSTATION):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {ERRBadClient}");
                case (NTStatus.STATUS_INVALID_LOGON_HOURS):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {ERRBadLogonTime}");
                case (NTStatus.STATUS_PASSWORD_EXPIRED):
                case (NTStatus.STATUS_PASSWORD_MUST_CHANGE):
                    throw new InvalidCredentialException($"{status.ToString()}: {ERRPasswordExpired}");

                //ERRHRD Class 0x03
                case (NTStatus.STATUS_MEDIA_WRITE_PROTECTED):
                    throw new AccessViolationException($"{status.ToString()}: {ERRNoWrite}");
                case (NTStatus.STATUS_SHARING_VIOLATION):
                    throw new InvalidOperationException($"{status.ToString()}: {ERRBadShare}");
                case (NTStatus.STATUS_FILE_LOCK_CONFLICT):
                    throw new InvalidOperationException($"{status.ToString()}: {ERRLock}");

                //Others

                case (NTStatus.STATUS_DISK_FULL):
                    throw new IOException($"{ status.ToString() }: Disk is full.");
                case (NTStatus.STATUS_LOGON_TYPE_NOT_GRANTED):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {NTStatus_STATUS_LOGON_TYPE_NOT_GRANTED}"); 
                case (NTStatus.STATUS_ACCOUNT_LOCKED_OUT):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {NTStatus_STATUS_ACCOUNT_LOCKED_OUT}");
                case (NTStatus.STATUS_ACCOUNT_RESTRICTION):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {NTStatus_STATUS_ACCOUNT_RESTRICTION}");
                case (NTStatus.SEC_E_INVALID_TOKEN):
                    throw new UnauthorizedAccessException($"{status.ToString()}");
                case (NTStatus.SEC_E_SECPKG_NOT_FOUND):
                    throw new InvalidCredentialException($"{status.ToString()}");
                case (NTStatus.STATUS_OBJECT_NAME_INVALID):
                    throw new MemberAccessException($"{status.ToString()}: {NTStatus_STATUS_OBJECT_NAME_INVALID}");
                case (NTStatus.STATUS_OBJECT_NAME_EXISTS):
                    throw new InvalidOperationException($"{status.ToString()}: {NTStatus_STATUS_OBJECT_NAME_EXISTS}");
                case (NTStatus.STATUS_LOCK_NOT_GRANTED):
                    throw new IOException($"{status.ToString()}: {NTStatus_STATUS_LOCK_NOT_GRANTED}");
                case (NTStatus.STATUS_BUFFER_TOO_SMALL):
                    throw new ArgumentException($"{status.ToString()}: {NTStatus_STATUS_BUFFER_TOO_SMALL}");
                case (NTStatus.STATUS_BAD_DEVICE_TYPE):
                    throw new InvalidOperationException($"{status.ToString()}: {NTStatus_STATUS_BAD_DEVICE_TYPE}");
                case (NTStatus.STATUS_FS_DRIVER_REQUIRED):
                    throw new FileLoadException($"{status.ToString()}: {NTStatus_STATUS_FS_DRIVER_REQUIRED}");
                case (NTStatus.STATUS_USER_SESSION_DELETED):
                    throw new UnauthorizedAccessException($"{status.ToString()}: {NTStatus_STATUS_USER_SESSION_DELETED}");
                case (NTStatus.SEC_I_CONTINUE_NEEDED):
                    throw new InvalidOperationException($"{status.ToString()}");
                case (NTStatus.STATUS_CANCELLED):
                    throw new IOException($"{status.ToString()}: {NTStatus_STATUS_CANCELLED}");
                case (NTStatus.STATUS_PENDING):
                    throw new InvalidOperationException($"{status.ToString()}: {NTStatus_STATUS_PENDING}");
                case (NTStatus.STATUS_NOTIFY_CLEANUP): //Indicates that a notify change request has been completed due to closing the handle that made the notify change request.
                case (NTStatus.STATUS_SUCCESS):
                    break;
                default:
                    break;
            }
        }


        //https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-smb/6ab6ca20-b404-41fd-b91a-2ed39e3762ea
        //https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-cifs/8f11e0f3-d545-46cc-97e6-f00569e3e1bc
        //Status codes and messages
        //Format:
        // "{Error code} - {POSIX code (if applicable) - {Description}"

        //ERRDOS Class 0x01
        private const string ERRBadFunc = "ERRbadfunc(0x0001) - EINVAL - Invalid Function";
        private const string ERRBadFile = "ERRbadFile(0x0002) - EOENT - File Not Found";
        private const string ERRBadPath = "ERRbadpath(0x0003) - ENOENT - A component in the path prefix is not a directory";
        private const string ERRNoFids = "ERRnofids(0x0004) - EMFILE - Too many open files. No FIDs are available";
        private const string ERRNoAccess = "ERRnoaccess(0x0005) - EPERM - Access denied";
        private const string ERRBadFid = "ERRbadfid(0x0006) - EBADF - Invalid FID";
        private const string ERRNoMem = "ERRnomem(0x0008) - ENOMEM - Insufficient server memory to perform the requested operation";
        private const string ERRBadAccess = "ERRbadaccess(0x000C) - Invalid open mode";
        private const string ERRBadData = "ERRbaddata(0x000D) - E2BIG - Bad data (May be generated by IOCTL calls on the server.)";
        private const string ERRRemCd = "ERRremcd(0x0010) - Remove of directory failed because it was not empty";
        private const string ERRNoFiles = "ERRnofiles(0x0012) - No (more) files found following a file search command";
        private const string ERREof = "ERReof(0x0026) - EEOF - Attempted to read beyond the end of the file";
        private const string ERRUnsup = "ERRunsup(0x0032) - This command is not supported by the server";
        private const string ERRFileExists = "ERRfilexists(0x0050) - EEXIST - An attempt to create a file or directory failed because an object with the same pathname already exists";
        private const string ERRInvalidParam = "ERRinvalidparam(0x0057) - A parameter supplied with the message is invalid";
        private const string ERRUnknownLevel = "ERRunknownlevel(0x007C) - Invalid information level";
        private const string ERROR_NOT_LOCKED = "ERROR_NOT_LOCKED(0x009E) - The byte range specified in an unlock request was not locked";
        private const string ERROR_NO_MORE_SEARCH_HANDLES = "ERROR_NO_MORE_SEARCH_HANDLES(0x0071) - Maximum number of searches has been exhausted.";
        private const string ERRBadPipe = "ERRbadpipe(0x00E6) - Invalid named pipe";
        private const string ERRMoreData = "ERRmoredata(0x00EA) - There is more data available to read on the designated named pipe. {Still Busy} The specified I/O request packet (IRP) cannot be disposed of because the I/O operation is not complete.";
        private const string ERR_NOTIFY_ENUM_DIR = "ERR_NOTIFY_ENUM_DIR(0x03FE) - More changes have occurred within the directory than will fit within the specified Change Notify response buffer";

        //ERRSRV Class 0x02
        private const string ERRInvSmb = "ERRError(0x0001) - An invalid SMB client request is received by the server";
        private const string ERRInvTid = "ERRinvtid(0x0005) - The client request received by the server contains an invalid TID value";
        private const string ERRInvNetName = "ERRinvnetname(0x0006) - Invalid server name in Tree Connect";
        private const string ERRBadCmd = "ERRbadcmd(0x0016) - An unknown SMB command code was received by the server";
        private const string ERRTooManyUids = "ERRtoomanyuids(0x005A) - Too many UIDs active for this SMB connection";
        private const string ERRAccountExpired = "ERRaccountExpired(0x08BF) - User account on the target machine is disabled or has expired";
        private const string ERRBadClient = "ERRbadClient(0x08C0) - The client does not have permission to access this server";
        private const string ERRBadLogonTime = "ERRbadLogonTime(0x08C0) - Access to the server is not permitted at this time";
        private const string ERRPasswordExpired = "ERRpasswordExpired(0x08C2) - The user's password has expired";

        //ERRHRD Class 0x03
        private const string ERRNoWrite = "ERRnowrite(0x0013) - EROFS - Attempt to modify a read-only file system";
        private const string ERRBadShare = "ERRbadshare(0x0020) - ETXTBSY - An attempted open operation conflicts with an existing open";
        private const string ERRLock = "ERRlock(0x0021) - EDEADLOCK - A lock request specified an invalid locking mode, or conflicted with an existing file lock";

        //Regular NTStatus Values 
        //https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55

        private const string NTStatus_STATUS_OBJECT_NAME_INVALID = "The object name is invalid. (0xC0000033)";
        private const string NTStatus_STATUS_ACCOUNT_RESTRICTION = "Indicates a referenced user name and authentication information are valid, but some user account restriction has prevented successful authentication (such as time-of-day restrictions). (0xC000006E)";
        private const string NTStatus_STATUS_ACCOUNT_LOCKED_OUT = "The user account has been automatically locked because too many invalid logon attempts or password change attempts have been requested. (0xC0000234)";
        private const string NTStatus_STATUS_OBJECT_NAME_EXISTS = "{Object Exists} An attempt was made to create an object but the object name already exists.(0x40000000)";
        private const string NTStatus_STATUS_LOGON_TYPE_NOT_GRANTED = "A user has requested a type of logon (for example, interactive or network) that has not been granted. An administrator has control over who can logon interactively and through the network. (0xC000015B)";
        private const string NTStatus_STATUS_LOCK_NOT_GRANTED = "A requested file lock cannot be granted due to other existing locks. (0xC0000055)";
        private const string NTStatus_STATUS_BUFFER_TOO_SMALL = "{Buffer Too Small} The buffer is too small to contain the entry. No information has been written to the buffer. (0xC0000023)";
        private const string NTStatus_STATUS_BAD_DEVICE_TYPE = "{Incorrect Network Resource Type} The specified device type (LPT, for example) conflicts with the actual device type on the remote resource. (0xC00000CB)";
        private const string NTStatus_STATUS_FS_DRIVER_REQUIRED = "A volume has been accessed for which a file system driver is required that has not yet been loaded. (0xC000019C)";
        private const string NTStatus_STATUS_USER_SESSION_DELETED = "The remote user session has been deleted. (0xC0000203)";
        private const string NTStatus_STATUS_CANCELLED = "The I/O request was canceled. (0xC0000120)";
        private const string NTStatus_STATUS_PENDING = "The operation that was requested is pending completion. (0x00000103)";
    }
}
