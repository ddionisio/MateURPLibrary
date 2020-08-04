using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace M8.URP {
    public class ShaderFeatureToonEditor : IShaderFeatureEditor {
        public const string propModeLight = "_LightMode";

        public const string keywordLightShadeOnly = "_LIGHT_SHADE_ONLY";
        public const string keywordLightSingleStep = "_LIGHT_SINGLE_STEP";

        public enum LightMode {
            ShadeOnly,
            SingleStep,
        }

        public MaterialProperty singleStepSmoothness;
        public MaterialProperty singleStepOffset;
        public MaterialProperty singleStepLitColor;
        public MaterialProperty singleStepDimColor;

        public void Setup(MaterialProperty[] properties) {
            singleStepSmoothness = EditorUtils.FindProperty("_SingleStepSmoothness", properties);
            singleStepOffset = EditorUtils.FindProperty("_SingleStepOffset", properties);
            singleStepLitColor = EditorUtils.FindProperty("_SingleStepLitColor", properties);
            singleStepDimColor = EditorUtils.FindProperty("_SingleStepDimColor", properties);
        }

        public void MaterialChanged(Material material) {
            var lightMode = (LightMode)material.GetFloat(propModeLight);
            switch(lightMode) {
                case LightMode.ShadeOnly:
                    CoreUtils.SetKeyword(material, keywordLightShadeOnly, true);
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, false);
                    break;
                case LightMode.SingleStep:
                    CoreUtils.SetKeyword(material, keywordLightShadeOnly, false);
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, true);
                    break;
            }
        }

        public void OnGUI(MaterialEditor materialEditor) {
            var material = materialEditor.target as Material;

            var _lightMode = (LightMode)material.GetFloat(propModeLight);
            var lightMode = (LightMode)EditorGUILayout.EnumPopup("Light Mode", _lightMode);
            if(lightMode != _lightMode)
                material.SetFloat(propModeLight, (float)lightMode);

            switch(lightMode) {
                case LightMode.ShadeOnly:
                    break;

                case LightMode.SingleStep:
                    singleStepSmoothness.floatValue = EditorGUILayout.Slider("Smoothness", singleStepSmoothness.floatValue, 0f, 1f);

                    singleStepOffset.floatValue = EditorGUILayout.Slider("Offset", singleStepOffset.floatValue, 0f, 1f);

                    singleStepLitColor.colorValue = EditorGUILayout.ColorField("Light Color", singleStepLitColor.colorValue);

                    singleStepDimColor.colorValue = EditorGUILayout.ColorField("Dark Color", singleStepDimColor.colorValue);
                    break;
            }
        }
    }
}