using NUnit.Framework;
using Tests.Runtime.utils;
using UnityEngine.EventSystems;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Tests.Runtime.Tests
{
    public class CopyWorldTest
    {
        [Test]
        public void TestCopyWorld()
        {
            ComponentsCollection collection = Components.Create();
        
            var srcWorld = new EcsWorld();

            
            
        
            var e1 = srcWorld.NewEntity();
            e1.EntityAdd<ComponentA>(srcWorld).Value = 1;
            e1.EntityAdd<ComponentB>(srcWorld).Value = 2;

            var e2 = srcWorld.NewEntity();
            e2.EntityAdd<ComponentA>(srcWorld).Value = 5;
            e2.EntityAdd<ComponentB>(srcWorld).Value = 6;
            e2.EntityAdd<ComponentC>(srcWorld);
        
            var e3 = srcWorld.NewEntity();
            e3.EntityAdd<ComponentA>(srcWorld).Value = 3;
            e3.EntityAdd<ComponentB>(srcWorld).Value = 4;
            
            //var e4 = srcWorld.NewEntity();
            //e4.EntityAdd<ComponentA>(srcWorld).Value = 5;
        
            srcWorld.DelEntity(e2);
        
       
            //create filters
            AssertEcsWorld.WorldsAreEqual(collection, srcWorld, srcWorld);
        

            var destWorld = new EcsWorld("copy");
            
            var ecsSystems = new EcsSystems(destWorld);
            ecsSystems.Add(new EventsSystem<ComponentA>());
            ecsSystems.Add(new EventsSystem<ComponentB>());
            ecsSystems.Add(new EventsSystem<ComponentC>());
            ecsSystems.Init();
            
            destWorld.CopyFrom(srcWorld, null);
            
            Assert.AreEqual(destWorld.FilterAdded<ComponentA>().End().GetEntitiesCount(), 0);
            Assert.AreEqual(destWorld.FilterChanged<ComponentA>().End().GetEntitiesCount(), 0);

            AssertEcsWorld.WorldsAreEqual(collection, srcWorld, destWorld);

            srcWorld.NewEntity().EntityAdd<ComponentC>(srcWorld);
        
            srcWorld.DelEntity(e1);
            srcWorld.DelEntity(e3);
        
            destWorld.CopyFrom(srcWorld, null);
            AssertEcsWorld.WorldsAreEqual(collection, srcWorld, destWorld);
        
            srcWorld.CopyFrom(destWorld, null);
            AssertEcsWorld.WorldsAreEqual(collection,srcWorld, destWorld);

            destWorld.NewEntity();
            int ee = destWorld.NewEntity();
            ee.EntityAdd<ComponentA>(destWorld);
            destWorld.NewEntity().EntityAdd<ComponentB>(destWorld);
            destWorld.DelEntity(ee);
        
            srcWorld.CopyFrom(destWorld, null);
            AssertEcsWorld.WorldsAreEqual(collection,srcWorld, destWorld);
        }

    }
}