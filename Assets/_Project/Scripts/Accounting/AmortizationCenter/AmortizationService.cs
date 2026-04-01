using System;
using System.Collections.Generic;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Accounting
{
    /// <summary>
    /// 年度平摊中心业务逻辑。管理年付/季付项目的按月拆解。
    /// </summary>
    public class AmortizationService
    {
        private readonly AccountingData _accounting;

        public AmortizationService(SaveData saveData)
        {
            _accounting = saveData.Accounting;
        }

        /// <summary>
        /// 添加一个平摊项目。
        /// </summary>
        /// <param name="name">项目名称（如"冬季取暖费"）</param>
        /// <param name="totalAmount">总金额</param>
        /// <param name="startMonth">起始月份 yyyy-MM</param>
        /// <param name="spreadMonths">覆盖月数</param>
        public AmortizedExpense AddItem(string name, float totalAmount, string startMonth, int spreadMonths)
        {
            if (spreadMonths <= 0)
            {
                Debug.LogError("[Amortization] 覆盖月数必须大于0");
                return null;
            }

            var item = new AmortizedExpense
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = name,
                TotalAmount = totalAmount,
                StartMonth = startMonth,
                SpreadMonths = spreadMonths,
                MonthlyAmount = Mathf.Round(totalAmount / spreadMonths * 100f) / 100f // 保留两位小数
            };

            _accounting.AmortizedExpenses.Add(item);

            EventBus.Publish(new AmortizationChanged
            {
                ItemId = item.Id,
                MonthlyAmount = item.MonthlyAmount
            });

            Debug.Log($"[Amortization] 新增平摊: {name}, {totalAmount}元/{spreadMonths}月 = {item.MonthlyAmount}元/月");
            return item;
        }

        /// <summary>
        /// 删除一个平摊项目。
        /// </summary>
        public bool RemoveItem(string itemId)
        {
            return _accounting.AmortizedExpenses.RemoveAll(i => i.Id == itemId) > 0;
        }

        /// <summary>
        /// 更新一个平摊项目。
        /// </summary>
        public bool UpdateItem(string itemId, string name, float totalAmount, string startMonth, int spreadMonths)
        {
            var item = _accounting.AmortizedExpenses.Find(i => i.Id == itemId);
            if (item == null) return false;

            item.Name = name;
            item.TotalAmount = totalAmount;
            item.StartMonth = startMonth;
            item.SpreadMonths = spreadMonths;
            item.MonthlyAmount = Mathf.Round(totalAmount / spreadMonths * 100f) / 100f;
            return true;
        }

        /// <summary>
        /// 获取所有平摊项目。
        /// </summary>
        public List<AmortizedExpense> GetAllItems()
        {
            return _accounting.AmortizedExpenses;
        }

        /// <summary>
        /// 获取平摊项目总数（用于成就检测）。
        /// </summary>
        public int GetItemCount()
        {
            return _accounting.AmortizedExpenses.Count;
        }
    }
}
