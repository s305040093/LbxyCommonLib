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

    /// <summary>
    /// 提供在当前操作系统中打开本地目录的辅助方法。
    /// </summary>
    /// <remarks>
    /// Author: LbxyCommonLib Contributors
    /// Created: 2026-02-22
    /// Last Modified: 2026-02-22
    /// </remarks>
    public static class DirectoryLauncher
    {
        /// <summary>
        /// 使用系统默认文件管理器打开指定目录。
        /// </summary>
        /// <param name="path">要打开的目录路径，必须是非空的绝对路径。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="path"/> 为空字符串、不是绝对路径或路径格式无效时抛出。
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">当指定目录不存在时抛出。</exception>
        /// <exception cref="UnauthorizedAccessException">当当前进程对目标目录没有读取权限时抛出。</exception>
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// 当启动外壳进程失败（例如找不到关联的外壳或权限不足）时可能抛出。
        /// </exception>
        /// <example>
        /// <code>
        /// DirectoryLauncher.Open(@"C:\Users\Public");
        /// </code>
        /// </example>
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

        /// <summary>
        /// 尝试使用系统默认文件管理器打开目录，并捕获可能发生的错误。
        /// </summary>
        /// <param name="path">要打开的目录路径。</param>
        /// <param name="errorMessage">
        /// 当返回值为 true 时为空字符串；当返回值为 false 时包含错误消息。
        /// </param>
        /// <returns>目录成功打开返回 true，否则返回 false。</returns>
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

        /// <summary>
        /// 验证目录路径是否为非空绝对路径且对应的目录存在。
        /// </summary>
        /// <param name="path">要验证的目录路径。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="path"/> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">
        /// 当路径为空字符串、不是绝对路径或规范化失败时抛出。
        /// </exception>
        /// <exception cref="DirectoryNotFoundException">当对应目录不存在时抛出。</exception>
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

        /// <summary>
        /// 检查当前进程是否对目标目录具有读取访问权限。
        /// </summary>
        /// <param name="path">要检查的目录路径。</param>
        /// <returns>具有读取访问权限返回 true，否则返回 false。</returns>
        /// <remarks>
        /// 该方法通过尝试枚举目录项来探测访问权限：若出现 <see cref="UnauthorizedAccessException"/> 或 <see cref="IOException"/>，则视为无访问权限。
        /// </remarks>
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
