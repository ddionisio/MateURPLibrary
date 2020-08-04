using UnityEngine;
using UnityEditor;

namespace M8.URP {
    public interface IShaderFeatureEditor {
        void Setup(MaterialProperty[] properties);
        void MaterialChanged(Material material);
        void OnGUI(MaterialEditor materialEditor);
    }
}