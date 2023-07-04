using TMPro;
using UnityEngine;

namespace UI
{
    public class Tooltip : MonoBehaviour
    {
        public CanvasGroup group;
        public bool changeOffset;
        [SerializeField] 
        private Vector2 defaultOffset = new Vector2(0.0f, 1.25f);
        [SerializeField] 
        private TMP_Text contentText;

        private RectTransform _rectTransform;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        // Update is called once per frame
        void Update()
        {
            var mousePos = Input.mousePosition;
            transform.position = mousePos;

            if (changeOffset)
            {
                var offsetX = mousePos.x / Screen.width;
                var offsetY = mousePos.y / Screen.height;
                _rectTransform.pivot = new Vector2(offsetX, offsetY);
            }
            else
            {
                _rectTransform.pivot = defaultOffset;
            }
        }
        
        public void SetText(string content)
        {
            contentText.text = content;
        }
    }
}
