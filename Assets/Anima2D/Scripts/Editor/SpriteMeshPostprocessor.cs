using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

namespace Anima2D
{
    [InitializeOnLoad]
    public class SpriteMeshPostprocessor : AssetPostprocessor
    {
        private static readonly Dictionary<string, string>
            s_SpriteMeshToTextureCache = new Dictionary<string, string>();

        private static bool s_Initialized;

        static SpriteMeshPostprocessor()
        {
            if (!Application.isPlaying) EditorApplication.delayCall += Initialize;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!s_Initialized) return;

            foreach (var importetAssetPath in importedAssets)
            {
                var spriteMesh = LoadSpriteMesh(importetAssetPath);

                if (spriteMesh)
                {
                    UpdateCachedSpriteMesh(spriteMesh);
                    UpgradeSpriteMesh(spriteMesh);
                }
            }
        }

        private void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
        {
            if (!s_Initialized) return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (s_SpriteMeshToTextureCache.ContainsValue(guid))
                foreach (var sprite in sprites)
                foreach (var pair in s_SpriteMeshToTextureCache)
                    if (pair.Value == guid)
                    {
                        var spriteMesh = LoadSpriteMesh(AssetDatabase.GUIDToAssetPath(pair.Key));

                        if (spriteMesh && spriteMesh.sprite && sprite.name == spriteMesh.sprite.name)
                        {
                            DoSpriteOverride(spriteMesh, sprite);
                            break;
                        }
                    }
        }

        private void OnPreprocessTexture()
        {
            if (!s_Initialized) return;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (s_SpriteMeshToTextureCache.ContainsValue(guid))
            {
                var textureImporter = (TextureImporter) assetImporter;
                var textureImporterSO = new SerializedObject(textureImporter);
                var textureImporterSprites = textureImporterSO.FindProperty("m_SpriteSheet.m_Sprites");

                foreach (var pair in s_SpriteMeshToTextureCache)
                    if (pair.Value == guid)
                    {
                        var spriteMesh = LoadSpriteMesh(AssetDatabase.GUIDToAssetPath(pair.Key));
                        var spriteMeshData = SpriteMeshUtils.LoadSpriteMeshData(spriteMesh);

                        if (spriteMesh && spriteMeshData && spriteMesh.sprite && spriteMeshData.vertices.Length > 0)
                        {
                            textureImporterSO.FindProperty("m_SpriteMeshType").intValue = 1;

                            if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
                            {
                                SerializedProperty spriteProp = null;
                                var i = 0;
                                var name = "";

                                while (i < textureImporterSprites.arraySize && name != spriteMesh.sprite.name)
                                {
                                    spriteProp = textureImporterSprites.GetArrayElementAtIndex(i);
                                    name = spriteProp.FindPropertyRelative("m_Name").stringValue;

                                    ++i;
                                }

                                if (name == spriteMesh.sprite.name)
                                {
                                    var textureRect = SpriteMeshUtils.CalculateSpriteRect(spriteMesh, 5);
                                    spriteProp.FindPropertyRelative("m_Rect").rectValue = textureRect;
                                    spriteProp.FindPropertyRelative("m_Alignment").intValue = 9;
                                    spriteProp.FindPropertyRelative("m_Pivot").vector2Value =
                                        Vector2.Scale(spriteMeshData.pivotPoint - textureRect.position,
                                            new Vector2(1f / textureRect.size.x, 1f / textureRect.size.y));
                                }
                            }
                            else
                            {
                                var width = 0;
                                var height = 0;
                                SpriteMeshUtils.GetSpriteTextureSize(spriteMesh.sprite, ref width, ref height);
                                textureImporterSO.FindProperty("m_Alignment").intValue = 9;
                                textureImporterSO.FindProperty("m_SpritePivot").vector2Value =
                                    Vector2.Scale(spriteMeshData.pivotPoint, new Vector2(1f / width, 1f / height));
                            }
                        }
                    }

                textureImporterSO.ApplyModifiedProperties();
            }
        }

