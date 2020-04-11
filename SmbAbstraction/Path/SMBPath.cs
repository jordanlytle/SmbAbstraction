using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
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
            if (!path1.IsSharePath())
            {
                return base.Combine(path1, path2);
            }

            if (path1.IsSmbUri())
            {
                return $"{path1}/{path2}";
            }

            if (path1.IsUncPath())
            {
                return $@"{path1}\{path2}";
            }

            throw new InvalidOperationException();
        }

        public override string Combine(string path1, string path2, string path3)
        {
            if (!path1.IsSharePath())
            {
                return base.Combine(path1, path2, path3);
            }

            if (path1.IsSmbUri())
            {
                return $"{path1}/{path2}/{path3}";
            }

            if (path1.IsUncPath())
            {
                return $@"{path1}\{path2}\{path3}";
            }

            throw new InvalidOperationException();
        }

        public override string Combine(string path1, string path2, string path3, string path4)
        {
            if (!path1.IsSharePath())
            {
                return base.Combine(path1, path2, path3, path4);
            }

            if (path1.IsSmbUri())
            {
                return $"{path1}/{path2}/{path3}/{path4}";
            }

            if (path1.IsUncPath())
            {
                return $@"{path1}\{path2}\{path3}\{path4}";
            }

            throw new InvalidOperationException();
        }

        public override string GetDirectoryName(string path)
        {
            if(!path.IsSharePath())
            {
                return base.GetDirectoryName(path);
            }

            var relativePath = path.RelativeSharePath();
            string directoryName = "";

            if (path == null || string.IsNullOrEmpty(relativePath))
            {
                return directoryName;
            }

            var segments = relativePath.Split(@"\");
            if (HasExtension(segments.Last()))
            {
                directoryName = string.Join('\\',segments.Take(segments.Length - 1));
            }
            else
            {
                directoryName = relativePath;
            }
            

            return directoryName;
        }

        public override string GetExtension(string path)
        {
            return base.GetExtension(path);
        }

        public override string GetFileName(string path)
        {
            if (!path.IsSharePath())
            {
                return base.GetFileName(path);
            }

            var relativePath = path.RelativeSharePath();
            string fileName = "";

            if (path == null || string.IsNullOrEmpty(relativePath))
            { 
                return fileName;
            }

            fileName = relativePath.Split(@"\").Last();

            return fileName;
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
            if (!path.IsSharePath())
            {
                return base.GetPathRoot(path);
            }
            
            var pathRoot = path.SharePath();
            return pathRoot;               
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
