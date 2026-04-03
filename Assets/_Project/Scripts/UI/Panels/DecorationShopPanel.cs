using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlaneLedger.UI;
using PlaneLedger.Collection;
using PlaneLedger.Currency;
using PlaneLedger.Core;
using PlaneLedger.Data.SO;
using PlaneLedger.UI.Common;

namespace PlaneLedger.UI.Panels
{
    /// <summary>
    /// 装饰商店面板，展示可购买的装饰物品并支持代币购买。
    /// </summary>
    public class DecorationShopPanel : BasePanel
    {
        /// <summary>商品列表容器</summary>
        [SerializeField] private Transform _shopItemContainer;

        /// <summary>关闭按钮</summary>
        [SerializeField] private Button _closeButton;

        /// <summary>商品项预制体（含 Image图标 + TMP名称 + TMP价格 + Button购买按钮）</summary>
        [SerializeField] private GameObject _shopItemPrefab;

        /// <summary>当前代币余额文本</summary>
        [SerializeField] private TextMeshProUGUI _tokenText;

        /// <summary>
        /// 初始化面板，绑定关闭按钮事件。
        /// </summary>
        protected override void OnInitialize()
        {
            _closeButton.onClick.AddListener(() => PanelManager.Instance.CloseCurrentPanel());
        }

        /// <summary>
        /// 面板显示时刷新商店列表。
        /// </summary>
        protected override void OnShow()
        {
            RefreshShop();
        }

        /// <summary>
        /// 面板隐藏时的回调。
        /// </summary>
        protected override void OnHide() { }

        /// <summary>
        /// 刷新商店，重新生成所有可购买装饰的商品项。
        /// </summary>
        private void RefreshShop()
        {
            var decorationSystem = ServiceLocator.Get<DecorationSystem>();
            var tokenManager = ServiceLocator.Get<TokenManager>();

            // 更新代币余额显示
            _tokenText.text = $"代币: {tokenManager.Balance}";

            // 清空容器中的旧项
            for (int i = _shopItemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_shopItemContainer.GetChild(i).gameObject);
            }

            // 获取可购买的装饰列表
            var purchasable = decorationSystem.GetPurchasableDecorations();

            foreach (var deco in purchasable)
            {
                CreateShopItem(deco, tokenManager, decorationSystem);
            }
        }

        /// <summary>
        /// 创建单个商品项UI元素。
        /// </summary>
        /// <param name="deco">装饰SO数据</param>
        /// <param name="tokenManager">代币管理器</param>
        /// <param name="decorationSystem">装饰系统</param>
        private void CreateShopItem(DecorationSO deco, TokenManager tokenManager, DecorationSystem decorationSystem)
        {
            var item = Instantiate(_shopItemPrefab, _shopItemContainer);

            // 设置图标
            var icon = item.GetComponentInChildren<Image>();
            if (icon != null && deco.Icon != null)
            {
                icon.sprite = deco.Icon;
            }

            // 设置名称和价格文本
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = deco.DisplayName;
                texts[1].text = $"{deco.TokenPrice} 代币";
            }

            // 绑定购买按钮
            var buyButton = item.GetComponentInChildren<Button>();
            if (buyButton != null)
            {
                string decoId = deco.Id;
                int price = deco.TokenPrice;
                string displayName = deco.DisplayName;

                buyButton.onClick.AddListener(() =>
                {
                    if (!tokenManager.CanAfford(price))
                    {
                        ToastNotification.Show("代币不足！");
                        return;
                    }

                    if (tokenManager.TrySpend(price, $"购买装饰: {displayName}"))
                    {
                        if (decorationSystem.Unlock(decoId))
                        {
                            ToastNotification.Show($"成功解锁: {displayName}");
                            RefreshShop();
                        }
                    }
                });
            }
        }
    }
}
