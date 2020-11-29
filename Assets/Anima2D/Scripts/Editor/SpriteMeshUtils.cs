using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TriangleNet.Geometry;
using TriangleNet.Tools;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

namespace Anima2D
{
    public class SpriteMeshUtils
    {
        private static Material m_DefaultMaterial;

        public static Material defaultMaterial
        {
            get
            {
                if (!m_DefaultMaterial)
                {
                    var go = new GameObject();
                    var sr = go.AddComponent<SpriteRenderer>();
                    m_DefaultMaterial = sr.sharedMaterial;
                    Object.DestroyImmediate(go);
                }

                return m_DefaultMaterial;
            }
        }

        public static SpriteMesh CreateSpriteMesh(Sprite sprite)
        {
            var spriteMesh = SpriteMeshPostprocessor.GetSpriteMeshFromSprite(sprite);
            SpriteMeshData spriteMeshData = null;

            if (!spriteMesh && sprite)
            {
                var spritePath = AssetDatabase.GetAssetPath(sprite);
                var directory = Path.GetDirectoryName(spritePath);
                var assetPath =
                    AssetDatabase.GenerateUniqueAssetPath(directory + Path.DirectorySeparatorChar + sprite.name +
                                                          ".asset");

                spriteMesh = ScriptableObject.CreateInstance<SpriteMesh>();
                InitFromSprite(spriteMesh, sprite);
                AssetDatabase.CreateAsset(spriteMesh, assetPath);

                spriteMeshData = ScriptableObject.CreateInstance<SpriteMeshData>();
                spriteMeshData.name = spriteMesh.name + "_Data";
                spriteMeshData.hideFlags = HideFlags.HideInHierarchy;
                InitFromSprite(spriteMeshData, sprite);
                AssetDatabase.AddObjectToAsset(spriteMeshData, assetPath);

                UpdateAssets(spriteMesh, spriteMeshData);

                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(assetPath);

                Selection.activeObject = spriteMesh;
            }

            return spriteMesh;
        }

        public static void CreateSpriteMesh(Texture2D texture)
        {
            if (texture)
            {
                var objects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture));

                for (var i = 0; i < objects.Length; i++)
                {
                    var o = objects[i];
                    var sprite = o as Sprite;
                    if (sprite)
                    {
                        EditorUtility.DisplayProgressBar("Processing " + texture.name, sprite.name,
                            (i + 1) / (float) objects.Length);
                        CreateSpriteMesh(sprite);
                    }
                }

