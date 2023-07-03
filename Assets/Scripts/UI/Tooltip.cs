using TMPro;
using UnityEngine;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        public CanvasGroup group;
        [SerializeField] 
        private TMP_Text contentText;

        // Update is called once per frame
        void Update()
        {
            var mousePos = Input.mousePosition;
            transform.position = mousePos;
        }
        
        public void SetText(string content)
        {
            contentText.text = content;
        }
    }
}
