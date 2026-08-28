using System;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Async
{
    /// <summary>
    /// A5. 파괴 후 깨어나는 버그 — 재현부터.
    ///
    /// 그냥 재생하면 3초 뒤 콘솔에 빨간 예외가 뜬다.
    ///   MissingReferenceException: The object of type 'A5_Worker' has been destroyed
    ///
    /// 무슨 일이 일어났나
    ///   0.0초  임시 오브젝트를 만들고 Worker.Run() 시작 → 3초 대기 진입
    ///   0.5초  그 오브젝트를 Destroy 한다
    ///   3.0초  대기가 끝나 코드가 깨어난다 → 이미 없는 자기 transform 을 건드린다 → 예외
    ///
    /// 과제 : A5_Worker.Run() 의 await 에 destroyCancellationToken 을 넘기고
    ///        try/catch (OperationCanceledException) 로 감싸서 예외를 없앤다.
    /// </summary>
    public sealed class A5_DestroyToken : MonoBehaviour
    {
        private async void Start()
        {
            GameObject temp = new GameObject("Temp");
            A5_Worker worker = temp.AddComponent<A5_Worker>();

            Debug.Log("0.0초 : Run() 시작");
            worker.Run();

            await Awaitable.WaitForSecondsAsync(0.5f);

            Debug.Log("0.5초 : Destroy 함. 2.5초 뒤를 지켜보세요.");
            Destroy(temp);
        }
    }

    public sealed class A5_Worker : MonoBehaviour
    {
        public async void Run()
        {
            try
            {
                // TODO: destroyCancellationToken 을 넘기고 try/catch 로 감싼다
                await Awaitable.WaitForSecondsAsync(3f, destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            // 여기 도달했을 때 이 오브젝트는 이미 파괴돼 있다
            Debug.Log("3.0초 : 깨어남. 내 위치는 " + transform.position);
        }
    }
}
