using System.Collections.Generic;
using NUnit.Framework;
using Tests.Runtime.utils;
using UnityEngine;
using XFlow.Ecs.Client.Components;
using XFlow.Ecs.ClientServer.Components;
using XFlow.Ecs.ClientServer.Utils;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.EcsLite;
using XFlow.Utils;

namespace Tests.Runtime.Tests
{
    public class DiffTest
    {
        ComponentsCollection _collection = Components.Create();

        private WorldDiff BuildEmptyWorldDiff(EcsWorld world, bool rev = false)
        {
            var worldEmpty = new EcsWorld("empty");
            if (rev)
                return BuildDiff(world, worldEmpty);
            return BuildDiff(worldEmpty, world);
        }
    
        private WorldDiff BuildDiff(EcsWorld worldA, EcsWorld worldB)
        {
            var dif = WorldDiff.BuildDiff(_collection, worldA, worldB);
            Check(dif);
            return dif;
        }

        private WorldDiff Check(WorldDiff diff)
        {
            var str = diff.ToJsonString();
            var array = diff.ToByteArray(false);

            var diff1 = WorldDiff.FromJsonString(_collection, str);
            var str1 = diff1.ToJsonString();
            var array1 = diff1.ToByteArray(false);
        
            Assert.AreEqual(str, str1);
            Assert.AreEqual(array, array1);
        
            var diff2 = WorldDiff.FromByteArray(_collection, array1);
            var str2 = diff2.ToJsonString();
            var array2 = diff2.ToByteArray(false);
        
            Assert.AreEqual(str, str2);
            Assert.AreEqual(array, array2);

            return diff;
        }
    
    
        [Test]
        public void TestDiffValues()
        {
            var world = new EcsWorld();
        
            var e = world.NewEntity();
            e.EntityAdd<ComponentA>(world).Value = 2;
        
            var dif = BuildEmptyWorldDiff(world);
        
            CollectionAssert.AreEqual(dif.CreatedEntities, new List<int>{1});
            CollectionAssert.AreEqual(dif.CreatedEntitiesGen, new List<int>{1});
            CollectionAssert.IsEmpty(dif.RemovedEntities);
        
        
            Assert.AreEqual(dif.Groups.Count, 1);
            var group = dif.Groups[0];
        
            CollectionAssert.AreEqual(group.ChangedEntities, new List<int>{1});
            CollectionAssert.IsEmpty(group.RemovedFromEntities);
            //Assert.AreEqual(group.FullComponentName, typeof(ComponentA).FullName);

        
            //reversed diff world->empty
            dif = BuildEmptyWorldDiff(world, true);
            CollectionAssert.IsEmpty(dif.CreatedEntities);
            CollectionAssert.IsEmpty(dif.CreatedEntitiesGen);
            CollectionAssert.IsEmpty(dif.Groups);
        
            CollectionAssert.AreEqual(dif.RemovedEntities, new List<int>{1});
        
            world.DelEntity(e);
            e = world.NewEntity();
            dif = BuildEmptyWorldDiff(world);
            CollectionAssert.AreEqual(dif.CreatedEntitiesGen, new List<int>{2});
        }

        [Test]
        public void TestDiffGen()
        {
            var world  = new EcsWorld();
        
            var e = world.NewEntity();

            var world0 = WorldUtils.CopyWorld(_collection, world);
        
            world.DelEntity(e);
            e = world.NewEntity();

            var dif = BuildDiff(world0, world);
        
            CollectionAssert.AreEqual(dif.RemovedEntities, new List<int>{1});
            CollectionAssert.AreEqual(dif.CreatedEntities, new List<int>{1});
            //gen changed to 2
            CollectionAssert.AreEqual(dif.CreatedEntitiesGen, new List<int>{2});
        }


