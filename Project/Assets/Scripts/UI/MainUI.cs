using Game.View;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainUI : MonoBehaviour
    {
        public Button UseButton;
        public Button InteractionButton;

        public RectTransform ObjectivesRectTransform;
        public GridLayoutGroup InventoryGrid;
        public ConnectView Connect;
    }
}