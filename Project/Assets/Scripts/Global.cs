using Game.View;
using UnityEngine;

namespace Game
{
    public class Global : MonoBehaviour
    {
        public CharacterView characterPrefab;

        [Header("Footprint")] public GameObject leftFootprintPrefab;

        public GameObject rightFootprintPrefab;
        public Transform footprintParent;

        [Space] public HighlightView highlightView;

        public ParticleSystem FireParticles;
    }
}