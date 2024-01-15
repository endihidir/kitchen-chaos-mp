using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class Loader
{
    private static Scene targetScene;

    public static Scene TargetScene => targetScene;

    public static async UniTask Load(Scene targetScene, bool loadNetwork = false, bool useLoadingScene = true)
    {
        Loader.targetScene = targetScene;

        if (useLoadingScene)
        {
            await SceneManager.LoadSceneAsync(Scene.LoadingScene.ToString());
        }

        if (loadNetwork)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(Loader.targetScene.ToString(), LoadSceneMode.Single);
        }
        else
        {
            SceneManager.LoadScene(Loader.targetScene.ToString());
        }
    }
}

public enum Scene
{
    MainMenuScene,
    GameScene,
    LoadingScene,
    LobbyScene,
    CharacterSelectScene
}