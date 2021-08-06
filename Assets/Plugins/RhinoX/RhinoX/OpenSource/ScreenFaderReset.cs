using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    public class ScreenFaderReset : MonoBehaviour
    {
        private void OnEnable()
        {
            ScreenFader.FadeIn(1f);
        }
    }
}
