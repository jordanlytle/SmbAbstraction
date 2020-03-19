using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SmbAbstraction
{
    public static class PathExtensions
    {
        static readonly string[] pathSeperators = { @"\", "/" };

        public static bool IsValidSharePath(this string path)
        {
            var uri = new Uri(path);
            var valid = uri.Segments.Length >= 2;

            return valid && path.IsSharePath();
        }

        public static bool IsSharePath(this string path)
        {
            try
            {
                var uri = new Uri(path);
                return uri.Scheme.Equals("smb") || uri.IsUnc;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSmbUri(this string path)
        {
            try
            {
                var uri = new Uri(path);
                return uri.Scheme.Equals("smb");
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUncPath(this string path)
        {
            try
            {
                var uri = new Uri(path);
                return uri.IsUnc;
            }
            catch
            {
                return false;
            }
        }

        public static string BuildSharePath(this string path, string shareName)
        {
            var uri = new Uri(path);
            if (!uri.IsUnc)
            {
                return $@"smb://{path.Hostname()}/{shareName}";
            }
            else
            {
                return $@"\\{path.Hostname()}\{shareName}";
            }
        }

        public static string Hostname(this string path)
        {
            try 
            {
                var uri = new Uri(path);
                return uri.Host;
            }
            catch(Exception ex)
            {
                throw new Exception($"Unable to parse hostname for path: {path}", ex);
            }
        }

        public static bool TryResolveHostnameFromPath(this string path, out IPAddress ipAddress)
        {
            return path.Hostname().TryResolveHostname(out ipAddress);
        }

        public static bool TryResolveHostname(this string hostnameOrAddress, out IPAddress ipAddress)
        {
            var parsedIPAddress = IPAddress.TryParse(hostnameOrAddress, out ipAddress);

            if (parsedIPAddress)
            {
                return true;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(hostnameOrAddress);
                ipAddress = hostEntry.AddressList.First(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                return true;
            }
            catch
            {
                ipAddress = IPAddress.None;
                return false;
            }
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

        public static string GetParentPath(this string path)
        {
            var pathUri = new Uri(path);
            var parentUri = pathUri.AbsoluteUri.EndsWith('/') ? new Uri(pathUri, "..") : new Uri(pathUri, ".");
            var pathString = parentUri.IsUnc ? parentUri.LocalPath : parentUri.AbsoluteUri;
            return pathString;
        }

        public static string GetLastPathSegment(this string path)
        {
            var uri = new Uri(path);
            return uri.Segments.Last().Replace("%20", " ");
        }

        public static string RemoveAnySeperators(this string input)
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
                if (input.StartsWith(pathSeperator))
                {
                    input = input.Remove(0, 1);
                }
            }

            return input;
        }
    }
}
