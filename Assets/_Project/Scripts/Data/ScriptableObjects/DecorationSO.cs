using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Data.SO
{
    /// <summary>
    /// 装饰品配置数据。每件装饰品一个 SO 实例。
    /// 路径: Assets/_Project/ScriptableObjects/Decorations/
    /// </summary>
    [CreateAssetMenu(fileName = "Decoration_", menuName = "PlaneLedger/Decoration")]
    public class DecorationSO : ScriptableObject
    {
        [Header("基础信息")]
        public string Id;
        public string DisplayName;
        [TextArea(1, 2)]
        public string Description;

        [Header("视觉")]
        public Sprite Icon;           // 商店/图鉴中的图标
        public Sprite SceneSprite;    // 位面空间中的场景Sprite
        [TextArea(1, 3)]
        public string VisualDescription; // 视觉描述（给美术的参考）

        [Header("分类")]
        public DecorationSlot Slot;
        public DecorationStyle Style;
        public Rarity Rarity;

        [Header("获取")]
        public bool CanDropFromPool = true;    // 是否进入随机掉落池
        public int TokenPrice;                  // 代币购买价格，0=不可购买
        public string UnlockAchievementId;      // 绑定的成就ID，空=无绑定

        [Header("特效")]
        public bool HasSpecialEffect;
        public GameObject EffectPrefab;         // 特效预制体（氛围槽位用）

        [Header("元数据")]
        public string Version = "1.0";          // 加入的版本号

        /// <summary>重复获取时转换的代币数量</summary>
        public int DuplicateTokenValue
        {
            get
            {
                switch (Rarity)
                {
                    case Rarity.Common: return 10;
                    case Rarity.Rare: return 20;
                    case Rarity.Epic: return 50;
                    default: return 0;
                }
            }
        }
    }
}
