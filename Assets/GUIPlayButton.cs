using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VelandelPiracyHill
{
    public class GUIPlayButton : MonoBehaviour {

        public GameObject PlayButton;
        public GameObject PlayButtonOverlayed;
        public GameManager GameManager;
        public GameObject NickNameChoicePanel;
        private bool Activated;

        private void OnMouseOver()
        {
            PlayButtonOverlayed.SetActive(true);
            PlayButton.SetActive(false);

            if (!Activated && Input.GetMouseButtonUp(0))
            {
                Activated = true;
                NickNameChoicePanel.SetActive(true);
            }
        }

        private void OnMouseExit()
        {
            if (!Activated)
            {
                PlayButton.SetActive(true);
                PlayButtonOverlayed.SetActive(false);
            }
        }
    }
}
