using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor;

namespace M8.URP {
    public class ToonCrossHatchShaderInspector : ShaderGUI {
        public IShaderFeatureEditor[] mFeatures;

        private bool mIsInit = false;

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties) {
            base.OnGUI(materialEditorIn, properties);

            //Initialize editor data
            if(mFeatures == null)
                mFeatures = new IShaderFeatureEditor[] { 
                    new ShaderFeatureToonEditor(), 
                    new ShaderFeatureCrossHatchEditor(),
                    //new ShaderFeatureRimLightEditor() 
                };

            for(int i = 0; i < mFeatures.Length; i++)
                mFeatures[i].Setup(properties);

            if(!mIsInit) {
                foreach(Material mat in materialEditorIn.targets)
                    MaterialChanged(mat);

                mIsInit = true;
            }

            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;

            EditorGUI.BeginChangeCheck();

            for(int i = 0; i < mFeatures.Length; i++) {
                EditorUtils.DrawSeparator();

                mFeatures[i].OnGUI(materialEditorIn);
            }

            if(EditorGUI.EndChangeCheck()) {
                foreach(Material mat in materialEditorIn.targets)
                    MaterialChanged(mat);
            }
        }

        // material changed check
        public void MaterialChanged(Material material) {
            for(int i = 0; i < mFeatures.Length; i++)
                mFeatures[i].MaterialChanged(material);
        }
    }
}