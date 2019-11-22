using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Abstractions.SMB.Tests.Path
{
    public interface IPathTestData
    {
        public string Root { get; }
        public string DirectoryAtRoot { get; }
        public string FileAtRoot { get; }
        public string NestedDirectoryAtRoot { get; }
        public string FileInNestedDirectoryAtRoot { get; }
        
    }

    public class SmbUriTestData : IPathTestData
    {
        public string Root { get { return "smb://host/share"; } }
        public string DirectoryAtRoot { get { return "smb://host/share/dir"; } }
        public string FileAtRoot { get { return "smb://host/share/file.txt"; } }
        public string NestedDirectoryAtRoot {  get { return "smb://host/share/dir/nested_dir"; } }
        public string FileInNestedDirectoryAtRoot { get { return "smb://host/share/dir/nested_dir/file.txt"; } }
    }

    public class UncPathTestData : IPathTestData
    {
        private char s = System.IO.Path.DirectorySeparatorChar;

        public string Root { get { return $@"{s}{s}host{s}share"; } }
        public string DirectoryAtRoot { get { return $@"{s}{s}host{s}share{s}dir"; } }
        public string FileAtRoot { get { return $@"{s}{s}host{s}share{s}file.txt"; } }
        public string NestedDirectoryAtRoot { get { return $@"{s}{s}host{s}share{s}dir{s}nested_dir"; } }
        public string FileInNestedDirectoryAtRoot { get { return $@"{s}{s}host{s}share{s}dir{s}nested_dir{s}file.text"; } }
    }
}
