using System;
using System.Collections.Generic;
using Game.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;



public class DevSubPanel : MonoBehaviour
{
    public delegate void SubPanelAction(DevSubPanel sub);

    public Button BackClick;
    public GameObject buttonPrefab;
    public GridLayoutGroup gridLayout;
    public GameObject subPanelPrefab;
    public UnityAction actionClose { get; set; }

    private int destroyedCount;
    private static int prevT;
    private Vector2 customCellSize = new Vector2(0,0);

    private void Awake()
    {
        destroyedCount = gridLayout.transform.childCount;
        //gridLayout.transform.DestroyChildren();
        BackClick.onClick.AddListener(close);
        actionClose = () => { Destroy(gameObject); };
        gridLayout.constraintCount = 5;
    }

    private void Start()
    {
        OnRectTransformDimensionsChange();
    }

    public void ChangeCellSize(Vector2 cs)
    {
        customCellSize = cs;
    }

    private void OnRectTransformDimensionsChange()
    {
        var rt = gridLayout.transform as RectTransform;
        var padding = gridLayout.padding;

        gridLayout.cellSize = new Vector2(10, 10);
        Canvas.ForceUpdateCanvases();
        var rtRect = rt.rect;
        var size = (rtRect.width - padding.left - padding.right)/gridLayout.constraintCount;
        var rows = gridLayout.transform.childCount / gridLayout.constraintCount + 1;
        
        var h = size * rows;

        var rtHeight = rtRect.height - padding.bottom - padding.top;
        if (h > rtHeight)
        {
            var sc = h / rtHeight;
            size /= sc;
        }

        if (customCellSize.magnitude > 0.001f)
            gridLayout.cellSize = customCellSize;
        else
            gridLayout.cellSize = new Vector2(size, size);
    }


    public void close()
    {
        actionClose();
    }


    public DevButton button(string name, Action<DevButton> handler, bool closePanel = true)
    {
        var go = Instantiate(buttonPrefab, gridLayout.transform);
        var btn = go.GetComponent<DevButton>();
        go.name = name;
        
        btn.Text.text = name;
        btn.Button.onClick.AddListener(() =>
        {
            Debug.Log("clicked dev button " + name);
            if (closePanel)
                close();
            handler(btn);
        });

        return btn;
    }
    

    public DevButton currentButton { get; set; }
    
    public DevButton button(string name, Action handler, bool closePanel = true)
    {
        var btn =  button(name, (b) =>
        {
            currentButton = b;
            handler();
        }, closePanel);

        prevT = 0;
        foreach (var ch in name)
            prevT+= ch;
        var h = (prevT % 31) / 31f;
        btn.setColor(Color.HSVToRGB(h, 0.5f, 1.0f));

        return btn;
    }

    public void nextLine(bool padding = false)
    {
        var count = gridLayout.transform.childCount - destroyedCount;
        /*
        for (int i = 0; i < count; ++i)
        {
            Debug.Log($"{i} - {gridLayout.transform.GetChild(i).name}");
        }*/
        
        var r = gridLayout.constraintCount - count % gridLayout.constraintCount;
        //if (r == gridLayout.constraintCount && padding)
        //    return;
        for (int i = 0; i < r; ++i)
        {
            var go = new GameObject("next_line");
            go.AddComponent<RectTransform>();
            go.transform.SetParent(gridLayout.transform);
        }
    }

    public DevButton subButton(string name, SubPanelAction handler)
    {
        return button(name, () =>
        {
            var sub = createSub();
            handler(sub);
        }, false);
    }

    public DevSubPanel createSub()
    {
        var r1 = gridLayout.GetComponent<RectTransform>();


        var go = Instantiate(subPanelPrefab, transform);
        var sub = go.GetComponent<DevSubPanel>();
        var rc = sub.gridLayout.GetComponent<RectTransform>();
        //rc.offsetMin = r1.offsetMin + new Vector2(50, 50);
        //rc.offsetMax = r1.offsetMax + new Vector2(-50, -50);
        return sub;
    }
    
    
    
    public void toggleBool(string name, Func<bool> fnGetState, Action<bool> fn2, string customTextOn = null, string customTextOff = null) {
        
        var tg = new DevPanelController.DevButtonLambdaHolder();

        string getTx() {
            bool on = fnGetState();
            if (on) {
                if (customTextOn == null)
                    return $"{name} enabled";
                return customTextOn;
            }
            
            if (customTextOff == null)
                return $"{name} disabled";
            return customTextOff;
        }
         
        tg.btn = button(getTx(), () => {
            fn2(!fnGetState());
            tg.btn.setText(getTx());
        }, false);

        fn2(fnGetState());
    }

    public void toggleMulti(string name, Func<int, string> fnGetText, Func<int> fnGetState, Action<int> fn2Set, bool closeInnerPanel = true)
    {
        var current = fnGetState();
        var txt = $"{name}";
        if (current != -1)
        {
            fn2Set(current);
            txt = $"{name} ({fnGetText(current)})";
        }

        var holder = new DevPanelController.DevButtonLambdaHolder();
        holder.btn = subButton(txt, (root) =>
        {
            var current = fnGetState();
            
            var buttons = new List<DevButton>();

            void updateColors(int sel)
            {
                for (int i = 0; i < buttons.Count; ++i)
                {
                    buttons[i].setColor(Color.yellow);
                    if (sel == i)
                        buttons[i].setColor(Color.green);
                }
                if (sel != -1)
                    holder.btn.setText($"{name} ({fnGetText(sel)})");
            }
            
            int n = 0;
            try
            {
                
                while (true)
                {
                    int nn = n;
                    var str = fnGetText(n) ;
                    if (str == null)
                        break;
                    var holder = new DevPanelController.DevButtonLambdaHolder();
                    holder.btn = root.button(str, () =>
                    {
                        fn2Set(nn);
                        updateColors(nn);
                        
                    }, closeInnerPanel);

                    buttons.Add(holder.btn);
                    ++n;
                }
            }
            catch (Exception e)
            {
            
            }  
            
            updateColors(current);
        });
    }
    
    private static string getText(int state) {
        return state.ToString();
    }
    
    private void toggleInt(string name, Func<int> fnGetState, Action<int> fn2, Func<int, string> getTxt = null) {
        int state = fnGetState();
        var tg = new DevPanelController.DevButtonLambdaHolder();

        if (getTxt == null)
            getTxt = getText;
        
        tg.btn = button($"{name} = {getTxt(state)}", () => {
            state = fnGetState();
            fn2(state + 1);
            state = fnGetState();
            tg.btn.setText($"{name} = {getTxt(state)}");
        }, false);
    }

}