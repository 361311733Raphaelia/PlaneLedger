using System.Collections.Generic;
using System.Linq;
using PlaneLedger.Core;
using PlaneLedger.Currency;
using PlaneLedger.Data.Models;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.Collection
{
    /// <summary>
    /// 掉落系统。管理每日登录掉落（装饰）和结算掉落（卡片+代币）。
    /// 包含稀有度权重、保底机制、重复转代币。
    /// </summary>
    public class DropSystem
    {
        private readonly CollectionData _collection;
        private readonly SettlementData _settlement;
        private readonly DecorationSystem _decorationSystem;
        private readonly WisdomCardSystem _cardSystem;
        private readonly TokenManager _tokenManager;

        // 装饰掉落权重
        private const float WEIGHT_COMMON = 70f;
        private const float WEIGHT_RARE = 25f;
        private const float WEIGHT_EPIC = 5f;

        // 卡片掉落权重
        private const float CARD_WEIGHT_COMMON = 65f;
        private const float CARD_WEIGHT_RARE = 28f;
        private const float CARD_WEIGHT_EPIC = 7f;

        // 保底阈值
        private const int DECORATION_PITY_THRESHOLD = 3; // 连续3次无稀有+，第4次保底
        private const int CARD_PITY_THRESHOLD = 4;       // 连续4次无稀有+，第5次保底

        // 每日结算代币奖励
        private const int SETTLEMENT_TOKEN_REWARD = 10;

        public DropSystem(SaveData saveData, DecorationSystem decorationSystem,
            WisdomCardSystem cardSystem, TokenManager tokenManager)
        {
            _collection = saveData.Collection;
            _settlement = saveData.Settlement;
            _decorationSystem = decorationSystem;
            _cardSystem = cardSystem;
            _tokenManager = tokenManager;
        }

        /// <summary>
        /// 执行每日登录掉落：随机装饰 ×1。
        /// 连续登录第7天，掉落等级提升一档。
        /// </summary>
        public DropResult PerformLoginDrop()
        {
            if (_settlement.TodayLoginRewardClaimed)
            {
                Debug.Log("[Drop] 今日登录奖励已领取");
                return default;
            }

            _settlement.TodayLoginRewardClaimed = true;

            // 确定稀有度
            bool isBonus = _settlement.ConsecutiveLoginDays > 0 && _settlement.ConsecutiveLoginDays % 7 == 0;
            Rarity rarity = RollDecorationRarity(isBonus);

            // 保底检查
            if (rarity == Rarity.Common)
            {
                _collection.DecorationPityCounter++;
                if (_collection.DecorationPityCounter > DECORATION_PITY_THRESHOLD)
                {
                    rarity = Rarity.Rare;
                    _collection.DecorationPityCounter = 0;
                }
            }
            else
            {
                _collection.DecorationPityCounter = 0;
            }

            // 从池中随机选择
            var pool = _decorationSystem.GetDropPoolDecorations()
                .Where(d => d.Rarity == rarity)
                .ToList();

            if (pool.Count == 0)
            {
                // 该稀有度无可用装饰，降级
                pool = _decorationSystem.GetDropPoolDecorations().ToList();
            }

            if (pool.Count == 0)
            {
                Debug.LogWarning("[Drop] 掉落池为空");
                return default;
            }

            var selected = pool[Random.Range(0, pool.Count)];
            bool isNew = _decorationSystem.Unlock(selected.Id);
            int tokenCompensation = 0;

            if (!isNew)
            {
                // 重复物品转代币
                tokenCompensation = selected.DuplicateTokenValue;
                _tokenManager.Earn(tokenCompensation, "duplicate");
            }

            var result = new DropResult
            {
                ItemId = selected.Id,
                IsDecoration = true,
                Rarity = (int)selected.Rarity,
                IsDuplicate = !isNew,
                TokenCompensation = tokenCompensation
            };

            EventBus.Publish(result);
            Debug.Log($"[Drop] 登录掉落: {selected.DisplayName} ({selected.Rarity}){(!isNew ? " [重复→" + tokenCompensation + "代币]" : "")}");
            return result;
        }

        /// <summary>
        /// 执行每日结算掉落：随机残章 ×1 + 代币 ×10。
        /// </summary>
        public DropResult PerformSettlementDrop()
        {
            // 发放代币
            _tokenManager.Earn(SETTLEMENT_TOKEN_REWARD, "settlement");

            // 确定卡片稀有度
            Rarity rarity = RollCardRarity();

            // 保底检查
            if (rarity == Rarity.Common)
            {
                _collection.CardPityCounter++;
                if (_collection.CardPityCounter > CARD_PITY_THRESHOLD)
                {
                    rarity = Rarity.Rare;
                    _collection.CardPityCounter = 0;
                }
            }
            else
            {
                _collection.CardPityCounter = 0;
            }

            // 从池中随机选择
            var pool = _cardSystem.GetDropPoolCards()
                .Where(c => c.Rarity == rarity)
                .ToList();

            if (pool.Count == 0)
            {
                pool = _cardSystem.GetDropPoolCards().ToList();
            }

            if (pool.Count == 0)
            {
                Debug.LogWarning("[Drop] 卡片池为空");
                return default;
            }

            var selected = pool[Random.Range(0, pool.Count)];
            bool isNew = _cardSystem.Unlock(selected.Id);
            int tokenCompensation = 0;

            if (!isNew)
            {
                // 卡片重复也转代币（使用装饰品的转换规则）
                switch (selected.Rarity)
                {
                    case Rarity.Common: tokenCompensation = 10; break;
                    case Rarity.Rare: tokenCompensation = 20; break;
                    case Rarity.Epic: tokenCompensation = 50; break;
                }
                _tokenManager.Earn(tokenCompensation, "duplicate");
            }

            var result = new DropResult
            {
                ItemId = selected.Id,
                IsDecoration = false,
                Rarity = (int)selected.Rarity,
                IsDuplicate = !isNew,
                TokenCompensation = tokenCompensation
            };

            EventBus.Publish(result);
            Debug.Log($"[Drop] 结算掉落: {selected.DisplayName} ({selected.Rarity}){(!isNew ? " [重复→" + tokenCompensation + "代币]" : "")}");
            return result;
        }

        /// <summary>
        /// 刷新当日随机掉落池（消耗20代币）。
        /// 实际效果：重新执行一次登录掉落。
        /// </summary>
        public bool RefreshAndReroll()
        {
            if (!_tokenManager.TrySpend(20, "refresh_pool"))
            {
                return false;
            }

            _settlement.PoolRefreshCount++;
            _settlement.TodayLoginRewardClaimed = false; // 重置以允许再次掉落
            PerformLoginDrop();
            return true;
        }

        private Rarity RollDecorationRarity(bool bonusTier = false)
        {
            float roll = Random.Range(0f, 100f);
            float epic = bonusTier ? WEIGHT_EPIC * 2f : WEIGHT_EPIC;
            float rare = bonusTier ? WEIGHT_RARE * 1.5f : WEIGHT_RARE;

            if (roll < epic) return Rarity.Epic;
            if (roll < epic + rare) return Rarity.Rare;
            return Rarity.Common;
        }

        private Rarity RollCardRarity()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < CARD_WEIGHT_EPIC) return Rarity.Epic;
            if (roll < CARD_WEIGHT_EPIC + CARD_WEIGHT_RARE) return Rarity.Rare;
            return Rarity.Common;
        }
    }
}
