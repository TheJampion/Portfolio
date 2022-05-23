using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythrenFighter
{
    public class GameSceneManager : MonoBehaviour
    {
        // Constants
        private const float EXTRA_SCENE_LOAD_TIME = 3;
        public const string MAIN_MENU_SCENE_NAME = "MainMenu";
        public const string BATTLE_SCENE_NAME = "Battle";

        public static GameSceneManager Instance { get; private set; }

        // Variables
        public string previousScene;
        public string currentScene;
        public bool sceneLoadInProgress;

        // Dependencies
        [Header("Assign These")]
        [SerializeField]
        private UI sceneLoadUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                previousScene = "";
                currentScene = getCurrentSceneName();
            }
        }

        public static string getCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public void Quit()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        public void loadScene(string sceneName)
        {
            if (currentScene.Contains(MAIN_MENU_SCENE_NAME))
            {

            }
            else if (currentScene.Contains(BATTLE_SCENE_NAME))
            {

            }

            if (sceneName.Contains(MAIN_MENU_SCENE_NAME))
            {
                
            }
            else if (sceneName.Contains(BATTLE_SCENE_NAME))
            {

            }

            loadSceneByName(sceneName);
        }

        private void loadSceneByName(string sceneName)
        {
            previousScene = currentScene;
            currentScene = sceneName;
            StartCoroutine(loadNewScene(sceneName));
        }

        private IEnumerator loadNewScene(string sceneName)
        {
            yield return 0;

            sceneLoadUI.EnableUI();
            sceneLoadInProgress = true;
            Time.timeScale = 0;

            yield return new WaitForSecondsRealtime(EXTRA_SCENE_LOAD_TIME);

            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            while (!async.isDone)
            {
                yield return null;
            }

            Time.timeScale = 1;
            sceneLoadInProgress = false;
            sceneLoadUI.DisableUI();
        }

    }
}