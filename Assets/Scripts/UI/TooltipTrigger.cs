using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string content;
        [SerializeField] 
        private bool changesOffset;
        private Coroutine _delay;
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _delay = StartCoroutine(DelayToggle());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StopCoroutine(_delay);
            TooltipSystem.Instance.Hide();
        }

        private IEnumerator DelayToggle()
        {
            yield return new WaitForSeconds(0.5f);
            TooltipSystem.Instance.Show(content, changesOffset);
        }
    }
}
