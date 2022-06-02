using System;
using Game.ClientServer;
using Game.Utils;
using UnityEngine;

namespace Game.View
{
    public class CharacterView : MonoBehaviour
    {
        public Animator Animator;

        private Vector3 last;
        [SerializeField]
        private AnimatorViewRootMotion rootMotion;

        public bool test;

        private Transform copy;
        public void Awake()
        {
            copy = transform;
            //Animator.applyRootMotion = false;
        }

        public void LateUpdate()
        {
            //var pos = Animator.transform.localPosition;
            //return;
            if (test)
                return;
            
            transform.position += rootMotion.Delta;
            rootMotion.Delta = Vector3.zero;


            
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
            //transform.position = Animator.transform.position;//.WithY(transform.position.y);
            //Animator.transform.localPosition = Vector3.zero;//.WithX(0).WithZ(0);
            //Animator.roo

            transform.position = Animator.rootPosition;
            //Animator.transform.localPosition = Vector3.zero;//.WithX(0).WithZ(0);
            
            Animator.rootPosition = Vector3.zero;
            
        }


        public void OnAnimatorMove()
        {
            //transform.parent.rotation = Animator.rootRotation;
            //if (Input.GetKey(KeyCode.Z))
            //    return;
            //transform.position += Animator.deltaPosition;
            //Animator.deltaPosition = Vector3.back;
            
            //Animator.ApplyBuiltinRootMotion();
            //transform.position
        }

        public void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position.WithY(1f), 0.1f);
            //Gizmos.color = Color.red;
            //Gizmos.DrawWireSphere(Animator.transform.position.WithY(1f) - Animator.deltaPosition, 0.3f);
        }
    }
}