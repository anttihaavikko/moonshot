using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public struct BoneWeight
    {
        public float weight0;
        public float weight1;
        public float weight2;
        public float weight3;
        public int boneIndex0;
        public int boneIndex1;
        public int boneIndex2;
        public int boneIndex3;

        public static BoneWeight Create()
        {
            var boneWeight = new BoneWeight();
            boneWeight.boneIndex0 = -1;
            boneWeight.boneIndex1 = -1;
            boneWeight.boneIndex2 = -1;
            boneWeight.boneIndex3 = -1;
            return boneWeight;
        }

        public float GetBoneWeight(int boneIndex)
        {
            if (boneIndex0 == boneIndex) return weight0;
            if (boneIndex1 == boneIndex)
                return weight1;
            if (boneIndex2 == boneIndex)
                return weight2;
            if (boneIndex3 == boneIndex) return weight3;

            return 0f;
        }

        public void SetBoneIndexWeight(int boneIndex, float weight, bool createInfluence = true,
            bool unassignIfWeightZero = false)
        {
            var weightIndex = GetWeightIndex(boneIndex);

            weight = Mathf.Clamp01(weight);

            if (createInfluence && weightIndex < 0) weightIndex = GetMinWeightIndex();

            SetWeight(weightIndex, boneIndex, weight);

            if (unassignIfWeightZero)
            {
                if (weight0 <= 0f) Unassign(boneIndex0);
                if (weight1 <= 0f) Unassign(boneIndex1);
                if (weight2 <= 0f) Unassign(boneIndex2);
                if (weight3 <= 0f) Unassign(boneIndex3);
            }
        }

        public int GetMinWeightIndex()
        {
            var boneIndexes = new List<int> {boneIndex0, boneIndex1, boneIndex2, boneIndex3};

            var weightIndex = boneIndexes.IndexOf(boneIndexes.Min());

            return weightIndex;
        }

        public int GetWeightIndex(int boneIndex)
        {
            if (boneIndex >= 0)
            {
                var boneIndexes = new List<int> {boneIndex0, boneIndex1, boneIndex2, boneIndex3};

                return boneIndexes.IndexOf(boneIndex);
            }

            return -1;
        }

        public void GetWeight(int weightIndex, out int boneIndex, out float weight)
        {
            var boneIndexes = new List<int> {boneIndex0, boneIndex1, boneIndex2, boneIndex3};
            var boneWeights = new List<float> {weight0, weight1, weight2, weight3};

            boneIndex = boneIndexes[weightIndex];
            weight = boneWeights[weightIndex];
        }

        public void SetWeight(int weightIndex, int boneIndex, float weight)
        {
            weight = Mathf.Clamp01(weight);

            if (weightIndex == 0)
            {
                boneIndex0 = boneIndex;
                weight0 = weight;
            }
            else if (weightIndex == 1)
            {
                boneIndex1 = boneIndex;
                weight1 = weight;
            }
            else if (weightIndex == 2)
            {
                boneIndex2 = boneIndex;
                weight2 = weight;
            }
            else if (weightIndex == 3)
            {
                boneIndex3 = boneIndex;
                weight3 = weight;
            }

            Normalize(weightIndex);
        }

        public void Unassign(int boneIndex)
        {
            SetWeight(GetWeightIndex(boneIndex), -1, 0f);
        }

        public void DeleteBoneIndex(int boneIndex)
        {
            if (boneIndex0 >= boneIndex) boneIndex0--;
            if (boneIndex1 >= boneIndex) boneIndex1--;
            if (boneIndex2 >= boneIndex) boneIndex2--;
            if (boneIndex3 >= boneIndex) boneIndex3--;
        }

        private void Normalize(int masterIndex)
        {
            if (masterIndex >= 0 && masterIndex < 4)
            {
                var sum = 0f;

                float[] weights = {weight0, weight1, weight2, weight3};
                int[] indices = {boneIndex0, boneIndex1, boneIndex2, boneIndex3};

                var numValidIndices = 0;

                for (var i = 0; i < 4; ++i)
                    if (indices[i] >= 0)
                        if (i != masterIndex)
                        {
                            numValidIndices++;
                            sum += weights[i];
                        }

                var targetSum = 1f - weights[masterIndex];

                if (numValidIndices > 0)
                {
                    for (var i = 0; i < 4; ++i)
                        if (i != masterIndex && indices[i] >= 0)
                        {
                            if (sum > 0f)
                                weights[i] = weights[i] * targetSum / sum;
                            else if (numValidIndices > 0) weights[i] = targetSum / numValidIndices;
                        }

                    weight0 = weights[0];
                    weight1 = weights[1];
                    weight2 = weights[2];
                    weight3 = weights[3];
                }
            }
        }
    }
}