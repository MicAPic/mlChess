using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance;
        public Tooltip tooltip;

        void Awake()
        {
            Instance = this;
        }

        public void Show(string content)
        {
            tooltip.SetText(content);
            
            tooltip.group.DOFade(1.0f, 0.25f);
        }
        
        public void Hide()
        {
            tooltip.group.DOFade(0.0f, 0.25f);
        }
    }
}
