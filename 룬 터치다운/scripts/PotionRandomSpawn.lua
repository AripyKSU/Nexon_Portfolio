--@ BeginProperty
--@ SyncDirection=All
number time = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=All
number maxSpawnCount = "15"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=None
table potionArray = "{}"
--@ EndProperty

--@ BeginMethod
--@ MethodExecSpace=ServerOnly
void OnUpdate(number delta)
{
self.time = self.time + delta

if self.time > 10 then
	self.time = 0
	local curPotionCnt = self:GetCurPotionCount()
	
	if curPotionCnt == nil then
		return
	end
	
	if curPotionCnt < self.maxSpawnCount then 
		self:SpawnPotion()
	end
end
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void SpawnPotion()
{
local parent = _EntityService:GetEntityByPath("/maps/inGamePlaying")
local nextNum = #self.potionArray + 1

local random_X_pos = math.random(-2.0, 14.0)
local random_Y_pos = math.random(11.0, 21.0)

self.potionArray[nextNum] = _SpawnService:SpawnByModelId("model://7f7fd793-0a64-41bf-89f9-ea07e6b71f81", "po", Vector3(random_X_pos, random_Y_pos, 0), _EntityService:GetEntity("e00acbe7-8173-4ae8-9cea-ea5873737b6f"))
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
number GetCurPotionCount()
{
local i = 1
while i <= #self.potionArray do
	if isvalid(self.potionArray[i]) == true then
		i = i + 1
	else
		table.remove(self.potionArray, i)
	end
end

return #self.potionArray
}
--@ EndMethod

