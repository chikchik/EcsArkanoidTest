using System;

namespace Game.ClientServer.Box2D
{
    public struct B2Filter
    {
        public UInt16 CategoryBits;
        public UInt16 MaskBits;
        public Int16 GroupIndex;
    };
}