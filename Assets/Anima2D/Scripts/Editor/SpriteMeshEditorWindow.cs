using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Sprites;
using UnityEngine;

namespace Anima2D
{
    public class SpriteMeshEditorWindow : TextureEditorWindow
    {
        public enum Mode
        {
            Mesh,
            Blendshapes
        }

        private readonly BlendShapeEditor blendShapeEditor = new BlendShapeEditor();
        private readonly BlendShapeFrameEditor blendShapeFrameEditor = new BlendShapeFrameEditor();
        private readonly InspectorEditor inspectorEditor = new InspectorEditor();

        private List<Color> m_BindPoseColors;

        private Edge m_ClosestEdge;

        private int m_LastNearestControl = -1;

        private Vector2 m_MousePositionWorld;
        private Texture2D m_OriginalTexture;

        private List<Vector3> m_Points;

        private readonly RectManipulator m_RectManipulator = new RectManipulator();
        private int m_selectionControlID;

        private SpriteMeshCache m_SpriteMeshCache;

        private Matrix4x4 m_SpriteMeshMatrix;

        private TextureImporter m_TextureImporter;
        private List<Vector2> m_UVs;
        private readonly MeshToolEditor meshToolEditor = new MeshToolEditor();

        private Material mMeshGuiMaterial;
        private readonly SliceEditor sliceEditor = new SliceEditor();

        private readonly WeightEditor weightEditor = new WeightEditor();

        private Material meshGuiMaterial
        {
            get
            {
                if (!mMeshGuiMaterial)
                {
                    mMeshGuiMaterial = new Material(Shader.Find("Hidden/Internal-GUITextureClip"));
                    mMeshGuiMaterial.hideFlags = HideFlags.DontSave;
                }

                return mMeshGuiMaterial;
            }
        }

        private Node hoveredNode { get; set; }
        private Hole hoveredHole { get; set; }
        private Edge hoveredEdge { get; set; }
        private BindInfo hoveredBindPose { get; set; }
        private Bone2D hoveredBone { get; set; }

        private bool showBones { get; set; }
        private bool showTriangles { get; set; }

        //GUIStyle m_DefaultWindowStyle = null;
        //GUIStyle m_Style = null;

        private Color vertexColor => Color.cyan;

        private void OnEnable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            Undo.undoRedoPerformed += UndoRedoPerformed;

            showBones = true;

            weightEditor.canShow = () => { return meshToolEditor.canShow() && m_SpriteMeshCache.isBound; };

            inspectorEditor.canShow = () => { return true; };

            meshToolEditor.canShow = () => { return m_SpriteMeshCache.mode == Mode.Mesh; };

            sliceEditor.canShow = () => { return meshToolEditor.canShow() && meshToolEditor.sliceToggle; };

            blendShapeEditor.canShow = () => { return m_SpriteMeshCache.mode == Mode.Blendshapes; };

            blendShapeFrameEditor.canShow = () =>
            {
                return blendShapeEditor.canShow() && m_SpriteMeshCache.selectedBlendshape;
            };

            UpdateFromSelection();

            EditorApplication.delayCall += () =>
            {
                RefreshTexture(true);
                SetScrollPositionAndZoomFromSprite();
            };
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            if (mMeshGuiMaterial) DestroyImmediate(mMeshGuiMaterial);

            if (m_Texture) DestroyImmediate(m_Texture);
        }

        protected override void OnGUI()
        {
            var matrix = Handles.matrix;

            textureColor = Color.gray;

            if (!m_SpriteMeshCache || !m_SpriteMeshCache.spriteMesh)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Label("No sprite mesh selected");
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                /*
                if(m_Style == null || m_DefaultWindowStyle == null)
                {
                    m_DefaultWindowStyle = GUI.skin.window;
                    m_Style = new GUIStyle(m_DefaultWindowStyle);

                    m_Style.active.background = null;
                    m_Style.focused.background = null;
                    m_Style.hover.background = null;
                    m_Style.normal.background = null;
                    m_Style.onActive.background = null;
                    m_Style.onFocused.background = null;
                    m_Style.onHover.background = null;
                    m_Style.onNormal.background = null;
                }
                */

                autoRepaintOnSceneChange = true;
                wantsMouseMove = true;

                HotKeys();

                if (Event.current.type == EventType.MouseUp && Event.current.button == 1)
                    meshToolEditor.tool = MeshToolEditor.MeshTool.None;

                base.OnGUI();

                GUI.color = Color.white;

                GUILayout.BeginArea(m_TextureViewRect);

                BeginWindows();

                //GUI.skin.window = m_Style;

                if (m_SpriteMeshCache.mode == Mode.Mesh)
                {
                    meshToolEditor.spriteMeshCache = m_SpriteMeshCache;
                    meshToolEditor.OnWindowGUI(m_TextureViewRect);
                }
                else if (m_SpriteMeshCache.mode == Mode.Blendshapes)
                {
                    blendShapeEditor.spriteMeshCache = m_SpriteMeshCache;
                    blendShapeEditor.OnWindowGUI(m_TextureViewRect);
                }

                //GUI.skin.window = m_DefaultWindowStyle;

                sliceEditor.spriteMeshCache = m_SpriteMeshCache;
                sliceEditor.OnWindowGUI(m_TextureViewRect);

                weightEditor.spriteMeshCache = m_SpriteMeshCache;
                weightEditor.OnWindowGUI(m_TextureViewRect);

                blendShapeFrameEditor.spriteMeshCache = m_SpriteMeshCache;
                blendShapeFrameEditor.OnWindowGUI(m_TextureViewRect);

                inspectorEditor.spriteMeshCache = m_SpriteMeshCache;
                inspectorEditor.OnWindowGUI(m_TextureViewRect);

                EndWindows();

                if (m_SpriteMeshCache.mode == Mode.Mesh &&
                    !meshToolEditor.sliceToggle)
                {
                    HandleDeleteVertex();
                    HandleDeleteHole();
                    HandleDeleteEdge();
                    HandleDeleteBone();
                    HandleDeleteBindPose();
                }

                GUILayout.EndArea();
            }

            Handles.matrix = matrix;

