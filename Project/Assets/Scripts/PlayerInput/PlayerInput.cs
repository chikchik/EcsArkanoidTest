using System;
using UnityEngine;
using Zenject;

namespace Game.PlayerInput
{
    public abstract class PlayerInput : IInitializable, IDisposable, ITickable
    {
        public abstract bool HasTouch { get; }
        public abstract Vector3 TouchPosition { get; }
        protected abstract float Horizontal { get; }
        protected abstract float Vertical { get; }
        public Vector3 Movement => new(Horizontal, 0, Vertical);

        public virtual void Dispose()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual void Tick()
        {
        }
    }
}