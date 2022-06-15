using System.IO;
using Fabros.Ecs;
using Fabros.Ecs.ClientServer.Serializer;
using Game;
using Game.Client;
using Game.ClientServer;
using Leopotam.EcsLite;
using UnityEditor;
using UnityEngine;

public static class ConvertWorld
{
    [MenuItem("World/Save ECS world")]
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
        var dif = WorldUtils.BuildDiff(pool, WorldUtils.CreateWorld("save", pool), world, true, false);
        
        
        
        
        
        File.WriteAllText("../ServerApp/world.ecs.json", JsonUtility.ToJson(dif, true));
    }
}