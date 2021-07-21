using System;
using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private string[] loadingTips;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void LoadMain()
        {
            StartCoroutine(LoadAsyncOperation("Main", () => !GameManager.IsLoading));
        }
        
        public void LoadMenu()
        {
            StartCoroutine(LoadAsyncOperation("Menu"));
        }

        private IEnumerator LoadAsyncOperation(string scene, Func<bool> endConditions = null)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
            var gameLevel = SceneManager.LoadSceneAsync(scene);
            while (!gameLevel.isDone || endConditions != null && !endConditions())
            {
                progressBar.value = Mathf.Clamp01((gameLevel.progress + 0.05f) / 0.9f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
