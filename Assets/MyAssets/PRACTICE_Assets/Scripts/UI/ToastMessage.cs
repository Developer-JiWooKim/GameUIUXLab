using TMPro;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.UI
{
    public class ToastMessage : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float lifeTime = 2f;

        public async Awaitable Show(string message)
        {
            messageText.text = message;
            await Awaitable.WaitForSecondsAsync(lifeTime, destroyCancellationToken);
        }
    }
}
