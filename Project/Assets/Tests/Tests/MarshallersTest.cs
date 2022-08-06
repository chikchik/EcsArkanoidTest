using System;
using System.Collections.Generic;
using NUnit.Framework;
using XFlow.Ecs.ClientServer.WorldDiff;
using XFlow.Ecs.ClientServer.WorldDiff.Attributes;
using XFlow.Utils;

namespace Tests.Runtime.Tests
{
    public class MarshallersTest
    {
        [ForceJsonSerialize]
        [Serializable]
        public struct MyStruct
        {
            public string str;
        }
        
        [ForceJsonSerialize]
        [Serializable]
        public struct MyStructPlain
        {
            public int value;
        }
    
        [Test]
        public void Test0()
        {
            var writer = new HGlobalWriter();
        
            var ob1 = new MyStructPlain { value = 123 };

            writer.WriteSingleT(ob1);
            
            var strings1 = new List<string> { "1", "2", "3" };
            writer.WriteStrings(strings1, true);
            
            
            var copy = writer.CopyToByteArray();
            var reader = new HGlobalReader(copy);
            
            
            var ob2 = reader.ReadStructure<MyStructPlain>();
            Assert.AreEqual(ob2, ob1);

            var strings2 = reader.ReadStrings();
            Assert.AreEqual(strings2, strings1);
        }


        [Test]
        public void Test1()
        {
            var writer = new HGlobalWriter();
        
            var ob1 = new MyStruct { str = "123" };
            var plain1 = new MyStructPlain { value = 123 };

            
            var collection = new ComponentsCollection();
            collection.AddComponent<MyStructPlain>();
            collection.AddComponent<MyStruct>();

            var component = collection.GetComponent(ob1.GetType());
            component.WriteSingleComponentWithId(writer, ob1);
            
            component = collection.GetComponent(plain1.GetType());
            component.WriteSingleComponentWithId(writer, plain1);
            
            var copy = writer.CopyToByteArray();
            var reader = new HGlobalReader(copy);
            
            
            component = collection.GetComponent(ob1.GetType());

            Assert.AreEqual(reader.ReadInt32(), component.GetId());
            var ob2 = component.ReadSingleComponent(reader);
            Assert.AreEqual(ob2, ob1);
            
            component = collection.GetComponent(plain1.GetType());
            Assert.AreEqual(reader.ReadInt32(), component.GetId());
            var plain2 = component.ReadSingleComponent(reader);
            Assert.AreEqual(plain2, plain1);
        }
    }
}