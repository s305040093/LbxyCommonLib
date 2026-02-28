namespace LbxyCommonLib.Cable
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 提供电缆规格列表的排序功能。
    /// </summary>
    public static class CableSorter
    {
        /// <summary>
        /// 对电缆规格列表进行分组排序。
        /// </summary>
        /// <param name="cables">待排序的电缆规格集合。</param>
        /// <returns>排序后的电缆规格列表。</returns>
        public static List<CableSpec> Sort(IEnumerable<CableSpec> cables)
        {
            if (cables == null)
            {
                return new List<CableSpec>();
            }

            // 1. 过滤空值并分类
            var validCables = cables.Where(c => c != null).ToList();
            var powerCables = new List<CableSpec>();
            var controlCables = new List<CableSpec>();
            var otherCables = new List<CableSpec>();

            foreach (var cable in validCables)
            {
                if (cable.Category == "动力电缆")
                {
                    powerCables.Add(cable);
                }
                else if (cable.Category == "控制电缆")
                {
                    controlCables.Add(cable);
                }
                else
                {
                    otherCables.Add(cable);
                }
            }

            // 2. 动力电缆排序
            // 第一优先级：PhaseCoreSection（相线截面积）从大到小
            // 第二优先级：NeutralCoreSection（中性线截面积）从大到小
            // 第三优先级：ProtectCoreCount（保护线芯数）从大到小
            var sortedPower = powerCables
                .OrderByDescending(c => c.PhaseCoreSection)
                .ThenByDescending(c => c.NeutralCoreSection)
                .ThenByDescending(c => c.ProtectCoreCount)
                .ToList();

            // 3. 控制电缆排序
            // 第一优先级：ControlCoreSection（控制线截面积）从大到小
            // 第二优先级：ControlCoreCount（控制线芯数）从大到小
            // 第三优先级：TwistedPairCount（对绞线对数）从大到小
            var sortedControl = controlCables
                .OrderByDescending(c => c.ControlCoreSection)
                .ThenByDescending(c => c.ControlCoreCount)
                .ThenByDescending(c => c.TwistedPairCount)
                .ToList();

            // 4. 合并结果：动力在前，控制在后，其他在最后
            var result = new List<CableSpec>();
            result.AddRange(sortedPower);
            result.AddRange(sortedControl);
            result.AddRange(otherCables);

            return result;
        }
    }
}
