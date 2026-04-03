using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.UI;
using PlaneLedger.Collection;
using PlaneLedger.Core;
using PlaneLedger.Data.SO;
using PlaneLedger.UI.Common;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 卡牌收藏面板，按书籍分组展示已收集和未收集的智慧卡牌。
    /// </summary>
    public class CardCollectionPanel : BasePanel
    {
        /// <summary>卡牌列表容器</summary>
        [SerializeField] private Transform _cardListContainer;

        /// <summary>关闭按钮</summary>
        [SerializeField] private Button _closeButton;

        /// <summary>书籍分组预制体（含 TMP书名 + Transform cardContainer）</summary>
        [SerializeField] private GameObject _bookGroupPrefab;

        /// <summary>卡牌项预制体（含 TMP卡名 + TMP稀有度 + Image背景）</summary>
        [SerializeField] private GameObject _cardItemPrefab;

        /// <summary>收集进度文本（X/89）</summary>
        [SerializeField] private TextMeshProUGUI _collectionProgressText;

        /// <summary>总卡牌数量</summary>
        private const int TotalCardCount = 89;

        /// <summary>
        /// 初始化面板，绑定关闭按钮事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _closeButton.onClick.AddListener(() => PanelManager.Instance.CloseCurrentPanel());
        }

        /// <summary>
        /// 面板显示时刷新卡牌列表。
        /// </summary>
        protected override void OnShow()
        {
            RefreshCards();
        }

        /// <summary>
        /// 面板隐藏时的回调。
        /// </summary>
        protected override void OnHide() { }

        /// <summary>
        /// 刷新卡牌收藏列表，按书籍分组展示所有卡牌。
        /// </summary>
        private void RefreshCards()
        {
            var cardSystem = ServiceLocator.Get<WisdomCardSystem>();

            // 清空容器中的旧项
            for (int i = _cardListContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_cardListContainer.GetChild(i).gameObject);
            }

            // 获取已解锁的卡牌集合，用于快速查找
            var unlockedCards = cardSystem.GetUnlockedCards();
            var unlockedIds = new HashSet<string>();
            foreach (var card in unlockedCards)
            {
                unlockedIds.Add(card.Id);
            }

            // 按书分组获取所有卡牌
            var cardsByBook = cardSystem.GetCardsByBook();

            foreach (var bookGroup in cardsByBook)
            {
                CreateBookGroup(bookGroup.Key, bookGroup.Value, unlockedIds);
            }

            // 更新收集进度
            _collectionProgressText.text = $"{cardSystem.UnlockedCount}/{TotalCardCount}";
        }

        /// <summary>
        /// 创建一个书籍分组UI元素。
        /// </summary>
        /// <param name="bookName">书籍名称</param>
        /// <param name="cards">该书的所有卡牌</param>
        /// <param name="unlockedIds">已解锁的卡牌ID集合</param>
        private void CreateBookGroup(string bookName, List<WisdomCardSO> cards, HashSet<string> unlockedIds)
        {
            var group = Instantiate(_bookGroupPrefab, _cardListContainer);

            // 设置书名
            var bookNameText = group.GetComponentInChildren<TextMeshProUGUI>();
            if (bookNameText != null)
            {
                bookNameText.text = bookName;
            }

            // 获取卡牌容器（第一个子 Transform，跳过书名）
            var cardContainer = group.transform.Find("cardContainer");
            if (cardContainer == null && group.transform.childCount > 1)
            {
                cardContainer = group.transform.GetChild(1);
            }

            if (cardContainer == null)
            {
                cardContainer = group.transform;
            }

            foreach (var card in cards)
            {
                CreateCardItem(card, unlockedIds.Contains(card.Id), cardContainer);
            }
        }

        /// <summary>
        /// 创建单个卡牌项UI元素。
        /// </summary>
        /// <param name="card">卡牌SO数据</param>
        /// <param name="isUnlocked">是否已解锁</param>
        /// <param name="parent">父容器</param>
        private void CreateCardItem(WisdomCardSO card, bool isUnlocked, Transform parent)
        {
            var item = Instantiate(_cardItemPrefab, parent);

            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            if (isUnlocked)
            {
                // 已解锁：显示卡牌名称和稀有度
                if (texts.Length >= 1)
                {
                    texts[0].text = card.DisplayName;
                }
                if (texts.Length >= 2)
                {
                    texts[1].text = GetRarityDisplayName(card.Rarity);
                }

                // 点击已解锁卡牌显示原文引用
                var button = item.GetComponent<Button>();
                if (button == null)
                {
                    button = item.AddComponent<Button>();
                }

                string quote = card.OriginalQuote;
                button.onClick.AddListener(() =>
                {
                    ToastNotification.Show(quote);
                });
            }
            else
            {
                // 未解锁：显示 "???"
                if (texts.Length >= 1)
                {
                    texts[0].text = "???";
                }
                if (texts.Length >= 2)
                {
                    texts[1].text = "";
                }
            }
        }

        /// <summary>
        /// 获取稀有度的中文显示名称。
        /// </summary>
        /// <param name="rarity">稀有度枚举值</param>
        /// <returns>中文稀有度名称</returns>
        private string GetRarityDisplayName(PlaneLedger.Data.Models.Rarity rarity)
        {
            switch (rarity)
            {
                case PlaneLedger.Data.Models.Rarity.Common: return "普通";
                case PlaneLedger.Data.Models.Rarity.Rare: return "稀有";
                case PlaneLedger.Data.Models.Rarity.Epic: return "史诗";
                case PlaneLedger.Data.Models.Rarity.Legendary: return "传说";
                default: return "未知";
            }
        }
    }
}
