using System.Collections.Generic;
using System.Linq;
using PlaneLedger.Core;
using PlaneLedger.Data.Models;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.Collection
{
    /// <summary>
    /// 装饰品系统。管理装饰的解锁、装备、查询。
    /// </summary>
    public class DecorationSystem
    {
        private readonly CollectionData _collection;
        private readonly Dictionary<string, DecorationSO> _allDecorations;

        public DecorationSystem(SaveData saveData, DecorationSO[] allDecorations)
        {
            _collection = saveData.Collection;
            _allDecorations = new Dictionary<string, DecorationSO>();
            foreach (var deco in allDecorations)
            {
                _allDecorations[deco.Id] = deco;
            }
        }

        /// <summary>解锁一件装饰品。返回是否为新解锁（非重复）。</summary>
        public bool Unlock(string decorationId)
        {
            if (!_allDecorations.ContainsKey(decorationId))
            {
                Debug.LogWarning($"[Decoration] 未知装饰ID: {decorationId}");
                return false;
            }

            bool isNew = _collection.UnlockedDecorationIds.Add(decorationId);

            if (isNew)
            {
                var deco = _allDecorations[decorationId];
                EventBus.Publish(new DecorationUnlocked
                {
                    DecorationId = decorationId,
                    Rarity = (int)deco.Rarity
                });
                Debug.Log($"[Decoration] 新解锁: {deco.DisplayName} ({deco.Rarity})");
            }

            return isNew;
        }

        /// <summary>装备一件装饰品到指定槽位。</summary>
        public bool Equip(int slotIndex, string decorationId)
        {
            if (slotIndex < 0 || slotIndex > 6) return false;
            if (!_collection.UnlockedDecorationIds.Contains(decorationId)) return false;

            // 验证槽位匹配
            if (_allDecorations.TryGetValue(decorationId, out var deco))
            {
                if ((int)deco.Slot != slotIndex)
                {
                    Debug.LogWarning($"[Decoration] 槽位不匹配: {deco.DisplayName} 属于 {deco.Slot}, 尝试装备到 {slotIndex}");
                    return false;
                }
            }

            string previousId = "";
            if (_collection.EquippedDecorations.ContainsKey(slotIndex))
            {
                previousId = _collection.EquippedDecorations[slotIndex];
            }

            _collection.EquippedDecorations[slotIndex] = decorationId;

            // 更新槽位更换计数
            if (!_collection.SlotChangeCount.ContainsKey(slotIndex))
                _collection.SlotChangeCount[slotIndex] = 0;
            _collection.SlotChangeCount[slotIndex]++;

            EventBus.Publish(new DecorationEquipped
            {
                SlotIndex = slotIndex,
                DecorationId = decorationId,
                PreviousDecorationId = previousId
            });

            return true;
        }

        /// <summary>获取指定槽位当前装备的装饰品ID。</summary>
        public string GetEquipped(int slotIndex)
        {
            return _collection.EquippedDecorations.TryGetValue(slotIndex, out var id) ? id : "";
        }

        /// <summary>获取指定槽位可用的所有已解锁装饰品。</summary>
        public List<DecorationSO> GetUnlockedForSlot(int slotIndex)
        {
            return _allDecorations.Values
                .Where(d => (int)d.Slot == slotIndex && _collection.UnlockedDecorationIds.Contains(d.Id))
                .ToList();
        }

        /// <summary>获取装饰品配置。</summary>
        public DecorationSO GetDecoration(string id)
        {
            return _allDecorations.TryGetValue(id, out var deco) ? deco : null;
        }

        /// <summary>已解锁装饰品总数。</summary>
        public int UnlockedCount => _collection.UnlockedDecorationIds.Count;

        /// <summary>检查某风格的装饰是否全部解锁。</summary>
        public bool IsStyleComplete(DecorationStyle style)
        {
            var styleDecos = _allDecorations.Values.Where(d => d.Style == style);
            return styleDecos.All(d => _collection.UnlockedDecorationIds.Contains(d.Id));
        }

        /// <summary>检查是否所有风格都已解锁完整。</summary>
        public bool AreAllStylesComplete()
        {
            var styles = new[] {
                DecorationStyle.Cthulhu, DecorationStyle.ChineseClassical,
                DecorationStyle.Modern, DecorationStyle.Futuristic, DecorationStyle.Fantasy
            };
            return styles.All(s => IsStyleComplete(s));
        }

        /// <summary>获取指定槽位的更换次数。</summary>
        public int GetSlotChangeCount(int slotIndex)
        {
            return _collection.SlotChangeCount.TryGetValue(slotIndex, out var count) ? count : 0;
        }

        /// <summary>获取所有可进入随机掉落池的装饰品（排除传说）。</summary>
        public List<DecorationSO> GetDropPoolDecorations()
        {
            return _allDecorations.Values
                .Where(d => d.CanDropFromPool && d.Rarity != Rarity.Legendary)
                .ToList();
        }

        /// <summary>获取可代币购买的装饰品（排除史诗和传说）。</summary>
        public List<DecorationSO> GetPurchasableDecorations()
        {
            return _allDecorations.Values
                .Where(d => d.TokenPrice > 0 && !_collection.UnlockedDecorationIds.Contains(d.Id))
                .ToList();
        }
    }
}
