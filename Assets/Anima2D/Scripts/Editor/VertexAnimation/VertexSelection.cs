using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class VertexSelection : ISerializationCallbackReceiver
    {
        [SerializeField] private int[] m_Keys = new int[0];

        private HashSet<int> m_Selection = new HashSet<int>();

        private bool m_SelectionInProgress;

        private HashSet<int> m_TemporalSelection = new HashSet<int>();

        private HashSet<int> selection
        {
            get
            {
                if (m_SelectionInProgress) return m_TemporalSelection;

                return m_Selection;
            }
        }

        public int Count => m_Selection.Count;

        public void OnBeforeSerialize()
        {
            m_Keys = m_Selection.ToArray();
        }

        public void OnAfterDeserialize()
        {
            m_Selection.Clear();

            m_Selection.UnionWith(m_Keys);
        }

        public int First()
        {
            return m_Selection.First();
        }

        public void Clear()
        {
            selection.Clear();
        }

        public void BeginSelection()
        {
            m_TemporalSelection.Clear();

            m_SelectionInProgress = true;
        }

        public void EndSelection(bool select)
        {
            m_SelectionInProgress = false;

            if (select)
                m_Selection.UnionWith(m_TemporalSelection);
            else
                foreach (var value in m_TemporalSelection)
                    if (m_Selection.Contains(value))
                        m_Selection.Remove(value);

            m_TemporalSelection.Clear();
        }

        public void Select(int index, bool select)
        {
            if (select)
                selection.Add(index);
            else if (IsSelected(index)) selection.Remove(index);
        }

        public bool IsSelected(int index)
        {
            return m_Selection.Contains(index) || m_TemporalSelection.Contains(index);
        }
    }
}