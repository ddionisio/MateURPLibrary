using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace M8.URP {
    public class ShaderFeatureRimLightEditor : IShaderFeatureEditor {
        public const string propRimLightEnabled = "_RimLightEnabled";

        public const string keywordRimLightEnabled = "_RIM_LIGHT_ENABLED";

        public MaterialProperty rimLightColor;
        public MaterialProperty rimLightSize;
        public MaterialProperty rimLightSmoothness;
        public MaterialProperty rimLightAlign;

        public void Setup(MaterialProperty[] properties) {
            rimLightColor = EditorUtils.FindProperty("_RimLightColor", properties);
            rimLightSize = EditorUtils.FindProperty("_RimLightSize", properties);
            rimLightSmoothness = EditorUtils.FindProperty("_RimLightSmoothness", properties);
            rimLightAlign = EditorUtils.FindProperty("_RimLightAlign", properties);
        }

        public void MaterialChanged(Material material) {
            var isRimLightEnabled = material.GetInt(propRimLightEnabled) != 0;

            CoreUtils.SetKeyword(material, keywordRimLightEnabled, isRimLightEnabled);
        }

        public void OnGUI(MaterialEditor materialEditor) {
            var material = materialEditor.target as Material;

            var isRimLightEnabled = EditorGUILayout.Toggle("Rim Light", material.GetInt(propRimLightEnabled) != 0);
            material.SetInt(propRimLightEnabled, isRimLightEnabled ? 1 : 0);

            if(isRimLightEnabled) {
                rimLightColor.colorValue = EditorGUILayout.ColorField("Color", rimLightColor.colorValue);
                rimLightSize.floatValue = EditorGUILayout.Slider("Size", rimLightSize.floatValue, 0f, 1f);
                rimLightSmoothness.floatValue = EditorGUILayout.Slider("Smoothness", rimLightSmoothness.floatValue, 0f, 1f);
                rimLightAlign.floatValue = EditorGUILayout.Slider("Align", rimLightAlign.floatValue, 0f, 1f);
            }
        }
    }
}