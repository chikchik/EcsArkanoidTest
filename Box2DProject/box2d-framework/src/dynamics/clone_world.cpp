#include "box2d/clone_world.h"

CloneWorldInfo::CloneWorldInfo(const b2BlockAllocator & newAllocator,
	const b2BlockAllocator& oldAllocator)
{
	for (int32 i = 0; i < oldAllocator.m_chunkCount; ++i)
	{
		b2Chunk* oldChunk = oldAllocator.m_chunks + i;
		b2Chunk* newChunk = newAllocator.m_chunks + i;

		size_t offset = reinterpret_cast<int8*>(newChunk->blocks)
			- reinterpret_cast<int8*>(oldChunk->blocks);

		offsets.insert(std::make_pair(oldChunk->blocks, offset));
	}
}

bool CloneWorldInfo::IsOldAdress(void* p)
{
	auto it = offsets.lower_bound(p);

	if (it == offsets.end()
		|| it != offsets.begin()
		&& it->first != p)
	{
		--it;
	}
	return (int8*)p >= (int8*)it->first
		&& (int8*)p <= (int8*)it->first + b2_chunkSize;
}

template<class  T>
T* CloneWorldInfo::GetMovedAdress(T* ptr, int idx)
{
	return reinterpret_cast<T*>(reinterpret_cast<int8*>(ptr)
		+ GetMovedOffset(ptr, idx));
}

size_t CloneWorldInfo::GetMovedOffset(void* p, int idx)
{
	auto it = offsets.lower_bound(p);

	if (it == offsets.end()
		|| it != offsets.begin()
		&& it->first != p)
	{
		--it;
	}

	b2Assert((int8*)p >= (int8*)it->first
		&& (int8*)p <= (int8*)it->first + b2_chunkSize);

	return it->second;
}

template<class T>
void CloneWorldInfo::IterateMove(T* head)
{
	T* curNode = head;
	while (curNode)
	{
		Move(curNode);
		curNode = curNode->m_next;
	}
}

template<class T>
void CloneWorldInfo::IterateMove2(T* head)
{
	T* curNode = head;
	while (curNode)
	{
		Move(curNode);
		curNode = curNode->next;
	}
}

void CloneWorldInfo::Move(b2World* newWorld, const b2World* oldWorld)
{
	m_newWorld = newWorld;
	if (oldWorld->m_bodyList)
	{
		m_newWorld->m_bodyList = GetMovedAdress(oldWorld->m_bodyList, 0);
		IterateMove(m_newWorld->m_bodyList);
	}

	if (oldWorld->m_jointList)
	{
		m_newWorld->m_jointList = GetMovedAdress(oldWorld->m_jointList, 1);
		IterateMove(m_newWorld->m_jointList);
	}
}

void CloneWorldInfo::Move(b2Body* obj)
{
	b2Assert(!IsOldAdress(obj));

	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);

	obj->m_world = m_newWorld;

	if (obj->m_prev)
	{
		obj->m_prev = GetMovedAdress(obj->m_prev, 2);
	}

	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next, 3);
	}

	if (obj->m_fixtureList)
	{
		obj->m_fixtureList = GetMovedAdress(obj->m_fixtureList, 4);
		IterateMove(obj->m_fixtureList);
	}

	if (obj->m_jointList)
	{
		obj->m_jointList = GetMovedAdress(obj->m_jointList, 5);
		IterateMove2(obj->m_jointList);
	}

	if (obj->m_contactList)
	{
		obj->m_contactList = GetMovedAdress(obj->m_contactList, 6);
		IterateMove2(obj->m_contactList);
	}
}

