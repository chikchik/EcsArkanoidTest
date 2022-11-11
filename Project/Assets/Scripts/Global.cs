using Game.UIView;
using Game.View;
using UnityEngine;
using XFlow.Modules.Inventory.Client.Demo.StaticData;

namespace Game
{
    public class Global : MonoBehaviour
    {
        public PlatformView PlatformPrefab;
        public BallView BallPrefab;
        public BrickView BrickPrefab;
        public BonusView BonusPrefab;
        public CharacterView CharacterPrefab;
        public BulletView BulletPrefab;
        public HpView HpViewPrefab;
        public Canvas HpViewCanvas;

        [Header("Footprint")]
        public FootprintView LeftFootprintPrefab;
        public FootprintView RightFootprintPrefab;

        public ParticleSystem FireParticles;

        public MechInfoView MechInfoView;
        public LoginUIView LoginUIView;

        public InventoryStaticData InventoryStaticData;
    }
}