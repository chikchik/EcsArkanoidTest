using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components.Objective;
using Game.UI.Mono;

using TMPro;
using UnityEngine;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Game.UI
{
    public class Objectives : 
        EventsSystem<ObjectiveCompletedComponent>.IAnyComponentChangedListener,
        EventsSystem<ObjectiveOpenedComponent>.IAnyComponentChangedListener
    {
        private readonly TextMeshProUGUI textPrefab;
        private readonly RectTransform verticalLayoutGroup;
        private EcsWorld world;
        public Objectives(MainUI ui, EcsWorld world)
        {
            this.world = world;

            verticalLayoutGroup = ui.ObjectivesRectTransform;
        
            var listener = world.CreateAnyListener();
            listener.SetAnyChangedListener<ObjectiveOpenedComponent>(this);
            listener.SetAnyChangedListener<ObjectiveCompletedComponent>(this);
        
        
            textPrefab = verticalLayoutGroup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            textPrefab.gameObject.SetActive(false);
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, ObjectiveCompletedComponent data, bool added)
        {
            entity.EntityWith<UiObjectComponent>(world, data =>
            {
                var go = data.gameObject;
                Object.Destroy(go);
                entity.EntityDel<UiObjectComponent>(world);
            });
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, ObjectiveOpenedComponent data, bool added)
        {
            var text = Object.Instantiate(textPrefab, verticalLayoutGroup.transform);
            text.text = entity.EntityGet<ObjectiveDescriptionComponent>(world).text;
            text.gameObject.SetActive(true);

            entity.EntityAdd<UiObjectComponent>(world).gameObject = text.gameObject;
        }
    }
}