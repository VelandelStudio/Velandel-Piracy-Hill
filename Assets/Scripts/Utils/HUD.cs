using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VelandelPiracyHill
{
    public class HUD : MonoBehaviour
    {
        static HUD instance;

        void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}