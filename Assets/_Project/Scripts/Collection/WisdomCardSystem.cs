using System.Collections.Generic;
using System.Linq;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.Collection
{
    /// <summary>
    /// 思想残章卡片系统。管理卡片的解锁和查询。
    /// </summary>
    public class WisdomCardSystem
    {
        private readonly CollectionData _collection;
        private readonly Dictionary<string, WisdomCardSO> _allCards;

        public WisdomCardSystem(SaveData saveData, WisdomCardSO[] allCards)
        {
            _collection = saveData.Collection;
            _allCards = new Dictionary<string, WisdomCardSO>();
            foreach (var card in allCards)
            {
                _allCards[card.Id] = card;
            }
        }

        /// <summary>解锁一张卡片。返回是否为新解锁。</summary>
        public bool Unlock(string cardId)
        {
            if (!_allCards.ContainsKey(cardId))
            {
                Debug.LogWarning($"[WisdomCard] 未知卡片ID: {cardId}");
                return false;
            }

            bool isNew = _collection.UnlockedCardIds.Add(cardId);

            if (isNew)
            {
                var card = _allCards[cardId];
                EventBus.Publish(new WisdomCardUnlocked
                {
                    CardId = cardId,
                    Rarity = (int)card.Rarity
                });
                Debug.Log($"[WisdomCard] 新解锁: {card.DisplayName} ({card.Rarity})");
            }

            return isNew;
        }

        /// <summary>获取卡片配置。</summary>
        public WisdomCardSO GetCard(string id)
        {
            return _allCards.TryGetValue(id, out var card) ? card : null;
        }

        /// <summary>已解锁卡片总数。</summary>
        public int UnlockedCount => _collection.UnlockedCardIds.Count;

        /// <summary>获取指定阵营已解锁的卡片数量。</summary>
        public int GetFactionUnlockedCount(CardFaction faction)
        {
            return _allCards.Values
                .Where(c => c.Faction == faction && _collection.UnlockedCardIds.Contains(c.Id))
                .Count();
        }

        /// <summary>获取指定阵营的全部卡片。</summary>
        public List<WisdomCardSO> GetFactionCards(CardFaction faction)
        {
            return _allCards.Values.Where(c => c.Faction == faction).ToList();
        }

        /// <summary>获取所有已解锁的卡片。</summary>
        public List<WisdomCardSO> GetUnlockedCards()
        {
            return _allCards.Values
                .Where(c => _collection.UnlockedCardIds.Contains(c.Id))
                .ToList();
        }

        /// <summary>检查是否全部收集。</summary>
        public bool IsAllCollected()
        {
            return _allCards.Values.All(c => _collection.UnlockedCardIds.Contains(c.Id));
        }

        /// <summary>获取可代币购买（未解锁）的卡片。</summary>
        public List<WisdomCardSO> GetPurchasableCards()
        {
            return _allCards.Values
                .Where(c => !_collection.UnlockedCardIds.Contains(c.Id))
                .ToList();
        }

        /// <summary>获取所有可进入随机掉落池的卡片。</summary>
        public List<WisdomCardSO> GetDropPoolCards()
        {
            return _allCards.Values.ToList(); // 所有卡片都可掉落
        }

        /// <summary>获取按书目分组的卡片。</summary>
        public Dictionary<string, List<WisdomCardSO>> GetCardsByBook()
        {
            return _allCards.Values
                .GroupBy(c => c.BookSource)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
