#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1402

namespace LbxyCommonLib.FolderLauncher
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class DirectoryLauncher
    {
        public static void Open(string path)
        {
            ValidatePath(path);

            if (!HasReadAccess(path))
            {
                throw new UnauthorizedAccessException("Insufficient permissions to open directory: " + path);
            }

            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open",
            };

            using (var proc = Process.Start(psi))
            {
            }
        }

        public static bool TryOpen(string path, out string errorMessage)
        {
            try
            {
                Open(path);
                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public static void ValidatePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("Path must not be empty.", nameof(path));
            }

            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException("Path must be an absolute path.", nameof(path));
            }

            try
            {
                var normalized = Path.GetFullPath(path);
                if (normalized.Length == 0)
                {
                    throw new ArgumentException("Normalized path is empty.", nameof(path));
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid path format: " + ex.Message, nameof(path));
            }

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException("Directory not found: " + path);
            }
        }

        public static bool HasReadAccess(string path)
        {
            try
            {
                foreach (var entry in Directory.EnumerateFileSystemEntries(path))
                {
                    break;
                }

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1649
#pragma warning restore SA1633
#pragma warning restore SA1602
#pragma warning restore SA1600
#pragma warning restore SA1204
#pragma warning restore SA1101
#pragma warning restore CS1591
