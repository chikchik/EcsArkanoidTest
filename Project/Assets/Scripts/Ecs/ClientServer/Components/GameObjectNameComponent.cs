using System;
using System.Runtime.InteropServices;
using Fabros.Ecs.ClientServer.WorldDiff;

namespace Game.Ecs.ClientServer.Components
{
    //[ForceJsonSerialize]
    [Serializable]
    public struct GameObjectNameComponent
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] 
        public string Name;
    }
}