using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using XFlow.EcsLite;
using XFlow.Modules.Inventory.ClientServer;
using XFlow.Modules.Inventory.ClientServer.Components;
using XFlow.Utils;

/*
 * Tests for inventory with max number of slots = 100, max capacity in one slot = 100
 */

namespace Tests.InventoryTests
{
    public class InventoryTests
    {
        public int CreateInventory(EcsWorld world, int slotCapacity)
        {
            int inventoryEntity = world.NewEntity();
            inventoryEntity.EntityAdd<InventoryComponent>(world).SlotCapacity = slotCapacity;

            return inventoryEntity;
        }
    
        private int CreateItem(EcsWorld world, string itemId, int amount = 1)
        {
            int itemEntity = world.NewEntity();
            itemEntity.EntityAdd<ItemIdComponent>(world).ItemId = itemId;
            itemEntity.EntityAdd<AmountComponent>(world).Value = amount;

            return itemEntity;
        }

        [Test]
        public void AddItemWithoutItemId_ItCannotBeAdded_InventoryShouldBeEmpty()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 1);
            itemEntity.EntityDel<ItemIdComponent>(world);

            bool added = inventoryService.Add(world, inventoryEntity, itemEntity);
            added.Should().Be(false);
        
            int amount = inventoryService.GetAmount(world, inventoryEntity);
            amount.Should().Be(0);
        }
    
        [Test]
        public void EmptyInventoryAmountShouldBe0()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);

            int amount = inventoryService.GetAmount(world, inventoryEntity);

            amount.Should().Be(0);
        }
    
        [Test]
        public void Add100StackableItems_AmountItemsShouldBe100_FilledSlotsShouldBe1()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 100);

            inventoryService.Add(world, inventoryEntity, itemEntity);
        
            int amountItems = inventoryService.GetAmount(world, inventoryEntity);
            int filledSlots = inventoryService.GetAllItems(world, inventoryEntity).Count;

            amountItems.Should().Be(100);
            filledSlots.Should().Be(1);
        }
    
        [Test]
        public void Add101StackableItems_AmountItemsShouldBe101_FilledSlotsShouldBe2()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 101);

            inventoryService.Add(world, inventoryEntity, itemEntity);
        
            int amountItems = inventoryService.GetAmount(world, inventoryEntity);
            int filledSlots = inventoryService.GetAllItems(world, inventoryEntity).Count;

            amountItems.Should().Be(101);
            filledSlots.Should().Be(2);
        }
    
        [Test]
        public void Add100UniqueItems_AmountItemsShouldBe100_FilledSlotsShouldBe100()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.UniqueItem1, 100);

            inventoryService.Add(world, inventoryEntity, itemEntity);
        
            int amountItems = inventoryService.GetAmount(world, inventoryEntity);
            int filledSlots = inventoryService.GetAllItems(world, inventoryEntity).Count;

            amountItems.Should().Be(100);
            filledSlots.Should().Be(100);
        }
    
        [Test]
        public void Add2DifferentStackableItemsWithAmount10_AmountItemsShouldBe20_FilledSlotsShouldBe2()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity1 = CreateItem(world, ItemConfig.StackableItem1, 10);
            int itemEntity2 = CreateItem(world, ItemConfig.StackableItem2, 10);

            inventoryService.Add(world, inventoryEntity, itemEntity1);
            inventoryService.Add(world, inventoryEntity, itemEntity2);
        
            int amountItems = inventoryService.GetAmount(world, inventoryEntity);
            int filledSlots = inventoryService.GetAllItems(world, inventoryEntity).Count;

            amountItems.Should().Be(20);
            filledSlots.Should().Be(2);
        }

        [Test]
        public void IsEnoughSpaceToAdd100UniqueItems_ShouldEnough()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.UniqueItem1, 100);

            bool added = inventoryService.Add(world, inventoryEntity, itemEntity);
            int amount = inventoryService.GetAmount(world, inventoryEntity);
        
            added.Should().Be(true);
            amount.Should().Be(100);
        }
    
        [Test]
        public void IsEnoughSpaceToAdd101UniqueItems_ShouldNot()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.UniqueItem1, 101);

            bool added = inventoryService.Add(world, inventoryEntity, itemEntity);
            int amount = inventoryService.GetAmount(world, inventoryEntity);
        
            added.Should().Be(false);
            amount.Should().Be(0);
        }
    
        [Test]
        public void IsEnoughSpaceToAdd10000StackableItems_ShouldEnough()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 10000);

            bool added = inventoryService.Add(world, inventoryEntity, itemEntity);
            int amount = inventoryService.GetAmount(world, inventoryEntity);
        
            added.Should().Be(true);
            amount.Should().Be(10000);
        }
    
        [Test]
        public void IsEnoughSpaceToAdd10001StackableItems_ShouldNot()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 10001);

            bool added = inventoryService.Add(world, inventoryEntity, itemEntity);
            int amount = inventoryService.GetAmount(world, inventoryEntity);
        
            added.Should().Be(false);
            amount.Should().Be(0);
        }
    
        [Test]
        public void ComplexAddTest()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
        
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 101);
            inventoryService.Add(world, inventoryEntity, itemEntity);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(101);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(2);
        
            // StackableItem1 - amount: 101, slots: 2
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.StackableItem2, 101);
            inventoryService.Add(world, inventoryEntity, itemEntity);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(202);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(4);
        
            // StackableItem1 - amount: 101, slots: 2
            // StackableItem2 - amount: 101, slots: 2
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.StackableItem1, 99);
            inventoryService.Add(world, inventoryEntity, itemEntity);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(301);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(4);

            // StackableItem1 - amount: 200, slots: 2
            // StackableItem2 - amount: 101, slots: 2
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.StackableItem1, 1);
            inventoryService.Add(world, inventoryEntity, itemEntity);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(302);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(5);
        
            // StackableItem1 - amount: 201, slots: 3
            // StackableItem2 - amount: 101, slots: 2
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.UniqueItem1, 45);
            inventoryService.Add(world, inventoryEntity, itemEntity);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(347);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(50);
        
            // StackableItem1 - amount: 201, slots: 3
            // StackableItem2 - amount: 101, slots: 2
            // UniqueItem1 - amount: 45, slots: 45
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.UniqueItem2, 50);
            inventoryService.Add(world, inventoryEntity, itemEntity);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(397);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(100);

            // StackableItem1 - amount: 201, slots: 3
            // StackableItem2 - amount: 101, slots: 2
            // UniqueItem1 - amount: 45, slots: 45
            // UniqueItem2 - amount: 50, slots: 50
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.StackableItem1, 2000);
            inventoryService.Add(world, inventoryEntity, itemEntity, 99);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(496);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(100);
        
            // StackableItem1 - amount: 300, slots: 3
            // StackableItem2 - amount: 101, slots: 2
            // UniqueItem1 - amount: 45, slots: 45
            // UniqueItem2 - amount: 50, slots: 50
            //------------------------------
        
            itemEntity = CreateItem(world, ItemConfig.StackableItem1, 2000);
            inventoryService.Add(world, inventoryEntity, itemEntity, 1);

            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(100);

            int amountStackable1 = inventoryService.GetAmount(world, inventoryEntity, ItemConfig.StackableItem1);
            int amountStackable2 = inventoryService.GetAmount(world, inventoryEntity, ItemConfig.StackableItem2);
            int amountUnique1 = inventoryService.GetAmount(world, inventoryEntity, ItemConfig.UniqueItem1);
            int amountUnique2 = inventoryService.GetAmount(world, inventoryEntity, ItemConfig.UniqueItem2);

            amountStackable1.Should().Be(300);
            amountStackable2.Should().Be(101);
            amountUnique1.Should().Be(45);
            amountUnique2.Should().Be(50);

            var slotsStackable1 = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            var slotsStackable2 = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem2);
            var slotsUnique1 = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.UniqueItem1);
            var slotsUnique2 = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.UniqueItem2);

            slotsStackable1.Should().HaveCount(3);
            slotsStackable2.Should().HaveCount(2);
            slotsUnique1.Should().HaveCount(45);
            slotsUnique2.Should().HaveCount(50);

            // StackableItem1 - amount: 300, slots: 3
            // StackableItem2 - amount: 101, slots: 2
            // UniqueItem1 - amount: 45, slots: 45
            // UniqueItem2 - amount: 50, slots: 50
            //------------------------------
        }

        [Test]
        public void RemoveTest1()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
        
            int itemEntity = CreateItem(world, ItemConfig.StackableItem1, 500);
            inventoryService.Add(world, inventoryEntity, itemEntity);
            inventoryService.Remove(world, inventoryEntity, ItemConfig.StackableItem1, 201);
        
            inventoryService.GetAmount(world, inventoryEntity).Should().Be(299);
        }
    
        [Test]
        public void RemoveTest2_RemoveAllItemsWithSpecificId()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
        
            int itemEntity1 = CreateItem(world, ItemConfig.StackableItem1, 500);
            int itemEntity2 = CreateItem(world, ItemConfig.StackableItem2, 500);
        
            inventoryService.Add(world, inventoryEntity, itemEntity1);
            inventoryService.Add(world, inventoryEntity, itemEntity2);
            inventoryService.Remove(world, inventoryEntity, ItemConfig.StackableItem1);
        
            inventoryService.GetAmount(world, inventoryEntity).Should().Be(500);

            bool hasTestItem1 = inventoryService.HasItems(world, inventoryEntity, ItemConfig.StackableItem1,out List<int> _);
        
            hasTestItem1.Should().BeFalse();
        }
    
        [Test]
        public void RemoveTest3_RemoveOneByOneSlot()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity1 = CreateItem(world, ItemConfig.StackableItem1, 300);

            inventoryService.Add(world, inventoryEntity, itemEntity1);

            List<int> slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(3);
        
            inventoryService.Remove(world, inventoryEntity, slotsWithId.First());
        
            slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(2);
        
            inventoryService.Remove(world, inventoryEntity, slotsWithId.First());
        
            slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(1);
        
            inventoryService.GetAmount(world, inventoryEntity).Should().Be(100);
        }
    
        [Test]
        public void RemoveTest4_RemoveAmount50FromSlotWithAmount100()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity1 = CreateItem(world, ItemConfig.StackableItem1, 300);

            inventoryService.Add(world, inventoryEntity, itemEntity1);

            List<int> slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(3);
            inventoryService.GetAmount(world, inventoryEntity).Should().Be(300);
        
            inventoryService.Remove(world, inventoryEntity, slotsWithId.First(), 50);
        
            slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(3);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(250);
        }
    
        [Test]
        public void RemoveTest5_RemoveAmount500FromSlotWithAmount100()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
            int itemEntity1 = CreateItem(world, ItemConfig.StackableItem1, 100);

            inventoryService.Add(world, inventoryEntity, itemEntity1);

            List<int> slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(1);
            inventoryService.GetAmount(world, inventoryEntity).Should().Be(100);
        
            int removedCount = inventoryService.Remove(world, inventoryEntity, slotsWithId.First(), 500);
            removedCount.Should().Be(100);
        
            slotsWithId = inventoryService.GetAllItems(world, inventoryEntity, ItemConfig.StackableItem1);
            slotsWithId.Should().HaveCount(0);

            inventoryService.GetAmount(world, inventoryEntity).Should().Be(0);
        }
    
        [Test]
        public void RemoveTest6_CompletelyClearInventory()
        {
            EcsWorld world = new EcsWorld();
            IInventoryService inventoryService = new TestInventoryService();

            int inventoryEntity = CreateInventory(world, 100);
        
            int itemEntity1 = CreateItem(world, ItemConfig.StackableItem1, 1234);
            int itemEntity2 = CreateItem(world, ItemConfig.StackableItem2, 2345);
            int itemEntity3 = CreateItem(world, ItemConfig.UniqueItem1, 15);
        
            inventoryService.Add(world, inventoryEntity, itemEntity1);
            inventoryService.Add(world, inventoryEntity, itemEntity2);
            inventoryService.Add(world, inventoryEntity, itemEntity3);

            inventoryService.Clear(world, inventoryEntity);
        
            inventoryService.GetAmount(world, inventoryEntity).Should().Be(0);
            inventoryService.GetAllItems(world, inventoryEntity).Count.Should().Be(0);
        }
    }
}