using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Abstractions.SMB
{
    public static class PathExtensions
    {
        static readonly string[] pathSeperators = { @"\", "/" };

        public static bool IsValidSharePath(this string path)
        {
            var uri = new Uri(path);
            var valid = uri.Segments.Length > 2;

            return valid;
        }

        public static bool IsSmbPath(this string path)
        {
            var uri = new Uri(path);
            return uri.Scheme.Equals("smb") || uri.IsUnc;
        }

        public static string HostName(this string path)
        {
            var uri = new Uri(path);
            return uri.Host;
        }

        public static string ShareName(this string path)
        {
            var uri = new Uri(path);
            var shareName = uri.Segments[1].RemoveAnySeperators();

            return shareName;
        }

        public static string SharePath(this string path)
        {
            var uri = new Uri(path);

            string sharePath = "";
            if (uri.Scheme.Equals("smb"))
                sharePath = $"{uri.Scheme}://{uri.Host}/{uri.Segments[1].RemoveAnySeperators()}";
            else if (uri.IsUnc)
                sharePath = $@"\\{uri.Host}\{uri.Segments[1].RemoveAnySeperators()}";

            return sharePath;
        }

        public static string RelativeSharePath(this string path)
        {
            var relativePath = path.RemoveShareNameFromPath().RemoveLeadingSeperators().Replace("/", @"\");

            return relativePath;
        }

        private static string RemoveAnySeperators(this string input)
        {
            foreach (var pathSeperator in pathSeperators)
            {
                input = input.Replace(pathSeperator, "");
            }

            return input;
        }

        private static string RemoveShareNameFromPath(this string input)
        {
            var sharePath = input.SharePath();

            input = input.Replace(sharePath, "", StringComparison.InvariantCultureIgnoreCase);

            return input;
        }

        private static string RemoveLeadingSeperators(this string input)
        {
            foreach (var pathSeperator in pathSeperators)
            {
                if(input.StartsWith(pathSeperator))
                {
                    input = input.Remove(0,1);
                }
            }

            return input;
        }
    }
}
