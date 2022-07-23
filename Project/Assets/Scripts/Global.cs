using Game.View;
using UnityEngine;

namespace Game
{
    public class Global : MonoBehaviour
    {
        public CharacterView characterPrefab;
        public BulletView BulletPrefab;

        [Header("Footprint")] 
        public FootprintView LeftFootprintPrefab;
        public FootprintView RightFootprintPrefab;

        public Transform footprintParent;

        [Space] public HighlightView highlightView;

        public ParticleSystem FireParticles;
    }
}