        public static SpriteMesh GetSpriteMeshFromSprite(Sprite sprite)
        {
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite));

            if (s_SpriteMeshToTextureCache.ContainsValue(guid))
                foreach (var pair in s_SpriteMeshToTextureCache)
                    if (pair.Value.Equals(guid))
                    {
                        var spriteMesh = LoadSpriteMesh(AssetDatabase.GUIDToAssetPath(pair.Key));

                        if (spriteMesh && spriteMesh.sprite == sprite) return spriteMesh;
                    }

            return null;
        }

        private static void Initialize()
        {
            s_SpriteMeshToTextureCache.Clear();

            var spriteMeshGUIDs = AssetDatabase.FindAssets("t:SpriteMesh");

            var needsOverride = new List<string>();

            foreach (var guid in spriteMeshGUIDs)
            {
                var spriteMesh = LoadSpriteMesh(AssetDatabase.GUIDToAssetPath(guid));

                if (spriteMesh)
                {
                    UpdateCachedSpriteMesh(spriteMesh);
                    UpgradeSpriteMesh(spriteMesh);

                    if (s_SpriteMeshToTextureCache.ContainsKey(guid) &&
                        SpriteMeshUtils.NeedsOverride(spriteMesh))
                        needsOverride.Add(s_SpriteMeshToTextureCache[guid]);
                }
            }

            s_Initialized = true;

            needsOverride = needsOverride.Distinct().ToList();

            AssetDatabase.StartAssetEditing();

            foreach (var textureGuid in needsOverride)
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(textureGuid));

            AssetDatabase.StopAssetEditing();
        }

        private static void UpgradeSpriteMesh(SpriteMesh spriteMesh)
        {
            if (spriteMesh)
            {
                var spriteMeshSO = new SerializedObject(spriteMesh);
                var apiVersionProp = spriteMeshSO.FindProperty("m_ApiVersion");

                if (apiVersionProp.intValue < SpriteMesh.api_version)
                {
                    if (apiVersionProp.intValue < 2)
                        Debug.LogError("SpriteMesh " + spriteMesh +
                                       " was created using an ancient version of Anima2D which can't be upgraded anymore.\n" +
                                       "The last version that can upgrade this asset is Anima2D 1.1.5");

                    if (apiVersionProp.intValue < 3) Upgrade_003(spriteMeshSO);

                    if (apiVersionProp.intValue < 4) Upgrade_004(spriteMeshSO);

                    spriteMeshSO.Update();
                    apiVersionProp.intValue = SpriteMesh.api_version;
                    spriteMeshSO.ApplyModifiedProperties();

                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static void Upgrade_003(SerializedObject spriteMeshSO)
        {
            var spriteMesh = spriteMeshSO.targetObject as SpriteMesh;
            var spriteMeshData = SpriteMeshUtils.LoadSpriteMeshData(spriteMesh);

            if (spriteMesh.sprite && spriteMeshData)
            {
                var textureImporter =
                    AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteMesh.sprite)) as TextureImporter;

                float maxImporterSize = textureImporter.maxTextureSize;

                var width = 1;
                var height = 1;

                SpriteMeshUtils.GetWidthAndHeight(textureImporter, ref width, ref height);

                var maxSize = Mathf.Max(width, height);

                var factor = maxSize / maxImporterSize;

                if (factor > 1f)
                {
                    var spriteMeshDataSO = new SerializedObject(spriteMeshData);
                    var smdPivotPointProp = spriteMeshDataSO.FindProperty("m_PivotPoint");
                    var smdVerticesProp = spriteMeshDataSO.FindProperty("m_Vertices");
                    var smdHolesProp = spriteMeshDataSO.FindProperty("m_Holes");

                    spriteMeshDataSO.Update();

                    smdPivotPointProp.vector2Value = spriteMeshData.pivotPoint * factor;

                    for (var i = 0; i < spriteMeshData.vertices.Length; ++i)
                        smdVerticesProp.GetArrayElementAtIndex(i).vector2Value = spriteMeshData.vertices[i] * factor;

                    for (var i = 0; i < spriteMeshData.holes.Length; ++i)
                        smdHolesProp.GetArrayElementAtIndex(i).vector2Value = spriteMeshData.holes[i] * factor;

                    spriteMeshDataSO.ApplyModifiedProperties();

                    EditorUtility.SetDirty(spriteMeshData);
                }
            }
        }

        private static void Upgrade_004(SerializedObject spriteMeshSO)
        {
            var materialsProp = spriteMeshSO.FindProperty("m_SharedMaterials");

            for (var i = 0; i < materialsProp.arraySize; ++i)
            {
                var materialProp = materialsProp.GetArrayElementAtIndex(i);
                var material = materialProp.objectReferenceValue as Material;

                if (material) Object.DestroyImmediate(material, true);
            }

            spriteMeshSO.Update();
            materialsProp.arraySize = 0;
            spriteMeshSO.ApplyModifiedProperties();
        }

        private static void UpdateCachedSpriteMesh(SpriteMesh spriteMesh)
        {
            if (spriteMesh)
            {
                var key = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteMesh));

                if (spriteMesh.sprite)
                {
                    var spriteMeshFromSprite = GetSpriteMeshFromSprite(spriteMesh.sprite);

                    if (!spriteMeshFromSprite || spriteMesh == spriteMeshFromSprite)
                    {
                        var value = AssetDatabase.AssetPathToGUID(
                            AssetDatabase.GetAssetPath(SpriteUtility.GetSpriteTexture(spriteMesh.sprite, false)));

                        s_SpriteMeshToTextureCache[key] = value;
                    }
                    else
                    {
                        Debug.LogWarning("Anima2D: SpriteMesh " + spriteMesh.name + " uses the same Sprite as " +
                                         spriteMeshFromSprite.name + ". Use only one SpriteMesh per Sprite.");
                    }
                }
                else if (s_SpriteMeshToTextureCache.ContainsKey(key))
                {
                    s_SpriteMeshToTextureCache.Remove(key);
                }
            }
        }

        private static bool IsSpriteMesh(string assetPath)
        {
            return s_SpriteMeshToTextureCache.ContainsKey(AssetDatabase.AssetPathToGUID(assetPath));
        }

        private static SpriteMesh LoadSpriteMesh(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath(assetPath, typeof(SpriteMesh)) as SpriteMesh;
        }

        private void DoSpriteOverride(SpriteMesh spriteMesh, Sprite sprite)
        {
            var textureImporter = (TextureImporter) assetImporter;

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3_OR_NEWER
            Debug.Assert(textureImporter.spriteImportMode == SpriteImportMode.Single ||
                         textureImporter.spriteImportMode == SpriteImportMode.Multiple,
                "Incompatible Sprite Mode. Use Single or Multiple.");
#endif

            var spriteMeshData = SpriteMeshUtils.LoadSpriteMeshData(spriteMesh);

            if (spriteMeshData)
            {
                var factor = Vector2.one;
                var spriteRect = sprite.rect;
                var rectTextureSpace = new Rect();

                if (textureImporter.spriteImportMode == SpriteImportMode.Single)
                {
                    var width = 0;
                    var height = 0;

                    SpriteMeshUtils.GetSpriteTextureSize(spriteMesh.sprite, ref width, ref height);
                    rectTextureSpace = new Rect(0, 0, width, height);
                }
                else if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
                {
                    rectTextureSpace = SpriteMeshUtils.CalculateSpriteRect(spriteMesh, 5);
                }

                factor = new Vector2(spriteRect.width / rectTextureSpace.width,
                    spriteRect.height / rectTextureSpace.height);

                var newVertices = new List<Vector2>(spriteMeshData.vertices).ConvertAll(v =>
                        MathUtils.ClampPositionInRect(Vector2.Scale(v, factor), spriteRect) - spriteRect.position)
                    .ToArray();
                var newIndices = new List<int>(spriteMeshData.indices).ConvertAll(i => (ushort) i).ToArray();

                sprite.OverrideGeometry(newVertices, newIndices);
            }
        }
    }
}