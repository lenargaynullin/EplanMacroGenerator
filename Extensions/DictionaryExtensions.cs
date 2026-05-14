using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LenarSoft.Extensions
{
    /// Методы расширения для Dictionary
    public static class DictionaryExtensions
    {
        /// Получить значение по ключу или создать новое, если ключ отсутствует
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue> valueFactory) where TKey : notnull
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory();
                dict[key] = value;
            }
            return value;
        }

        /// Добавить множество пар ключ-значение
        public static void AddRange<TKey, TValue>(
            this Dictionary<TKey, TValue> dict,
            IEnumerable<KeyValuePair<TKey, TValue>> values) where TKey : notnull
        {
            foreach (var kvp in values)
            {
                dict[kvp.Key] = kvp.Value;
            }
        }
    }
}
