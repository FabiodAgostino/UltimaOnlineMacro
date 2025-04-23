using System;
using System.Text.RegularExpressions;
using System.IO;

namespace AutoClicker.Utils
{
    public static class PathHelper
    {
        /// <summary>
        /// Decodifica le sequenze di escape Unicode nei percorsi di file
        /// </summary>
        /// <param name="encodedPath">Percorso codificato con sequenze escape Unicode</param>
        /// <returns>Percorso decodificato</returns>
        public static string DecodeUnicodePath(string encodedPath)
        {
            if (string.IsNullOrEmpty(encodedPath))
                return encodedPath;

            // Decodifica sequenze \uXXXX usando regex
            return Regex.Replace(encodedPath, @"\\u([0-9a-fA-F]{4})", match =>
            {
                string hex = match.Groups[1].Value;
                int charCode = Convert.ToInt32(hex, 16);
                return char.ConvertFromUtf32(charCode);
            });
        }

        /// <summary>
        /// Verifica se un percorso contiene caratteri speciali che potrebbero causare problemi
        /// </summary>
        /// <param name="path">Percorso da verificare</param>
        /// <returns>True se il percorso contiene caratteri speciali</returns>
        public static bool ContainsSpecialCharacters(string path)
        {
            // Caratteri che potrebbero causare problemi nei percorsi
            char[] specialChars = { '\'', '"', '`', '´', '\\', ':', '*', '?', '<', '>', '|' };

            return path != null && path.IndexOfAny(specialChars) >= 0;
        }

        /// <summary>
        /// Ottiene la versione normalizzata di un percorso, sostituendo i separatori di percorso
        /// e decodificando le sequenze Unicode
        /// </summary>
        /// <param name="path">Percorso da normalizzare</param>
        /// <returns>Percorso normalizzato</returns>
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            // Decodifica sequenze Unicode
            string decodedPath = DecodeUnicodePath(path);

            // Sostituisci separatori di percorso con quelli appropriati per il sistema
            decodedPath = decodedPath.Replace('/', Path.DirectorySeparatorChar)
                                   .Replace('\\', Path.DirectorySeparatorChar);

            // Gestisci eventuali doppie barre
            while (decodedPath.Contains(string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar)))
            {
                decodedPath = decodedPath.Replace(
                    string.Concat(Path.DirectorySeparatorChar, Path.DirectorySeparatorChar),
                    Path.DirectorySeparatorChar.ToString());
            }

            return decodedPath;
        }
    }
}