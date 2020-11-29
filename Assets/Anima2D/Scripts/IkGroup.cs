using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    public class IkGroup : MonoBehaviour
    {
        [SerializeField] [HideInInspector] private List<Ik2D> m_IkComponents = new List<Ik2D>();

        private void LateUpdate()
        {
            UpdateGroup();
        }

        public void UpdateGroup()
        {
            for (var i = 0; i < m_IkComponents.Count; i++)
            {
                var ik = m_IkComponents[i];

                if (ik)
                {
                    ik.enabled = false;
                    ik.UpdateIK();
                }
            }
        }
    }
}