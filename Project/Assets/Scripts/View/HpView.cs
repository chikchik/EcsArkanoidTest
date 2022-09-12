using UnityEngine;
using UnityEngine.UI;
using XFlow.Utils;

namespace Game.View
{
    public class HpView : MonoBehaviour
    {
        public Image ProgressImage;
        public void SetValue(float p)
        {
            ProgressImage.transform.localScale = ProgressImage.transform.localScale.WithX(p);
        }
    }
}