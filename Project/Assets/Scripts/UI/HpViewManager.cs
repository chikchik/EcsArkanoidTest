using DG.Tweening;
using Game.Ecs.Client.Components;
using Game.Ecs.ClientServer.Components;
using Game.UI.Mono;
using Game.View;
using UnityEngine;
using XFlow.Ecs.ClientServer;
using XFlow.Ecs.ClientServer.Components;
using XFlow.EcsLite;
using XFlow.Net.ClientServer.Ecs.Components;
using Zenject;

namespace Game.UI
{
    public class HpViewManager :
        EventsSystem<HpComponent>.IAnyComponentChangedListener,
        EventsSystem<HpComponent>.IAnyComponentRemovedListener,
        EventsSystem<DeletedEntityComponent>.IAnyComponentChangedListener,
        EventsSystem<DeletedEntityComponent>.IAnyComponentRemovedListener
    {
        private HpView _hpViewPrefab;
        private Canvas _canvas;
        private Camera _camera;

        private EcsWorld _world;
        private EcsWorld _deadWorld;
        private AnyListener _listener;

        private EcsPool<HpViewComponent> _poolView;
        //private EcsPool<HpViewComponent> _poolDeadView;

        private EcsPool<PositionComponent> _poolPosition;

        private EcsPool<HpComponent> _poolHp;
        private EcsPool<MaxHpComponent> _poolMaxHp;


        private EcsFilter _filter;
        //private EcsPool<H>

        public HpViewManager(EcsWorld world, [Inject(Id = EcsWorlds.Dead)] EcsWorld deadWorld, HpView hpViewPrefab,
            [Inject(Id = "HpViewCanvas")] Canvas canvas, Camera camera)
        {
            _hpViewPrefab = hpViewPrefab;
            _canvas = canvas;
            _camera = camera;
            _world = world;
            _deadWorld = deadWorld;

            _poolView = world.GetPool<HpViewComponent>();
            //_poolDeadView = deadWorld.GetPool<HpViewComponent>();
            _poolPosition = world.GetPool<PositionComponent>();
            _poolHp = world.GetPool<HpComponent>();
            _poolMaxHp = world.GetPool<MaxHpComponent>();
            _filter = world.Filter<HpViewComponent>().End();

            _listener = world.CreateAnyListener();
            _listener.SetAnyChangedListener<HpComponent>(this);
            _listener.SetAnyChangedListener<DeletedEntityComponent>(this);

            //_deadWorld.EntityDestroyedListeners.Add(this);
        }


        public void OnAnyComponentChanged(EcsWorld world, int entity, HpComponent data, bool added)
        {
            UpdateHp(entity, data);
        }

        public void UpdateHp(int entity, in HpComponent data)
        {
            HpView view;

            if (_poolView.TryGet(entity, out HpViewComponent viewComponent))
            {
                view = viewComponent.View;
            }
            else
            {
                //Debug.Log($"create view {data.Value}");
                view = GameObject.Instantiate(_hpViewPrefab, _canvas.transform);
                _poolView.Add(entity).View = view;
            }

            var max = _poolMaxHp.Get(entity);
            var p = data.Value / max.Value;
            view.SetValue(p);
        }


        public void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<DeletedEntityComponent> alwaysNull)
        {
            if (!_poolHp.TryGet(entity, out HpComponent component))
                return;
            //если DeletedEntityComponent пропал, то значит это был некорректный prediction
            //нужно вернуть Hp
            UpdateHp(entity, component);
        }
        

        public void OnAnyComponentChanged(EcsWorld world, int entity, DeletedEntityComponent data, bool added)
        {
            //Debug.Log("DestroyView DestroyedEntityComponent");
            DestroyView(_poolView, entity);
        }

        private void DestroyView(EcsPool<HpViewComponent> pool, int entity)
        {
            if (!pool.TryGet(entity, out HpViewComponent viewComponent))
                return;
            //Debug.Log("Destroy Hp View");
            var view = viewComponent.View;
            view.transform.DOScaleY(0, 0.3f).OnComplete(() => { GameObject.Destroy(view.gameObject); });
            pool.Del(entity);
        }

        public void OnAnyComponentRemoved(EcsWorld world, int entity, AlwaysNull<HpComponent> alwaysNull)
        {
            DestroyView(_poolView, entity);
        }

        public void LateUpdate()
        {
            foreach (var entity in _filter)
            {
                var view = _poolView.Get(entity).View;
                var pos = _poolPosition.Get(entity).Value;

                var screenPoint = _camera.WorldToScreenPoint(pos);
                view.transform.position = screenPoint;
            }
        }


        public void OnEntityWillBeDestroyed(EcsWorld world, int entity)
        {
            // DestroyView(_poolDeadView, entity);
        }

        public bool IsCopyable()
        {
            throw new System.NotImplementedException();
        }
    }
}