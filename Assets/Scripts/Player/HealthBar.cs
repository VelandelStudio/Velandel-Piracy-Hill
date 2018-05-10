using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VelandelPiracyHill
{
    /// <summary>
    /// HealthBar Class
    /// Dealing with the UI Image of LifeBar
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Image healthBarImage;
        float scale;

        private void Awake()
        {
            scale = healthBarImage.transform.localScale.x;
        }

        /// <summary>
        /// Set the size and the color of remaining life
        /// </summary>
        /// <param name="value">purcent of remaining life</param>
        public void SetHealthBarValue(float value)
        {
            healthBarImage.fillAmount = value;
            healthBarImage.transform.localScale = new Vector3(Mathf.Lerp(0, scale, value), healthBarImage.transform.localScale.y, healthBarImage.transform.localScale.z);
            healthBarImage.color = Color.Lerp(Color.red, Color.green, value);
        }
    }
}
