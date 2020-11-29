using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    public class MaterialCache
    {
        public MaterialCache(Renderer renderer)
        {
            if (renderer as SpriteRenderer) return;

            var l_materialList = new List<Material>();

            foreach (var material in renderer.sharedMaterials)
            {
                Material materialInstance = null;

                if (material)
                {
                    materialInstance = Object.Instantiate(material);
                    materialInstance.hideFlags = HideFlags.DontSave;
                    materialInstance.shader = Shader.Find("Sprites/Default");
                }

                l_materialList.Add(materialInstance);
            }

            materials = l_materialList.ToArray();

            renderer.sharedMaterials = materials;
        }

        public Material[] materials { get; private set; }

        public void Destroy()
        {
            if (materials != null)
                foreach (var material in materials)
                    if (material)
                        Object.DestroyImmediate(material);
        }

        public void SetColor(Color color)
        {
            if (materials != null)
                foreach (var material in materials)
                    if (material)
                    {
                        color.a = material.color.a;
                        material.color = color;
                    }
        }

        public void SetAlpha(float alpha)
        {
            if (materials != null)
                foreach (var material in materials)
                    if (material)
                    {
                        var color = material.color;
                        color.a = alpha;
                        material.color = color;
                    }
        }
    }
}