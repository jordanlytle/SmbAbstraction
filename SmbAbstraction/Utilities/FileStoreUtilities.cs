using SMBLibrary.Client;

namespace SmbAbstraction.Utilities
{
    internal static class FileStoreUtilities
    {
        public static void CloseFile(ISMBFileStore fileStore, ref object fileHandle)
        {
            if (fileStore == null || fileHandle == null)
                return;

            fileStore.CloseFile(fileHandle);
            fileHandle = null;
        }
    }
}
