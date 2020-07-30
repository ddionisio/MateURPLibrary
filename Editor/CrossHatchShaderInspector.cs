using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor;

namespace M8.URP {
    public class CrossHatchShaderInspector : ShaderGUI {
        public const string propModeLight = "_LightMode";
        public const string propModeShade = "_ShadeMode";

        public const string keywordLightShadeOnly = "_LIGHT_SHADE_ONLY";

        public const string keywordShadeGradient = "_USE_SHADE_GRADIENT";

        public struct Properties {
            // Surface Input Props
            public MaterialProperty shadeGradientMap;

            public Properties(MaterialProperty[] properties) {
                // Surface Input Props
                shadeGradientMap = FindProperty("_ShadeGradientMap", properties);
            }
        }

        public enum LightMode {
            ShadeOnly,
        }

        public enum ShadeMode {
            None,
            Gradient
        }

        public static class StylesExt {
            public static GUIContent shadeGradientMapText = new GUIContent("Shade Gradient Map", "Look-up luminance of the shade based on the lambert value.");
        }
        
        // Properties
        private Properties mProperties;

        private bool mIsInit = false;

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
            base.OnGUI(materialEditorIn, properties);

            //Initialize editor data
            mProperties = new Properties(properties);

            if(!mIsInit) {
                foreach(Material mat in materialEditorIn.targets)
                    MaterialChanged(mat);

                mIsInit = true;
            }

            ShaderPropertiesGUI(materialEditorIn);
        }

        // material changed check
        public void MaterialChanged(Material material) {
            var lightMode = (LightMode)material.GetFloat(propModeLight);
            switch(lightMode) {
                case LightMode.ShadeOnly:
                    CoreUtils.SetKeyword(material, keywordLightShadeOnly, true);
                    break;
            }

            var shadeMode = (ShadeMode)material.GetFloat(propModeShade);
            switch(shadeMode) {
                case ShadeMode.None:
                    CoreUtils.SetKeyword(material, keywordShadeGradient, false);
                    break;
                case ShadeMode.Gradient:
                    CoreUtils.SetKeyword(material, keywordShadeGradient, true);
                    break;
            }
        }

        public void ShaderPropertiesGUI(MaterialEditor materialEditor) {
            var material = materialEditor.target as Material;
            if(material == null)
                throw new ArgumentNullException("material");

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck();

            //light settings
            DrawSeparator();

            var lightMode = (LightMode)material.GetFloat(propModeLight);
            var _lightMode = (LightMode)EditorGUILayout.EnumPopup("Light Mode", lightMode);
            if(lightMode != _lightMode) {
                lightMode = _lightMode;
                material.SetFloat(propModeShade, (float)lightMode);
            }

            switch(lightMode) {
                case LightMode.ShadeOnly:
                    break;
            }
            //

            //shade settings
            DrawSeparator();

            var shadeMode = (ShadeMode)material.GetFloat(propModeShade);
            var _shadeMode = (ShadeMode)EditorGUILayout.EnumPopup("Shade Mode", shadeMode);
            if(shadeMode != _shadeMode) {
                shadeMode = _shadeMode;
                material.SetFloat(propModeShade, (float)shadeMode);
            }

            switch(shadeMode) {
                case ShadeMode.Gradient:
                    mProperties.shadeGradientMap.textureValue = EditorGUILayout.ObjectField(mProperties.shadeGradientMap.textureValue, typeof(Texture2D), false) as Texture;
                    break;
            }
            //

            if(EditorGUI.EndChangeCheck()) {
                foreach(Material mat in materialEditor.targets)
                    MaterialChanged(mat);
            }
        }

        public void DrawSeparator() {
            GUILayout.Space(12f);

            if(Event.current.type == EventType.Repaint) {
                Texture2D tex = EditorGUIUtility.whiteTexture;
                Rect rect = GUILayoutUtility.GetLastRect();
                GUI.color = new Color(0f, 0f, 0f, 0.25f);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 4f), tex);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 6f, Screen.width, 1f), tex);
                GUI.DrawTexture(new Rect(0f, rect.yMin + 9f, Screen.width, 1f), tex);
                GUI.color = Color.white;
            }
        }
    }
}