        [Test]
        public void TestEqualComponents()
        {
            var w1 = new EcsWorld();
            var w2 = new EcsWorld();

            var e1 = w1.NewEntity();
            e1.EntityAdd<ComponentA>(w1).Value = 1;
        
            var e11 = w1.NewEntity();
            e11.EntityAdd<ComponentA>(w1).Value = 1;
        
            var e2 = w2.NewEntity();
            e2.EntityAdd<ComponentB>(w2).Value = 2;
        
            var delta = BuildDiff(w2, w1);
            delta.ApplyChanges(w2);

            Assert.AreEqual(
                e1.EntityGet<ComponentA>(w2).Value,
                e1.EntityGet<ComponentA>(w1).Value);
        
            Assert.AreEqual(
                e2.EntityGet<ComponentA>(w2).Value,
                e2.EntityGet<ComponentA>(w1).Value);
        }
    
    
        [Test]
        public void Test1()
        {
            var worldEmpty = new EcsWorld();
            var world1 = new EcsWorld();
        
            var entity0 = world1.NewEntity();
            entity0.EntityAdd<ComponentA>(world1).Value = 1;
        
            var entity01 = world1.NewEntity();
            entity01.EntityAdd<ComponentB>(world1).Value = 3;

            var world1Copy = WorldUtils.CopyWorld(_collection, world1);


            entity0.EntityAdd<ComponentB>(world1).Value = 4;
            entity0.EntityAdd<ComponentC>(world1);
        
            var entity1 = world1.NewEntity();
            entity1.EntityAdd<ComponentB>(world1).Value = 5;
        
            var dif = BuildDiff(world1Copy, world1);
            dif.ApplyChanges(world1Copy);

            var e1 = world1Copy.NewEntity();
            var e2 = world1.NewEntity();
        
            Assert.AreEqual(e1,e2);
        
            AssertEcsWorld.WorldsAreEqual(_collection, world1Copy, world1);
        }

        [Test]
        public void TestNewEntityEq()
        {
            var w1 = new EcsWorld("w1");
            var w2 = new EcsWorld("w2");

            var e1 = w1.NewEntity();
            var e2 = w1.NewEntity();
            var e3 = w1.NewEntity();
            var e4 = w1.NewEntity();
        
            w1.DelEntity(e2);
        
            var dif = BuildDiff(w1, w2);
        
            dif.ApplyChanges(w1);

            var q1 = w1.NewEntity();
            var q2 = w2.NewEntity();
        
            Assert.AreEqual(q1, q2);
        }


        [Test]
        public void TestR()
        {
            var world = new EcsWorld();
            var defCount = world.GetAllEntitiesCount();
        
            var e1 = world.NewEntity();

            e1.EntityAdd<ComponentA>(world).Value = 3;
        
            var e2 = world.NewEntity();
            e2.EntityAdd<ComponentB>(world).Value = 1;

            var dif = BuildEmptyWorldDiff(world);
            var difRev = BuildEmptyWorldDiff(world, true);
            var world2 = new EcsWorld();
        
            dif.ApplyChanges(world2);
            difRev.ApplyChanges(world2);
            Assert.AreEqual(world2.GetAllEntitiesCount(), defCount);
        }
    
        [Test]
        public void TestComplex()
        {
            var world = new EcsWorld();

            var e1 = world.NewEntity();
            var e2 = world.NewEntity();
            var e2_ = world.NewEntity();
            var e3 = world.NewEntity();
            var e4 = world.NewEntity();
            var e5 = world.NewEntity();
        
            e1.EntityAdd<ComponentA>(world).Value = 1;
        
            e2.EntityAdd<ComponentA>(world).Value = 2;
            e2.EntityAdd<ComponentB>(world).Value = 7;

            e3.EntityAdd<ComponentA>(world).Value = 3;
            e3.EntityAdd<ComponentB>(world).Value = 9;
            e3.EntityAdd<ComponentC>(world).Value = 12;
        
        
            var world2 = new EcsWorld();

            var q1 = world2.NewEntity();
            var q2 = world2.NewEntity();
            var q2_ = world2.NewEntity();
            var q3 = world2.NewEntity();
            var q4 = world2.NewEntity();
        
            q1.EntityAdd<ComponentA>(world2).Value = 2;
            q1.EntityAdd<ComponentB>(world2).Value = 11;
            q2.EntityAdd<ComponentB>(world2).Value = 7;
            q3.EntityAdd<ComponentA>(world2).Value = 3;
        
            world2.DelEntity(q2_);

            AssertEcsWorld.WorldsAreNotEqual(_collection, world, world2);

            var w1tow2 = BuildDiff(world, world2);
            var w2tow1 = BuildDiff(world2, world);
            w1tow2.ApplyChanges(world);
            w2tow1.ApplyChanges(world);
        
            w2tow1.ApplyChanges(world2);
            AssertEcsWorld.WorldsAreEqual(_collection, world, world2);
        }

