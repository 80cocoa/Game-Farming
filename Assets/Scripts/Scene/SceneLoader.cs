using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace MyGame.Transition
{
    public class SceneLoader : MonoBehaviour
    {
        private HashSet<string> _loadedScenes = new();
        [SerializeField]private Scene currentScene;

        private void Start()
        {
            // 将已加载的场景存入HashSet中
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    _loadedScenes.Add(scene.name);
                }
            }
        }
        
        private void OnEnable()
        {
            EventBus.OnSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            EventBus.OnSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged()
        {
            if (_loadedScenes.Contains(currentScene.name))
            {
                _loadedScenes.Remove(currentScene.name);
                currentScene = SceneManager.GetActiveScene();
            }
            else
            {
                currentScene = SceneManager.GetActiveScene();
            }
        }
    }
}