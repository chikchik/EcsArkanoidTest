#include "box2d/clone_world_service.h"

bool CloneWorldService::IsOldAdress(void* p)
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

size_t CloneWorldService::GetMovedOffset(void* p)
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