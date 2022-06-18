using Fabros;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.WorldDiff.Internal;
using Newtonsoft.Json;


class JsonComponentSerializer : IComponentSerializer
{
    public string Serialize(object ob)
    {
        return JsonConvert.SerializeObject(ob);
    }

    public T Deserialize<T>(string s) where T:struct
    {
        var res = JsonConvert.DeserializeObject<T>(s);
        return res;
    }
}