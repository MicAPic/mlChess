using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class InGameUI : UserInterface
    {
        [Header("Animation")]
        public float pauseAnimationDuration = 0.25f;
        [SerializeField]
        private Animator transition;

        [Header("Utility")]
        public bool canPause = true;
    
        [Header("Interface Elements")]
        public TMP_Text statusBarText;
        public GameObject pauseScreen;
        public TooltipTrigger pauseButtonTooltipTrigger;
        public TooltipTrigger cameraButtonTooltipTrigger;
        public GameObject playIcon;
        public GameObject pauseBackground;
        public Button pauseButton;
        public RectTransform retireButton;
        public RectTransform menuButton;
        public Dictionary<Piece.PieceColour, TMP_Text> TakenPiecesListsUI;
        public Dictionary<Piece.PieceColour, TooltipTrigger> TakenPiecesListsTooltipTriggers;
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
        
        private Dictionary<Piece.PieceColour, int> _scores = new()
        {
            // Set of pieces taken
            {Piece.PieceColour.white, 0},
            {Piece.PieceColour.black, 0}
        };

        public IEnumerator ShowEndgameScreen(string titleText, string statusText)
        {
            canPause = false;
            
            yield return new WaitForSeconds(1.5f);
            ToggleSubmenu(pauseScreen);
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

            _scores[thisPieceColour] += IconToScore(takenPieceIcon);
            TakenPiecesListsTooltipTriggers[thisPieceColour].content = "Score: " + _scores[thisPieceColour];
        }

        private bool _canAnimate = true;
        public IEnumerator WaitUntilCanAnimate(float targetValue)
        {
            yield return new WaitUntil(() => _canAnimate);
            StartCoroutine(LerpSlider(targetValue));
        }

        private readonly string[] _tipContents = {"Unlocked", "Locked"};
        private int _tipIndex;
        public void ChangeCameraButtonTooltip()
        {
            var currentTip = "Camera: " + _tipContents[++_tipIndex % _tipContents.Length];
            TooltipSystem.Instance.tooltip.SetText(currentTip);
            cameraButtonTooltipTrigger.content = currentTip;
        }

        protected override IEnumerator SceneTransition(string sceneName)
        {
            var sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);
            sceneLoadOperation.allowSceneActivation = false;
            
            GameManager.Instance.isPaused = true;
            pauseButton.interactable = false;

            if (pauseScreen.activeInHierarchy)
            {
                ToggleSubmenu(pauseScreen);
            }

            float duration;
            if (sceneName == "Menu")
            {
                transition.enabled = true;
                duration = 1.0f;
            }
            else
            {
                duration = GameManager.Instance.currentDelay + .75f;
            }
            
            foreach (var takenPiecesList in TakenPiecesListsUI)
            {
                takenPiecesList.Value.DOColor(Color.clear, duration);
            }
            yield return new WaitForSeconds(duration);
            
            sceneLoadOperation.allowSceneActivation = true;
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

        int IconToScore(char pieceIcon)
        {
            return pieceIcon switch
            {
                '♟' or '♙' => 1,
                '♞' or '♝' or '♘' or '♗' => 3,
                '♜' or '♖' => 5,
                '♛' or '♕' => 9,
                _ => 0
            };
        }
    }
}
