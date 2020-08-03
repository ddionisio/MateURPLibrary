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
        public const string keywordLightSingleStep = "_LIGHT_SINGLE_STEP";

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
            SingleStep,
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

        //Material Properties
        private int mMatPropSingleStepSmoothness = Shader.PropertyToID("_SingleStepSmoothness");
        private int mMatPropSingleStepOffset = Shader.PropertyToID("_SingleStepOffset");
        private int mMatPropSingleStepLitColor = Shader.PropertyToID("_SingleStepLitColor");
        private int mMatPropSingleStepDimColor = Shader.PropertyToID("_SingleStepDimColor");

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
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, false);
                    break;
                case LightMode.SingleStep:
                    CoreUtils.SetKeyword(material, keywordLightShadeOnly, false);
                    CoreUtils.SetKeyword(material, keywordLightSingleStep, true);
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

            ///////////////////////////////////
            //light settings
            DrawSeparator();

            var _lightMode = (LightMode)material.GetFloat(propModeLight);
            var lightMode = (LightMode)EditorGUILayout.EnumPopup("Light Mode", _lightMode);
            if(lightMode != _lightMode)
                material.SetFloat(propModeLight, (float)lightMode);

            switch(lightMode) {
                case LightMode.ShadeOnly:
                    break;

                case LightMode.SingleStep:
                    var _singleStepSmoothness = material.GetFloat(mMatPropSingleStepSmoothness);
                    var singleStepSmoothness = EditorGUILayout.Slider("Smoothness", _singleStepSmoothness, 0f, 1f);
                    if(singleStepSmoothness != _singleStepSmoothness)
                        material.SetFloat(mMatPropSingleStepSmoothness, singleStepSmoothness);

                    var _singleStepOfs = material.GetFloat(mMatPropSingleStepOffset);
                    var singleStepOfs = EditorGUILayout.Slider("Offset", _singleStepOfs, 0f, 1f);
                    if(singleStepOfs != _singleStepOfs)
                        material.SetFloat(mMatPropSingleStepOffset, singleStepOfs);

                    var _singleStepLitClr = material.GetColor(mMatPropSingleStepLitColor);
                    var singleStepLitClr = EditorGUILayout.ColorField("Light Color", _singleStepLitClr);
                    if(singleStepLitClr != _singleStepLitClr)
                        material.SetColor(mMatPropSingleStepLitColor, singleStepLitClr);

                    var _singleStepDimClr = material.GetColor(mMatPropSingleStepDimColor);
                    var singleStepDimClr = EditorGUILayout.ColorField("Dark Color", _singleStepDimClr);
                    if(singleStepDimClr != _singleStepDimClr)
                        material.SetColor(mMatPropSingleStepDimColor, singleStepDimClr);
                    break;
            }
            //

            ///////////////////////////////////
            //shade settings
            DrawSeparator();

            var _shadeMode = (ShadeMode)material.GetFloat(propModeShade);
            var shadeMode = (ShadeMode)EditorGUILayout.EnumPopup("Shade Mode", _shadeMode);
            if(shadeMode != _shadeMode)
                material.SetFloat(propModeShade, (float)shadeMode);

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