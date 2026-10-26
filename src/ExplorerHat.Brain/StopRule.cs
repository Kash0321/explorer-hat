using System.Globalization;
using System.Text;

namespace ExplorerHat.Brain
{
    /// <summary>
    /// The stop never goes through the language model. Two of the measured models failed an urgent stop order, so a
    /// message that starts with a stop word stops the robot without asking anybody.
    /// The rule looks at the FIRST word only. A false stop ("Para la luz roja") is safe, but a missed stop ("¡¡PARA YA!!
    /// ¡Que se cae por las escaleras!") is not, so the rule must catch long messages too. Words in the middle do not
    /// count: "Gira para la derecha" is a movement order.
    /// </summary>
    public static class StopRule
    {
        private static readonly HashSet<string> StopWords = new HashSet<string>
        {
            "para", "parar", "stop", "alto", "detente", "quieto", "frena", "basta",
        };

        public static bool IsStop(string text)
        {
            string first = Normalize(text).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            return StopWords.Contains(first);
        }

        /// <summary>Lower case, without accents, and everything that is not a letter becomes a space</summary>
        public static string Normalize(string text)
        {
            var builder = new StringBuilder();
            foreach (char c in text.ToLowerInvariant().Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                builder.Append(char.IsLetter(c) ? c : ' ');
            }

            return builder.ToString();
        }
    }
}
