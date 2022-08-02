using System.IO;
using Game;
using Game.ClientServer;

using UnityEditor;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;

public static class ConvertWorld
{
    //[MenuItem("World/Save ECS world")]
    static void SaveWorld()
    {
        var world = new EcsWorld();
        
        //создание entities  и мира на сервере
        //собирает все объекты на которых висит компонент Syncronizable
        //и создает для них entities, в GameObjectNameComponent добавляем имя
        //чтоб клиент уже нашел gameobject по имени и связал их между собой
        
        ClientServices.InitializeNewWorldFromScene(world);
        
        Debug.Log("saving world");
        
        var collection = new ComponentsCollection();
        ComponentsCollectionUtils.AddComponents(collection);
        
        var dif = WorldDiff.BuildDiff(collection, new EcsWorld("save"), world);

        File.WriteAllText("../ServerApp/world.ecs.json", dif.ToJsonString(true));
    }
}