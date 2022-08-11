using DG.Tweening;
using Game.ClientServer;
using Game.Utils;
using UnityEngine;
using UnityEngine.UI;
using XFlow.Utils;

namespace Game.UI.Mono
{
    public class HpView : MonoBehaviour
    {
        public Image Image;
        private float offset;

        private Transform target;

        private void LateUpdate()
        {
            if (target == null) return;

            /*

            var gs = GameSetup.instance;

            var screen = gs.Camera.WorldToScreenPoint(target.position.WithAddedToY(offset));
            var local = new Vector2();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetRT(),
                screen, null, out local);

            transform.GetRT().localPosition = local;
            */
        }

        public void Init(Transform target, float offset)
        {
            this.offset = offset;
            this.target = target;
        }

        /*
        public void OnUpdate(float p)
        {
            var size = Image.GetRT().sizeDelta.WithX(p * 70);

            var c1 = Color.red;
            var c2 = Color.green;

            var c = Color.Lerp(c1, c2, p);

            Image.DOColor(c, 0.2f);
            Image.GetRT().DOSizeDelta(size, 0.2f);
        }*/
    }
}