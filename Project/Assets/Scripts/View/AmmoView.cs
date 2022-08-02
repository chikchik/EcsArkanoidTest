using UnityEngine;

namespace Game.View
{
    public class AmmoView : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate(0, 180 * Time.deltaTime, 0);
        }
    }
}
