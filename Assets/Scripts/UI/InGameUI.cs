using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : UserInterface
    {
        [Header("Animation")]
        [SerializeField]
        private Animator transition;
    
        [Header("Interface Elements")]
        public TMP_Text statusBarText;
        public GameObject pauseScreen;
        public GameObject playIcon;
        public Dictionary<Piece.PieceColour, TMP_Text> TakenPiecesListsUI;
        [SerializeField]
        private Slider evaluationSlider;
        [SerializeField]
        private TMP_Dropdown pieceDropdown;
    
        private Dictionary<Piece.PieceColour, List<char>> _takenPiecesLists = new()
        {
            // Set of pieces taken
            {Piece.PieceColour.white, new List<char>()},
            {Piece.PieceColour.black, new List<char>()}
        };
    
        public IEnumerator ShowEndgameScreen(string titleText, string statusText)
        {
            yield return new WaitForSeconds(1.5f);
            pauseScreen.SetActive(true);
            playIcon.SetActive(true);
            pauseScreen.GetComponentsInChildren<TMP_Text>()[0].text = titleText;
            pauseScreen.GetComponentsInChildren<TMP_Text>()[1].text = "New Game"; // instead of "resign"
            statusBarText.text = statusText;
        }

        public void ChangePieceForPromoting()
        {
            var gameManager = FindObjectOfType<GameManager>();
            int index = pieceDropdown.value;
            string newPieceForPromoting = pieceDropdown.options[index].text.Substring(1); // get the substring that doesn't include the piece symbol
        
            gameManager.promoteTo = newPieceForPromoting;
        }

        public void UpdateTakenPiecesList(Piece.PieceColour thisPieceColour, char takenPieceIcon)
        {
            // adds the taken piece to the taken piece list on the side of the screen
            _takenPiecesLists[thisPieceColour].Add(takenPieceIcon);
            _takenPiecesLists[thisPieceColour].Sort();
            TakenPiecesListsUI[thisPieceColour].text = string.Join("", _takenPiecesLists[thisPieceColour]);
        }

        private bool _canAnimate = true;
        public IEnumerator WaitUntilCanAnimate(float targetValue)
        {
            yield return new WaitUntil(() => _canAnimate);
            StartCoroutine(LerpSlider(targetValue));
        }

        protected override IEnumerator SceneTransition(string sceneName)
        {
            if (sceneName == "Menu")
            {
                transition.enabled = true;
            }
            // TODO: load a scene only after all animations are finished
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(sceneName);
        }

        IEnumerator LerpSlider(float targetValue)
        {
            _canAnimate = false;
        
            float timeElapsed = 0;
            while (timeElapsed < 1)
            {
                evaluationSlider.value = Mathf.Lerp(evaluationSlider.value, targetValue, timeElapsed / 1);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            evaluationSlider.value = targetValue;

            _canAnimate = true;
        }
    }
}
