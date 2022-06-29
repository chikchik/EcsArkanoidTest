using System.IO;
using Fabros.Ecs.ClientServer.WorldDiff;
using Game.Client;
using Game.ClientServer;
using Leopotam.EcsLite;
using UnityEditor;
using UnityEngine;

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
        var pool = SharedComponents.CreateComponentsPool();
        var dif = WorldDiff.BuildDiff(pool, new EcsWorld("save"), world);

        File.WriteAllText("../ServerApp/world.ecs.json", dif.ToJsonString(true));
    }
}