                EditorUtility.ClearProgressBar();
            }
        }

        public static SpriteMeshData LoadSpriteMeshData(SpriteMesh spriteMesh)
        {
            if (spriteMesh)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(spriteMesh));

                foreach (var asset in assets)
                {
                    var data = asset as SpriteMeshData;

                    if (data) return data;
                }
            }

            return null;
        }

        public static void UpdateAssets(SpriteMesh spriteMesh)
        {
            UpdateAssets(spriteMesh, LoadSpriteMeshData(spriteMesh));
        }

        public static void UpdateAssets(SpriteMesh spriteMesh, SpriteMeshData spriteMeshData)
        {
            if (spriteMesh && spriteMeshData)
            {
                var spriteMeshPath = AssetDatabase.GetAssetPath(spriteMesh);

                var spriteMeshSO = new SerializedObject(spriteMesh);
                var sharedMeshProp = spriteMeshSO.FindProperty("m_SharedMesh");

                if (!spriteMesh.sharedMesh)
                {
                    var mesh = new Mesh();
                    mesh.hideFlags = HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(mesh, spriteMeshPath);

                    spriteMeshSO.Update();
                    sharedMeshProp.objectReferenceValue = mesh;
                    spriteMeshSO.ApplyModifiedProperties();
                    EditorUtility.SetDirty(mesh);
                }

                spriteMesh.sharedMesh.name = spriteMesh.name;

                spriteMeshData.hideFlags = HideFlags.HideInHierarchy;
                EditorUtility.SetDirty(spriteMeshData);

                var width = 0;
                var height = 0;

                GetSpriteTextureSize(spriteMesh.sprite, ref width, ref height);

                var vertices = GetMeshVertices(spriteMesh.sprite, spriteMeshData);

                var textureWidthHeightInv = new Vector2(1f / width, 1f / height);

                var uvs = new List<Vector2>(spriteMeshData.vertices)
                    .ConvertAll(v => Vector2.Scale(v, textureWidthHeightInv)).ToArray();

                var normals = new List<Vector3>(vertices).ConvertAll(v => Vector3.back).ToArray();

                var boneWeightsData = spriteMeshData.boneWeights;

                if (boneWeightsData.Length != spriteMeshData.vertices.Length)
                    boneWeightsData = new BoneWeight[spriteMeshData.vertices.Length];

                var boneWeights = new List<UnityEngine.BoneWeight>(boneWeightsData.Length);

                var verticesOrder = new List<float>(spriteMeshData.vertices.Length);

                for (var i = 0; i < boneWeightsData.Length; i++)
                {
                    var boneWeight = boneWeightsData[i];

                    var pairs = new List<KeyValuePair<int, float>>();
                    pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex0, boneWeight.weight0));
                    pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex1, boneWeight.weight1));
                    pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex2, boneWeight.weight2));
                    pairs.Add(new KeyValuePair<int, float>(boneWeight.boneIndex3, boneWeight.weight3));

                    pairs = pairs.OrderByDescending(s => s.Value).ToList();

                    var boneWeight2 = new UnityEngine.BoneWeight();
                    boneWeight2.boneIndex0 = Mathf.Max(0, pairs[0].Key);
                    boneWeight2.boneIndex1 = Mathf.Max(0, pairs[1].Key);
                    boneWeight2.boneIndex2 = Mathf.Max(0, pairs[2].Key);
                    boneWeight2.boneIndex3 = Mathf.Max(0, pairs[3].Key);
                    boneWeight2.weight0 = pairs[0].Value;
                    boneWeight2.weight1 = pairs[1].Value;
                    boneWeight2.weight2 = pairs[2].Value;
                    boneWeight2.weight3 = pairs[3].Value;

                    boneWeights.Add(boneWeight2);

                    float vertexOrder = i;

                    if (spriteMeshData.bindPoses.Length > 0)
                        vertexOrder = spriteMeshData.bindPoses[boneWeight2.boneIndex0].zOrder * boneWeight2.weight0 +
                                      spriteMeshData.bindPoses[boneWeight2.boneIndex1].zOrder * boneWeight2.weight1 +
                                      spriteMeshData.bindPoses[boneWeight2.boneIndex2].zOrder * boneWeight2.weight2 +
                                      spriteMeshData.bindPoses[boneWeight2.boneIndex3].zOrder * boneWeight2.weight3;

                    verticesOrder.Add(vertexOrder);
                }

                var weightedTriangles = new List<WeightedTriangle>(spriteMeshData.indices.Length / 3);

                for (var i = 0; i < spriteMeshData.indices.Length; i += 3)
                {
                    var p1 = spriteMeshData.indices[i];
                    var p2 = spriteMeshData.indices[i + 1];
                    var p3 = spriteMeshData.indices[i + 2];

                    weightedTriangles.Add(new WeightedTriangle(p1, p2, p3,
                        verticesOrder[p1],
                        verticesOrder[p2],
                        verticesOrder[p3]));
                }

                weightedTriangles = weightedTriangles.OrderBy(t => t.weight).ToList();

                var indices = new List<int>(spriteMeshData.indices.Length);

                for (var i = 0; i < weightedTriangles.Count; ++i)
                {
                    var t = weightedTriangles[i];
                    indices.Add(t.p1);
                    indices.Add(t.p2);
                    indices.Add(t.p3);
                }

                var bindposes = new List<BindInfo>(spriteMeshData.bindPoses).ConvertAll(p => p.bindPose);

                for (var i = 0; i < bindposes.Count; i++)
                {
                    var bindpose = bindposes[i];

                    bindpose.m23 = 0f;

                    bindposes[i] = bindpose;
                }

                spriteMesh.sharedMesh.Clear();
                spriteMesh.sharedMesh.vertices = vertices;
                spriteMesh.sharedMesh.uv = uvs;
                spriteMesh.sharedMesh.triangles = indices.ToArray();
                spriteMesh.sharedMesh.normals = normals;
                spriteMesh.sharedMesh.boneWeights = boneWeights.ToArray();
                spriteMesh.sharedMesh.bindposes = bindposes.ToArray();
                spriteMesh.sharedMesh.RecalculateBounds();
