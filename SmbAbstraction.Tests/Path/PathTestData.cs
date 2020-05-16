namespace SmbAbstraction.Tests.Path
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
        public string DirectoryAtRootWithTrailingSlash { get { return "smb://host/share/dir"; } }
        public string SpaceInDirectoryAtRoot { get { return "smb://host/share/dir dir/file.txt"; } }
        public string FileAtRoot { get { return "smb://host/share/file.txt"; } }
        public string SpaceInFileAtRoot { get { return "smb://host/share/text file.txt"; } }
        public string NestedDirectoryAtRoot {  get { return "smb://host/share/dir/nested_dir"; } }
        public string NestedDirectoryAtRootWithTrailingSlash { get { return "smb://host/share/dir/nested_dir/"; } }
        public string FileInNestedDirectoryAtRoot { get { return "smb://host/share/dir/nested_dir/file.txt"; } }
        public string SpaceInFileInNestedDirectoryAtRoot { get { return "smb://host/share/dir/nested_dir/text file.txt"; } }
        public string SpaceInFileAndInNestedDirectoryAtRoot { get { return "smb://host/share/dir/nested dir/text file.txt"; } }

    }

    public class UncPathTestData : IPathTestData
    {
        public string Root { get { return $@"\\host\share"; } }
        public string DirectoryAtRoot { get { return $@"\\host\share\dir"; } }
        public string DirectoryAtRootWithTrailingSlash { get { return $@"\\host\share\dir\"; } }
        public string SpaceInDirectoryAtRoot { get { return @"\\host\share\dir dir\file.txt"; } }
        public string FileAtRoot { get { return $@"\\host\share\file.txt"; } }
        public string SpaceInFileAtRoot { get { return @"\\host\share\text file.txt"; } }
        public string NestedDirectoryAtRoot { get { return $@"\\host\share\dir\nested_dir"; } }
        public string NestedDirectoryAtRootWithTrailingSlash { get { return $@"\\host\share\dir\nested_dir\"; } }
        public string SpaceInNestedDirectoryAtRoot { get { return $@"\\host\share\dir\nested dir"; } }
        public string SpaceInNestedDirectoryAtRootWithTrailingSlash { get { return $@"\\host\share\dir\nested dir\"; } }
        public string FileInNestedDirectoryAtRoot { get { return $@"\\host\share\dir\nested_dir\file.text"; } }
        public string SpaceInFileInNestedDirectoryAtRoot { get { return $@"\\host\share\dir\nested_dir\text file.text"; } }
        public string SpaceInFileAndInNestedDirectoryAtRoot { get { return $@"\\host\share\dir\nested dir\text file.text"; } }
    }
}
