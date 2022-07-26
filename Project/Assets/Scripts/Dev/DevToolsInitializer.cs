using Game.Utils;
using UnityEngine;
using Zenject;

namespace Game.Dev
{
    public class DevToolsInitializer : MonoBehaviour {
        private int step = 0;
        private float lastSecretClickTime;
        private int secretClicksCount;

        [Inject] private DevPanelController DevPanelController;
    
        // Start is called before the first frame update
        void Awake()
        {
            LogTracer.start();
            UnityMainThreadDispatcher.init();
        
            PlayerPrefs.SetInt("su", 1);

            if (PlayerPrefs.GetInt("su", 0) == 1) {
                show();
            }
        }


        void show() {
            DevPanelController.createShow();
        }
    
        public void Update()
        {
            bool clicked = false;

            void secretClick(Vector2 pos)
            {
                if (clicked)
                    return;
                clicked = true;

                if (step == 0) {
                    if (pos.x < Screen.width * 0.75)
                        return;
                    if (pos.y < Screen.height * 0.9)
                        return;
                }
            
                if (step == 1) {
                    if (pos.x > Screen.width * 0.25)
                        return;
                    if (pos.y < Screen.height * 0.9)
                        return;
                }

                var tm = Time.time;
                if (lastSecretClickTime + 0.5 < tm)
                    secretClicksCount = 0;
                else
                    secretClicksCount++;
                lastSecretClickTime = tm;
            
#if UNITY_EDITOR
                if (secretClicksCount == 3)
#else
            if (secretClicksCount == 20)
#endif
                {
                    secretClicksCount = 0;
                    step++;
                    UnityEngine.Debug.Log("Secret Click!");

                    if (step == 2) {
                        PlayerPrefs.SetInt("su", 1);
                        show();
                    }
                }
            }


            if (Input.GetMouseButtonUp(0))
                secretClick(Input.mousePosition);

            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended)
                {
                    secretClick(touch.position);
                    break;
                }
            }
        }
    }
}
