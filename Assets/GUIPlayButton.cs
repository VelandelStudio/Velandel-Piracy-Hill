using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VelandelPiracyHill
{
    public class GUIPlayButton : MonoBehaviour {

        public GameObject PlayButton;
        public GameObject PlayButtonOverlayed;
        public GameManager GameManager;
        private void OnMouseOver()
        {
            Debug.Log("Hello world");
            PlayButtonOverlayed.SetActive(true);
            PlayButton.SetActive(false);

            if (Input.GetMouseButtonUp(0))
            {
                GameManager.JoinGame();
            }
        }

        private void OnMouseExit()
        {
            PlayButton.SetActive(true);
            PlayButtonOverlayed.SetActive(false);
        }
    }
}
