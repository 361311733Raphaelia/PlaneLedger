using UnityEngine;
using UnityEngine.Events;

namespace PlaneLedger.PlaneSpace
{
    /// <summary>
    /// 场景中的可交互点。点击后触发绑定的功能（打开面板等）。
    /// 挂载到书桌（→记账）、行李箱（→商店）等场景物件上。
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneInteractionPoint : MonoBehaviour
    {
        [Header("交互配置")]
        [SerializeField] private string _interactionName; // 用于提示文字
        [SerializeField] private UnityEvent _onClicked;

        [Header("视觉反馈")]
        [SerializeField] private float _hoverScale = 1.05f;
        [SerializeField] private float _scaleSpeed = 8f;

        private Vector3 _originalScale;
        private bool _isHovered;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void Update()
        {
            // 平滑缩放反馈
            Vector3 targetScale = _isHovered ? _originalScale * _hoverScale : _originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * _scaleSpeed);
        }

        private void OnMouseEnter()
        {
            _isHovered = true;
        }

        private void OnMouseExit()
        {
            _isHovered = false;
        }

        private void OnMouseDown()
        {
            _onClicked?.Invoke();
        }
    }
}
