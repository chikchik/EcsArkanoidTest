using System;
using System.Linq;
using Game.ClientServer;
using Game.Utils;
using UnityEngine;

namespace Game.View
{
    public class AnimatorViewRootMotion : MonoBehaviour
    {
        private Animator animator;
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public Vector3 Delta { get; set; }
        public void OnAnimatorMove()
        {
            //var clips = animator.runtimeAnimatorController.animationClips;
            //var clip = clips.First(clip => clip.name == "Walking");
            //clip.apparentSpeed
            //if (clip == null)
              //  return;
            //var averageSpeed = clip.averageSpeed;
            
            Delta += animator.deltaPosition;

            //transform.localPosition += animator.deltaPosition.WithX(0) - averageSpeed.WithX(0) * Time.deltaTime;
        }
        
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position.WithY(1f), 0.3f);
        }
    }
}