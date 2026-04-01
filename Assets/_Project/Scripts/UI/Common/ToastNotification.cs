using System.Collections;
using TMPro;
using UnityEngine;

namespace PlaneLedger.UI
{
    /// <summary>
    /// Toast 通知组件。用于短暂显示提示信息（成就解锁、代币获取等）。
    /// 挂载到 Canvas_Popup 下的 Toast 预制体上。
    /// </summary>
    public class ToastNotification : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("动画配置")]
        [SerializeField] private float _displayDuration = 2.5f;
        [SerializeField] private float _fadeInDuration = 0.3f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        private static ToastNotification _instance;

        private void Awake()
        {
            _instance = this;
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        /// <summary>显示一条 Toast 消息。</summary>
        public static void Show(string message)
        {
            if (_instance != null)
            {
                _instance.ShowMessage(message);
            }
        }

        private void ShowMessage(string message)
        {
            StopAllCoroutines();
            if (_messageText != null)
                _messageText.text = message;
            gameObject.SetActive(true);
            StartCoroutine(AnimateToast());
        }

        private IEnumerator AnimateToast()
        {
            // Fade in
            float timer = 0f;
            while (timer < _fadeInDuration)
            {
                timer += Time.unscaledDeltaTime;
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / _fadeInDuration);
                yield return null;
            }

            // Display
            yield return new WaitForSecondsRealtime(_displayDuration);

            // Fade out
            timer = 0f;
            while (timer < _fadeOutDuration)
            {
                timer += Time.unscaledDeltaTime;
                if (_canvasGroup != null)
                    _canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / _fadeOutDuration);
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
