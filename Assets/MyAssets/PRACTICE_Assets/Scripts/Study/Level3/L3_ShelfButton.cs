using Assets.MyAssets.PORTFOLIO_Assets.Scripts.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Level3
{
    /// <summary>
    /// 3-4. 첫 MonoBehaviour. 자기 Button 을 효율적으로 내어준다.
    ///
    /// 요구사항
    ///   1. type 은 [SerializeField] private
    ///   2. Button 을 100번 읽어도 GetComponentCallCount 는 1
    ///   3. Awake() 가 아직 안 불린 시점에 Button 을 읽어도 정상 동작한다
    ///   4. [RequireComponent] 를 붙인다
    ///
    /// 검증
    ///   for (int i = 0; i &lt; 100; i++) { var _ = shelfButton.Button; }
    ///   Debug.Log(shelfButton.GetComponentCallCount);   // 1
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class L3_ShelfButton : MonoBehaviour
    {
        [SerializeField] private DessertType type;

        public DessertType Type => type;

        private Button button;

        /// <summary>이 오브젝트의 Button. 몇 번을 읽어도 GetComponent 는 한 번만 부른다.</summary>
        public Button Button
        {
            get
            {
                if (button == null)
                {
                    button = GetComponent<Button>();
                    GetComponentCallCount++;
                }
                return button;
            }
        }
        // 기존코드 
        // public Button Button => button ??= GetComponent<Button>();
        // 이렇게 하면 문제의 의도인 GetComponentCallCount 값이 무조건 1인 것이 충족되지 않음,
        // 또한 ??= 는 진짜 참조 null만 보므로, 파괴된 컴포넌트를 캐시에 그대로 들고 있게 됨


        /// <summary>채점용. GetComponent 를 실제로 호출한 횟수.</summary>
        public int GetComponentCallCount { get; private set; }
    }
}
