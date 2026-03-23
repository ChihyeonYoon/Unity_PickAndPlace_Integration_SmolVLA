using UnityEngine;
using UnityEngine.SceneManagement; // SceneManagement 네임스페이

public class SceneResetter : MonoBehaviour
{
    public void ResetCurrentScene()
    {
        // 현재 활성화된 씬의 이름을 가져와 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}