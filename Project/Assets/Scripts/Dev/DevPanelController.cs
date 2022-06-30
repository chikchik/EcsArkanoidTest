using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Dev;
using TMPro;


using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.Networking;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;


public class DevPanelController : MonoBehaviour
{
    public class DevButtonLambdaHolder 
    {
        public DevButton btn;
    }

    
    public static DevPanelController Instance;
    
    public CanvasGroup devButtonCanvasGroup;
    public GameObject logContent;
    
    
    private const int LOG_NUM_LINES = 300;
    private static string status;
    private DevButton buttonSwitchGame;
    private float deltaTime;
    private readonly List<GameObject> logItems = new List<GameObject>();
    
    public GameObject logPanel;
    public GameObject logTextPrefab;
    public DevSubPanel root;
    public ScrollRect scrollRect;
    public Button showButton;
    public Text txtFPS;
    public TextMeshProUGUI textStatus;
    
    public static void createShow()
    {
        if (Instance == null) {
            Instantiate(Resources.Load("DEV/DevPanel"));
        }
    }

    public static void setStatus(string s)
    {
        status = s;
    }
    
    private void init() {
        
        root.button("Show Log", () =>
        {
            logPanel.SetActive(true);
            showButton.gameObject.SetActive(false);


            //logging = true;

            for (var i = 0; i < logItems.Count; ++i)
                Destroy(logItems[i]);

            logItems.Clear();

            var from = LogTracer.logData.Count - LOG_NUM_LINES;
            var to = from + LOG_NUM_LINES;
            if (from < 0)
                from = 0;
            for (var i = from; i < to; ++i)
                addLine(LogTracer.logData[i]);

            scrollRect.verticalNormalizedPosition = 0.0f;
        });
        
        root.nextLine(true);
    }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void show(bool sh)
    {
        root.gameObject.SetActive(sh);
        showButton.gameObject.SetActive(!sh);
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        Debug.LogFormat("DevPanelController start");
        
        root.gameObject.SetActive(false);
        logPanel.SetActive(false);

        var str = Application.version;
        showButton.transform.Find("TextVersion").GetComponentInChildren<Text>().text = str;

        logPanel.GetComponentInChildren<Button>().onClick.AddListener(() =>
        {
            //logging = false;
            logPanel.SetActive(false);
            showButton.gameObject.SetActive(true);
        });

        showButton.onClick.AddListener(() => { show(true); });


        root.actionClose = () => { show(false); };

        //var rc = root.gridLayout.GetComponent<RectTransform>();
        //rc.offsetMin = new Vector2(50, 150);
        //rc.offsetMax = new Vector2(-50, -150);

        init();

        LogTracer.onLineAdded += addLine;
    }


    private void addLine(string str)
    {
        if (logItems.Count > LOG_NUM_LINES)
        {
            var last = logItems[0];
            Destroy(last);
            logItems.RemoveAt(0);
        }

        var go = Instantiate(logTextPrefab, logContent.transform, false);
        var txt = go.GetComponent<Text>();
        //DontDestroyOnLoad(go);

        if (str.Length > 1000)
            str = str.Substring(0, 1000) + "\n.......... too long";

        txt.text = str;
        txt.transform.SetParent(logContent.transform);

        logItems.Add(go);
    }



    // Update is called once per frame
    private void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        var fps = 1.0f / deltaTime;
        
        var st = "";
        if (st.Length > 0)
        {
            st = st.Substring(0, st.Length - 3);
        }
        
        txtFPS.text = Mathf.Ceil(fps) + "\n" + st;
        textStatus.text = status;
    }
}


