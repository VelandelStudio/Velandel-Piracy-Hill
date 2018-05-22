using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NicknameField : MonoBehaviour {

    [SerializeField] private InputField inputField;
    [SerializeField] private Button validateButton;

    public void CheckNickname()
    {
        validateButton.interactable = inputField.text.Length > 3 && inputField.text.Length <= 12;
    }
}
