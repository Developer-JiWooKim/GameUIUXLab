using System;
using System.Threading;
using UnityEngine;

namespace Assets.MyAssets.PRACTICE_Assets.Scripts.Study.Test1
{
    /// <summary>
    /// 테스트 1. 재시도 가능한 업로드 표시기.  [Unity C#]
    ///
    /// 파일 업로드 버튼을 눌렀다고 가정한다. 업로드는 1초 걸린다.
    /// 업로드 중에 다시 누르면, 이전 시도는 버리고 처음부터 새로 1초를 기다린다.
    /// (5-4의 "무시한다" 와는 다른 동작이다. 여기서는 "최신 요청이 이긴다".)
    ///
    /// 규칙
    ///   1. Upload() 가 불리면 즉시 Status = "업로드 중..." 로 바뀐다
    ///   2. 이미 업로드가 진행 중이었다면, 그 이전 시도는 버려지고 이번 것으로 대체된다
    ///      (이전 시도는 어떤 상태 변경도, 카운트 증가도 남기지 않아야 한다)
    ///   3. 1초 뒤, 그 사이 대체되지 않았다면 Status = "완료!", UploadCount 1 증가, IsUploading = false
    ///   4. OnDisable / OnDestroy 에서도 진행 중인 업로드를 정리한다
    ///
    /// 검증 (컴포넌트를 씬에 올리고 재생)
    ///   Upload();
    ///   await 0.3초;
    ///   Upload();                    ← 새 시도가 이전 것을 대체
    ///   await 0.3초;                 (두 번째 호출로부터 0.3초 경과)
    ///     Status == "업로드 중..."   (아직 1초 안 지남)
    ///     UploadCount == 0
    ///   await 0.7초 더;              (두 번째 호출로부터 1.0초 경과, 완료 시점)
    ///     Status == "완료!"
    ///     UploadCount == 1           ← 2 가 아니다. 첫 번째 시도는 아무것도 남기지 않았다
    ///     IsUploading == false
    ///
    /// 힌트는 없다. 지금까지 배운 것 중 어떤 패턴이 여기 맞는지 스스로 판단할 것.
    /// </summary>
    public sealed class T1_Uploader : MonoBehaviour
    {
        [SerializeField] private float uploadSeconds = 1f;

        public string Status { get; private set; } = "대기 중";
        private string[] statusValue = { "대기 중", "업로드 중...", "완료!" };
        private enum E_Status
        {
            Wait,
            Uploading,
            Complete,
        }
        private E_Status e_Status = E_Status.Wait;
        private CancellationTokenSource cts;

        public bool IsUploading { get; private set; }

        /// <summary>채점용. 완료까지 도달한 횟수.</summary>
        public int UploadCount { get; private set; }

        public void Upload()
        {
            IsUploading = true;
            e_Status = E_Status.Uploading;
            Status = statusValue[(int)e_Status];

            _ = UploadAwait();
        }

        private async Awaitable UploadAwait()
        {
            Cancel();

            cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);

            try
            {
                await Awaitable.WaitForSecondsAsync(uploadSeconds, cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            UploadCount++;
            IsUploading = false;
            e_Status = E_Status.Complete;
            Status = statusValue[(int)e_Status];
        }

        private void Cancel()
        {
            if (cts == null)
            {
                return;
            }
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }

        private void OnDisable()
        {
            Cancel();
        }
        private void OnDestroy()
        {
            Cancel();
        }
    }
}
