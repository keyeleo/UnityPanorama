using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ximmerse.RhinoX
{
    /// <summary>
    /// Glow renderer by adding glowing material.
    /// </summary>
	public class GlowFX : MonoBehaviour
	{
        [SerializeField]
		Color m_glowColor=Color.white;

        /// <summary>
        /// Gets/Sets the glow color
        /// </summary>
        public Color GlowColor
        {
            get
            {
                return m_glowColor;
            }
            set
            {
                m_glowColor = value;
                if (glowMaterial)
                {
                    glowMaterial.color = m_glowColor;
                }
            }
        }

        [SerializeField]
		private Renderer[] renderers = { };
		private Material glowMaterial;
        const string kGlowShaderName = "HoloMuseum/Glow";
        private const string kPow = "_Pow";
        
        public float GlowMin = 0.03f, GlowMax = 2.5f;

        public AnimationCurve glowControlCurve = new AnimationCurve()
        {
            postWrapMode = WrapMode.PingPong,
            keys = AnimationCurve.EaseInOut(0, 0, 1, 1).keys,
        };

        void Awake()
		{
            if(!glowMaterial)
            {
                var glowShader = Shader.Find(kGlowShaderName);
                glowMaterial = new Material(glowShader);
                glowMaterial.color = m_glowColor;
                glowMaterial.SetFloat(kPow, 2.8f);
            }
        }

		void OnEnable()
		{
            foreach (var _renderer in renderers)
            {

                // Append outline shaders
                var materials = _renderer.sharedMaterials.ToList();

                materials.Add(glowMaterial);

                _renderer.materials = materials.ToArray();
            }
        }

        void OnDisable()
        {
            foreach (var _renderer in renderers)
            {
                // Remove outline shaders
                var materials = _renderer.sharedMaterials.ToList();
                materials.Remove(glowMaterial);
                _renderer.materials = materials.ToArray();
            }
        }

        private void Update()
        {
            float glowParam = Mathf.Lerp(this.GlowMin, this.GlowMax, glowControlCurve.Evaluate(Time.realtimeSinceStartup));
            glowMaterial.SetFloat(kPow, glowParam);
        }

        [ContextMenu("Setup renderers")]
        public void SetupChildrenRenderers()
        {
            this.renderers = GetComponentsInChildren<Renderer>(true);
        }
    }
}