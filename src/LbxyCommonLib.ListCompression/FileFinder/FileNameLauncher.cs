#pragma warning disable CS1591
#pragma warning disable SA1101
#pragma warning disable SA1204
#pragma warning disable SA1600
#pragma warning disable SA1602
#pragma warning disable SA1633
#pragma warning disable SA1649
#pragma warning disable SA1503
#pragma warning disable SA1513
#pragma warning disable SA1629
#pragma warning disable SA1402
#pragma warning disable CS8603

namespace LbxyCommonLib.FileFinder
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Search mode for file name matching.
    /// </summary>
    public enum MatchMode
    {
        Exact = 0,
        Fuzzy = 1,
    }

    /// <summary>
    /// Preview information for a file.
    /// </summary>
    public sealed class FilePreview
    {
        public string Name { get; set; } = string.Empty;

        public string FullPath { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Provides file name search and open functions.
    /// </summary>
    public static class FileNameLauncher
    {
        private static readonly string[] DefaultExtensions = new[] { ".txt", ".doc", ".docx", ".pdf", ".xls", ".xlsx", ".csv", ".md" };

        /// <summary>
        /// Searches files under root directory by name using exact or fuzzy match.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search (must exist).</param>
        /// <param name="query">Query text.</param>
        /// <param name="mode">Exact or Fuzzy.</param>
        /// <param name="allowedExtensions">Allowed extensions (dot-prefixed). If null, uses default set.</param>
        /// <param name="maxResults">Maximum results; if less than or equal to zero, unlimited.</param>
        /// <returns>List of matching file paths, ordered by simple relevance; empty list on invalid inputs or errors.</returns>
        /// <code>
        /// var hits = FileNameLauncher.SearchFiles(root, "report", MatchMode.Fuzzy, null, 50);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyList<string> SearchFiles(string rootDirectory, string query, MatchMode mode, IEnumerable<string> allowedExtensions, int maxResults = 0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rootDirectory) || !Directory.Exists(rootDirectory))
                {
                    return new string[0];
                }

                if (query == null)
                {
                    return new string[0];
                }

                var extSet = BuildExtensionSet(allowedExtensions ?? DefaultExtensions);
                var results = new List<string>();
                var q = query.Trim();
                var tokens = Tokenize(q);

                foreach (var file in Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories))
                {
                    var ext = Path.GetExtension(file);
                    if (string.IsNullOrEmpty(ext) || !extSet.Contains(ext.ToLowerInvariant()))
                    {
                        continue;
                    }

                    var name = Path.GetFileName(file);
                    if (name == null)
                    {
                        continue;
                    }

                    var matched = mode == MatchMode.Exact
                        ? string.Equals(name, q, StringComparison.OrdinalIgnoreCase)
                        : FuzzyMatch(name, tokens);

                    if (matched)
                    {
                        results.Add(file);
                        if (maxResults > 0 && results.Count >= maxResults)
                        {
                            break;
                        }
                    }
                }

                // Order by simple relevance: exact name equals first, then contains startswith, then length
                var ordered = results
                    .OrderByDescending(p => string.Equals(Path.GetFileName(p), q, StringComparison.OrdinalIgnoreCase))
                    .ThenByDescending(p => Path.GetFileName(p).StartsWith(q, StringComparison.OrdinalIgnoreCase))
                    .ThenBy(p => Path.GetFileName(p).Length, Comparer<int>.Default)
                    .ToArray();

                return ordered;
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Asynchronously searches files under root directory by name using exact or fuzzy match.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search (must exist).</param>
        /// <param name="query">Query text.</param>
        /// <param name="mode">Exact or Fuzzy.</param>
        /// <param name="allowedExtensions">Allowed extensions (dot-prefixed). If null, uses default set.</param>
        /// <param name="maxResults">Maximum results; if less than or equal to zero, unlimited.</param>
        /// <param name="cancellationToken">Cancellation token; returns empty list when cancelled.</param>
        /// <returns>List of matching file paths; empty list on invalid inputs, errors, or cancellation.</returns>
        /// <code>
        /// var hits = await FileNameLauncher.SearchFilesAsync(root, "report", MatchMode.Fuzzy, null, 50, ct);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<IReadOnlyList<string>> SearchFilesAsync(string rootDirectory, string query, MatchMode mode, IEnumerable<string> allowedExtensions, int maxResults, CancellationToken cancellationToken)
        {
            try
            {
                return await Task.Run(
                    () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        return SearchFiles(rootDirectory, query, mode, allowedExtensions, maxResults);
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return new string[0];
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Tries to open a file by name; searches using given mode and opens the best match via shell.
        /// </summary>
        /// <param name="rootDirectory">Root directory to search.</param>
        /// <param name="query">Query text.</param>
        /// <param name="mode">Exact or Fuzzy.</param>
        /// <param name="allowedExtensions">Allowed extensions set; null for defaults.</param>
        /// <param name="errorMessage">Outputs error message when failed.</param>
        /// <param name="actuallyOpen">If false, validates only without launching external apps (useful for tests).</param>
        /// <returns>True when opened successfully; otherwise false.</returns>
        /// <code>
        /// var ok = FileNameLauncher.TryOpenByName(root, "report", MatchMode.Fuzzy, null, out var error);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryOpenByName(string rootDirectory, string query, MatchMode mode, IEnumerable<string> allowedExtensions, out string errorMessage, bool actuallyOpen = true)
        {
            errorMessage = string.Empty;
            try
            {
                var hits = SearchFiles(rootDirectory, query, mode, allowedExtensions, 1);
                if (hits.Count == 0)
                {
                    errorMessage = "未找到匹配的文件。";
                    return false;
                }

                var path = hits[0];
                if (!File.Exists(path))
                {
                    errorMessage = "文件不存在。";
                    return false;
                }

                if (!actuallyOpen)
                {
                    return true;
                }

                try
                {
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                    return true;
                }
                catch (UnauthorizedAccessException)
                {
                    errorMessage = "无访问权限，无法打开该文件。";
                    return false;
                }
                catch (Exception ex)
                {
                    errorMessage = $"打开文件失败：{ex.Message}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"搜索或打开过程失败：{ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets preview information for an existing file.
        /// </summary>
        /// <param name="path">File path.</param>
        /// <param name="errorMessage">Outputs error message on failure.</param>
        /// <returns>Preview info or null when not available.</returns>
        /// <code>
        /// var info = FileNameLauncher.GetPreview(path, out var error);
        /// </code>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FilePreview GetPreview(string path, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    errorMessage = "文件不存在或路径非法。";
                    return null;
                }

                var fi = new FileInfo(path);
                return new FilePreview
                {
                    Name = fi.Name,
                    FullPath = fi.FullName,
                    SizeBytes = fi.Exists ? fi.Length : 0,
                    LastWriteTimeUtc = fi.Exists ? fi.LastWriteTimeUtc : DateTime.MinValue,
                    Type = fi.Extension?.ToLowerInvariant() ?? string.Empty,
                };
            }
            catch (UnauthorizedAccessException)
            {
                errorMessage = "无访问权限，无法读取文件信息。";
                return null;
            }
            catch (Exception ex)
            {
                errorMessage = $"读取文件信息失败：{ex.Message}";
                return null;
            }
        }

        private static HashSet<string> BuildExtensionSet(IEnumerable<string> exts)
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in exts)
            {
                if (!string.IsNullOrEmpty(e))
                {
                    var norm = e.StartsWith(".", StringComparison.Ordinal) ? e.ToLowerInvariant() : ("." + e.ToLowerInvariant());
                    set.Add(norm);
                }
            }

            return set;
        }

        private static string[] Tokenize(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return new string[0];
            }

            var list = new List<string>();
            var sb = new StringBuilder();
            for (var i = 0; i < query.Length; i++)
            {
                var ch = query[i];
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        list.Add(sb.ToString());
                        sb.Clear();
                    }
                }
            }

            if (sb.Length > 0)
            {
                list.Add(sb.ToString());
            }

            if (list.Count == 0)
            {
                list.Add(query.ToLowerInvariant());
            }

            return list.ToArray();
        }

        private static bool FuzzyMatch(string name, string[] tokens)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var low = name.ToLowerInvariant();
            for (var i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Length == 0)
                {
                    continue;
                }

                if (low.IndexOf(tokens[i], StringComparison.Ordinal) < 0)
                {
                    return false;
                }
            }

            return true;
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
#pragma warning restore SA1629
#pragma warning restore SA1513
#pragma warning restore SA1503
#pragma warning restore CS8603
#pragma warning restore CS1591
