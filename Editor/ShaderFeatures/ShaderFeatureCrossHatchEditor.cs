using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace M8.URP {
    public class ShaderFeatureCrossHatchEditor : IShaderFeatureEditor {
        public const string propModeShade = "_ShadeMode";

        public const string keywordShadeGradient = "_SHADE_GRADIENT";

        public enum ShadeMode {
            None,
            Gradient
        }

        public MaterialProperty shadeGradientMap;

        public void Setup(MaterialProperty[] properties) {
            shadeGradientMap = EditorUtils.FindProperty("_ShadeGradientMap", properties);
        }

        public void MaterialChanged(Material material) {
            var shadeMode = (ShadeMode)material.GetInt(propModeShade);
            switch(shadeMode) {
                case ShadeMode.None:
                    CoreUtils.SetKeyword(material, keywordShadeGradient, false);
                    break;
                case ShadeMode.Gradient:
                    CoreUtils.SetKeyword(material, keywordShadeGradient, true);
                    break;
            }
        }

        public void OnGUI(MaterialEditor materialEditor) {
            var material = materialEditor.target as Material;

            var shadeMode = (ShadeMode)EditorGUILayout.EnumPopup("Shade Mode", (ShadeMode)material.GetInt(propModeShade));
            material.SetInt(propModeShade, (int)shadeMode);

            switch(shadeMode) {
                case ShadeMode.Gradient:
                    shadeGradientMap.textureValue = EditorGUILayout.ObjectField(shadeGradientMap.textureValue, typeof(Texture2D), false) as Texture;
                    break;
            }
        }
    }
}