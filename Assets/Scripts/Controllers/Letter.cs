using DG.Tweening;
using UnityEngine;
using static Managers.GameManager;

namespace Controllers
{
    public class Letter : MonoBehaviour
    {
        // Serialised Fields
        [SerializeField] private float animateInDuration = .5f;
        [SerializeField] private float animateOutDuration = .75f;

        private Canvas _canvas;

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            Close();
        }
        
        public void Open()
        {
            _canvas.enabled = true;
            Manager.EnterMenu();
            Jukebox.Instance.PlayScrunch();
            var rot = Random.Range(0f, -5f);
            transform.DOLocalRotate(new Vector3(0, 0, rot), animateInDuration);
            transform.DOLocalMove(Vector3.zero, animateInDuration);
        }
        
        public void Close()
        {
            Manager.ExitMenu();
            transform.DOLocalMove(new Vector3(-1000, 1500, 0), animateOutDuration);
            transform.DOLocalRotate(new Vector3(0, 0, 20), animateOutDuration)
                .OnComplete(() => _canvas.enabled = false);
        }
    }
}