using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerModelSkinSetup
{
    [Serializable]
    public class SkinData
    {
        public Renderer renderer;
        public Material[] skinMaterials;

        public void SetActive(bool active)
        {
            if (renderer == null) return;

            renderer.gameObject.SetActive(active);
        }

        public void ApplyMaterials()
        {
            if (renderer == null || !HasMaterials()) return;

            renderer.sharedMaterials = skinMaterials;
        }

        public bool HasMaterials()
        {
            return skinMaterials != null && skinMaterials.Length > 0;
        }
    }

    public List<SkinData> skinObjects;

    public void SetActive(bool active)
    {
        foreach (SkinData skinData in skinObjects)
        {
            if (skinData.renderer == null) continue;

            if (active) skinData.ApplyMaterials();
            skinData.SetActive(active);
        }
    }
}