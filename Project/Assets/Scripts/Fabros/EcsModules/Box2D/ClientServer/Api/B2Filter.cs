using System;

namespace Fabros.EcsModules.Box2D.ClientServer.Api
{
    public struct B2Filter
    {
        public UInt16 CategoryBits;
        public UInt16 MaskBits;
        public Int16 GroupIndex;
    };
}