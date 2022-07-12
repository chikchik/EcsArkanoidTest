using Game.ClientServer;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class DevPanel
    {
        public DevPanel(DevPanelController dev, GameSettings settings)
        {
            var root = dev.root;
            root.init();
        
            root.button("lunar console", () =>
            {
                LunarConsolePlugin.LunarConsole.Show();    
            });
        
            root.subButton("connect to room", (sub) =>
            {
                sub.ChangeCellSize(sub.gridLayout.cellSize.WithMultipliedX(2));
                sub.gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
                //sub.gridLayout.cellSize = sub.gridLayout.cellSize.WithMultipliedX(2);
                var pref = Resources.Load<GameObject>("DEV/DevInputField");
                var tfGo = GameObject.Instantiate(pref, sub.gridLayout.transform);
                var input = tfGo.GetComponentInChildren<TMP_InputField>();
                input.contentType = TMP_InputField.ContentType.IntegerNumber;
                input.text = "1963";
           

                sub.button("connect MainScene", (btn) =>
                {
                    Config.ROOM_B = input.text;
                    SceneManager.LoadScene("MainScene");
                });
            
                sub.button("connect MiniScene", (btn) =>
                {
                    Config.ROOM_B = input.text;
                
                    SceneManager.LoadScene("MiniScene");
                });
            
                sub.button("connect HugeScene", (btn) =>
                {
                    Config.ROOM_B = input.text;
                
                    SceneManager.LoadScene("HugeScene");
                });
            });
        
            root.toggleBool("multiplayer", () => settings.MultiPlayer, b =>
            {
                settings.MultiPlayer = b;
            });
        }
    }
}