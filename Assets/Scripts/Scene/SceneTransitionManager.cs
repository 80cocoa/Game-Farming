using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement; //引用该命名空间实现场景切换方法

namespace MyGame.Transition
{
    public class SceneTransitionManager : Singleton<SceneTransitionManager>
    {
        [SerializeField] private SceneName startSceneName;
        [SerializeField] private UISettings_SO settings;

        private CanvasGroup _fadeCanvasGroup;

        private Scene _currentScene;
        public string CurrentSceneName => _currentScene.name ?? SceneManager.GetActiveScene().name;

        void OnEnable()
        {
            EventBus.OnSceneChanged += OnSceneChanged;
        }

        void OnDisable()
        {
            EventBus.OnSceneChanged -= OnSceneChanged;
        }

        void Start()
        {
            _fadeCanvasGroup = GameObject.FindWithTag("FadeUI").GetComponent<CanvasGroup>();

            StartCoroutine(LoadSceneAndSetActive(EnumUtils.MapToString(startSceneName)));
        }

        private void OnSceneChanged()
        {
            _currentScene = SceneManager.GetActiveScene();
        }

        /// <summary>
        /// 调用此方法切换场景
        /// </summary>
        /// <param name="sceneName">场景名</param>
        public void Transition(string sceneName)
        {
            StartCoroutine(TransitionScene(sceneName));
        }

        /// <summary>
        /// 切换场景协程
        /// </summary>
        /// <param name="sceneName">切换至场景名</param>
        /// <returns></returns>
        private IEnumerator TransitionScene(string sceneName)
        {
            EventBus.RaiseSceneUnloading();

            yield return FadeOut();

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            yield return LoadSceneAndSetActive(sceneName);

            yield return new WaitForSeconds(settings.fadeDuration);

            yield return FadeIn();
        }

        /// <summary>
        /// 加载场景并激活
        /// </summary>
        /// <param name="sceneName">场景名</param>
        /// <returns></returns>
        private IEnumerator LoadSceneAndSetActive(string sceneName)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            Scene newScene = SceneManager.GetSceneByName(sceneName);

            SceneManager.SetActiveScene(newScene);

            EventBus.RaiseSceneChanged();
        }


        private IEnumerator FadeOut()
        {
            _fadeCanvasGroup.blocksRaycasts = true;

            yield return _fadeCanvasGroup.DOFade(1, settings.fadeOutDuration)
                .OnComplete(EventBus.RaiseFadedOut)
                .WaitForCompletion();
        }

        private IEnumerator FadeIn()
        {
            _fadeCanvasGroup.blocksRaycasts = false;

            yield return _fadeCanvasGroup.DOFade(0, settings.fadeInDuration)
                .OnComplete(EventBus.RaiseFadedIn)
                .WaitForCompletion();
        }
    }
}