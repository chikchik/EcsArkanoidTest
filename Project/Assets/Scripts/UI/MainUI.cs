using Game.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainUI : MonoBehaviour
    {
        public Button ShotButton;
        public Button InteractionButton;

        public RectTransform ObjectivesRectTransform;
        public GridLayoutGroup InventoryGrid;
        public ConnectView Connect;
        public TMP_Text FoodText;
    }
}