void CloneWorldInfo::Move(b2Joint* obj)
{
	b2Assert(!IsOldAdress(obj));

	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);

	if (obj->m_prev)
	{
		obj->m_prev = GetMovedAdress(obj->m_prev, 7);
	}

	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next, 8);
	}

	if (obj->m_bodyA)
	{
		obj->m_bodyA = GetMovedAdress(obj->m_bodyA, 9);
		Move(obj->m_bodyA);
	}
	if (obj->m_bodyB)
	{
		obj->m_bodyB = GetMovedAdress(obj->m_bodyB, 10);
		Move(obj->m_bodyB);
	}

	Move(&obj->m_edgeA);
	Move(&obj->m_edgeB);

	if (auto gearJoint = dynamic_cast<b2GearJoint*>(obj))
	{
		if (gearJoint->m_joint1)
		{
			gearJoint->m_joint1 = GetMovedAdress(gearJoint->m_joint1, 11);
			Move(gearJoint->m_joint1);
		}
		if (gearJoint->m_joint1)
		{
			gearJoint->m_joint2 = GetMovedAdress(gearJoint->m_joint2, 12);
			Move(gearJoint->m_joint2);
		}

		if (gearJoint->m_bodyC)
		{
			gearJoint->m_bodyC = GetMovedAdress(gearJoint->m_bodyC, 13);
			Move(gearJoint->m_bodyC);
		}
		if (gearJoint->m_bodyD)
		{
			gearJoint->m_bodyD = GetMovedAdress(gearJoint->m_bodyD, 14);
			Move(gearJoint->m_bodyD);
		}
	}
}

void CloneWorldInfo::Move(b2JointEdge* obj)
{
	b2Assert(!IsOldAdress(obj));

	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);

	if (obj->prev)
	{
		obj->prev = GetMovedAdress(obj->prev, 17);
	}

	if (obj->next)
	{
		obj->next = GetMovedAdress(obj->next, 18);
	}

	if (obj->other)
	{
		obj->other = GetMovedAdress(obj->other, 15);
		Move(obj->other);
	}

	if (obj->joint)
	{
		obj->joint = GetMovedAdress(obj->joint, 16);
		IterateMove(obj->joint);
	}

}

void CloneWorldInfo::Move(b2ContactEdge* obj)
{
	b2Assert(!IsOldAdress(obj));
	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);

	if (obj->prev)
	{
		obj->prev = GetMovedAdress(obj->prev, 21);
	}

	if (obj->next)
	{
		obj->next = GetMovedAdress(obj->next, 22);
	}

	if (obj->other)
	{
		obj->other = GetMovedAdress(obj->other, 19);
		Move(obj->other);
	}

	if (obj->contact)
	{
		obj->contact = GetMovedAdress(obj->contact, 20);
		Move(obj->contact);
	}
}

void CloneWorldInfo::Move(b2Contact* obj)
{
	b2Assert(!IsOldAdress(obj));
	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);

	if (obj->m_prev)
	{
		obj->m_prev = GetMovedAdress(obj->m_prev, 23);
	}
	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next, 24);
	}

	IterateMove2(&obj->m_nodeA);
	IterateMove2(&obj->m_nodeB);

	if (obj->m_fixtureA)
	{
		obj->m_fixtureA = GetMovedAdress(obj->m_fixtureA, 25);
		IterateMove(obj->m_fixtureA);
	}
	if (obj->m_fixtureB)
	{
		obj->m_fixtureB = GetMovedAdress(obj->m_fixtureB, 26);
		IterateMove(obj->m_fixtureB);
	}
}

void CloneWorldInfo::Move(b2Fixture* obj)
{
	b2Assert(!IsOldAdress(obj));

	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);
	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next, 27);
	}
	if (obj->m_body)
	{
		obj->m_body = GetMovedAdress(obj->m_body, 28);
		Move(obj->m_body);
	}
	if (obj->m_shape)
	{
		obj->m_shape = GetMovedAdress(obj->m_shape, 29);
	}
	if (obj->m_proxies)
	{
		obj->m_proxies = GetMovedAdress(obj->m_proxies, 30);
	}
}

void CloneWorldInfo::Move(b2Shape* obj)
{
	b2Assert(!IsOldAdress(obj));

	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);
	// TODO chain shape maybe remove
}

void CloneWorldInfo::Move(b2FixtureProxy* obj)
{
	b2Assert(!IsOldAdress(obj));

	if (movedObjs.find(obj) != movedObjs.end()) return;
	movedObjs.insert(obj);

	if (obj->fixture)
	{
		obj->fixture = GetMovedAdress(obj->fixture, 31);
		IterateMove(obj->fixture);
	}
}
