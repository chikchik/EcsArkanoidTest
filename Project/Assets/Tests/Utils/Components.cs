using System;
using Moq;
using UnityEngine;
using XFlow.Ecs.ClientServer.WorldDiff;

namespace Tests.Runtime.utils
{
    [Serializable]
    public struct ComponentA
    {
        public int Value;
    }
    
    [Serializable]
    public struct ComponentD
    {
        [Serializable]
        public struct Inner
        {
            public byte V1;
            public short V3;
            public int V4;
            public Vector3 V2;
        }
        
        public byte V1;
        public short V3;
        public int V4;
        public Vector3 V2;
        

        public Inner V5;
    }
    
    [Serializable]
    public struct ComponentB
    {
        public short Value;
    }
    
    [Serializable]
    public struct ComponentC
    {
        public float Value;
    }
    
    public struct ComponentX //not serializable
    {
        public string data;
    }

    public static class Components
    {
        public static ComponentsCollection Create()
        {
            var ser = new ComponentsCollection();
            ser.AddComponent<ComponentA>();
            ser.AddComponent<ComponentB>();
            ser.AddComponent<ComponentC>();
            ser.AddComponent<ComponentD>();

            return ser;
        }
    }

    public static class MockExt
    {
        public static void VerifyOnce<T>(Mock This, Action<T> expression)  where T : class
        {
            //Mock.Verify(This, expression, Times.Once(), null);
        }
    }
}