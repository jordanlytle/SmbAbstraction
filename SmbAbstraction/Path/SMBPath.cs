using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;

namespace SmbAbstraction
{
    public class SMBPath : PathWrapper, IPath
    {
        public SMBPath(IFileSystem fileSystem): base(new FileSystem())
        {
        }

        public override char AltDirectorySeparatorChar
        {
            get { return base.AltDirectorySeparatorChar; }
        }

        public override char DirectorySeparatorChar
        {
            get { return base.DirectorySeparatorChar; }
        }

        [Obsolete("Please use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
        public override char[] InvalidPathChars
        {
            get { return base.InvalidPathChars; }
        }

        public override char PathSeparator
        {
            get { return base.PathSeparator; }
        }

        public override char VolumeSeparatorChar
        {
            get { return base.VolumeSeparatorChar; }
        }

        public override string ChangeExtension(string path, string extension)
        {
            return base.ChangeExtension(path, extension);
        }

        public override string Combine(params string[] paths)
        {
            return base.Combine(paths);
        }

        public override string Combine(string path1, string path2)
        {
            return base.Combine(path1, path2);
        }

        public override string Combine(string path1, string path2, string path3)
        {
            return base.Combine(path1, path2, path3);
        }

        public override string Combine(string path1, string path2, string path3, string path4)
        {
            return base.Combine(path1, path2, path3, path4);
        }

        public override string GetDirectoryName(string path)
        {
            return base.GetDirectoryName(path);
        }

        public override string GetExtension(string path)
        {
            return base.GetExtension(path);
        }

        public override string GetFileName(string path)
        {
            return base.GetFileName(path);
        }

        public override string GetFileNameWithoutExtension(string path)
        {
            return base.GetFileNameWithoutExtension(path);
        }

        public override string GetFullPath(string path)
        {
            return base.GetFullPath(path);
        }

        public override char[] GetInvalidFileNameChars()
        {
            return base.GetInvalidFileNameChars();
        }

        public override char[] GetInvalidPathChars()
        {
            return base.GetInvalidPathChars();
        }

        public override string GetPathRoot(string path)
        {
            return base.GetPathRoot(path);
        }

        public override string GetRandomFileName()
        {
            return base.GetRandomFileName();
        }

        public override string GetTempFileName()
        {
            return base.GetTempFileName();
        }

        public override string GetTempPath()
        {
            return base.GetTempPath();
        }

        public override bool HasExtension(string path)
        {
            return base.HasExtension(path);
        }

#if FEATURE_ADVANCED_PATH_OPERATIONS
        public override bool IsPathFullyQualified(string path)
        {
            return base.IsPathFullyQualified(path);
        }

        public override string GetRelativePath(string relativeTo, string path)
        {
            return base.GetRelativePath(relativeTo, path);
        }
#endif

        public override bool IsPathRooted(string path)
        {
            return base.IsPathRooted(path);
        }
    }
}
