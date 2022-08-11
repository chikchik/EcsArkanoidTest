using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game.Utils
{
    public static partial class TransformExt
    {
        public static void DestroyChildren(this Transform target, bool immediate = false)
        {
            for (var i = target.childCount - 1; i >= 0; --i)
                if (immediate)
                    Object.DestroyImmediate(target.GetChild(i).gameObject);
                else
                    Object.Destroy(target.GetChild(i).gameObject);
        }

        public static void DestroyChildrenExcept(this Transform target, Transform except)
        {
            for (var i = target.childCount - 1; i >= 0; --i)
            {
                var t = target.GetChild(i);
                if (t == except) continue;

                Object.Destroy(t.gameObject);
            }
        }
    }
}