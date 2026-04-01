using PlaneLedger.Data.Models;
using UnityEngine;

namespace PlaneLedger.Data.SO
{
    /// <summary>
    /// 思想残章卡片配置数据。每张卡片一个 SO 实例。
    /// 路径: Assets/_Project/ScriptableObjects/WisdomCards/
    /// </summary>
    [CreateAssetMenu(fileName = "Card_", menuName = "PlaneLedger/WisdomCard")]
    public class WisdomCardSO : ScriptableObject
    {
        [Header("基础信息")]
        public string Id;
        public string DisplayName;      // 卡片名称

        [Header("归属")]
        public string BookSource;       // 所属书目名称
        public CardFaction Faction;     // 阵营：财富 or 健康
        public Rarity Rarity;

        [Header("内容")]
        [TextArea(2, 5)]
        public string OriginalQuote;    // 正面：原文摘录（意译）
        [TextArea(2, 5)]
        public string PlaneAnnotation;  // 背面：位面注解（世界观改写）

        [Header("获取")]
        public int TokenPrice = 30;     // 代币指定购买价格
    }
}
