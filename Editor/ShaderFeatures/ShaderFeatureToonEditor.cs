using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace M8.URP {
    public class ShaderFeatureToonEditor : IShaderFeatureEditor {
        public const string propModeLight = "_LightMode";

        public const string keywordLightSingleStep = "_LIGHT_SINGLE_STEP";
        public const string keywordLightGradient = "_LIGHT_GRADIENT";
        public const string keywordLightGradientColor = "_LIGHT_GRADIENT_COLOR";

        public enum LightMode {
            SingleStep,
            Gradient,
            GradientColor
        }

        public MaterialProperty singleStepSmoothness;
        public MaterialProperty singleStepOffset;

        public MaterialProperty gradientMap;

        public MaterialProperty litColor;
        public MaterialProperty dimColor;
                
        public void Setup(MaterialProperty[] properties) {
            singleStepSmoothness = EditorUtils.FindProperty("_SingleStepSmoothness", properties);
            singleStepOffset = EditorUtils.FindProperty("_SingleStepOffset", properties);

            gradientMap = EditorUtils.FindProperty("_LightGradientMap", properties);

            litColor = EditorUtils.FindProperty("_LightLitColor", properties);
            dimColor = EditorUtils.FindProperty("_LightDimColor", properties);
        }

        public void MaterialChanged(Material material) {
            var lightMode = (LightMode)material.GetInt(propModeLight);
            switch(lightMode) {
                case LightMode.SingleStep:
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, true);
                    CoreUtils.SetKeyword(material, keywordLightGradient, false);
                    CoreUtils.SetKeyword(material, keywordLightGradientColor, false);
                    break;
                case LightMode.Gradient:
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, false);
                    CoreUtils.SetKeyword(material, keywordLightGradient, true);
                    CoreUtils.SetKeyword(material, keywordLightGradientColor, false);
                    break;
                case LightMode.GradientColor:
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, false);
                    CoreUtils.SetKeyword(material, keywordLightGradient, false);
                    CoreUtils.SetKeyword(material, keywordLightGradientColor, true);
                    break;
            }
        }

        public void OnGUI(MaterialEditor materialEditor) {
            var material = materialEditor.target as Material;

            var lightMode = (LightMode)EditorGUILayout.EnumPopup("Light Mode", (LightMode)material.GetInt(propModeLight));
            material.SetInt(propModeLight, (int)lightMode);

            switch(lightMode) {
                case LightMode.SingleStep:
                    singleStepSmoothness.floatValue = EditorGUILayout.Slider("Smoothness", singleStepSmoothness.floatValue, 0f, 1f);

                    singleStepOffset.floatValue = EditorGUILayout.Slider("Offset", singleStepOffset.floatValue, 0f, 1f);

                    litColor.colorValue = EditorGUILayout.ColorField("Light Color", litColor.colorValue);

                    dimColor.colorValue = EditorGUILayout.ColorField("Dark Color", dimColor.colorValue);
                    break;

                case LightMode.Gradient:
                    gradientMap.textureValue = EditorGUILayout.ObjectField(gradientMap.textureValue, typeof(Texture2D), false) as Texture;

                    litColor.colorValue = EditorGUILayout.ColorField("Light Color", litColor.colorValue);

                    dimColor.colorValue = EditorGUILayout.ColorField("Dark Color", dimColor.colorValue);
                    break;

                case LightMode.GradientColor:
                    gradientMap.textureValue = EditorGUILayout.ObjectField(gradientMap.textureValue, typeof(Texture2D), false) as Texture;
                    break;
            }
        }
    }
}