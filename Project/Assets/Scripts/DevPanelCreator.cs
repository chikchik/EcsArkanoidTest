
using Game.ClientServer;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DevPanelCreator
{
    public void create(DevPanelController dev)
    {
        var root = dev.root;
        root.button("lunar console", () =>
        {
            LunarConsolePlugin.LunarConsole.Show();    
        });
        
        root.subButton("connect to room", (sub) =>
        {
            sub.ChangeCellSize(sub.gridLayout.cellSize.WithMultipliedX(2));
            //sub.gridLayout.cellSize = sub.gridLayout.cellSize.WithMultipliedX(2);
            var pref = Resources.Load<GameObject>("DEV/DevInputField");
            var tfGo = GameObject.Instantiate(pref, sub.gridLayout.transform);
            var input = tfGo.GetComponentInChildren<TMP_InputField>();
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
           

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
            
        });
    }
}