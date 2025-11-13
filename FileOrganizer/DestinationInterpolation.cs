using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FileOrganizer
{
    public interface IPathTemplateRenderer
    {
        string Render(string template, DestinationContext ctx);
    }

    public sealed class DestinationInterpolator : IPathTemplateRenderer
    {

        private static readonly Regex Placeholder =
            new Regex(@"\{(?<name>[a-zA-Z_][\w\.]*)(:(?<fmt>[^}]+))?\}",
                      RegexOptions.Compiled);

        public string Render(string template, DestinationContext ctx)
        {
            string tmp = template.Replace("{{", "\uFFFF").Replace("}}", "\uFFFE");

            string replaced = Placeholder.Replace(tmp, m =>
            {
                string name = m.Groups["name"].Value;
                string fmt = m.Groups["fmt"].Success ? m.Groups["fmt"].Value : null;

                object value = Resolve(name, ctx);
                if (value == null) return string.Empty;

                if (value is DateTime dt)
                {
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();
                    return fmt == null
                        ? dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                        : dt.ToString(fmt, CultureInfo.InvariantCulture);
                }

                return (value as IFormattable)?.ToString(fmt, CultureInfo.InvariantCulture)
                       ?? value.ToString() ?? string.Empty;
            });

            string normalized = replaced.Replace('/', Path.DirectorySeparatorChar)
                                        .Replace('\uFFFF', '{').Replace('\uFFFE', '}');

            var sep = Path.DirectorySeparatorChar;

            // --- preserve and FIX the path root ---
            string rawRoot = Path.GetPathRoot(normalized) ?? string.Empty;  // "C:\" or "C:" or "\\server\share\"
            bool isBareDrive = rawRoot.Length == 2 && rawRoot[1] == ':';    // e.g., "C:"
            string root = isBareDrive ? rawRoot + sep : rawRoot;            // ensure "C:\"
            string remainder = normalized.Substring(rawRoot.Length);        // use RAW length here

            var cleanedSegments = remainder
                .Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries)
                .Select(CleanSegment);

            string cleaned = root + string.Join(sep.ToString(), cleanedSegments);

            return cleaned;
        }

        private static object Resolve(string name, DestinationContext c)
        {
            switch (name)
            {
                case "sourceDir":
                    return c.SourceDir;
                case "targetDir":
                    return c.TargetDir;
                case "sourcePath":
                    return c.SourcePath;
                case "fileName":
                    return c.FileName;
                case "fileNameNoExt":
                    return c.FileNameNoExt;
                case "extension":
                    return c.Extension;
                case "extensionUpper":
                    return c.Extension.ToUpperInvariant();
                case "sizeBytes":
                    return c.SizeBytes;
                case "createdAt":
                    return c.CreatedAt;
                case "modifiedAt":
                    return c.ModifiedAt;
                case "mime":
                    return c.Mime;
                case "slug":
                    return Slugify(c.FileNameNoExt);
                case "hash8":
                    return SafeHash8(c.SourcePath);
            }

            // Example nested token
            if (string.Equals(name, "createdAt.year", StringComparison.OrdinalIgnoreCase))
                return c.CreatedAt.ToLocalTime().Year;

            return null;
        }

        private static string CleanSegment(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                bool isInvalid = false;
                for (int i = 0; i < invalid.Length; i++)
                {
                    if (ch == invalid[i]) { isInvalid = true; break; }
                }
                sb.Append(isInvalid ? '_' : ch);
            }
            var seg = sb.ToString().Trim();

            // Reserved Windows names
            var reserved = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "CON","PRN","AUX","NUL","COM1","LPT1","COM2","LPT2","COM3","LPT3" };
            if (reserved.Contains(seg)) seg = "_" + seg;

            return seg;
        }

        private static string Slugify(string input)
        {
            if (string.IsNullOrEmpty(input)) return "file";
            var sb = new StringBuilder(input.Length);
            foreach (char ch in input)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(char.ToLowerInvariant(ch));
                }
                else if (ch == ' ' || ch == '-' || ch == '_' || ch == '.')
                {
                    sb.Append('-');
                }
            }
            var s = Regex.Replace(sb.ToString(), "-{2,}", "-").Trim('-');
            return s.Length > 0 ? s : "file";
        }

        private static string SafeHash8(string path)
        {
            try
            {
                using (var stream = File.OpenRead(path))
                using (var sha = SHA256.Create())
                {
                    var hash = sha.ComputeHash(stream);
                    var hex = BitConverter.ToString(hash).Replace("-", "");
                    return hex.Substring(0, 8).ToLowerInvariant();
                }
            }
            catch
            {
                return "00000000";
            }
        }
    }
}
