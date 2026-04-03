using PlaneLedger.Core;
using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.Data
{
    /// <summary>
    /// 数据库管理器，提供对所有 ScriptableObject 数据资源的集中访问。
    /// 挂载到场景中，通过 ServiceLocator 注册供全局使用。
    /// </summary>
    public class DatabaseManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("所有装饰物配置")]
        private DecorationSO[] _allDecorations;

        [SerializeField]
        [Tooltip("所有智慧卡牌配置")]
        private WisdomCardSO[] _allCards;

        [SerializeField]
        [Tooltip("所有成就配置")]
        private AchievementSO[] _allAchievements;

        /// <summary>
        /// 所有装饰物配置（只读）。
        /// </summary>
        public DecorationSO[] AllDecorations => _allDecorations;

        /// <summary>
        /// 所有智慧卡牌配置（只读）。
        /// </summary>
        public WisdomCardSO[] AllCards => _allCards;

        /// <summary>
        /// 所有成就配置（只读）。
        /// </summary>
        public AchievementSO[] AllAchievements => _allAchievements;

        /// <summary>
        /// 初始化时注册到 ServiceLocator。
        /// </summary>
        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        /// <summary>
        /// 销毁时从 ServiceLocator 注销。
        /// </summary>
        private void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }
    }
}
