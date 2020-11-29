using System;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class RectManipulator : VertexManipulator
    {
        public RectManipulatorParams rectManipulatorParams;

        public override void DoManipulate()
        {
            var rect = GetRect(rectManipulatorParams.position, rectManipulatorParams.rotation);

            var vertexCount = 0;

            foreach (var vm in manipulables) vertexCount += vm.GetManipulableVertexCount();

            if (Event.current.type == EventType.MouseDown &&
                Event.current.button == 0)
                foreach (var vm in manipulables)
                    Normalize(vm, rect, rectManipulatorParams.position, rectManipulatorParams.rotation);

            if (!EditorGUI.actionKey && vertexCount > 2)
            {
                EditorGUI.BeginChangeCheck();

                RectHandles.Do(ref rect, ref rectManipulatorParams.position, ref rectManipulatorParams.rotation, true,
                    true);

                if (EditorGUI.EndChangeCheck())
                    foreach (var vm in manipulables)
                        Denormalize(vm, rect, rectManipulatorParams.position, rectManipulatorParams.rotation);
            }
        }

        private Rect GetRect(Vector3 position, Quaternion rotation)
        {
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            var rect = new Rect();

            foreach (var vm in manipulables)
                for (var i = 0; i < vm.GetManipulableVertexCount(); i++)
                {
                    var vertex = vm.GetManipulableVertex(i);
                    var v = Quaternion.Inverse(rotation) * (vertex - position);
                    if (v.x < min.x)
                        min.x = v.x;
                    if (v.y < min.y)
                        min.y = v.y;
                    if (v.x > max.x)
                        max.x = v.x;
                    if (v.y > max.y)
                        max.y = v.y;
                }

            var offset = Vector2.one * 0.05f * HandleUtility.GetHandleSize(position);
            rect.min = min - offset;
            rect.max = max + offset;

            return rect;
        }

        private void Normalize(IVertexManipulable vm, Rect rect, Vector3 position, Quaternion rotation)
        {
            var rm = vm as IRectManipulable;

            if (rm == null) return;

            rm.rectManipulatorData.normalizedVertices.Clear();

            for (var i = 0; i < vm.GetManipulableVertexCount(); i++)
            {
                var v = vm.GetManipulableVertex(i);

                v = Quaternion.Inverse(rotation) * (v - position) - (Vector3) rect.min;
                v.x /= rect.width;
                v.y /= rect.height;

                rm.rectManipulatorData.normalizedVertices.Add(v);
            }
        }

        private void Denormalize(IVertexManipulable vm, Rect rect, Vector3 position, Quaternion rotation)
        {
            var rm = vm as IRectManipulable;

            if (rm == null) return;

            for (var i = 0; i < vm.GetManipulableVertexCount(); i++)
            {
                var v = rm.rectManipulatorData.normalizedVertices[i];

                v = rotation * (Vector3.Scale(v, rect.size) + (Vector3) rect.min) + position;

                vm.SetManipulatedVertex(i, v);
            }
        }
    }
}