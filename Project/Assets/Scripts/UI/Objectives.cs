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
        private readonly TextMeshProUGUI _textPrefab;
        private readonly RectTransform _verticalLayoutGroup;
        private EcsWorld _world;
        public Objectives(MainUI ui, EcsWorld world)
        {
            this._world = world;

            _verticalLayoutGroup = ui.ObjectivesRectTransform;
        
            var listener = world.CreateAnyListener();
            listener.SetAnyChangedListener<ObjectiveOpenedComponent>(this);
            listener.SetAnyChangedListener<ObjectiveCompletedComponent>(this);
        
        
            _textPrefab = _verticalLayoutGroup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            _textPrefab.gameObject.SetActive(false);
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, ObjectiveCompletedComponent data, bool added)
        {
            entity.EntityWith<UiObjectComponent>(world, data =>
            {
                var go = data.GameObject;
                Object.Destroy(go);
                entity.EntityDel<UiObjectComponent>(world);
            });
        }

        public void OnAnyComponentChanged(EcsWorld world, int entity, ObjectiveOpenedComponent data, bool added)
        {
            var text = Object.Instantiate(_textPrefab, _verticalLayoutGroup.transform);
            text.text = entity.EntityGet<ObjectiveDescriptionComponent>(world).text;
            text.gameObject.SetActive(true);

            entity.EntityAdd<UiObjectComponent>(world).GameObject = text.gameObject;
        }
    }
}