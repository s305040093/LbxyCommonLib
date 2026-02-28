namespace LbxyCommonLib.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// 字典合并模式。
    /// </summary>
    public enum DictionaryMergeMode
    {
        /// <summary>
        /// 浅层合并。如果值也是字典，则直接替换，不进行递归合并。
        /// </summary>
        Shallow,

        /// <summary>
        /// 深度合并。如果值也是字典，则递归合并其内容。
        /// </summary>
        Deep,
    }

    /// <summary>
    /// 字典冲突策略。
    /// </summary>
    public enum DictionaryConflictStrategy
    {
        /// <summary>
        /// 覆盖目标字典中的值。
        /// </summary>
        Overwrite,

        /// <summary>
        /// 保留目标字典中的值（跳过源字典中的值）。
        /// </summary>
        KeepTarget,

        /// <summary>
        /// 抛出异常。
        /// </summary>
        Throw,
    }

    /// <summary>
    /// 提供字典操作的辅助方法。
    /// </summary>
    public static class DictionaryOperations
    {
        /// <summary>
        /// 合并两个字典。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="target">目标字典（将被修改）。</param>
        /// <param name="source">源字典。</param>
        /// <param name="mode">合并模式（默认为浅层合并）。</param>
        /// <param name="strategy">冲突策略（默认为覆盖）。</param>
        /// <exception cref="ArgumentNullException">target 或 source 为 null。</exception>
        /// <exception cref="ArgumentException">当策略为 Throw 且发生键冲突时。</exception>
        public static void Merge<TKey, TValue>(
            IDictionary<TKey, TValue> target,
            IDictionary<TKey, TValue> source,
            DictionaryMergeMode mode = DictionaryMergeMode.Shallow,
            DictionaryConflictStrategy strategy = DictionaryConflictStrategy.Overwrite)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var kvp in source)
            {
                var key = kvp.Key;
                var sourceValue = kvp.Value;

                if (target.ContainsKey(key))
                {
                    if (mode == DictionaryMergeMode.Deep)
                    {
                        var targetValue = target[key];

                        // 尝试递归合并，要求两个值都实现了 IDictionary 接口
                        if (targetValue is IDictionary targetDict && sourceValue is IDictionary sourceDict)
                        {
                            MergeDictionaries(targetDict, sourceDict, mode, strategy);
                            continue;
                        }
                    }

                    switch (strategy)
                    {
                        case DictionaryConflictStrategy.Overwrite:
                            target[key] = sourceValue;
                            break;
                        case DictionaryConflictStrategy.KeepTarget:
                            // 保留目标值，不做任何操作
                            break;
                        case DictionaryConflictStrategy.Throw:
                            throw new ArgumentException($"Key '{key}' already exists in target dictionary.", nameof(source));
                    }
                }
                else
                {
                    target[key] = sourceValue;
                }
            }
        }

        /// <summary>
        /// 使用源字典的值替换目标字典中相同键的值。
        /// </summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="target">目标字典（将被修改）。</param>
        /// <param name="source">源字典。</param>
        /// <exception cref="ArgumentNullException">target 或 source 为 null。</exception>
        public static void Replace<TKey, TValue>(
            IDictionary<TKey, TValue> target,
            IDictionary<TKey, TValue> source)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var kvp in source)
            {
                if (target.ContainsKey(kvp.Key))
                {
                    target[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// 递归合并非泛型字典（用于深度合并）。
        /// </summary>
        private static void MergeDictionaries(IDictionary target, IDictionary source, DictionaryMergeMode mode, DictionaryConflictStrategy strategy)
        {
            foreach (DictionaryEntry entry in source)
            {
                var key = entry.Key;
                var sourceValue = entry.Value;

                if (target.Contains(key))
                {
                    if (mode == DictionaryMergeMode.Deep)
                    {
                        var targetValue = target[key];
                        if (targetValue is IDictionary targetDict && sourceValue is IDictionary sourceDict)
                        {
                            MergeDictionaries(targetDict, sourceDict, mode, strategy);
                            continue;
                        }
                    }

                    switch (strategy)
                    {
                        case DictionaryConflictStrategy.Overwrite:
                            target[key] = sourceValue;
                            break;
                        case DictionaryConflictStrategy.KeepTarget:
                            break;
                        case DictionaryConflictStrategy.Throw:
                            throw new ArgumentException($"Key '{key}' already exists in target dictionary.");
                    }
                }
                else
                {
                    target[key] = sourceValue;
                }
            }
        }
    }
}
