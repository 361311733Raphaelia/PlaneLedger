using PlaneLedger.Collection;
using PlaneLedger.Core;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.PlaneSpace
{
    /// <summary>
    /// 位面空间场景控制器。管理 7 个装饰槽位的视觉呈现。
    /// 挂载到 MainPlane 场景的 DecorationRoot 对象上。
    /// </summary>
    public class PlaneSpaceController : MonoBehaviour
    {
        [Header("装饰槽位 (按顺序: 账本/窗帘/窗外/地毯/灯具/桌椅/氛围)")]
        [SerializeField] private DecorationSlotView[] _slots = new DecorationSlotView[7];

        private DecorationSystem _decorationSystem;

        private void Start()
        {
            _decorationSystem = ServiceLocator.Get<DecorationSystem>();
            RefreshAllSlots();

            EventBus.Subscribe<DecorationEquipped>(OnDecorationEquipped);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<DecorationEquipped>(OnDecorationEquipped);
        }

        /// <summary>刷新所有槽位的视觉显示。</summary>
        public void RefreshAllSlots()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                RefreshSlot(i);
            }
        }

        /// <summary>刷新单个槽位的视觉显示。</summary>
        public void RefreshSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length) return;
            if (_slots[slotIndex] == null) return;

            string decoId = _decorationSystem.GetEquipped(slotIndex);
            if (string.IsNullOrEmpty(decoId))
            {
                _slots[slotIndex].SetEmpty();
                return;
            }

            var deco = _decorationSystem.GetDecoration(decoId);
            if (deco != null)
            {
                _slots[slotIndex].SetDecoration(deco);
            }
        }

        private void OnDecorationEquipped(DecorationEquipped e)
        {
            RefreshSlot(e.SlotIndex);
        }
    }
}
