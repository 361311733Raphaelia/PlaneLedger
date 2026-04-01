using System;
using System.Collections.Generic;
using System.Linq;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Accounting
{
    /// <summary>
    /// 本月统计面板业务逻辑。管理收入、大额支出、核心财务指标。
    /// </summary>
    public class MonthlyOverviewService
    {
        private readonly SaveData _saveData;
        private readonly AccountingData _accounting;

        public MonthlyOverviewService(SaveData saveData)
        {
            _saveData = saveData;
            _accounting = saveData.Accounting;
        }

        #region 收入管理

        /// <summary>获取指定月份的收入数据，不存在则创建</summary>
        public MonthlyIncome GetOrCreateMonthlyIncome(int year, int month)
        {
            var key = $"{year:D4}-{month:D2}";
            if (!_accounting.MonthlyIncomes.ContainsKey(key))
            {
                _accounting.MonthlyIncomes[key] = new MonthlyIncome();
            }
            return _accounting.MonthlyIncomes[key];
        }

        /// <summary>添加一笔收入明细</summary>
        public void AddIncomeItem(int year, int month, string type, string description, float amount)
        {
            var income = GetOrCreateMonthlyIncome(year, month);
            income.IncomeItems.Add(new IncomeItem
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = type,
                Description = description,
                Amount = amount
            });

            // 更新汇总
            RecalculateIncomeTotals(income);

            EventBus.Publish(new MonthlyDataUpdated
            {
                Year = year, Month = month,
                TotalActiveIncome = income.ActiveIncome,
                TotalPassiveIncome = income.PassiveIncome,
                TotalLargeExpense = GetMonthlyLargeExpenseTotal(year, month)
            });
        }

        /// <summary>删除一笔收入明细</summary>
        public bool RemoveIncomeItem(int year, int month, string itemId)
        {
            var income = GetOrCreateMonthlyIncome(year, month);
            var removed = income.IncomeItems.RemoveAll(i => i.Id == itemId);
            if (removed > 0)
            {
                RecalculateIncomeTotals(income);
                return true;
            }
            return false;
        }

        private void RecalculateIncomeTotals(MonthlyIncome income)
        {
            income.ActiveIncome = income.IncomeItems
                .Where(i => i.Type == "Active").Sum(i => i.Amount);
            income.PassiveIncome = income.IncomeItems
                .Where(i => i.Type == "Passive").Sum(i => i.Amount);
            income.InvestmentIncome = income.IncomeItems
                .Where(i => i.Type == "Investment").Sum(i => i.Amount);
        }

        #endregion

        #region 大额支出管理

        /// <summary>添加一笔大额支出</summary>
        public void AddLargeExpense(int year, int month, string category, string description, float amount)
        {
            var key = $"{year:D4}-{month:D2}";
            if (!_accounting.MonthlyLargeExpenses.ContainsKey(key))
            {
                _accounting.MonthlyLargeExpenses[key] = new List<LargeExpense>();
            }

            _accounting.MonthlyLargeExpenses[key].Add(new LargeExpense
            {
                Id = Guid.NewGuid().ToString("N"),
                Category = category,
                Description = description,
                Amount = amount
            });
        }

        /// <summary>删除一笔大额支出</summary>
        public bool RemoveLargeExpense(int year, int month, string expenseId)
        {
            var key = $"{year:D4}-{month:D2}";
            if (!_accounting.MonthlyLargeExpenses.TryGetValue(key, out var list)) return false;
            return list.RemoveAll(e => e.Id == expenseId) > 0;
        }

        /// <summary>获取指定月份大额支出总额</summary>
        public float GetMonthlyLargeExpenseTotal(int year, int month)
        {
            var key = $"{year:D4}-{month:D2}";
            if (!_accounting.MonthlyLargeExpenses.TryGetValue(key, out var list)) return 0f;
            return list.Sum(e => e.Amount);
        }

        #endregion

        #region 核心财务指标

        /// <summary>
        /// 生存天数 = 当前存款 / 日均支出。
        /// 返回 -1 表示日均支出为0（无限生存）。
        /// </summary>
        public float GetSurvivalDays()
        {
            float savings = _saveData.Profile.TotalSavings;
            float dailyAvg = GetOverallDailyAverage();

            if (dailyAvg <= 0) return -1f;
            return savings / dailyAvg;
        }

        /// <summary>
        /// 被动收入占比 = 被动收入 / 总支出 × 100%。
        /// </summary>
        public float GetPassiveIncomeRatio(int year, int month)
        {
            var income = GetOrCreateMonthlyIncome(year, month);
            float passiveTotal = income.PassiveIncome + income.InvestmentIncome;
            float totalExpense = GetMonthlyTotalExpense(year, month);

            if (totalExpense <= 0) return 0f;
            return passiveTotal / totalExpense * 100f;
        }

        /// <summary>
        /// 月净存款 = 总收入 - 总支出。
        /// </summary>
        public float GetMonthlySavings(int year, int month)
        {
            var income = GetOrCreateMonthlyIncome(year, month);
            float totalIncome = income.ActiveIncome + income.PassiveIncome + income.InvestmentIncome;
            float totalExpense = GetMonthlyTotalExpense(year, month);
            return totalIncome - totalExpense;
        }

        /// <summary>
        /// 获取指定月份总支出（日常 + 大额 + 平摊）。
        /// </summary>
        public float GetMonthlyTotalExpense(int year, int month)
        {
            // 日常消费
            float dailyTotal = 0f;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= daysInMonth; day++)
            {
                var dateKey = new DateTime(year, month, day).ToString("yyyy-MM-dd");
                if (_accounting.DailyEntries.TryGetValue(dateKey, out var entries))
                {
                    dailyTotal += entries.Sum(e => e.Amount);
                }
            }

            // 大额支出
            float largeTotal = GetMonthlyLargeExpenseTotal(year, month);

            // 平摊费用
            float amortizedTotal = GetMonthlyAmortizedTotal(year, month);

            return dailyTotal + largeTotal + amortizedTotal;
        }

        /// <summary>
        /// 获取指定月份的平摊费用总额。
        /// </summary>
        public float GetMonthlyAmortizedTotal(int year, int month)
        {
            float total = 0f;
            var monthKey = $"{year:D4}-{month:D2}";

            foreach (var item in _accounting.AmortizedExpenses)
            {
                // 判断该月是否在平摊覆盖范围内
                if (IsMonthInAmortizationRange(item, year, month))
                {
                    total += item.MonthlyAmount;
                }
            }
            return total;
        }

        /// <summary>
        /// 计算总体日均支出（近30天）。
        /// </summary>
        public float GetOverallDailyAverage()
        {
            float total = 0f;
            int days = 0;
            var today = DateTime.Now.Date;

            for (int i = 0; i < 30; i++)
            {
                var date = today.AddDays(-i);
                var dateKey = date.ToString("yyyy-MM-dd");
                if (_accounting.DailyEntries.TryGetValue(dateKey, out var entries))
                {
                    total += entries.Sum(e => e.Amount);
                    days++;
                }
            }

            return days > 0 ? total / 30f : 0f; // 始终除以30（含无支出日）
        }

        /// <summary>
        /// 获取指定月份总收入。
        /// </summary>
        public float GetMonthlyTotalIncome(int year, int month)
        {
            var income = GetOrCreateMonthlyIncome(year, month);
            return income.ActiveIncome + income.PassiveIncome + income.InvestmentIncome;
        }

        /// <summary>
        /// 获取收入类型数量（用于成就：收入多元化）。
        /// </summary>
        public int GetIncomeTypeCount(int year, int month)
        {
            var income = GetOrCreateMonthlyIncome(year, month);
            int count = 0;
            if (income.ActiveIncome > 0) count++;
            if (income.PassiveIncome > 0) count++;
            if (income.InvestmentIncome > 0) count++;
            return count;
        }

        #endregion

        private bool IsMonthInAmortizationRange(AmortizedExpense item, int year, int month)
        {
            if (!DateTime.TryParse(item.StartMonth + "-01", out var start)) return false;
            var end = start.AddMonths(item.SpreadMonths - 1);
            var check = new DateTime(year, month, 1);
            return check >= start && check <= end;
        }
    }
}
