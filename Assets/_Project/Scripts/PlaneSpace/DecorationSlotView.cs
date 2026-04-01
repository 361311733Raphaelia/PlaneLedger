using PlaneLedger.Data.SO;
using UnityEngine;

namespace PlaneLedger.PlaneSpace
{
    /// <summary>
    /// 单个装饰槽位的视觉组件。
    /// 挂载到每个槽位 GameObject 上，负责 Sprite 切换和特效管理。
    /// </summary>
    public class DecorationSlotView : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Transform _effectRoot;

        private GameObject _currentEffect;

        /// <summary>设置装饰品显示。</summary>
        public void SetDecoration(DecorationSO decoration)
        {
            if (_spriteRenderer != null && decoration.SceneSprite != null)
            {
                _spriteRenderer.sprite = decoration.SceneSprite;
                _spriteRenderer.enabled = true;
            }

            // 清除旧特效
            ClearEffect();

            // 生成新特效
            if (decoration.HasSpecialEffect && decoration.EffectPrefab != null && _effectRoot != null)
            {
                _currentEffect = Instantiate(decoration.EffectPrefab, _effectRoot);
            }
        }

        /// <summary>设为空槽位。</summary>
        public void SetEmpty()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = null;
                _spriteRenderer.enabled = false;
            }
            ClearEffect();
        }

        private void ClearEffect()
        {
            if (_currentEffect != null)
            {
                Destroy(_currentEffect);
                _currentEffect = null;
            }
        }
    }
}
