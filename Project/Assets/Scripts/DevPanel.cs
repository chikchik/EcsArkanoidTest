using Game.ClientServer;
using Game.Dev;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using XFlow.Net.Client;
using XFlow.Utils;
using Zenject;

namespace Game
{
    public class DevPanel
    {
        private bool simLags;
        public DevPanel(DevPanelController dev, CommonEcsGameSettings settings, DiContainer container)
        {
            var root = dev.root;
            root.init();
        
            root.button("lunar console", () =>
            {
                LunarConsolePlugin.LunarConsole.Show();    
            });

            root.toggleBool("pause network", () =>
            {
                return simLags;
            }, b =>
            {
                //var netClient = container.Resolve<NetClient>();
                //netClient.SimulateLags(b);
                //simLags = b;
            });
            
            root.button("net client delay", () =>
            {
                var netClient = container.Resolve<NetClient>();
                
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
           

                /*
                sub.button("connect MainScene", (btn) =>
                {
                    Config.RoomPartB = input.text;
                    SceneManager.LoadScene("MainScene");
                });
            
                sub.button("connect MiniScene", (btn) =>
                {
                    Config.RoomPartB = input.text;
                
                    SceneManager.LoadScene("MiniScene");
                });
            
                sub.button("connect HugeScene", (btn) =>
                {
                    Config.RoomPartB = input.text;
                
                    SceneManager.LoadScene("HugeScene");
                });*/
            });
        
            root.toggleBool("multiplayer", () => settings.MultiPlayer, b =>
            {
                settings.MultiPlayer = b;
            });
        }
    }
}