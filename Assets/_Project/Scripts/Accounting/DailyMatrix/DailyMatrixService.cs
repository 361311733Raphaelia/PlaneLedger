using System;
using System.Collections.Generic;
using System.Linq;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Accounting
{
    /// <summary>
    /// 日常消耗矩阵业务逻辑。管理每日碎片化即时记账。
    /// </summary>
    public class DailyMatrixService
    {
        private readonly AccountingData _accounting;
        private readonly SettlementData _settlement;

        public DailyMatrixService(SaveData saveData)
        {
            _accounting = saveData.Accounting;
            _settlement = saveData.Settlement;
        }

        /// <summary>
        /// 添加一笔日常消费记录。
        /// </summary>
        public DailyEntry AddEntry(DateTime date, string category, string subCategory, float amount, string note = "")
        {
            var dateKey = date.ToString("yyyy-MM-dd");

            if (!_accounting.DailyEntries.ContainsKey(dateKey))
            {
                _accounting.DailyEntries[dateKey] = new List<DailyEntry>();
            }

            var entry = new DailyEntry
            {
                Id = Guid.NewGuid().ToString("N"),
                Category = category,
                SubCategory = subCategory,
                Amount = amount,
                Note = note ?? "",
                Time = DateTime.Now.ToString("HH:mm")
            };

            _accounting.DailyEntries[dateKey].Add(entry);

            // 追踪已使用分类
            _accounting.UsedCategories.Add(category);
            _accounting.UsedCategories.Add(subCategory);

            // 发布事件
            EventBus.Publish(new DailyEntryAdded
            {
                Date = date,
                Category = category,
                SubCategory = subCategory,
                Amount = amount
            });

            return entry;
        }

        /// <summary>
        /// 删除一笔记录。
        /// </summary>
        public bool RemoveEntry(DateTime date, string entryId)
        {
            var dateKey = date.ToString("yyyy-MM-dd");
            if (!_accounting.DailyEntries.TryGetValue(dateKey, out var entries)) return false;

            var removed = entries.RemoveAll(e => e.Id == entryId);
            if (removed > 0)
            {
                EventBus.Publish(new DailyEntryRemoved { Date = date, EntryId = entryId });
                return true;
            }
            return false;
        }

        /// <summary>
        /// 修改一笔记录的金额。
        /// </summary>
        public bool UpdateEntryAmount(DateTime date, string entryId, float newAmount)
        {
            var dateKey = date.ToString("yyyy-MM-dd");
            if (!_accounting.DailyEntries.TryGetValue(dateKey, out var entries)) return false;

            var entry = entries.Find(e => e.Id == entryId);
            if (entry != null)
            {
                entry.Amount = newAmount;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取指定日期的所有记录。
        /// </summary>
        public List<DailyEntry> GetEntries(DateTime date)
        {
            var dateKey = date.ToString("yyyy-MM-dd");
            if (_accounting.DailyEntries.TryGetValue(dateKey, out var entries))
                return entries;
            return new List<DailyEntry>();
        }

        /// <summary>
        /// 获取指定日期的总支出。
        /// </summary>
        public float GetDailyTotal(DateTime date)
        {
            return GetEntries(date).Sum(e => e.Amount);
        }

        /// <summary>
        /// 获取指定日期某大类的总支出。
        /// </summary>
        public float GetDailyCategoryTotal(DateTime date, string category)
        {
            return GetEntries(date).Where(e => e.Category == category).Sum(e => e.Amount);
        }

        /// <summary>
        /// 计算指定月份的日均支出。
        /// </summary>
        public float GetMonthlyDailyAverage(int year, int month)
        {
            float total = 0f;
            int daysWithEntries = 0;

            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                if (date > DateTime.Now.Date) break;

                var dayTotal = GetDailyTotal(date);
                if (dayTotal > 0)
                {
                    total += dayTotal;
                    daysWithEntries++;
                }
            }

            return daysWithEntries > 0 ? total / daysWithEntries : 0f;
        }

        /// <summary>
        /// 获取指定月份某大类的总支出。
        /// </summary>
        public float GetMonthlyCategoryTotal(int year, int month, string category)
        {
            float total = 0f;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                if (date > DateTime.Now.Date) break;
                total += GetDailyCategoryTotal(date, category);
            }
            return total;
        }

        /// <summary>
        /// 获取已使用的不同分类数量（用于成就检测）。
        /// </summary>
        public int GetUsedCategoryCount()
        {
            return _accounting.UsedCategories.Count;
        }

        /// <summary>
        /// 检查指定日期是否所有支出仅含饮食类。
        /// </summary>
        public bool IsOnlyFoodDay(DateTime date)
        {
            var entries = GetEntries(date);
            if (entries.Count == 0) return false;
            return entries.All(e => e.Category == "EnergyIntake");
        }

        /// <summary>
        /// 检查指定日期是否有早中晚三餐记录。
        /// </summary>
        public bool HasAllThreeMeals(DateTime date)
        {
            var entries = GetEntries(date);
            bool hasBreakfast = entries.Any(e => e.SubCategory == "Breakfast");
            bool hasLunch = entries.Any(e => e.SubCategory == "Lunch");
            bool hasDinner = entries.Any(e => e.SubCategory == "Dinner");
            return hasBreakfast && hasLunch && hasDinner;
        }
    }
}
