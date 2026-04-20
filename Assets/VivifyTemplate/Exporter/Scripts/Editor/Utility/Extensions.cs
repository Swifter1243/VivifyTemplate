using System;

namespace VivifyTemplate.Exporter.Scripts.Editor.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Removes `target` from the end of `src` if found
        /// </summary>
        /// <param name="src">Input string to trim</param>
        /// <param name="target">Substring to remove from input</param>
        /// <param name="comparison">Handles cultural case-sensitive stuff</param>
        /// <returns>The trimmed string</returns>
        public static string TrimEnd(this string src, string target, StringComparison comparison = StringComparison.CurrentCulture)
        {
            if(target != null && src.EndsWith(target, comparison))
            {
                return src.Substring(0, src.Length - target.Length);
            }

            return src;
        }
    }
}
