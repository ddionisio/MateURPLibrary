using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEditor;

namespace M8.URP {
    public struct EditorUtils {
        public static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties) {
            return FindProperty(propertyName, properties, true);
        }

        public static MaterialProperty FindProperty(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory) {
            for(int index = 0; index < properties.Length; ++index) {
                if(properties[index] != null && properties[index].name == propertyName)
                    return properties[index];
            }
            if(propertyIsMandatory)
                throw new ArgumentException("Could not find MaterialProperty: '" + propertyName + "', Num properties: " + (object)properties.Length);
            return null;
        }

        public static void DrawSeparator() {
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