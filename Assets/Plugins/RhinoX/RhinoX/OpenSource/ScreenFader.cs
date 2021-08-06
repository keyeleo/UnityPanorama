using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Tool Class for Screen Fading, put on canvas which has a large child image
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        public Canvas canvas;

        static ScreenFader instance;
 
        public static ScreenFader Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<ScreenFader>();
                }
                return instance;
            }
        }

        /// <summary>
        /// Screen fade out, in 'duration' time
        /// </summary>
        /// <param name="duration">fade out time</param>
        [ContextMenu("Fade Out")]
        public static void FadeOut(float duration)
        {
            if (!Instance)
            {
                return;
            }
            Instance.StartCoroutine(Instance.FadeOut());
        }

        /// <summary>
        /// Screen fade in, in 'duration' time
        /// </summary>
        /// <param name="duration">fade out time</param>
        [ContextMenu("Fade In")]
        public static void FadeIn(float duration)
        {
            if(!Instance)
            {
                return;
            }
            Instance.StartCoroutine(Instance.FadeIn());
        }

        /// <summary>
        /// 逐渐变黑
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeOut()
        {
            canvas.enabled = true;
            Image img = Instance.GetComponentInChildren<Image>();
            img.enabled = true;
            float st = Time.time;
            while((Time.time-st)<=1)
            {
                float _alpha = Time.time - st / 1;
                img.color = new Color(0, 0, 0, _alpha);
                yield return null;
            }
            img.color = new Color(0, 0, 0, 1);
            canvas.enabled = true;
            img.enabled = true;
        }

        /// <summary>
        /// 逐渐变透明
        /// </summary>
        /// <returns></returns>
        IEnumerator FadeIn()
        {
            //Debug.Log("Fade in start");
            canvas.enabled = true;
            Image img = Instance.GetComponentInChildren<Image>();
            img.enabled = true;
            float st = Time.time;
            while ((Time.time - st) <= 1)
            {
                float _alpha = 1 - (Time.time - st / 1);
                img.color = new Color(0, 0, 0, _alpha);
                yield return null;
            }
            img.color = new Color(0, 0, 0, 0);
            img.enabled = false;
            canvas.enabled = false;
            //Debug.Log("Fade in done");
        }
    }
}
