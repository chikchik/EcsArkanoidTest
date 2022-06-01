#ifndef CLONE_B2_WORLD_H
#define CLONE_B2_WORLD_H

#include "box2d/b2_body.h"
#include "box2d/b2_contact.h"
#include "box2d/b2_edge_shape.h"
#include "box2d/b2_fixture.h"
#include "box2d/b2_polygon_shape.h"
#include "box2d/b2_world.h"
#include "box2d/b2_gear_joint.h"
#include "b2_contact_manager.h"

#include <unordered_set>
#include <map>

struct CloneWorldService;

struct CloneWorldInfo
{
	b2BlockAllocator* allocator = nullptr;
	int offset = 0;
	std::unordered_set<void*> movedObjs;
	CloneWorldService* m_cloneService;

	b2World* m_newWorld;

	CloneWorldInfo(void** arrayOfReferences,
		const int& count,
		const b2BlockAllocator& newAllocator,
		const b2BlockAllocator& oldAllocator,
		CloneWorldService& cloneService);

	template<class T>
	void IterateMove(T* head);

	template<class T>
	void IterateMove2(T* head);

	void Move(b2TreeNode* obj);

	void Move(b2World* newWorld);
	
	void Move(b2Body* obj);

	void Move(b2Joint* obj);

	void Move(b2JointEdge* obj);

	void Move(b2ContactEdge* obj);

	void Move(b2Contact* obj);

	void Move(b2Fixture* obj);

	void Move(b2Shape* obj);

	void Move(b2FixtureProxy* obj);
};
#endif