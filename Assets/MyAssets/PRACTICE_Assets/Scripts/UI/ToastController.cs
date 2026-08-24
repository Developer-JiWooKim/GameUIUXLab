using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UI
{
    public class ToastController : MonoBehaviour
    {
        [Header("Pool")]
        [SerializeField] private ToastMessage toastPrefab;
        [SerializeField] private RectTransform toastParent;
        [SerializeField] private int defaultCapacity = 5;
        [SerializeField] private int maxPoolSize = 10;

        private ObjectPool<ToastMessage> _toastPool;

        private void Awake()
        {
            _toastPool = new ObjectPool<ToastMessage>(
                createFunc: CreateToast,
                actionOnGet: OnGetToast,
                actionOnRelease: OnReleaseToast,
                actionOnDestroy: OnDestroyToast,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxPoolSize);
        }

        private void OnDestroy()
        {
            _toastPool?.Dispose();
        }

        public void ShowToast() => ShowToast("버튼을 클릭해서 Toast 프리팹을 활성화");

        private async void ShowToast(string message)
        {
            ToastMessage toast = _toastPool.Get();

            try
            {
                await toast.Show(message);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _toastPool.Release(toast);
        }

        private ToastMessage CreateToast()
        {
            ToastMessage toast = Instantiate(toastPrefab, toastParent);
            toast.gameObject.SetActive(false);
            return toast;
        }

        private void OnGetToast(ToastMessage toast)
        {
            toast.transform.SetAsLastSibling();
            toast.gameObject.SetActive(true);
        }

        private void OnReleaseToast(ToastMessage toast)
        {
            toast.gameObject.SetActive(false);
        }

        private void OnDestroyToast(ToastMessage toast)
        {
            if (toast != null)
            {
                Destroy(toast.gameObject);
            }
        }
    }
}
