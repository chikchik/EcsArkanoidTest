using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;
using Game.View;
using UnityEngine;
using XFlow.EcsLite;
using Zenject;

namespace Game.UI
{
    public class HpViewManager:EventsSystem<HPComponent>.IAnyComponentChangedListener, EventsSystem<HPComponent>.IAnyComponentRemovedListener
    {
        private HpView _hpViewPrefab;
        private Canvas _canvas;
        
        private EcsWorld _world;
        private AnyListener _listener;
        
        private EcsPool<HPViewComponent> _poolView;
        //private EcsPool<H>

        public HpViewManager(EcsWorld world, HpView hpViewPrefab, [Inject(Id = "HpViewCanvas")] Canvas canvas)
        {
            _hpViewPrefab = hpViewPrefab;
            _canvas = canvas;
            _world = world;

            _poolView = world.GetPool<HPViewComponent>();

            _listener = world.CreateAnyListener();
            _listener.SetAnyChangedListener<HPComponent>(this);
            _listener.SetAnyRemovedListener<HPComponent>(this);
        }


        public void OnAnyComponentChanged(EcsWorld world, int entity, HPComponent data, bool added)
        {
            HpView view;
            if (_poolView.TryGet(entity, out HPViewComponent viewComponent))
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

        public void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<HPComponent> alwaysNull)
        {
            if (!_poolView.TryGet(entity, out HPViewComponent viewComponent))
                return;
            var view = viewComponent.View;
            GameObject.Destroy(view.gameObject);
            _poolView.Del(entity);
        }

        public void LateUpdate()
        {
            
        }
    }
}