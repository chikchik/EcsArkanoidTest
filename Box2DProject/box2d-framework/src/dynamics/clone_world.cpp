#include "box2d/clone_world.h"

struct BodyReferenceComponent
{
public: void* BodyReference;
};

CloneWorldInfo::CloneWorldInfo(BodyReferenceComponent* vertices, const int& count,
	const b2BlockAllocator& newAllocator,
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

	for (int32 i = 0; i < count; i++)
	{
		vertices[i].BodyReference = GetMovedAdress(vertices[i].BodyReference);
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
T* CloneWorldInfo::GetMovedAdress(T* ptr)
{
	
	b2Assert(IsOldAdress(ptr));

	auto newAdrr = reinterpret_cast<T*>(reinterpret_cast<int8*>(ptr)
		+ GetMovedOffset(ptr));
	b2Assert(!IsOldAdress(newAdrr));
	return newAdrr;
}

size_t CloneWorldInfo::GetMovedOffset(void* p)
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

void CloneWorldInfo::Move(b2World* newWorld)
{
	m_newWorld = newWorld;

	if (newWorld->m_contactManager.m_contactList)
	{
		newWorld->m_contactManager.m_contactList
			= GetMovedAdress(newWorld->m_contactManager.m_contactList);
		IterateMove(newWorld->m_contactManager.m_contactList);
	}

	for (int i = 0; i < newWorld->m_contactManager.m_broadPhase.m_tree.m_nodeCount; ++i)
	{
		Move(newWorld->m_contactManager.m_broadPhase.m_tree.m_nodes + i);
	}

	if (newWorld->m_bodyList)
	{
		m_newWorld->m_bodyList = GetMovedAdress(newWorld->m_bodyList);
		IterateMove(m_newWorld->m_bodyList);
	}

	if (newWorld->m_jointList)
	{
		m_newWorld->m_jointList = GetMovedAdress(newWorld->m_jointList);
		IterateMove(m_newWorld->m_jointList);
	}
}

void CloneWorldInfo::Move(b2TreeNode* obj)
{
	b2Assert(!IsOldAdress(obj));
	auto ptr = static_cast<b2FixtureProxy*>(obj->userData);
	//b2Assert(ptr);
	if (ptr)
	{
		ptr = GetMovedAdress(ptr);


		obj->userData = ptr;

		
		Move(ptr);
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
		obj->m_prev = GetMovedAdress(obj->m_prev);
	}

	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next);
	}

	if (obj->m_fixtureList)
	{
		obj->m_fixtureList = GetMovedAdress(obj->m_fixtureList);
		IterateMove(obj->m_fixtureList);
	}

	if (obj->m_jointList)
	{
		obj->m_jointList = GetMovedAdress(obj->m_jointList);
		IterateMove2(obj->m_jointList);
	}

	if (obj->m_contactList)
	{
		obj->m_contactList = GetMovedAdress(obj->m_contactList);
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
		obj->m_prev = GetMovedAdress(obj->m_prev);
	}

	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next);
	}

	if (obj->m_bodyA)
	{
		obj->m_bodyA = GetMovedAdress(obj->m_bodyA);
		Move(obj->m_bodyA);
	}
	if (obj->m_bodyB)
	{
		obj->m_bodyB = GetMovedAdress(obj->m_bodyB);
		Move(obj->m_bodyB);
	}

	Move(&obj->m_edgeA);
	Move(&obj->m_edgeB);

	if (auto gearJoint = dynamic_cast<b2GearJoint*>(obj))
	{
		if (gearJoint->m_joint1)
		{
			gearJoint->m_joint1 = GetMovedAdress(gearJoint->m_joint1);
			Move(gearJoint->m_joint1);
		}
		if (gearJoint->m_joint1)
		{
			gearJoint->m_joint2 = GetMovedAdress(gearJoint->m_joint2);
			Move(gearJoint->m_joint2);
		}

		if (gearJoint->m_bodyC)
		{
			gearJoint->m_bodyC = GetMovedAdress(gearJoint->m_bodyC);
			Move(gearJoint->m_bodyC);
		}
		if (gearJoint->m_bodyD)
		{
			gearJoint->m_bodyD = GetMovedAdress(gearJoint->m_bodyD);
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
		obj->prev = GetMovedAdress(obj->prev);
	}

	if (obj->next)
	{
		obj->next = GetMovedAdress(obj->next);
	}

	if (obj->other)
	{
		obj->other = GetMovedAdress(obj->other);
		Move(obj->other);
	}

	if (obj->joint)
	{
		obj->joint = GetMovedAdress(obj->joint);
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
		obj->prev = GetMovedAdress(obj->prev);
	}

	if (obj->next)
	{
		obj->next = GetMovedAdress(obj->next);
	}

	if (obj->other)
	{
		obj->other = GetMovedAdress(obj->other);
		Move(obj->other);
	}

	if (obj->contact)
	{
		obj->contact = GetMovedAdress(obj->contact);
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
		obj->m_prev = GetMovedAdress(obj->m_prev);
	}
	if (obj->m_next)
	{
		obj->m_next = GetMovedAdress(obj->m_next);
	}

	IterateMove2(&obj->m_nodeA);
	IterateMove2(&obj->m_nodeB);

	if (obj->m_fixtureA)
	{
		obj->m_fixtureA = GetMovedAdress(obj->m_fixtureA);
		IterateMove(obj->m_fixtureA);
	}
	if (obj->m_fixtureB)
	{
		obj->m_fixtureB = GetMovedAdress(obj->m_fixtureB);
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
		obj->m_next = GetMovedAdress(obj->m_next);
	}
	if (obj->m_body)
	{
		obj->m_body = GetMovedAdress(obj->m_body);
		Move(obj->m_body);
	}
	if (obj->m_shape)
	{
		obj->m_shape = GetMovedAdress(obj->m_shape);
	}
	if (obj->m_proxies)
	{
		obj->m_proxies = GetMovedAdress(obj->m_proxies);
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
		obj->fixture = GetMovedAdress(obj->fixture);
		IterateMove(obj->fixture);
	}
}
