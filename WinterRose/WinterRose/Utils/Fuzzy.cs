using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WinterRose
{
    /// <summary>
    /// Provides functions to preform fuzzy searches on collections of strings.
    /// </summary>
    public static class Fuzzy
    {
        /// <summary>
        /// The searchType of comparison to use when searching for a string.
        /// </summary>
        [Flags]
        public enum ComparisonType
        {
            /// <summary>
            /// Default comparison searchType.
            /// </summary>
            None,
            /// <summary>
            /// The search will ignore the case of the characters.
            /// </summary>
            IgnoreCase,
            /// <summary>
            /// The search will trim the strings before comparing them.
            /// </summary>
            Trim
        }

        // --- Updated scoring helper (more typo-tolerant) ---
        private static float ComputeFuzzyScore(string name, string search)
        {
            if (name == null) return 0f;
            if (search == null) return 0f;
            if (search.Length == 0) return 0f;

            // exact match stays supreme (preserves existing behavior)
            if (name == search) return int.MaxValue;

            const float PREFIX_BONUS = 2f;
            const float CONTAINS_BONUS = 2f;
            const float INORDER_BONUS = 0.45f;
            const float LEADING_MATCH_SCORE = 1f;
            const float CONSECUTIVE_BONUS = 0.35f; // reward runs of correct chars
            const float MISMATCH_PENALTY_BASE = 0.75f;

            float score = 0f;

            // prefix / contains bonuses
            if (name.StartsWith(search))
                score += PREFIX_BONUS;
            else if (name.Contains(search))
                score += CONTAINS_BONUS;

            // leading character matches + escalating penalties + consecutive runs bonus
            int prefixMatchCount = 0;
            float missedChars = 0f;
            int limit = Math.Min(search.Length, name.Length);
            int consecutive = 0;
            for (int j = 0; j < limit; j++)
            {
                if (search[j] == name[j])
                {
                    score += LEADING_MATCH_SCORE;
                    prefixMatchCount++;
                    consecutive++;
                    if (consecutive > 1)
                        score += CONSECUTIVE_BONUS * (consecutive - 1);
                }
                else
                {
                    consecutive = 0;
                    missedChars++;
                    score -= MISMATCH_PENALTY_BASE * missedChars;
                }
            }

            // subsequence (in-order) matches across the whole name
            int seqIndex = 0;
            for (int j = 0; j < name.Length && seqIndex < search.Length; j++)
            {
                if (name[j] == search[seqIndex])
                    seqIndex++;
            }
            int seqMatches = seqIndex;
            int extraMatches = Math.Max(0, seqMatches - prefixMatchCount);
            score += extraMatches * INORDER_BONUS;

            // normalize base score to 0..1 (except exact matches which are int.MaxValue)
            float maxPossibleScore = (search.Length * (LEADING_MATCH_SCORE + CONSECUTIVE_BONUS))
                                     + PREFIX_BONUS
                                     + (search.Length * INORDER_BONUS);
            if (maxPossibleScore <= 0f) return 0f;

            float baseNormalized = score / maxPossibleScore;
            baseNormalized = Math.Max(0f, Math.Min(1f, baseNormalized));

            // incorporate edit distance (Levenshtein) to accept typos/transpositions
            int dist = LevenshteinDistance(name, search);
            int maxLen = Math.Max(name.Length, search.Length);
            float editNormalized = 0f;
            if (maxLen > 0)
            {
                editNormalized = 1f - ((float)dist / maxLen);
                editNormalized = Math.Max(0f, Math.Min(1f, editNormalized));
            }

            // choose weights: for very short searches rely more on edit distance
            float baseWeight = search.Length <= 2 ? 0.35f : 0.65f;
            float editWeight = 1f - baseWeight;

            float combined = (baseNormalized * baseWeight) + (editNormalized * editWeight);
            combined = Math.Max(0f, Math.Min(1f, combined));
            return combined;
        }

        // --- New helper: Levenshtein distance (simple, robust) ---
        private static int LevenshteinDistance(string s, string t)
        {
            if (s == null) return t?.Length ?? 0;
            if (t == null) return s.Length;

            int n = s.Length;
            int m = t.Length;
            if (n == 0) return m;
            if (m == 0) return n;

            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1,    // deletion
                                 d[i, j - 1] + 1),   // insertion
                        d[i - 1, j - 1] + cost);    // substitution
                }
            }

            return d[n, m];
        }

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <returns>A copy of <paramref name="items"/> sorted in such a way where the item with the highest score is first</returns>
        public static List<(string item, float score)> SearchMany(
            this IEnumerable<string> items,
            string search, 
            ComparisonType searchType = ComparisonType.None)
        {
            List<(string item, float score)> results = new();
            var itemsList = items.ToList();

            if (searchType.HasFlag(ComparisonType.IgnoreCase))
                search = search.ToLower();
            if (searchType.HasFlag(ComparisonType.Trim))
                search = search.Trim();

            for (int i = 0; i < itemsList.Count; i++)
            {
                string? raw = itemsList[i];
                if (raw is null) continue;

                string item = raw;
                if (searchType.HasFlag(ComparisonType.Trim))
                    item = item.Trim();
                if (searchType.HasFlag(ComparisonType.IgnoreCase))
                    item = item.ToLower();

                var score = ComputeFuzzyScore(item, search);
                results.Add((item, score));
            }

            return results.OrderByDescending(x => x.score).ToList();
        }

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <param name="stringSelector"></param>
        /// <returns>A copy of <paramref name="items"/> sorted in such a way where the item with the highest score is first</returns>
        public static List<(T item, float score)> SearchMany<T>(
            this IEnumerable<T> items,
            string search, 
            Func<T, string?> stringSelector,
            ComparisonType searchType = ComparisonType.None)
        {
            if (searchType.HasFlag(ComparisonType.IgnoreCase))
                search = search.ToLower();
            if (searchType.HasFlag(ComparisonType.Trim))
                search = search.Trim();

            List<(T item, float score)> results = new();
            foreach (var item in items)
            {
                string? name = stringSelector(item);
                if (name is null) continue;

                string proc = name;
                if (searchType.HasFlag(ComparisonType.Trim))
                    proc = proc.Trim();
                if (searchType.HasFlag(ComparisonType.IgnoreCase))
                    proc = proc.ToLower();

                var score = ComputeFuzzyScore(proc, search);
                results.Add((item, score));
            }

            return results.OrderByDescending(x => x.score).ToList();
        }

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <param name="stringSelector"></param>
        /// <returns>A copy of <paramref name="items"/> sorted in such a way where the item with the highest score is first</returns>
        public static List<(T item, float score)> SearchMany<T>(this IEnumerable<T> items, string search, Func<T, string> stringSelector, float threshold, ComparisonType searchType = ComparisonType.None)
        {
            return items.SearchMany(search, stringSelector, searchType).Where(x => x.score >= threshold).ToList();
        }

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <returns>A copy of <paramref name="items"/> sorted in such a way where the item with the highest score is first</returns>
        public static List<(string item, float score)> SearchMany(this IEnumerable<string> items, string search, float threshold, ComparisonType searchType = ComparisonType.None)
        {
            return items.SearchMany(search, searchType).Where(x => x.score >= threshold).ToList();
        }

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <param name="stringSelector"></param>
        /// <returns>The item from the collection with the highest score</returns>
        public static T Search<T>(this IEnumerable<T> items, string search, Func<T, string> stringSelector, ComparisonType searchType = ComparisonType.None)
        {
            return items.SearchMany(search, stringSelector, searchType).FirstOrDefault().item;
        }

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <returns>The item from the collection with the highest score</returns>
        public static string Search(this IEnumerable<string> items, string search, ComparisonType searchType = ComparisonType.None)
        {
            return items.SearchMany(search, searchType).FirstOrDefault().item;
        }
    }
}


