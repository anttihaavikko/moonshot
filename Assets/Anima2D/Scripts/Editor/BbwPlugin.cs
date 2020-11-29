using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Anima2D
{
    public class BbwPlugin
    {
        [DllImport("Anima2D")]
        private static extern int Bbw(int iterations,
            [In] [Out] IntPtr vertices, int vertexCount, int originalVertexCount,
            [In] [Out] IntPtr indices, int indexCount,
            [In] [Out] IntPtr controlPoints, int controlPointsCount,
            [In] [Out] IntPtr boneEdges, int boneEdgesCount,
            [In] [Out] IntPtr pinIndices, int pinIndexCount,
            [In] [Out] IntPtr weights
        );

        public static UnityEngine.BoneWeight[] CalculateBbw(Vector2[] vertices, IndexedEdge[] edges,
            Vector2[] controlPoints, IndexedEdge[] controlPointEdges, int[] pins)
        {
            var sampledEdges = SampleEdges(controlPoints, controlPointEdges, 10);

            var verticesAndSamplesList = new List<Vector2>(vertices.Length + sampledEdges.Length);

            verticesAndSamplesList.AddRange(vertices);
            verticesAndSamplesList.AddRange(controlPoints);
            verticesAndSamplesList.AddRange(sampledEdges);

            var edgesList = new List<IndexedEdge>(edges);
            var holes = new List<Hole>();
            var indicesList = new List<int>();

            SpriteMeshUtils.Tessellate(verticesAndSamplesList, edgesList, holes, indicesList, 4f);

            var verticesAndSamples = verticesAndSamplesList.ToArray();
            var indices = indicesList.ToArray();

            var weights = new UnityEngine.BoneWeight[vertices.Length];

            var verticesHandle = GCHandle.Alloc(verticesAndSamples, GCHandleType.Pinned);
            var indicesHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
            var controlPointsHandle = GCHandle.Alloc(controlPoints, GCHandleType.Pinned);
            var boneEdgesHandle = GCHandle.Alloc(controlPointEdges, GCHandleType.Pinned);
            var pinsHandle = GCHandle.Alloc(pins, GCHandleType.Pinned);
            var weightsHandle = GCHandle.Alloc(weights, GCHandleType.Pinned);

            Bbw(-1,
                verticesHandle.AddrOfPinnedObject(), verticesAndSamples.Length, vertices.Length,
                indicesHandle.AddrOfPinnedObject(), indices.Length,
                controlPointsHandle.AddrOfPinnedObject(), controlPoints.Length,
                boneEdgesHandle.AddrOfPinnedObject(), controlPointEdges.Length,
                pinsHandle.AddrOfPinnedObject(), pins.Length,
                weightsHandle.AddrOfPinnedObject());

            verticesHandle.Free();
            indicesHandle.Free();
            controlPointsHandle.Free();
            boneEdgesHandle.Free();
            pinsHandle.Free();
            weightsHandle.Free();

            return weights;
        }

        private static Vector2[] SampleEdges(Vector2[] controlPoints, IndexedEdge[] controlPointEdges,
            int samplesPerEdge)
        {
            var sampledVertices = new List<Vector2>();

            for (var i = 0; i < controlPointEdges.Length; i++)
            {
                var edge = controlPointEdges[i];

                var tip = controlPoints[edge.index1];
                var tail = controlPoints[edge.index2];

                for (var s = 0; s < samplesPerEdge; s++)
                {
                    var f = (s + 1f) / (samplesPerEdge + 1f);
                    sampledVertices.Add(f * tail + (1f - f) * tip);
                }
            }

            return sampledVertices.ToArray();
        }
    }
}