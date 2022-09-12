using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;
using Game.View;
using UnityEngine;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using Zenject;

namespace Game.UI
{
    public class HpViewManager:
        EventsSystem<HpComponent>.IAnyComponentChangedListener, 
        EventsSystem<HpComponent>.IAnyComponentRemovedListener
    {
        private HpView _hpViewPrefab;
        private Canvas _canvas;
        private Camera _camera;
        
        private EcsWorld _world;
        private AnyListener _listener;
        
        private EcsPool<HpViewComponent> _poolView;
        private EcsPool<PositionComponent> _poolPosition;
        

        private EcsFilter _filter;
        //private EcsPool<H>

        public HpViewManager(EcsWorld world, HpView hpViewPrefab, [Inject(Id = "HpViewCanvas")] Canvas canvas, Camera camera)
        {
            _hpViewPrefab = hpViewPrefab;
            _canvas = canvas;
            _camera = camera;
            _world = world;

            _poolView = world.GetPool<HpViewComponent>();
            _poolPosition = world.GetPool<PositionComponent>();
            _filter = world.Filter<HpViewComponent>().End();

            _listener = world.CreateAnyListener();
            _listener.SetAnyChangedListener<HpComponent>(this);
            _listener.SetAnyRemovedListener<HpComponent>(this);
        }


        public void OnAnyComponentChanged(EcsWorld world, int entity, HpComponent data, bool added)
        {
            HpView view;
            if (_poolView.TryGet(entity, out HpViewComponent viewComponent))
            {
                view = viewComponent.View;
            }
            else
            {
                view = GameObject.Instantiate(_hpViewPrefab, _canvas.transform);
                _poolView.Add(entity).View = view;
            }

            var p = data.Value / data.MaxValue;
            view.SetValue(p);
        }

        public void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<HpComponent> alwaysNull)
        {
            if (!_poolView.TryGet(entity, out HpViewComponent viewComponent))
                return;
            var view = viewComponent.View;
            GameObject.Destroy(view.gameObject);
            _poolView.Del(entity);
        }

        public void LateUpdate()
        {
            foreach (var entity in _filter)
            {
                var view = _poolView.Get(entity).View;
                var pos = _poolPosition.Get(entity).value;

                var screenPoint = _camera.WorldToScreenPoint(pos);
                view.transform.position = screenPoint;
            }
        }
    }
}