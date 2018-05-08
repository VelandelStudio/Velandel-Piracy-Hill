using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VelandelPiracyHill
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Image healthBarImage;
        float scale;

        private void Awake()
        {
            scale = healthBarImage.transform.localScale.x;
        }

        public void SetHealthBarValue(float value)
        {
            healthBarImage.fillAmount = value;
            healthBarImage.transform.localScale = new Vector3(Mathf.Lerp(0, scale, value), healthBarImage.transform.localScale.y, healthBarImage.transform.localScale.z);
            healthBarImage.color = Color.Lerp(Color.red, Color.green, value);
        }
    }
}
