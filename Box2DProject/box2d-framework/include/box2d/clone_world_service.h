#ifndef CLONE_B2_WORLD_SERVICE_H
#define CLONE_B2_WORLD_SERVICE_H

#include <map>
#include "b2_allocator_constants.h"
#include "b2_common.h"

struct CloneWorldService
{
	std::map<void* const, ptrdiff_t> offsets;

	bool IsOldAdress(void* p);
	size_t GetMovedOffset(void* p);
	template<class T>
	T* GetMovedAdress(T* ptr)
	{
		b2Assert(IsOldAdress(ptr));
		auto newAdrr = reinterpret_cast<T*>(reinterpret_cast<int8*>(ptr)
			+ GetMovedOffset(ptr));
		b2Assert(!IsOldAdress(newAdrr));
		return newAdrr;
	}
};
#endif