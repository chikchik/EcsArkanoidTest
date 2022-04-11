using Fabros.Ecs.Utils;
using Game.ClientServer;
using Game.Ecs.ClientServer.Components.Objective;
using Game.UI;
using Leopotam.EcsLite;
using TMPro;
using UnityEngine;

public class Objectives : EventsSystem<ObjectiveCompletedComponent>.IAnyListener, EventsSystem<ObjectiveOpenedComponent>.IAnyListener 
    //IAnyCompletedListener, IAnyOpenedListener
{
    private RectTransform verticalLayoutGroup;
    private TextMeshProUGUI textPrefab;

    private int objectivesListener;
    private EcsWorld world;
    
    public Objectives(MainUI ui, EcsWorld world)
    {
        this.world = world;
        
        verticalLayoutGroup = ui.ObjectivesRectTransform;
        objectivesListener = 0;// world.NewEntity();
        
        objectivesListener.AddAnyListener<ObjectiveCompletedComponent>(world, this);
        objectivesListener.AddAnyListener<ObjectiveOpenedComponent>(world, this);

        textPrefab = verticalLayoutGroup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textPrefab.gameObject.SetActive(false);
    }


    public void AnyChanged(EcsWorld world, int entity, ObjectiveCompletedComponent data)
    {
        entity.EntityWith<UiObjectComponent>(world, data =>
        {
            GameObject go = data.gameObject;
            Object.Destroy(go);
            entity.EntityDel<UiObjectComponent>(world);
        });
    }

    public void AnyChanged(EcsWorld world, int entity, ObjectiveOpenedComponent data)
    {
        var text = Object.Instantiate(textPrefab, verticalLayoutGroup.transform);
        text.text = entity.EntityGet<ObjectiveDescriptionComponent>(world).text;
        text.gameObject.SetActive(true);

        entity.EntityAdd<UiObjectComponent>(world).gameObject = text.gameObject;
    }
}