using System;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Abstractions.SMB
{
    public static class PathExtensions
    {
        static readonly string[] slashieThings = { @"\", @"/", @"\\", @"//" };
        static readonly string[] pathSeperators = { @"\", @"/" };

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

        public static string GetHostName(this string path)
        {
            var uri = new Uri(path);
            return uri.Host;
        }

        public static string GetShareName(this string path)
        {
            var uri = new Uri(path);
            var shareName = uri.Segments[1].RemoveAnySlashes();

            return shareName;
        }

        public static string GetSharePath(this string path)
        {
            var uri = new Uri(path);
            var sharePath = uri.Scheme + "://" + uri.Host + "/" +  uri.Segments[1].RemoveAnySlashes();
            
            if(!path.IsSmbPath())
            {
                sharePath = new Uri(sharePath).LocalPath;
            }

            return sharePath;
        }

        public static string GetRelativeSharePath(this string path)
        {
            var sharePath = path.GetSharePath();

            var relativePath = path.Replace(sharePath, "").RemoveAnySlashes();

            return relativePath;
        }

        private static string RemoveAnySlashes(this string input)
        {
            foreach(var slash in slashieThings)
            {
                input = input.Replace(slash, "");
            }

            return input;
        }

        private static string ReplacePathSeperators(this string input, string newValue)
        {
            foreach(var pathSeperator in pathSeperators)
            {
                input = input.Replace(pathSeperator, newValue);
            }

            return input;
        }
    }
}
