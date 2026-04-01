using System;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Currency
{
    /// <summary>
    /// 代币管理器。管理代币的获取和消费。
    /// </summary>
    public class TokenManager
    {
        private readonly SettlementData _settlement;

        /// <summary>当前代币余额</summary>
        public int Balance => _settlement.TokenBalance;

        /// <summary>余额变化事件（供UI绑定）</summary>
        public event Action<int> OnBalanceChanged;

        public TokenManager(SaveData saveData)
        {
            _settlement = saveData.Settlement;
        }

        /// <summary>
        /// 获得代币。
        /// </summary>
        public void Earn(int amount, string source)
        {
            if (amount <= 0) return;

            _settlement.TokenBalance += amount;
            OnBalanceChanged?.Invoke(_settlement.TokenBalance);

            EventBus.Publish(new TokensEarned { Amount = amount, Source = source });
            Debug.Log($"[Token] +{amount} (来源: {source}), 余额: {_settlement.TokenBalance}");
        }

        /// <summary>
        /// 尝试消费代币。余额不足返回 false。
        /// </summary>
        public bool TrySpend(int amount, string purpose)
        {
            if (amount <= 0) return false;
            if (_settlement.TokenBalance < amount) return false;

            _settlement.TokenBalance -= amount;
            OnBalanceChanged?.Invoke(_settlement.TokenBalance);

            EventBus.Publish(new TokensSpent { Amount = amount, Purpose = purpose });
            Debug.Log($"[Token] -{amount} (用途: {purpose}), 余额: {_settlement.TokenBalance}");
            return true;
        }

        /// <summary>
        /// 检查是否有足够代币。
        /// </summary>
        public bool CanAfford(int amount)
        {
            return _settlement.TokenBalance >= amount;
        }
    }
}