#if UNITY_5_6_OR_NEWER
                spriteMesh.sharedMesh.RecalculateTangents();
#endif
                RebuildBlendShapes(spriteMesh);
            }
        }

        public static Vector3[] GetMeshVertices(SpriteMesh spriteMesh)
        {
            return GetMeshVertices(spriteMesh.sprite, LoadSpriteMeshData(spriteMesh));
        }

        public static Vector3[] GetMeshVertices(Sprite sprite, SpriteMeshData spriteMeshData)
        {
            var pixelsPerUnit = GetSpritePixelsPerUnit(sprite);

            return new List<Vector2>(spriteMeshData.vertices)
                .ConvertAll(v => TexCoordToVertex(spriteMeshData.pivotPoint, v, pixelsPerUnit)).ToArray();
        }

        public static void GetSpriteTextureSize(Sprite sprite, ref int width, ref int height)
        {
            if (sprite)
            {
                var texture = SpriteUtility.GetSpriteTexture(sprite, false);

                var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;

                GetWidthAndHeight(textureImporter, ref width, ref height);
            }
        }

        public static void GetWidthAndHeight(TextureImporter textureImporter, ref int width, ref int height)
        {
            var methodInfo =
                typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.Instance | BindingFlags.NonPublic);

            if (methodInfo != null)
            {
                object[] parameters = {null, null};
                methodInfo.Invoke(textureImporter, parameters);
                width = (int) parameters[0];
                height = (int) parameters[1];
            }
        }

        public static float GetSpritePixelsPerUnit(Sprite sprite)
        {
            var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;

            return textureImporter.spritePixelsPerUnit;
        }

        private static void InitFromSprite(SpriteMesh spriteMesh, Sprite sprite)
        {
            var spriteMeshSO = new SerializedObject(spriteMesh);
            var spriteProp = spriteMeshSO.FindProperty("m_Sprite");
            var apiProp = spriteMeshSO.FindProperty("m_ApiVersion");

            spriteMeshSO.Update();
            apiProp.intValue = SpriteMesh.api_version;
            spriteProp.objectReferenceValue = sprite;
            spriteMeshSO.ApplyModifiedProperties();
        }

        private static void InitFromSprite(SpriteMeshData spriteMeshData, Sprite sprite)
        {
            Vector2[] vertices;
            IndexedEdge[] edges;
            int[] indices;
            Vector2 pivotPoint;

            if (sprite)
            {
                GetSpriteData(sprite, out vertices, out edges, out indices, out pivotPoint);

                spriteMeshData.vertices = vertices;
                spriteMeshData.edges = edges;
                spriteMeshData.indices = indices;
                spriteMeshData.pivotPoint = pivotPoint;
            }
        }

        public static void GetSpriteData(Sprite sprite, out Vector2[] vertices, out IndexedEdge[] edges,
            out int[] indices, out Vector2 pivotPoint)
        {
            var width = 0;
            var height = 0;

            GetSpriteTextureSize(sprite, ref width, ref height);

            pivotPoint = Vector2.zero;

            var uvs = SpriteUtility.GetSpriteUVs(sprite, false);

            vertices = new Vector2[uvs.Length];

            for (var i = 0; i < uvs.Length; ++i) vertices[i] = new Vector2(uvs[i].x * width, uvs[i].y * height);

            var l_indices = sprite.triangles;

            indices = new int[l_indices.Length];

            for (var i = 0; i < l_indices.Length; ++i) indices[i] = l_indices[i];

            var edgesSet = new HashSet<IndexedEdge>();

            for (var i = 0; i < indices.Length; i += 3)
            {
                var index1 = indices[i];
                var index2 = indices[i + 1];
                var index3 = indices[i + 2];

                var edge1 = new IndexedEdge(index1, index2);
                var edge2 = new IndexedEdge(index2, index3);
                var edge3 = new IndexedEdge(index1, index3);

                if (edgesSet.Contains(edge1))
                    edgesSet.Remove(edge1);
                else
                    edgesSet.Add(edge1);

                if (edgesSet.Contains(edge2))
                    edgesSet.Remove(edge2);
                else
                    edgesSet.Add(edge2);

                if (edgesSet.Contains(edge3))
                    edgesSet.Remove(edge3);
                else
                    edgesSet.Add(edge3);
            }

            edges = new IndexedEdge[edgesSet.Count];
            var edgeIndex = 0;
            foreach (var edge in edgesSet)
            {
                edges[edgeIndex] = edge;
                ++edgeIndex;
            }

            pivotPoint = GetPivotPoint(sprite);
        }

        public static void InitFromOutline(Texture2D texture, Rect rect, float detail, float alphaTolerance,
            bool holeDetection,
            out List<Vector2> vertices, out List<IndexedEdge> indexedEdges, out List<int> indices)
        {
            vertices = new List<Vector2>();
            indexedEdges = new List<IndexedEdge>();
            indices = new List<int>();

            if (texture)
            {
                var paths = GenerateOutline(texture, rect, detail, (byte) (alphaTolerance * 255f), holeDetection);

                var startIndex = 0;
                for (var i = 0; i < paths.Length; i++)
                {
                    var path = paths[i];
                    for (var j = 0; j < path.Length; j++)
                    {
                        vertices.Add(path[j] + rect.center);
                        indexedEdges.Add(new IndexedEdge(startIndex + j, startIndex + (j + 1) % path.Length));
                    }

                    startIndex += path.Length;
                }

                var holes = new List<Hole>();
                Triangulate(vertices, indexedEdges, holes, ref indices);
            }
        }

        private static Vector2[][] GenerateOutline(Texture2D texture, Rect rect, float detail, byte alphaTolerance,
            bool holeDetection)
        {
            Vector2[][] paths = null;

            var methodInfo =
                typeof(SpriteUtility).GetMethod("GenerateOutline", BindingFlags.Static | BindingFlags.NonPublic);

            if (methodInfo != null)
            {
                object[] parameters = {texture, rect, detail, alphaTolerance, holeDetection, null};
                methodInfo.Invoke(null, parameters);

                paths = (Vector2[][]) parameters[5];
            }

            return paths;
        }

        public static void Triangulate(List<Vector2> vertices, List<IndexedEdge> edges, List<Hole> holes,
            ref List<int> indices)
        {
            indices.Clear();

            if (vertices.Count >= 3)
            {
                var inputGeometry = new InputGeometry(vertices.Count);

                for (var i = 0; i < vertices.Count; ++i)
                {
                    var position = vertices[i];
                    inputGeometry.AddPoint(position.x, position.y);
                }

                for (var i = 0; i < edges.Count; ++i)
                {
                    var edge = edges[i];
                    inputGeometry.AddSegment(edge.index1, edge.index2);
                }

                for (var i = 0; i < holes.Count; ++i)
                {
                    var hole = holes[i].vertex;
                    inputGeometry.AddHole(hole.x, hole.y);
                }

                var triangleMesh = new TriangleNet.Mesh();

                triangleMesh.Triangulate(inputGeometry);

                foreach (var triangle in triangleMesh.Triangles)
                    if (triangle.P0 >= 0 && triangle.P0 < vertices.Count &&
                        triangle.P0 >= 0 && triangle.P1 < vertices.Count &&
                        triangle.P0 >= 0 && triangle.P2 < vertices.Count)
                    {
                        indices.Add(triangle.P0);
                        indices.Add(triangle.P2);
                        indices.Add(triangle.P1);
                    }
            }
        }

        public static void Tessellate(List<Vector2> vertices, List<IndexedEdge> indexedEdges, List<Hole> holes,
            List<int> indices, float tessellationAmount)
        {
            if (tessellationAmount <= 0f) return;

            indices.Clear();

            if (vertices.Count >= 3)
            {
                var inputGeometry = new InputGeometry(vertices.Count);

                for (var i = 0; i < vertices.Count; ++i)
                {
                    var vertex = vertices[i];
                    inputGeometry.AddPoint(vertex.x, vertex.y);
                }

                for (var i = 0; i < indexedEdges.Count; ++i)
                {
                    var edge = indexedEdges[i];
                    inputGeometry.AddSegment(edge.index1, edge.index2);
                }

                for (var i = 0; i < holes.Count; ++i)
                {
                    var hole = holes[i].vertex;
                    inputGeometry.AddHole(hole.x, hole.y);
                }

                var triangleMesh = new TriangleNet.Mesh();
                var statistic = new Statistic();

                triangleMesh.Triangulate(inputGeometry);

                triangleMesh.Behavior.MinAngle = 20.0;
                triangleMesh.Behavior.SteinerPoints = -1;
                triangleMesh.Refine(true);

                statistic.Update(triangleMesh, 1);

                triangleMesh.Refine(statistic.LargestArea / tessellationAmount);
                triangleMesh.Renumber();

                vertices.Clear();
                indexedEdges.Clear();

                foreach (var vertex in triangleMesh.Vertices)
                    vertices.Add(new Vector2((float) vertex.X, (float) vertex.Y));

                foreach (var segment in triangleMesh.Segments)
                    indexedEdges.Add(new IndexedEdge(segment.P0, segment.P1));

                foreach (var triangle in triangleMesh.Triangles)
                    if (triangle.P0 >= 0 && triangle.P0 < vertices.Count &&
                        triangle.P0 >= 0 && triangle.P1 < vertices.Count &&
                        triangle.P0 >= 0 && triangle.P2 < vertices.Count)
                    {
                        indices.Add(triangle.P0);
                        indices.Add(triangle.P2);
                        indices.Add(triangle.P1);
                    }
            }
        }

        public static Vector3 TexCoordToVertex(Vector2 pivotPoint, Vector2 vertex, float pixelsPerUnit)
        {
            return (Vector3) (vertex - pivotPoint) / pixelsPerUnit;
        }

        public static Vector2 VertexToTexCoord(SpriteMesh spriteMesh, Vector2 pivotPoint, Vector3 vertex,
            float pixelsPerUnit)
        {
            Vector2 texCoord = Vector3.zero;

            if (spriteMesh != null) texCoord = (Vector2) vertex * pixelsPerUnit + pivotPoint;

            return texCoord;
        }

        public static Rect GetRect(Sprite sprite)
        {
            var pixelsPerUnit = GetSpritePixelsPerUnit(sprite);
            var factor = pixelsPerUnit / sprite.pixelsPerUnit;
            var position = sprite.rect.position * factor;
            var size = sprite.rect.size * factor;

            return new Rect(position.x, position.y, size.x, size.y);
        }

        public static Vector2 GetPivotPoint(Sprite sprite)
        {
            var pixelsPerUnit = GetSpritePixelsPerUnit(sprite);
            return (sprite.pivot + sprite.rect.position) * pixelsPerUnit / sprite.pixelsPerUnit;
        }

        public static Rect CalculateSpriteRect(SpriteMesh spriteMesh, int padding)
        {
            var width = 0;
            var height = 0;

            GetSpriteTextureSize(spriteMesh.sprite, ref width, ref height);

            var spriteMeshData = LoadSpriteMeshData(spriteMesh);

            var rect = spriteMesh.sprite.rect;

            if (spriteMeshData)
            {
                var min = new Vector2(float.MaxValue, float.MaxValue);
                var max = new Vector2(float.MinValue, float.MinValue);

                for (var i = 0; i < spriteMeshData.vertices.Length; i++)
                {
                    var v = spriteMeshData.vertices[i];

                    if (v.x < min.x) min.x = v.x;
                    if (v.y < min.y) min.y = v.y;
                    if (v.x > max.x) max.x = v.x;
                    if (v.y > max.y) max.y = v.y;
                }

                rect.position = min - Vector2.one * padding;
                rect.size = max - min + Vector2.one * padding * 2f;
                rect = MathUtils.ClampRect(rect, new Rect(0f, 0f, width, height));
            }

            return rect;
        }

        public static SpriteMeshInstance CreateSpriteMeshInstance(SpriteMesh spriteMesh, bool undo = true)
        {
            if (spriteMesh)
            {
                var gameObject = new GameObject(spriteMesh.name);

                if (undo) Undo.RegisterCreatedObjectUndo(gameObject, Undo.GetCurrentGroupName());

                return CreateSpriteMeshInstance(spriteMesh, gameObject, undo);
            }

            return null;
        }

        public static SpriteMeshInstance CreateSpriteMeshInstance(SpriteMesh spriteMesh, GameObject gameObject,
            bool undo = true)
        {
            SpriteMeshInstance spriteMeshInstance = null;

            if (spriteMesh && gameObject)
            {
                if (undo)
                    spriteMeshInstance = Undo.AddComponent<SpriteMeshInstance>(gameObject);
                else
                    spriteMeshInstance = gameObject.AddComponent<SpriteMeshInstance>();
                spriteMeshInstance.spriteMesh = spriteMesh;
                spriteMeshInstance.sharedMaterial = defaultMaterial;

                var spriteMeshData = LoadSpriteMeshData(spriteMesh);

                var bones = new List<Bone2D>();
                var paths = new List<string>();

                var zero = new Vector4(0f, 0f, 0f, 1f);

                foreach (var bindInfo in spriteMeshData.bindPoses)
                {
                    var m = spriteMeshInstance.transform.localToWorldMatrix * bindInfo.bindPose.inverse;

                    var bone = new GameObject(bindInfo.name);

                    if (undo) Undo.RegisterCreatedObjectUndo(bone, Undo.GetCurrentGroupName());

                    var boneComponent = bone.AddComponent<Bone2D>();

                    boneComponent.localLength = bindInfo.boneLength;
                    bone.transform.position = m * zero;
                    bone.transform.rotation = m.GetRotation();
                    bone.transform.parent = gameObject.transform;

                    bones.Add(boneComponent);
                    paths.Add(bindInfo.path);
                }

                BoneUtils.ReconstructHierarchy(bones, paths);

                spriteMeshInstance.bones = bones;

                UpdateRenderer(spriteMeshInstance, undo);

                EditorUtility.SetDirty(spriteMeshInstance);
            }

            return spriteMeshInstance;
        }

        public static bool HasNullBones(SpriteMeshInstance spriteMeshInstance)
        {
            if (spriteMeshInstance) return spriteMeshInstance.bones.Contains(null);
            return false;
        }

        public static bool CanEnableSkinning(SpriteMeshInstance spriteMeshInstance)
        {
            return spriteMeshInstance.spriteMesh && !HasNullBones(spriteMeshInstance) &&
                   spriteMeshInstance.bones.Count > 0 && spriteMeshInstance.spriteMesh.sharedMesh.bindposes.Length ==
                   spriteMeshInstance.bones.Count;
        }

        public static void UpdateRenderer(SpriteMeshInstance spriteMeshInstance, bool undo = true)
        {
            if (!spriteMeshInstance) return;

            var spriteMeshInstaceSO = new SerializedObject(spriteMeshInstance);

            var spriteMesh = spriteMeshInstaceSO.FindProperty("m_SpriteMesh").objectReferenceValue as SpriteMesh;

            if (spriteMesh)
            {
                var sharedMesh = spriteMesh.sharedMesh;

                if (sharedMesh.bindposes.Length > 0 && spriteMeshInstance.bones.Count > sharedMesh.bindposes.Length)
                    spriteMeshInstance.bones = spriteMeshInstance.bones.GetRange(0, sharedMesh.bindposes.Length);

                if (CanEnableSkinning(spriteMeshInstance))
                {
                    var meshFilter = spriteMeshInstance.cachedMeshFilter;
                    var meshRenderer = spriteMeshInstance.cachedRenderer as MeshRenderer;

                    if (meshFilter)
                    {
                        if (undo)
                            Undo.DestroyObjectImmediate(meshFilter);
                        else
                            Object.DestroyImmediate(meshFilter);
                    }

                    if (meshRenderer)
                    {
                        if (undo)
                            Undo.DestroyObjectImmediate(meshRenderer);
                        else
                            Object.DestroyImmediate(meshRenderer);
                    }

                    var skinnedMeshRenderer = spriteMeshInstance.cachedSkinnedRenderer;

                    if (!skinnedMeshRenderer)
                    {
                        if (undo)
                            skinnedMeshRenderer = Undo.AddComponent<SkinnedMeshRenderer>(spriteMeshInstance.gameObject);
                        else
                            skinnedMeshRenderer = spriteMeshInstance.gameObject.AddComponent<SkinnedMeshRenderer>();
                    }

                    skinnedMeshRenderer.bones = spriteMeshInstance.bones.ConvertAll(bone => bone.transform).ToArray();

                    if (spriteMeshInstance.bones.Count > 0)
                        skinnedMeshRenderer.rootBone = spriteMeshInstance.bones[0].transform;

                    EditorUtility.SetDirty(skinnedMeshRenderer);
                }
                else
                {
                    var skinnedMeshRenderer = spriteMeshInstance.cachedSkinnedRenderer;
                    var meshFilter = spriteMeshInstance.cachedMeshFilter;
                    var meshRenderer = spriteMeshInstance.cachedRenderer as MeshRenderer;

                    if (skinnedMeshRenderer)
                    {
                        if (undo)
                            Undo.DestroyObjectImmediate(skinnedMeshRenderer);
                        else
                            Object.DestroyImmediate(skinnedMeshRenderer);
                    }

                    if (!meshFilter)
                    {
                        if (undo)
                            meshFilter = Undo.AddComponent<MeshFilter>(spriteMeshInstance.gameObject);
                        else
                            meshFilter = spriteMeshInstance.gameObject.AddComponent<MeshFilter>();

                        EditorUtility.SetDirty(meshFilter);
                    }

                    if (!meshRenderer)
                    {
                        if (undo)
                            meshRenderer = Undo.AddComponent<MeshRenderer>(spriteMeshInstance.gameObject);
                        else
                            meshRenderer = spriteMeshInstance.gameObject.AddComponent<MeshRenderer>();

                        EditorUtility.SetDirty(meshRenderer);
                    }
                }
            }
        }

        public static bool NeedsOverride(SpriteMesh spriteMesh)
        {
            if (!spriteMesh || !spriteMesh.sprite) return false;

            var spriteMeshData = LoadSpriteMeshData(spriteMesh);

            if (!spriteMeshData) return false;

            var triangles = spriteMesh.sprite.triangles;

            if (triangles.Length != spriteMeshData.indices.Length) return true;

            for (var i = 0; i < triangles.Length; i++)
                if (spriteMeshData.indices[i] != triangles[i])
                    return true;

            return false;
        }

        public static BlendShape CreateBlendShape(SpriteMesh spriteMesh, string blendshapeName)
        {
            BlendShape l_blendshape = null;

            var spriteMeshData = LoadSpriteMeshData(spriteMesh);

            if (spriteMeshData)
            {
                l_blendshape = BlendShape.Create(blendshapeName);

                l_blendshape.hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.AddObjectToAsset(l_blendshape, spriteMeshData);

                var l_blendshapes = new List<BlendShape>(spriteMeshData.blendshapes);

                l_blendshapes.Add(l_blendshape);

                spriteMeshData.blendshapes = l_blendshapes.ToArray();

                EditorUtility.SetDirty(spriteMeshData);
                EditorUtility.SetDirty(l_blendshape);
            }

            return l_blendshape;
        }

        public static BlendShapeFrame CreateBlendShapeFrame(BlendShape blendshape, float weight, Vector3[] vertices)
        {
            BlendShapeFrame l_blendshapeFrame = null;

            if (blendshape && vertices != null)
            {
                l_blendshapeFrame = BlendShapeFrame.Create(weight, vertices);

                l_blendshapeFrame.hideFlags = HideFlags.HideInHierarchy;

                AssetDatabase.AddObjectToAsset(l_blendshapeFrame, blendshape);

                var l_blendshapeFrames = new List<BlendShapeFrame>(blendshape.frames);

                l_blendshapeFrames.Add(l_blendshapeFrame);

                l_blendshapeFrames.Sort((a, b) => { return a.weight.CompareTo(b.weight); });

                blendshape.frames = l_blendshapeFrames.ToArray();

                EditorUtility.SetDirty(blendshape);
                EditorUtility.SetDirty(l_blendshapeFrame);
            }

            return l_blendshapeFrame;
        }

        public static void DestroyBlendShapes(SpriteMesh spriteMesh)
        {
            DestroyBlendShapes(spriteMesh, false, "");
        }

        public static void DestroyBlendShapes(SpriteMesh spriteMesh, bool undo, string undoName)
        {
            DestroyBlendShapes(LoadSpriteMeshData(spriteMesh), false, "");
        }

        public static void DestroyBlendShapes(SpriteMeshData spriteMeshData, bool undo, string undoName)
        {
            if (spriteMeshData)
            {
                if (undo && !string.IsNullOrEmpty(undoName)) Undo.RegisterCompleteObjectUndo(spriteMeshData, undoName);

                foreach (var blendShape in spriteMeshData.blendshapes)
                {
                    foreach (var frame in blendShape.frames)
                        if (undo)
                            Undo.DestroyObjectImmediate(frame);
                        else
                            Object.DestroyImmediate(frame, true);

                    if (undo)
                        Undo.DestroyObjectImmediate(blendShape);
                    else
                        Object.DestroyImmediate(blendShape, true);
                }

                spriteMeshData.blendshapes = new BlendShape[0];
            }
        }

        public static void RebuildBlendShapes(SpriteMesh spriteMesh)
        {
            RebuildBlendShapes(spriteMesh, spriteMesh.sharedMesh);
        }

        public static void RebuildBlendShapes(SpriteMesh spriteMesh, Mesh mesh)
        {
            if (!mesh)
                return;

            if (!spriteMesh)
                return;

            BlendShape[] blendShapes = null;

            var spriteMeshData = LoadSpriteMeshData(spriteMesh);

            if (spriteMeshData) blendShapes = spriteMeshData.blendshapes;

            if (spriteMesh.sharedMesh.vertexCount != mesh.vertexCount) return;

            if (blendShapes != null)
            {
#if !(UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
                var blendShapeNames = new List<string>();

                mesh.ClearBlendShapes();

                var from = mesh.vertices;

                for (var i = 0; i < blendShapes.Length; i++)
                {
                    var blendshape = blendShapes[i];

                    if (blendshape)
                    {
                        var blendShapeName = blendshape.name;

                        if (blendShapeNames.Contains(blendShapeName))
                        {
                            Debug.LogWarning("Found repeated BlendShape name '" + blendShapeName + "' in SpriteMesh: " +
                                             spriteMesh.name);
                        }
                        else
                        {
                            blendShapeNames.Add(blendShapeName);

                            for (var j = 0; j < blendshape.frames.Length; j++)
                            {
                                var l_blendshapeFrame = blendshape.frames[j];

                                if (l_blendshapeFrame && from.Length == l_blendshapeFrame.vertices.Length)
                                {
                                    var deltaVertices = GetDeltaVertices(from, l_blendshapeFrame.vertices);

                                    mesh.AddBlendShapeFrame(blendShapeName, l_blendshapeFrame.weight, deltaVertices,
                                        null, null);
                                }
                            }
                        }
                    }
                }

                mesh.UploadMeshData(false);

                EditorUtility.SetDirty(mesh);
#endif
            }
        }

        private static Vector3[] GetDeltaVertices(Vector3[] from, Vector3[] to)
        {
            var result = new Vector3[from.Length];

            for (var i = 0; i < to.Length; i++) result[i] = to[i] - from[i];

            return result;
        }

        public class WeightedTriangle
        {
            public WeightedTriangle(int _p1, int _p2, int _p3,
                float _w1, float _w2, float _w3)
            {
                p1 = _p1;
                p2 = _p2;
                p3 = _p3;
                w1 = _w1;
                w2 = _w2;
                w3 = _w3;
                weight = (w1 + w2 + w3) / 3f;
            }

            public int p1 { get; }

            public int p2 { get; }

            public int p3 { get; }

            public float w1 { get; }

            public float w2 { get; }

            public float w3 { get; }

            public float weight { get; }
        }
    }
}