        [Test]
        public void TestBinary()
        {
            var world = new EcsWorld();
        
            var e1 = world.NewEntity();
            e1.EntityAdd<ComponentA>(world).Value = 123456;
        
            var e2 = world.NewEntity();
            ref var component = ref e2.EntityAdd<ComponentD>(world);
            component.V1 = 12;
            component.V2 = new Vector3(1, 2, 3);
            component.V3 = 123;
            component.V4 = 1234;
            component.V5 = new ComponentD.Inner
            {
                V1 = 23,
                V2 = new Vector3(2,3,4),
                V3 = 234,
                V4 = 2345
            };

            var writer = new HGlobalWriter();
        
            var diff1 = BuildEmptyWorldDiff(world);
            diff1.WriteBinary(false, writer);
        
            var array = writer.CopyToByteArray();
        
            var diff2 = WorldDiff.FromByteArray(_collection, array);

        
            AssertEcsWorld.DiffsAreEqual(diff1, diff2);
        }

        [Test]
        public void TestInnerWorlds()
        {
            var srcWorld = new EcsWorld("world");
            

            var se1 = srcWorld.NewEntity();
            var srcInner1 = new EcsWorld("sinner1");
            se1.EntityAdd<WorldComponent>(srcWorld).World = srcInner1;
            
            srcInner1.NewEntity().EntityAdd<ComponentA>(srcInner1).Value = 123;
            
            var se2 = srcWorld.NewEntity();
            
            //will be copied
            var se3 = srcWorld.NewEntity();
            var srcInner3 = new EcsWorld("sinner3");
            se3.EntityAdd<WorldComponent>(srcWorld).World = srcInner3;


            var destWorld = new EcsWorld("dest");
            
            var de1 = destWorld.NewEntity();
            
            //will be deleted
            var de2 = destWorld.NewEntity();
            de2.EntityAdd<WorldComponent>(destWorld).World = new EcsWorld("dinner2");
            
            //will be reused
            var de3 = destWorld.NewEntity();
            de3.EntityAdd<WorldComponent>(destWorld).World = new EcsWorld("dinner3");


            WorldUtils.CopyWithInnerWorld(srcWorld, destWorld, _collection);
            
            AssertEcsWorld.WorldsAreEqual(_collection, srcWorld, destWorld);
            
            
            Assert.AreEqual(destWorld.EntityGet<WorldComponent>(de1).World.GetDebugName(), "sinner1");
            Assert.AreEqual(destWorld.EntityGet<WorldComponent>(de3).World.GetDebugName(), "dinner3");
            
            Assert.AreNotEqual(
                srcWorld.EntityGet<WorldComponent>(de3).World,
                destWorld.EntityGet<WorldComponent>(de3).World);
            Assert.AreNotEqual(
                srcWorld.EntityGet<WorldComponent>(de1).World,
                destWorld.EntityGet<WorldComponent>(de1).World);
            
        }
        /*
        [Test]
        public void TestLocalEntityMove()
        {
            var world1 = new EcsWorld();
            var localEntity = world1.NewLocalEntity();
            localEntity.Entity.EntityAdd<ComponentX>(world1).data = "hello";

            var df = new WorldDiffJsonSerializable
            {
                CreatedEntities = new[] { 1 },
                CreatedEntitiesGen = new[] { (short)1 }
            };

            var diff = WorldDiff.FromJsonSerializable(_collection, df);
            diff.ApplyChanges(world1);
            
            Assert.AreEqual(localEntity.Entity, 2);
            Assert.AreEqual(localEntity.Entity.EntityGet<ComponentX>(world1).data, "hello");
        }
        
        [Test]
        public void TestLocalEntityDel()
        {
            var world1 = new EcsWorld();
            var localEntity = world1.NewLocalEntity();
            localEntity.Entity.EntityAdd<ComponentX>(world1).data = "hello";

            var df = new WorldDiffJsonSerializable
            {
                RemovedEntities = new int[]{localEntity.Entity} 
            };

            var diff = WorldDiff.FromJsonSerializable(_collection, df);
            diff.ApplyChanges(world1);
            
            Assert.AreEqual(localEntity.Entity, 1);
            Assert.AreEqual(localEntity.Entity.EntityGet<ComponentX>(world1).data, "hello");
        }*/
        
    }
}