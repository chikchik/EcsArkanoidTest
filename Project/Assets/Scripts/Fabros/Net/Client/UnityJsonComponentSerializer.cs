using Fabros.Ecs.ClientServer.Serializer;
using UnityEngine;

namespace Game.Fabros.Net.Client
{
    internal class UnityJsonComponentSerializer : IComponentSerializer
    {
        public string Serialize(object ob)
        {
            return JsonUtility.ToJson(ob);
        }

        public T Deserialize<T>(string s) where T : struct
        {
            return JsonUtility.FromJson<T>(s);
        }
    }
}