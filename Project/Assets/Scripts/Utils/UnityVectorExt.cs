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
    public static partial class UnityVectorExt
    {
        private static readonly string cyr = "ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮЁ";
        private static readonly string cyrLower = cyr.ToLower();

        public static void SetPrivatePropertyValue<T>(object obj, string propName, T val)
        {
            var t = obj.GetType();
            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
                throw new ArgumentOutOfRangeException("propName",
                    string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));

            t.InvokeMember(propName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null,
                obj,
                new object[] {val});
        }

        public static void RunOfFirst<T>(this IEnumerable<T> source, Action<T> action)
        {
            var first = source.FirstOrDefault();
            if (first == null) return;

            action(first);
        }


        public static void ForEachIndexed<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            var i = 0;
            foreach (var item in source)
            {
                action(item, i);
                ++i;
            }
        }

        public static int MaxIndex<T>(this List<T> sequence, Func<T, int> fn)
        {
            if (sequence.Count == 0) return -1;

            var maxIndex = -1;
            var maxValue = fn(sequence[0]);

            var index = 0;
            for (var i = 1; i < sequence.Count; ++i)
            {
                var v = fn(sequence[i]);
                if (v >= maxValue)
                {
                    maxValue = v;
                    maxIndex = i;
                }
            }

            return maxIndex;
        }

        public static bool IsNull(this Object This)
        {
            return (object) This == null;
        }

        public static bool IsNotNull(this Object This)
        {
            return (object) This != null;
        }

        public static bool IsNull(this GameObject This)
        {
            return (object) This == null;
        }

        public static bool IsNotNull(this GameObject This)
        {
            return (object) This != null;
        }


        public static void SnapTo(this ScrollRect scrollRect, Transform target, bool x = true, bool y = true)
        {
            Canvas.ForceUpdateCanvases();

            var pos = (Vector2) scrollRect.transform.InverseTransformPoint(scrollRect.content.position)
                      - (Vector2) scrollRect.transform.InverseTransformPoint(target.position);

            //pos.x = scrollRect.content.anchoredPosition.x;
            pos.y -= (target as RectTransform).rect.height;

            if (!x) pos.x = scrollRect.content.anchoredPosition.x;

            if (!y) pos.y = scrollRect.content.anchoredPosition.y;

            scrollRect.content.anchoredPosition = pos;
        }


        public static RectTransform GetRT(this Transform This)
        {
            return This as RectTransform;
        }

        public static RectTransform GetRT(this Component This)
        {
            return This.transform as RectTransform;
        }

        public static RectTransform GetRT(this GameObject This)
        {
            return This.transform as RectTransform;
        }

        public static RectTransform ToRectTransform(this MonoBehaviour This)
        {
            return This.transform as RectTransform;
        }

        public static RectTransform ToRectTransform(this GameObject This)
        {
            return This.transform as RectTransform;
        }

        public static Vector2 WorldToCanvasPosition(RectTransform canvas, Camera camera, Vector3 position)
        {
            //Vector position (percentage from 0 to 1) considering camera size.
            //For example (0,0) is lower left, middle is (0.5,0.5)
            Vector2 temp = camera.WorldToViewportPoint(position);

            //Calculate position considering our percentage, using our canvas size
            //So if canvas size is (1100,500), and percentage is (0.5,0.5), current value will be (550,250)
            temp.x *= canvas.sizeDelta.x;
            temp.y *= canvas.sizeDelta.y;

            //The result is ready, but, this result is correct if canvas recttransform pivot is 0,0 - left lower corner.
            //But in reality its middle (0.5,0.5) by default, so we remove the amount considering cavnas rectransform pivot.
            //We could multiply with constant 0.5, but we will actually read the value, so if custom rect transform is passed(with custom pivot) , 
            //returned value will still be correct.

            temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
            temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

            return temp;
        }


        public static string ReplaceKey(this string data, string key, string val)
        {
            var n = 0;
            while (true)
            {
                var ind = data.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
                if (ind == -1) break;

                ++n;
                data = data.Remove(ind, key.Length);
                data = data.Insert(ind, val);

                if (n > 100)
                {
                    n = 0;
                    data = "<inf>";
                    break;
                }
            }

#if UNITY_EDITOR
            if (n == 0) return $"{data}|{key}={val}";
#endif

            return data;
        }

        /*	
    public static string replaceKey2(this string data, string key, string val)
    {
        while (true)
        {
            var ind = data.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
            if (ind == -1)
                break;
            
            data = data.Remove(ind, key.Length);
            data = data.Insert(ind, val);
        } 
        
#if UNITY_EDITOR
        if (data.EndsWith("|"))
            data += $"{key}|";
        else
            data += $"\n<size=25>{key}|";
#endif
        
        return data;
    }*/


        public static string ReplaceKeyOpt(this string data, string key, string val)
        {
            while (true)
            {
                var ind = data.IndexOf(key, StringComparison.InvariantCultureIgnoreCase);
                if (ind == -1) break;

                data = data.Remove(ind, key.Length);
                data = data.Insert(ind, val);
            }

            return data;
        }


        public static string SmartTimeFormatEn(int minutes)
        {
            if (minutes == 24 * 60) return "24 hours";

            var str = "";

            var hours = minutes / 60;
            var days = hours / 24;

            minutes %= 60;
            hours %= 24;

            if (days > 0) str += days + (days == 1 ? " day" : " days");

            if (hours > 0)
            {
                if (str.Length > 0) str += " and ";

                str += hours + (hours == 1 ? " hour" : " hours");
            }

            if (minutes > 0 || str.Length == 0)
            {
                if (str.Length > 0) str += " and ";

                str += minutes + (minutes == 1 ? " minute" : " minutes");
            }

            /*
         1 day
         2 days
         3 days
         1
         * 
         */
            return str;
        }

        public static string SmartTimeFormat(int minutes)
        {
            /*
        var str = "";
        if (GameApp.instance.language == "en")
            str = SmartTimeFormatEn(minutes);
        else
            str = FormatLeftTime(minutes * 60);

        return str;
        */
            return "abcd";
        }

        public static string SmartDurationReplaceMinutes(this string str, int minutes)
        {
            if (str.IndexOf("{duration", StringComparison.InvariantCultureIgnoreCase) == -1) return str;

            return str.ReplaceKeyOpt("{duration_hours}", (minutes / 60).ToString())
                .ReplaceKeyOpt("{duration_days}", (minutes / 60 / 24).ToString())
                .ReplaceKeyOpt("{duration_minutes}", minutes.ToString())
                .ReplaceKeyOpt("{duration}", SmartTimeFormat(minutes));
        }

        public static Transform FindDeepChild(this Transform aParent, string aName, Queue<Transform> queue)
        {
            queue.Clear();

            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name.Equals(aName, StringComparison.InvariantCultureIgnoreCase)) return c;

                for (var i = 0; i < c.childCount; ++i) queue.Enqueue(c.GetChild(i));
            }

            return null;
        }

        public static Transform SetIdentity(this Transform transform)
        {
            transform.localPosition = new Vector3(0, 0, 0);
            transform.localScale = new Vector3(1, 1, 1);
            transform.localRotation = Quaternion.identity;

            return transform;
        }

        public static string CreateTMPSprite(string name)
        {
            return $"<sprite name=\"{name}\">";
        }

        public static string Int2stringXX(int v)
        {
            var s = v.ToString();
            if (s.Length == 1) s = "0" + s;

            return s;
        }

        public static Vector3 CreateRotatedVector3(float angleRad, float rad)
        {
            return new Vector3(Mathf.Sin(angleRad), Mathf.Cos(angleRad), 0) * rad;
        }

        public static void ResizeArray<T>(ref T[,] original, int width, int height)
        {
            var newArray = new T[width, height];
            var minRows = Math.Min(width, original.GetLength(0));
            var minCols = Math.Min(height, original.GetLength(1));
            for (var i = 0; i < minRows; i++)
            for (var j = 0; j < minCols; j++)
                newArray[i, j] = original[i, j];

            original = newArray;
        }

        public static bool ContainsFlag(this string str, string flag)
        {
            return str.IndexOf(flag, StringComparison.InvariantCultureIgnoreCase) != -1;
        }

        public static bool ContainsAnyFlag(this string str, string flags)
        {
            foreach (var flag in flags)
                if (str.IndexOf(flag.ToString(), StringComparison.InvariantCultureIgnoreCase) != -1)
                    return true;

            return false;
        }


        public static string Prepare4TmpSprite(this string str)
        {
            while (true)
            {
                var a = str.IndexOf('[');
                if (a == -1) break;

                var b = str.IndexOf(']', a);
                if (b == -1) break;

                var name = str.Substring(a + 1, b - a - 1);
                str = str.Substring(0, a) + CreateTMPSprite(name.Trim()) + str.Substring(b + 1);
            }

            return str;
        }

        public static void Check4CyrillicSymbols(string key, string str)
        {
            var errors = false;
            for (var i = 0; i < cyr.Length; ++i)
            {
                var a = cyr[i].ToString();
                var b = cyrLower[i].ToString();
                var ia = str.IndexOf(a);
                var ib = str.IndexOf(b);
                var n = ia;
                if (n == -1) n = ib;

                if (n == -1) continue;

                str = str.Insert(n, "->");
                errors = true;
            }

#if !UNITY_EDITOR
		if (errors)
			Debug.LogError($"check4CyrillicSymbols {key} - {str}");
#endif
        }

        public static bool SplitStringSep(string str, string sep, out string a, out string b)
        {
            var pos = str.LastIndexOf(sep);
            a = str;
            b = "";
            if (pos == -1) return false;

            a = str.Substring(0, pos);
            b = str.Substring(pos + 1);

            return true;
        }

        public static int ParseVersion(string ver)
        {
            var items = ver.Split('.');
            //0.9.34
            var x = int.Parse(items[0]) * 1000 * 1000;
            var y = int.Parse(items[1]) * 1000;
            var z = int.Parse(items[2]);
            return x + y + z;
        }

        /*
    public static Point ClampPos(Point pos, Point min, Point max) {
        return new Point(Math.Min(max.x, Math.Max(min.x, pos.x)), Math.Min(max.y, Math.Max(min.y, pos.y)));
    }*/


        public static void SetLayerRecursively(this GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform) child.gameObject.SetLayerRecursively(layer);
        }

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


        public static void PlayerPrefsSetDateTime(string key, DateTime dt)
        {
            PlayerPrefs.SetString(key, dt.Ticks.ToString());
        }

        public static DateTime PlayerPrefsGetDateTime(string key, DateTime defValue)
        {
            if (!PlayerPrefs.HasKey(key)) return defValue;

            var tms = PlayerPrefs.GetString(key);
            long tm = 0;
            if (long.TryParse(tms, out tm)) return new DateTime(tm);

            return defValue;
        }

        public static DateTime PlayerPrefsGetDateTimeFn(string key, Func<DateTime> fn)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                var dt = fn();
                PlayerPrefsSetDateTime(key, dt);
                return dt;
            }

            var tms = PlayerPrefs.GetString(key);
            long tm = 0;
            if (long.TryParse(tms, out tm)) return new DateTime(tm);

            var dtt = fn();
            PlayerPrefsSetDateTime(key, dtt);
            return dtt;
        }


        public static Canvas FindCanvas(this RectTransform transform)
        {
            var current = transform;
            Canvas result = null;
            while (current != null)
                if (current.TryGetComponent(out result))
                    return result;
                else
                    current = current.parent as RectTransform;

            return null;
        }

        public static float GetAlpha(this Image image)
        {
            return image.color.a;
        }

        public static void SetAlpha(this Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        public static void SetAlpha(this SpriteRenderer image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }

        public static long GetUnixTimeMilliseconds()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /*
    public static DateTime toDateTime(this long unixTime)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(unixTime).DateTime;
    }
    
    public static long toUnixTimeMillis(this DateTime dt)
    {
        return DateTimeOffset.FromUnixTimeSeconds()
    }*/

        public static string FormatLeftTime(TimeSpan time)
        {
            if (time.Ticks <= 0) return "00:00";

            //time += TimeSpan.FromSeconds(1);
            var hours = (int) time.TotalHours;
            var minutes = time.Minutes;
            var seconds = time.Seconds;
            var str = $"{minutes:00}:{seconds:00}";

            if (hours > 0) str = $"{hours:00}:{str}";

            return str;
        }

        public static string FormatLeftTime(float seconds)
        {
            return FormatLeftTime(new TimeSpan((long) (seconds * TimeSpan.TicksPerSecond)));
        }

        /*
    public static string formatLeftTimeMinutes(TimeSpan time) {
        
        var hours = time.TotalHours;
        var minutes = time.Minutes;
        var seconds = time.Seconds;
        var str = $"{minutes:00}:{seconds:00}";
        
        if (hours > 0)
            str = $"{(int)hours:00}:{str}";
        
        return str;
    }*/

        public static TextMeshPro CreateDebugText3D(Transform parent)
        {
            var textGo = new GameObject("debugText");
            var tx = textGo.AddComponent<TextMeshPro>();
            tx.transform.SetParent(parent, false);
            tx.sortingLayerID = SortingLayer.NameToID("markers");
            tx.text = "hello";
            tx.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tx.verticalAlignment = VerticalAlignmentOptions.Middle;
            tx.fontSize = 1.5f;
            return tx;
        }
    }
}