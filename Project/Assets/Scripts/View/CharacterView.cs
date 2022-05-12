using Game.Utils;
using UnityEngine;

namespace Game.View
{
    public class CharacterView : MonoBehaviour
    {
        public Animator Animator;

        private Vector3 last;

        public bool test;
        
        public void LateUpdate()
        {
            //var pos = Animator.transform.localPosition;
            //return;
            if (test)
                return;
            
            
            //return;
            //Debug.Log(Animator.velocity);
            var currentAnimatorClipInfo = Animator.GetCurrentAnimatorClipInfo(0);
            if (currentAnimatorClipInfo.Length == 0)
                return;
            
            var clip = currentAnimatorClipInfo[0].clip;
            var speed = clip.averageSpeed;
            Debug.Log(speed);

            //Debug.Log($"{last - Animator.transform.position}");
            transform.position = Animator.transform.position;//.WithY(transform.position.y);
            Animator.transform.localPosition = Vector3.zero;//.WithX(0).WithZ(0);
        }
    }
}