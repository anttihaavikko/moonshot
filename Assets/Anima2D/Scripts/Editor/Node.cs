using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class Node : ScriptableObject
    {
        public int index = -1;

        public static Node Create(int index)
        {
            var node = CreateInstance<Node>();
            node.hideFlags = HideFlags.DontSave;
            node.index = index;
            return node;
        }
    }
}