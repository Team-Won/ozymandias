using UnityEngine;
using UnityEngine.UI;
using static Managers.GameManager;
using DG.Tweening;
using TMPro;

namespace UI
{
    public class Stability : UiUpdater
    {
        [SerializeField] private Sprite[] chevrons;
        [SerializeField] private RectTransform threatBar, defenceBadge, threatBadge;
        [SerializeField] private Image direction;
        [SerializeField] private TextMeshProUGUI defenceCount, threatCount;
        private const float MinWidth = 50f;
        private const float BarLength = 600f;
        private const float Height = 30;
        private int _oldDefence, _oldThreat;
        
        protected override void UpdateUi()
        {
            int defence = Manager.Defence;
            int threat = Manager.Threat;
            int change = defence - threat;
            
            if (_oldDefence != defence)
            {
                defenceCount.text = defence.ToString();
                PunchBadge(defenceBadge);
                _oldDefence = defence;
            }

            if (_oldThreat != threat)
            {
                threatCount.text = threat.ToString();
                PunchBadge(threatBadge);
                _oldThreat = threat;
            }
            
            float width = MinWidth + BarLength * (100 - Mathf.Max(Manager.Stability, 0)) / 100f;
            threatBar.DOSizeDelta(new Vector2(width, Height), 0.5f);

            direction.enabled = change != 0 && Manager.Stability > 0;
            direction.rectTransform.DOAnchorPosX(Mathf.Clamp(-width-30f, -610,-110), 0.5f);
            direction.rectTransform.DORotate(new Vector3(0,0, change > 0 ? 90: -90), 0.5f);
            direction.sprite = chevrons[Mathf.Clamp(Mathf.Abs(change) / 5, 0, 2)];
        }
        
        private void PunchBadge(RectTransform badge)
        {
            badge.DOPunchScale(new Vector3(0.2f,0.2f,0), 0.5f);
        }
    }
}
