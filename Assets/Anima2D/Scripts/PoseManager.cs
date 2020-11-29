using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    public class PoseManager : MonoBehaviour
    {
        [SerializeField] [HideInInspector] private List<Pose> m_Poses;
    }
}