            if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl != m_LastNearestControl)
            {
                m_LastNearestControl = HandleUtility.nearestControl;
                Repaint();
            }
        }

        private void OnSelectionChange()
        {
            UpdateFromSelection();
            Repaint();
        }

        [MenuItem("Window/Anima2D/SpriteMesh Editor", false, 0)]
        private static void ContextInitialize()
        {
            GetWindow();
        }

        public static SpriteMeshEditorWindow GetWindow()
        {
            return GetWindow<SpriteMeshEditorWindow>("SpriteMesh Editor");
        }

        private void UndoRedoPerformed()
        {
            m_SpriteMeshCache.PrepareManipulableVertices();

            Repaint();
        }

        protected override void DoTextureGUIExtras()
        {
            m_selectionControlID = GUIUtility.GetControlID("SelectionRect".GetHashCode(), FocusType.Keyboard);

            m_Points = m_SpriteMeshCache.GetTexVerticesV3();
            m_UVs = m_SpriteMeshCache.uvs;
            m_BindPoseColors = m_SpriteMeshCache.bindPoses.ConvertAll(b => b.color);

            hoveredNode = null;
            hoveredEdge = null;
            hoveredHole = null;
            hoveredBindPose = null;
            hoveredBone = null;
            m_ClosestEdge = null;

            m_MousePositionWorld = ScreenToWorld(Event.current.mousePosition - new Vector2(2f, 3f));

            if (m_SpriteMeshCache.mode == Mode.Mesh) m_ClosestEdge = GetClosestEdge(m_MousePositionWorld);

            if (showBones)
            {
                if (m_SpriteMeshCache.isBound)
                    BindPosesGUI(meshToolEditor.sliceToggle ||
                                 m_SpriteMeshCache.mode != Mode.Mesh);
                else
                    BonesGUI(meshToolEditor.sliceToggle ||
                             m_SpriteMeshCache.mode != Mode.Mesh);
            }

            var disableEditGeometry = m_SpriteMeshCache.mode == Mode.Mesh && meshToolEditor.sliceToggle;

            EdgesGUI(disableEditGeometry);
            VerticesGUI(disableEditGeometry);

            if (m_SpriteMeshCache.mode == Mode.Mesh)
            {
                PivotHandleGUI(meshToolEditor.sliceToggle);
                HolesGUI(meshToolEditor.sliceToggle);

                if (!meshToolEditor.sliceToggle)
                {
                    HandleAddVertex();
                    HandleAddHole();
                    HandleAddEdge();
                    HandleAddBindPose();
                }
            }

            if (!meshToolEditor.sliceToggle || m_SpriteMeshCache.mode != Mode.Mesh) HandleSelection();

            RectManipulatorGUI();
        }

        private void HandleAddBindPose()
        {
            if (m_SpriteMeshCache.spriteMeshInstance && DragAndDrop.objectReferences.Length > 0)
            {
                var eventType = Event.current.GetTypeForControl(0);

                var dragAndDropGameObjects = DragAndDrop.objectReferences.ToList().ConvertAll(o => o as GameObject);
                var dragAndDropBones = dragAndDropGameObjects.ConvertAll(go => go ? go.GetComponent<Bone2D>() : null);

                if (eventType == EventType.DragPerform)
                {
                    var performAutoWeights = m_SpriteMeshCache.bindPoses.Count == 0;

                    m_SpriteMeshCache.RegisterUndo("add bind pose");
                    foreach (var bone in dragAndDropBones) m_SpriteMeshCache.BindBone(bone);

                    if (performAutoWeights) m_SpriteMeshCache.CalculateAutomaticWeights();

                    //EditorUtility.SetDirty(m_SpriteMeshCache.spriteMeshInstance);

                    Event.current.Use();
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
        }

        private void HandleDeleteBone()
        {
            var eventType = Event.current.GetTypeForControl(0);

            if (eventType == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete) &&
                m_SpriteMeshCache.selectedBone &&
                m_SpriteMeshCache.selection.Count == 0 &&
                !m_SpriteMeshCache.selectedHole &&
                !m_SpriteMeshCache.selectedEdge)
            {
                Undo.RegisterCompleteObjectUndo(m_SpriteMeshCache.spriteMeshInstance, "delete bone");
                m_SpriteMeshCache.DeleteBone(m_SpriteMeshCache.selectedBone);

                Event.current.Use();
            }
        }

        private void HandleDeleteBindPose()
        {
            var eventType = Event.current.GetTypeForControl(0);

            if (eventType == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete) &&
                m_SpriteMeshCache.selectedBindPose &&
                m_SpriteMeshCache.selection.Count == 0 &&
                !m_SpriteMeshCache.selectedHole &&
                !m_SpriteMeshCache.selectedEdge)
            {
                m_SpriteMeshCache.RegisterUndo("delete bind pose");
                m_SpriteMeshCache.DeleteBindPose(m_SpriteMeshCache.selectedBindPose);

                Event.current.Use();
            }
        }

        private void HandleSelection()
        {
            if (Event.current.GetTypeForControl(0) == EventType.MouseDown &&
                !Event.current.alt && Event.current.button == 0 &&
                HandleUtility.nearestControl == m_selectionControlID ||
                Event.current.button == 1)
                if (!EditorGUI.actionKey && m_SpriteMeshCache.selection.Count > 0 ||
                    m_SpriteMeshCache.selectedEdge ||
                    m_SpriteMeshCache.selectedHole ||
                    (m_SpriteMeshCache.selectedBindPose || m_SpriteMeshCache.selectedBone) && Event.current.button == 1)
                {
                    m_SpriteMeshCache.RegisterUndo("selection");
                    m_SpriteMeshCache.ClearSelection();
                    m_SpriteMeshCache.selectedEdge = null;
                    m_SpriteMeshCache.selectedHole = null;

                    if (Event.current.button == 1 &&
                        (m_SpriteMeshCache.selectedBindPose || m_SpriteMeshCache.selectedBone))
                    {
                        m_SpriteMeshCache.selectedBindPose = null;
                        m_SpriteMeshCache.selectedBone = null;
                    }
                }

            if (GUIUtility.hotControl == m_selectionControlID &&
                Event.current.GetTypeForControl(m_selectionControlID) == EventType.MouseUp &&
                Event.current.button == 0)
            {
                var numSelectedBeforeSelection = m_SpriteMeshCache.selection.Count;

                m_SpriteMeshCache.RegisterUndo("selection");

                m_SpriteMeshCache.EndSelection(true);

                InitRectManipulatorParams(numSelectedBeforeSelection);

                m_SpriteMeshCache.PrepareManipulableVertices();

                Repaint();
            }

            EditorGUI.BeginChangeCheck();

            var rect = SelectionRectTool.Do(m_selectionControlID);

            if (EditorGUI.EndChangeCheck())
            {
                m_SpriteMeshCache.BeginSelection();

                if (m_Points != null)
                    for (var i = 0; i < m_Points.Count; i++)
                    {
                        Vector2 p = m_Points[i];
                        if (rect.Contains(p, true)) m_SpriteMeshCache.selection.Select(i, true);
                    }
            }
        }

        private void BonesGUI(bool disable)
        {
            if (!m_SpriteMeshCache.spriteMeshInstance) return;

            var old = Handles.matrix;
            Handles.matrix = m_SpriteMeshMatrix;
            var radius = 7.5f / m_TextureImporter.spritePixelsPerUnit / m_Zoom;

            for (var i = 0; i < m_SpriteMeshCache.spriteMeshInstance.bones.Count; i++)
            {
                var controlID = GUIUtility.GetControlID("BonesHandle".GetHashCode(), FocusType.Passive);
                var eventType = Event.current.GetTypeForControl(controlID);

                var bone = m_SpriteMeshCache.spriteMeshInstance.bones[i];

                if (bone)
                {
                    var position =
                        m_SpriteMeshCache.spriteMeshInstance.transform.InverseTransformPoint(bone.transform.position);
                    var endPoint =
                        m_SpriteMeshCache.spriteMeshInstance.transform.InverseTransformPoint(bone.endPosition);

                    if (!disable)
                    {
                        if (HandleUtility.nearestControl == controlID &&
                            GUIUtility.hotControl == 0)
                        {
                            hoveredBone = bone;

                            if (eventType == EventType.MouseDown &&
                                Event.current.button == 0 &&
                                !Event.current.shift &&
                                !Event.current.alt)
                            {
                                if (m_SpriteMeshCache.selectedBone != bone)
                                {
                                    m_SpriteMeshCache.RegisterUndo("select bone");
                                    m_SpriteMeshCache.selectedBone = bone;
                                }

                                Event.current.Use();
                            }
                        }

                        if (eventType == EventType.Layout)
                            HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(position, endPoint));
                    }

                    if (bone && eventType == EventType.Repaint)
                    {
                        if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controlID ||
                            m_SpriteMeshCache.selectedBone == bone)
                            BoneUtils.DrawBoneOutline(position, endPoint, radius, radius * 0.25f, Color.yellow);

                        var color = bone.color;
                        color.a = 1f;
                        BoneUtils.DrawBoneBody(position, endPoint, radius, color);
                        color = bone.color * 0.25f;
                        color.a = 1f;
                        BoneUtils.DrawBoneCap(position, radius, color);
                    }
                }
            }

            Handles.matrix = old;
        }

        private void BindPosesGUI(bool disable)
        {
            var old = Handles.matrix;
            Handles.matrix = m_SpriteMeshMatrix;
            var radius = 7.5f / m_TextureImporter.spritePixelsPerUnit / m_Zoom;

            for (var i = 0; i < m_SpriteMeshCache.bindPoses.Count; i++)
            {
                var controlID = GUIUtility.GetControlID("BindPoseHandle".GetHashCode(), FocusType.Passive);
                var eventType = Event.current.GetTypeForControl(controlID);

                var bindPose = m_SpriteMeshCache.bindPoses[i];

                if (!disable)
                {
                    if (HandleUtility.nearestControl == controlID &&
                        GUIUtility.hotControl == 0)
                    {
                        hoveredBindPose = bindPose;

                        if (eventType == EventType.MouseDown &&
                            Event.current.button == 0 &&
                            !Event.current.shift &&
                            !Event.current.alt)
                        {
                            if (m_SpriteMeshCache.selectedBindPose != bindPose)
                            {
                                m_SpriteMeshCache.RegisterUndo("select bind pose");
                                m_SpriteMeshCache.selectedBindPose = bindPose;
                            }

                            Event.current.Use();
                        }
                    }

                    if (eventType == EventType.Layout)
                        HandleUtility.AddControl(controlID,
                            HandleUtility.DistanceToLine(bindPose.position, bindPose.endPoint));
                }

                if (eventType == EventType.Repaint)
                {
                    var innerColor = m_BindPoseColors[i] * 0.25f;
                    innerColor.a = 1f;

                    if (GUIUtility.hotControl == 0 && HandleUtility.nearestControl == controlID ||
                        m_SpriteMeshCache.selectedBindPose == bindPose)
                    {
                        BoneUtils.DrawBoneOutline(bindPose.position, bindPose.endPoint, radius, radius * 0.25f,
                            Color.white);
                    }
                    else if (m_SpriteMeshCache.mode == Mode.Mesh && weightEditor.overlayColors)
                    {
                        var c = m_BindPoseColors[i] * 0.5f;
                        c.a = 1f;

                        BoneUtils.DrawBoneOutline(bindPose.position, bindPose.endPoint, radius, radius * 0.25f, c);
                    }

                    BoneUtils.DrawBoneBody(bindPose.position, bindPose.endPoint, radius, m_BindPoseColors[i]);
                    var color = m_BindPoseColors[i] * 0.25f;
                    color.a = 1f;
                    BoneUtils.DrawBoneCap(bindPose.position, radius, color);
                }
            }

            Handles.matrix = old;
        }

        private void HolesGUI(bool disable)
        {
            GUI.color = Color.white;

            for (var i = 0; i < m_SpriteMeshCache.holes.Count; i++)
            {
                var hole = m_SpriteMeshCache.holes[i];
                var position = hole.vertex;

                var controlID = GUIUtility.GetControlID("HoleHandle".GetHashCode(), FocusType.Passive);
                var eventType = Event.current.GetTypeForControl(controlID);

                if (!disable)
                {
                    if (HandleUtility.nearestControl == controlID &&
                        GUIUtility.hotControl == 0)
                    {
                        hoveredHole = hole;

                        if (eventType == EventType.MouseDown &&
                            Event.current.button == 0 &&
                            !Event.current.alt)
                        {
                            m_SpriteMeshCache.RegisterUndo("select hole");
                            m_SpriteMeshCache.selectedHole = hole;
                            m_SpriteMeshCache.ClearSelection();
                            m_SpriteMeshCache.selectedEdge = null;
                            Undo.IncrementCurrentGroup();
                        }
                    }

                    EditorGUI.BeginChangeCheck();

                    var newPosition = HandlesExtra.Slider2D(controlID, position, null);

                    if (EditorGUI.EndChangeCheck())
                    {
                        var delta = newPosition - position;
                        m_SpriteMeshCache.RegisterUndo("move hole");
                        hole.vertex += delta;
                        m_SpriteMeshCache.Triangulate();
                    }

                    if (eventType == EventType.Layout)
                        HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(position, 2.5f));
                }

                if (eventType == EventType.Repaint)
                {
                    var highlight = false;

                    if (HandleUtility.nearestControl == controlID && GUIUtility.hotControl == 0 ||
                        m_SpriteMeshCache.selectedHole == hole)
                        highlight = true;

                    if (highlight)
                        HandlesExtra.DrawDotYellow(position);
                    else
                        HandlesExtra.DrawDotRed(position);
                }
            }
        }

        private void EdgesGUI(bool disable)
        {
            for (var i = 0; i < m_SpriteMeshCache.edges.Count; i++)
            {
                var controlID = GUIUtility.GetControlID("EdgeHandle".GetHashCode(), FocusType.Passive);
                var eventType = Event.current.GetTypeForControl(controlID);

                var edge = m_SpriteMeshCache.edges[i];
                var position = m_SpriteMeshCache.GetVertex(edge.node1);

                if (!disable)
                {
                    if (HandleUtility.nearestControl == controlID &&
                        GUIUtility.hotControl == 0)
                    {
                        hoveredEdge = edge;

                        if (eventType == EventType.MouseDown &&
                            Event.current.button == 0 &&
                            m_SpriteMeshCache.selectedEdge != edge &&
                            !Event.current.shift &&
                            !Event.current.alt)
                        {
                            m_SpriteMeshCache.RegisterUndo("select edge");
                            m_SpriteMeshCache.selectedEdge = edge;
                            m_SpriteMeshCache.ClearSelection();
                            m_SpriteMeshCache.selectedHole = null;
                        }
                    }

                    if (!Event.current.shift)
                    {
                        EditorGUI.BeginChangeCheck();

                        var newPosition = HandlesExtra.Slider2D(controlID, position, null);

                        if (EditorGUI.EndChangeCheck())
                        {
                            m_SpriteMeshCache.RegisterUndo("move edge");

                            var delta = newPosition - position;

                            if (m_SpriteMeshCache.IsSelected(edge.node1) && m_SpriteMeshCache.IsSelected(edge.node2))
                            {
                                var selectedNodes = m_SpriteMeshCache.selectedNodes;

                                foreach (var node in selectedNodes)
                                {
                                    var texcoord = m_SpriteMeshCache.GetVertex(node);
                                    m_SpriteMeshCache.SetVertex(node, texcoord + delta);
                                }
                            }
                            else
                            {
                                var texcoord1 = m_SpriteMeshCache.GetVertex(edge.node1);
                                var texcoord2 = m_SpriteMeshCache.GetVertex(edge.node2);
                                m_SpriteMeshCache.SetVertex(edge.node1, texcoord1 + delta);
                                m_SpriteMeshCache.SetVertex(edge.node2, texcoord2 + delta);
                            }

                            m_SpriteMeshCache.Triangulate();
                        }
                    }

                    if (eventType == EventType.Layout)
                    {
                        var texcoord1 = m_SpriteMeshCache.GetVertex(edge.node1);
                        var texcoord2 = m_SpriteMeshCache.GetVertex(edge.node2);
                        HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(texcoord1, texcoord2));
                    }
                }

                if (eventType == EventType.Repaint)
                {
                    var drawEdge = true;

                    if ((HandleUtility.nearestControl == m_selectionControlID ||
                         HandleUtility.nearestControl == controlID) &&
                        Event.current.shift &&
                        !m_SpriteMeshCache.selectedNode &&
                        edge == m_ClosestEdge)
                        drawEdge = false;

                    if (Event.current.shift &&
                        m_SpriteMeshCache.selectedNode &&
                        HandleUtility.nearestControl == controlID)
                        drawEdge = false;

                    if (drawEdge)
                    {
                        Handles.color = vertexColor * new Color(1f, 1f, 1f, 0.75f);

                        if (disable) Handles.color = new Color(0.75f, 0.75f, 0.75f, 1f);

                        if (HandleUtility.nearestControl == controlID && GUIUtility.hotControl == 0 ||
                            m_SpriteMeshCache.selectedEdge == edge)
                            Handles.color = Color.yellow * new Color(1f, 1f, 1f, 0.75f);

                        DrawEdge(edge, 1.5f / m_Zoom);
                    }
                }
            }
        }

        private void VerticesGUI(bool disable)
        {
            GUI.color = Color.white;

            foreach (var node in m_SpriteMeshCache.nodes)
            {
                var position = m_SpriteMeshCache.GetVertex(node);

                var controlID = GUIUtility.GetControlID("VertexHandle".GetHashCode(), FocusType.Passive);
                var eventType = Event.current.GetTypeForControl(controlID);

                if (!disable)
                {
                    if (HandleUtility.nearestControl == controlID &&
                        GUIUtility.hotControl == 0)
                    {
                        hoveredNode = node;

                        if (eventType == EventType.MouseDown &&
                            Event.current.button == 0 &&
                            !Event.current.shift &&
                            !Event.current.alt)
                        {
                            m_SpriteMeshCache.RegisterUndo("select vertex");

                            if (!m_SpriteMeshCache.IsSelected(node))
                            {
                                var numSelectedBeforeSelection = m_SpriteMeshCache.selection.Count;

                                m_SpriteMeshCache.Select(node, EditorGUI.actionKey);

                                InitRectManipulatorParams(numSelectedBeforeSelection);

                                m_SpriteMeshCache.PrepareManipulableVertices();

                                Repaint();
                            }
                            else
                            {
                                if (EditorGUI.actionKey) m_SpriteMeshCache.Unselect(node);
                            }

                            m_SpriteMeshCache.selectedHole = null;
                            m_SpriteMeshCache.selectedEdge = null;

                            Undo.IncrementCurrentGroup();
                        }
                    }

                    if (!Event.current.shift && !EditorGUI.actionKey)
                    {
                        EditorGUI.BeginChangeCheck();

                        var newPosition = HandlesExtra.Slider2D(controlID, position, null);

                        if (EditorGUI.EndChangeCheck())
                        {
                            var delta = newPosition - position;

                            m_SpriteMeshCache.RegisterUndo("move vertices");

                            var selectedNodes = m_SpriteMeshCache.selectedNodes;

                            foreach (var selectedNode in selectedNodes)
                            {
                                var l_position = m_SpriteMeshCache.GetVertex(selectedNode);
                                m_SpriteMeshCache.SetVertex(selectedNode, l_position + delta);
                            }

                            m_SpriteMeshCache.Triangulate();
                        }
                    }

                    if (eventType == EventType.Layout)
                        HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(position, 2.5f));
                }

                if (eventType == EventType.Repaint)
                {
                    var highlight = false;

                    if (HandleUtility.nearestControl == controlID && GUIUtility.hotControl == 0 ||
                        m_SpriteMeshCache.IsSelected(node))
                        highlight = true;

                    if (hoveredEdge && hoveredEdge.ContainsNode(node)) highlight = true;

                    if (m_SpriteMeshCache.selectedEdge && m_SpriteMeshCache.selectedEdge.ContainsNode(node))
                        highlight = true;

                    if (weightEditor.isShown && weightEditor.showPie)
                    {
                        var boneWeigth = m_SpriteMeshCache.GetBoneWeight(node);
                        DrawPie(position, boneWeigth, 10f / m_Zoom, highlight);
                    }
                    else
                    {
                        if (disable)
                        {
                            HandlesExtra.DrawDotCyan(position);
                        }
                        else
                        {
                            if (highlight)
                                HandlesExtra.DrawDotYellow(position);
                            else
                                HandlesExtra.DrawDotCyan(position);
                        }
                    }
                }
            }
        }

        private void PivotHandleGUI(bool disable)
        {
            GUI.color = Color.white;

            var controlID = GUIUtility.GetControlID("PivotHandle".GetHashCode(), FocusType.Passive);
            var eventType = Event.current.GetTypeForControl(controlID);

            if (!disable)
            {
                EditorGUI.BeginChangeCheck();

                var newPivotPoint = HandlesExtra.Slider2D(controlID, m_SpriteMeshCache.pivotPoint, null);

                if (EditorGUI.EndChangeCheck())
                {
                    if (EditorGUI.actionKey)
                    {
                        newPivotPoint.x = (int) (newPivotPoint.x + 0.5f);
                        newPivotPoint.y = (int) (newPivotPoint.y + 0.5f);
                    }

                    m_SpriteMeshCache.RegisterUndo("move pivot");
                    m_SpriteMeshCache.SetPivotPoint(newPivotPoint);
                }

                if (eventType == EventType.Layout)
                    HandleUtility.AddControl(controlID,
                        HandleUtility.DistanceToCircle(m_SpriteMeshCache.pivotPoint, 5f));
            }

            HandlesExtra.PivotCap(controlID, m_SpriteMeshCache.pivotPoint, Quaternion.identity, 1f, eventType);
        }

        private void HandleAddHole()
        {
            if (meshToolEditor.tool != MeshToolEditor.MeshTool.Hole ||
                HandleUtility.nearestControl != m_selectionControlID) return;

            var eventType = Event.current.GetTypeForControl(0);

            if (eventType == EventType.MouseDown &&
                Event.current.button == 0 &&
                Event.current.clickCount == 2)
            {
                m_SpriteMeshCache.RegisterUndo("add hole");
                m_SpriteMeshCache.AddHole(m_MousePositionWorld);
                m_SpriteMeshCache.selectedEdge = null;
                m_SpriteMeshCache.ClearSelection();
                Event.current.Use();
            }
        }

        private void HandleAddVertex()
        {
            if (meshToolEditor.tool != MeshToolEditor.MeshTool.None ||
                HandleUtility.nearestControl != m_selectionControlID && !hoveredEdge) return;

            var eventType = Event.current.GetTypeForControl(0);

            if (eventType == EventType.MouseDown &&
                Event.current.button == 0 &&
                (Event.current.clickCount == 2 || Event.current.shift && !m_SpriteMeshCache.selectedNode))
            {
                m_SpriteMeshCache.RegisterUndo("add point");

                Edge edge = null;

                if (hoveredEdge) edge = hoveredEdge;

                if (Event.current.shift && meshToolEditor.tool == MeshToolEditor.MeshTool.None) edge = m_ClosestEdge;

                m_SpriteMeshCache.AddNode(m_MousePositionWorld, edge);

                m_SpriteMeshCache.selectedEdge = null;

                Event.current.Use();
            }

            if (!m_SpriteMeshCache.selectedNode &&
                !hoveredNode &&
                meshToolEditor.tool == MeshToolEditor.MeshTool.None &&
                Event.current.shift &&
                m_ClosestEdge)
            {
                if (eventType == EventType.Repaint) DrawSplitEdge(m_ClosestEdge, m_MousePositionWorld);

                if (eventType == EventType.MouseMove || eventType == EventType.MouseDrag) Repaint();
            }
        }

        private void HandleDeleteVertex()
        {
            if (Event.current.GetTypeForControl(0) == EventType.KeyDown &&
                m_SpriteMeshCache.selection.Count > 0 &&
                (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete))
            {
                m_SpriteMeshCache.RegisterUndo("delete vertex");

                var selectedNodes = m_SpriteMeshCache.selectedNodes;

                foreach (var node in selectedNodes) m_SpriteMeshCache.DeleteNode(node, false);

                m_SpriteMeshCache.ClearSelection();

                m_SpriteMeshCache.Triangulate();

                Event.current.Use();
            }
        }

        private void HandleDeleteEdge()
        {
            if (Event.current.GetTypeForControl(0) == EventType.KeyDown &&
                m_SpriteMeshCache.selectedEdge != null &&
                (Event.current.keyCode == KeyCode.Backspace || Event.current.keyCode == KeyCode.Delete))
            {
                m_SpriteMeshCache.RegisterUndo("delete edge");

                m_SpriteMeshCache.DeleteEdge(m_SpriteMeshCache.selectedEdge);
                m_SpriteMeshCache.selectedEdge = null;
                m_SpriteMeshCache.Triangulate();

                Event.current.Use();
            }
        }

        private void HandleAddEdge()
        {
            if (GUIUtility.hotControl == 0 &&
                m_SpriteMeshCache.selectedNode &&
                Event.current.shift)
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (Event.current.button == 0 &&
                            m_SpriteMeshCache.selectedNode &&
                            m_SpriteMeshCache.selectedNode != hoveredNode)
                        {
                            m_SpriteMeshCache.RegisterUndo("add edge");

                            var targetVertex = hoveredNode;

                            if (!targetVertex)
                                targetVertex = m_SpriteMeshCache.AddNode(m_MousePositionWorld, hoveredEdge);

                            if (targetVertex)
                            {
                                m_SpriteMeshCache.AddEdge(m_SpriteMeshCache.selectedNode, targetVertex);
                                m_SpriteMeshCache.Select(targetVertex, false);
                            }

                            Event.current.Use();
                            Repaint();
                        }

                        break;
                    case EventType.MouseMove:
                        Event.current.Use();
                        break;
                    case EventType.Repaint:
                        Vector3 targetPosition = m_MousePositionWorld;

                        if (hoveredNode)
                            targetPosition = m_SpriteMeshCache.GetVertex(hoveredNode);
                        else
                            DrawSplitEdge(hoveredEdge, m_MousePositionWorld);

                        Handles.color = Color.yellow;
                        DrawEdge(m_SpriteMeshCache.GetVertex(m_SpriteMeshCache.selectedNode), targetPosition,
                            2f / m_Zoom);

                        break;
                }
        }

        private void HandleDeleteHole()
        {
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    if (GUIUtility.hotControl == 0 &&
                        m_SpriteMeshCache.selectedHole != null &&
                        (Event.current.keyCode == KeyCode.Backspace ||
                         Event.current.keyCode == KeyCode.Delete))
                    {
                        m_SpriteMeshCache.RegisterUndo("delete hole");

                        m_SpriteMeshCache.DeleteHole(m_SpriteMeshCache.selectedHole);

                        m_SpriteMeshCache.selectedHole = null;

                        Event.current.Use();
                    }

                    break;
            }
        }

        private void InitRectManipulatorParams(int numSelectedBeforeSelection)
        {
            if (numSelectedBeforeSelection <= 2)
            {
                var rectParams = m_SpriteMeshCache.rectManipulatorParams;

                var rect = GetSelectedVerticesRect(Vector3.zero, Quaternion.identity);

                rectParams.rotation = Quaternion.identity;
                rectParams.position = rect.center;

                m_SpriteMeshCache.rectManipulatorParams = rectParams;
            }
        }

        private void RectManipulatorGUI()
        {
            if (!EditorGUI.actionKey &&
                m_SpriteMeshCache.mode == Mode.Blendshapes &&
                m_SpriteMeshCache.selection.Count > 1)
            {
                m_RectManipulator.Clear();

                m_RectManipulator.AddVertexManipulable(m_SpriteMeshCache);

                EditorGUI.BeginChangeCheck();

                m_RectManipulator.rectManipulatorParams = m_SpriteMeshCache.rectManipulatorParams;

                m_RectManipulator.DoManipulate();

                if (EditorGUI.EndChangeCheck())
                {
                    m_SpriteMeshCache.RegisterUndo("Rect manipulation");
                    m_SpriteMeshCache.rectManipulatorParams = m_RectManipulator.rectManipulatorParams;
                }
            }
        }

        private Rect GetSelectedVerticesRect(Vector2 position, Quaternion rotation)
        {
            var rect = new Rect();

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            var selectedNodes = m_SpriteMeshCache.selectedNodes;

            foreach (var node in selectedNodes)
            {
                Vector2 v = Quaternion.Inverse(rotation) * (m_SpriteMeshCache.GetVertex(node) - position);

                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
            }

            var offset = Vector2.one * 5f;
            rect.min = min - offset;
            rect.max = max + offset;

            return rect;
        }

        protected override void HandleEvents()
        {
            if (!blendShapeFrameEditor.isHovered) HandleZoom();

            HandlePanning();

            SetupSpriteMeshMatrix();
        }

        protected override void DrawGizmos()
        {
            DrawSpriteMesh();

            if (showTriangles) DrawTriangles();
        }

        private Vector2 ScreenToWorld(Vector2 position)
        {
            return Handles.inverseMatrix.MultiplyPoint(position);
        }

        private Edge GetClosestEdge(Vector2 position)
        {
            var minSqrtDistance = float.MaxValue;
            Edge closestEdge = null;

            for (var i = 0; i < m_SpriteMeshCache.edges.Count; ++i)
            {
                var edge = m_SpriteMeshCache.edges[i];
                if (edge && edge.node1 && edge.node2)
                {
                    var sqrtDistance = MathUtils.SegmentSqrtDistance(position,
                        m_SpriteMeshCache.GetVertex(edge.node1),
                        m_SpriteMeshCache.GetVertex(edge.node2));
                    if (sqrtDistance < minSqrtDistance)
                    {
                        closestEdge = edge;
                        minSqrtDistance = sqrtDistance;
                    }
                }
            }

            return closestEdge;
        }

        private void DoApply()
        {
            m_SpriteMeshCache.ApplyChanges();
        }

        private void InvalidateCache()
        {
            if (m_SpriteMeshCache)
            {
                Undo.ClearUndo(m_SpriteMeshCache);

                DestroyImmediate(m_SpriteMeshCache);
            }

            m_SpriteMeshCache = CreateInstance<SpriteMeshCache>();
            m_SpriteMeshCache.hideFlags = HideFlags.DontSave;
        }

        private void DoRevert()
        {
            var spriteMesh = m_SpriteMeshCache.spriteMesh;
            var spriteMeshInstance = m_SpriteMeshCache.spriteMeshInstance;

            InvalidateCache();

            m_SpriteMeshCache.SetSpriteMesh(spriteMesh, spriteMeshInstance);
        }

        protected override void DoToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(m_SpriteMeshCache.spriteMesh == null);

            showBones = GUILayout.Toggle(showBones, s_Styles.showBonesIcon, EditorStyles.toolbarButton,
                GUILayout.Width(32));
            showTriangles = true;

            if (GUILayout.Toggle(m_SpriteMeshCache.mode == Mode.Mesh,
                new GUIContent("Mesh", "Edit the mesh's geometry"), EditorStyles.toolbarButton))
                m_SpriteMeshCache.mode = Mode.Mesh;

            /*
            if(GUILayout.Toggle(m_SpriteMeshCache.mode == Mode.Blendshapes, new GUIContent("Blendshapes", "Blendshapes"), EditorStyles.toolbarButton))
            {
                m_SpriteMeshCache.mode = Mode.Blendshapes;
            }
            */

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("Revert", "Revert changes"), EditorStyles.toolbarButton)) DoRevert();

            if (GUILayout.Button(new GUIContent("Apply", "Apply changes"), EditorStyles.toolbarButton)) DoApply();

            DoAlphaZoomToolbarGUI();

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private void HotKeys()
        {
            if (GUIUtility.hotControl == 0 &&
                Event.current.type == EventType.KeyDown)
                if (Event.current.keyCode == KeyCode.H)
                {
                    meshToolEditor.tool = MeshToolEditor.MeshTool.Hole;
                    Event.current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                    Repaint();
                }
        }

        private void SetupSpriteMeshMatrix()
        {
            var textureRect = m_TextureRect;
            textureRect.position -= m_ScrollPosition;

            var invertY = new Vector3(1f, -1f, 0f);

            m_SpriteMeshMatrix.SetTRS(
                new Vector3(textureRect.x, textureRect.y + textureRect.height, 0f) +
                Vector3.Scale(m_SpriteMeshCache.pivotPoint, invertY) * m_Zoom,
                Quaternion.Euler(0f, 0f, 0f),
                invertY * m_Zoom * m_TextureImporter.spritePixelsPerUnit);
        }

        private void DrawMesh(Vector3[] vertices, Vector2[] uvs, Color[] colors, int[] triangles, Material material)
        {
            var mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;

            material.SetPass(0);

            Graphics.DrawMeshNow(mesh, Handles.matrix * GUI.matrix);

            DestroyImmediate(mesh);
        }

        private void DrawSpriteMesh()
        {
            if (m_SpriteMeshCache)
            {
                Texture mainTexture = m_Texture;

                var colors = new List<Color>(m_SpriteMeshCache.nodes.Count);

                if (m_SpriteMeshCache.mode == Mode.Mesh &&
                    m_SpriteMeshCache.isBound &&
                    weightEditor.overlayColors)
                {
                    foreach (var node in m_SpriteMeshCache.nodes)
                    {
                        var boneWeight = m_SpriteMeshCache.GetBoneWeight(node);
                        var boneIndex0 = boneWeight.boneIndex0;
                        var boneIndex1 = boneWeight.boneIndex1;
                        var boneIndex2 = boneWeight.boneIndex2;
                        var boneIndex3 = boneWeight.boneIndex3;
                        var weight0 = boneIndex0 < 0 ? 0f : boneWeight.weight0;
                        var weight1 = boneIndex1 < 0 ? 0f : boneWeight.weight1;
                        var weight2 = boneIndex2 < 0 ? 0f : boneWeight.weight2;
                        var weight3 = boneIndex3 < 0 ? 0f : boneWeight.weight3;

                        var vertexColor = m_BindPoseColors[Mathf.Max(0, boneIndex0)] * weight0 +
                                          m_BindPoseColors[Mathf.Max(0, boneIndex1)] * weight1 +
                                          m_BindPoseColors[Mathf.Max(0, boneIndex2)] * weight2 +
                                          m_BindPoseColors[Mathf.Max(0, boneIndex3)] * weight3;

                        colors.Add(vertexColor);
                    }

                    mainTexture = null;
                }

                meshGuiMaterial.mainTexture = mainTexture;
                DrawMesh(m_Points.ToArray(),
                    m_UVs.ToArray(),
                    colors.ToArray(),
                    m_SpriteMeshCache.indices.ToArray(),
                    meshGuiMaterial);
            }
        }


        private void DrawTriangles()
        {
            Handles.color = Color.white * new Color(1f, 1f, 1f, 0.25f);

            for (var i = 0; i < m_SpriteMeshCache.indices.Count; i += 3)
            {
                var index = m_SpriteMeshCache.indices[i];
                var index1 = m_SpriteMeshCache.indices[i + 1];
                var index2 = m_SpriteMeshCache.indices[i + 2];
                var v1 = m_Points[index];
                var v2 = m_Points[index1];
                var v3 = m_Points[index2];

                Handles.DrawLine(v1, v2);
                Handles.DrawLine(v2, v3);
                Handles.DrawLine(v1, v3);
            }
        }

        private void DrawEdge(Edge edge, float width)
        {
            DrawEdge(edge, width, 0f);
        }

        private void DrawEdge(Edge edge, float width, float vertexSize)
        {
            if (edge && edge.node1 && edge.node2)
                DrawEdge(m_SpriteMeshCache.GetVertex(edge.node1),
                    m_SpriteMeshCache.GetVertex(edge.node2), width);
        }

        private void DrawEdge(Vector2 p1, Vector2 p2, float width)
        {
            HandlesExtra.DrawLine(p1, p2, Vector3.forward, width);
        }

        private void DrawPie(Vector3 position, BoneWeight boneWeight, float pieSize, bool selected = false)
        {
            var boneIndex = boneWeight.boneIndex0;
            var angleStart = 0f;
            var angle = 0f;

            if (selected)
                HandlesExtra.DrawDotYellowBig(position);
            else
                HandlesExtra.DrawDotBlackBig(position);

            if (boneIndex >= 0)
            {
                angleStart = 0f;
                angle = Mathf.Lerp(0f, 360f, boneWeight.weight0);
                Handles.color = m_BindPoseColors[boneWeight.boneIndex0];
                Handles.DrawSolidArc(position, Vector3.forward, Vector3.up, angle, pieSize);
            }

            boneIndex = boneWeight.boneIndex1;

            if (boneIndex >= 0)
            {
                angleStart += angle;
                angle = Mathf.Lerp(0f, 360f, boneWeight.weight1);
                Handles.color = m_BindPoseColors[boneWeight.boneIndex1];
                Handles.DrawSolidArc(position, Vector3.forward,
                    Quaternion.AngleAxis(angleStart, Vector3.forward) * Vector3.up, angle, pieSize);
            }

            boneIndex = boneWeight.boneIndex2;

            if (boneIndex >= 0)
            {
                angleStart += angle;
                angle = Mathf.Lerp(0f, 360f, boneWeight.weight2);
                Handles.color = m_BindPoseColors[boneWeight.boneIndex2];
                Handles.DrawSolidArc(position, Vector3.forward,
                    Quaternion.AngleAxis(angleStart, Vector3.forward) * Vector3.up, angle, pieSize);
            }

            boneIndex = boneWeight.boneIndex3;

            if (boneIndex >= 0)
            {
                angleStart += angle;
                angle = Mathf.Lerp(0f, 360f, boneWeight.weight3);
                Handles.color = m_BindPoseColors[boneWeight.boneIndex3];
                Handles.DrawSolidArc(position, Vector3.forward,
                    Quaternion.AngleAxis(angleStart, Vector3.forward) * Vector3.up, angle, pieSize);
            }
        }

        private void DrawSplitEdge(Edge edge, Vector2 vertexPosition)
        {
            if (edge != null)
            {
                Vector3 p1 = m_SpriteMeshCache.GetVertex(edge.node1);
                Vector3 p2 = m_SpriteMeshCache.GetVertex(edge.node2);

                Handles.color = Color.yellow;

                DrawEdge(p1, vertexPosition, 2f / m_Zoom);
                DrawEdge(p2, vertexPosition, 2f / m_Zoom);

                HandlesExtra.DrawDotYellow(p1);
                HandlesExtra.DrawDotYellow(p2);
            }

            HandlesExtra.DrawDotYellow(vertexPosition);
        }

        public void UpdateFromSelection()
        {
            if (!m_SpriteMeshCache) InvalidateCache();

            SpriteMesh l_spriteMesh = null;
            SpriteMeshInstance l_spriteMeshInstance = null;

            if (Selection.activeGameObject)
                l_spriteMeshInstance = Selection.activeGameObject.GetComponent<SpriteMeshInstance>();

            if (l_spriteMeshInstance)
                l_spriteMesh = l_spriteMeshInstance.spriteMesh;
            else if (Selection.activeObject is SpriteMesh) l_spriteMesh = Selection.activeObject as SpriteMesh;

            if (l_spriteMeshInstance || l_spriteMesh) m_SpriteMeshCache.spriteMeshInstance = l_spriteMeshInstance;

            if (l_spriteMesh && l_spriteMesh != m_SpriteMeshCache.spriteMesh)
            {
                HandleApplyRevertDialog();

                InvalidateCache();

                if (l_spriteMesh && l_spriteMesh.sprite)
                {
                    m_SpriteMeshCache.SetSpriteMesh(l_spriteMesh, l_spriteMeshInstance);

                    RefreshTexture();

                    SetScrollPositionAndZoomFromSprite();
                }
            }
        }

        private void SetScrollPositionAndZoomFromSprite()
        {
            if (!m_SpriteMeshCache || !m_Texture) return;

            var newZoom = Mathf.Min(m_TextureViewRect.width / m_SpriteMeshCache.rect.width,
                m_TextureViewRect.height / m_SpriteMeshCache.rect.height) * 0.9f;
            if (m_Zoom > newZoom) m_Zoom = newZoom;

            var width = 1;
            var height = 1;

            SpriteMeshUtils.GetWidthAndHeight(m_TextureImporter, ref width, ref height);

            m_ScrollPosition = Vector2.Scale(m_SpriteMeshCache.rect.center - new Vector2(width, height) * 0.5f,
                new Vector2(1f, -1f)) * m_Zoom;
        }

        private void RefreshTexture(bool force = false)
        {
            if (!m_SpriteMeshCache || !m_SpriteMeshCache.spriteMesh || !m_SpriteMeshCache.spriteMesh.sprite) return;

            var spriteTexture = SpriteUtility.GetSpriteTexture(m_SpriteMeshCache.spriteMesh.sprite, false);

            if (force || spriteTexture != m_OriginalTexture)
            {
                m_OriginalTexture = spriteTexture;
                m_TextureImporter =
                    AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(m_OriginalTexture)) as TextureImporter;

                if (m_Texture) DestroyImmediate(m_Texture);

                if (m_OriginalTexture)
                {
                    var width = 0;
                    var height = 0;

                    SpriteMeshUtils.GetWidthAndHeight(m_TextureImporter, ref width, ref height);

                    m_Texture = CreateTemporaryDuplicate(m_OriginalTexture, width, height);
                    m_Texture.filterMode = FilterMode.Point;
                    m_Texture.hideFlags = HideFlags.DontSave;
                }
            }
        }

        private Texture2D CreateTemporaryDuplicate(Texture2D original, int width, int height)
        {
            if (!ShaderUtil.hardwareSupportsRectRenderTexture || !original) return null;

            var active = RenderTexture.active;
            var flag1 = !GetLinearSampled(original);
            var temporary = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.Default,
                !flag1 ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            GL.sRGBWrite = flag1 && QualitySettings.activeColorSpace == ColorSpace.Linear;
            Graphics.Blit(original, temporary);
            GL.sRGBWrite = false;
            RenderTexture.active = temporary;
            var flag2 = width >= SystemInfo.maxTextureSize || height >= SystemInfo.maxTextureSize;
            var texture2D = new Texture2D(width, height, TextureFormat.ARGB32, original.mipmapCount > 1 || flag2);
            texture2D.ReadPixels(new Rect(0.0f, 0.0f, width, height), 0, 0);
            texture2D.Apply();
            RenderTexture.ReleaseTemporary(temporary);
            SetRenderTextureNoViewport(active);
            texture2D.alphaIsTransparency = original.alphaIsTransparency;
            return texture2D;
        }

        private bool GetLinearSampled(Texture2D texture)
        {
            var result = false;

            var methodInfo = typeof(Editor).Assembly.GetType("UnityEditor.TextureUtil").GetMethod("GetLinearSampled",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (methodInfo != null)
            {
                object[] parameters = {texture};
                result = (bool) methodInfo.Invoke(null, parameters);
            }

            return result;
        }

        private void SetRenderTextureNoViewport(RenderTexture rt)
        {
            var methodInfo = typeof(EditorGUIUtility).GetMethod("SetRenderTextureNoViewport",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (methodInfo != null)
            {
                object[] parameters = {rt};
                methodInfo.Invoke(null, parameters);
            }
        }

        private void HandleApplyRevertDialog()
        {
            if (m_SpriteMeshCache && m_SpriteMeshCache.isDirty && m_SpriteMeshCache.spriteMesh)
            {
                if (EditorUtility.DisplayDialog("Unapplied changes",
                    "Unapplied changes for '" + m_SpriteMeshCache.spriteMesh.name + "'", "Apply", "Revert"))
                    DoApply();
                else
                    DoRevert();
            }
        }
    }
}