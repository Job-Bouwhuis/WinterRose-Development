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

        /// <summary>
        /// Searches a collection of items for a string using a fuzzy search algorithm.
        /// <br></br> The more characters are in the correct order from the start of the string, the higher the score.
        /// <br></br> The score will be <see cref="int.MaxValue"/> if the string is an exact match.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="search"></param>
        /// <returns>A copy of <paramref name="items"/> sorted in such a way where the item with the highest score is first</returns>
        public static List<(string item, float score)> SearchMany(this IEnumerable<string> items, string search, ComparisonType searchType = ComparisonType.None)
        {
            List<(string item, float score)> results = new();
            List<string> itemsList = items.ToList();
            int index = 0;

            if (searchType.HasFlag(ComparisonType.IgnoreCase))
                search = search.ToLower();
            if(searchType.HasFlag(ComparisonType.Trim))
                search = search.Trim();

            for (int i = 0; i < itemsList.Count; i++)
            {
                string? item = itemsList[i];
                if (searchType.HasFlag(ComparisonType.Trim))
                    item = item.Trim();
                if (searchType.HasFlag(ComparisonType.IgnoreCase))
                    item = item.ToLower();
                if (item == search)
                {
                    results.Add((item, int.MaxValue));
                    continue;
                }
                float score = 0;

                if (item.Contains(search))
                    score += 4;

                float missedChars = 0;
                for (int j = 0; j < search.Length; i++)
                {
                    if (item.Length <= j)
                        break;
                    if (search[j] == item[j])
                    {
                        score++;
                    }
                    else
                    {
                        missedChars++;
                        score -= 1f / missedChars;
                    }

                }

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
        public static List<(T item, float score)> SearchMany<T>(this IEnumerable<T> items, string search, Func<T, string?> stringSelector, ComparisonType searchType = ComparisonType.None)
        {
            if (searchType.HasFlag(ComparisonType.IgnoreCase))
                search = search.ToLower();
            if (searchType.HasFlag(ComparisonType.Trim))
                search = search.Trim();

            List<(T item, float score)> results = new();
            foreach (var item in items)
            {
                string? name = stringSelector(item);

                if (searchType.HasFlag(ComparisonType.Trim))
                    name = name.Trim();
                if (searchType.HasFlag(ComparisonType.IgnoreCase))
                    name = name.ToLower();

                if (name is null)
                    continue;

                if (name == search)
                {
                    results.Add((item, int.MaxValue));
                    continue;
                }
                float score = 0;


                if (name.StartsWith(search))
                {
                    score += 5;
                }
                else if (name.Contains(search))
                    score += 3;
                    

                float missedChars = 0;
                for (int i = 0; i < search.Length; i++)
                {
                    if (name.Length <= i)
                        break;
                    if (search[i] == name[i])
                    {
                        score++;
                    }
                    else
                    {
                        missedChars++;
                        score -= 1f / missedChars;
                    }

                }

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
        /// <param name="stringSelector"></param>
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


