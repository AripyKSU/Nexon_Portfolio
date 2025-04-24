--@ BeginProperty
--@ SyncDirection=All
number leftTeamScore = "0"
--@ EndProperty

--@ BeginProperty
--@ SyncDirection=All
number rightTeamScore = "0"
--@ EndProperty

--@ BeginMethod
--@ MethodExecSpace=Server
void Timer()
{
local time = 300
local gameTimerId = 0
local GameTimer = function()
	if(self.gameStopFlag) then
		self:PlayerStop()
	else
		self:PlayerMoveAllow()
		time = time - 1
		self:UpdateTimerUI(time)
		if time == 90 then
			--UI text넣기
			local random = math.random(1,2)
			local y
			if(random == 1) then
				y = 23.5
			else 
				y = 7.5
			end
			self:TimeStopRuneUI() 
			_SpawnService:SpawnByModelId("model://f1ae0f4c-07f8-478e-ac1b-2d75f88ddd21", "timeStop", Vector3(5.5, y, 0),_EntityService:GetEntityByPath("/maps/inGamePlaying"))
		end
		if time <= 0 then
			--game end
			self:PlayerStop()
			self:GameEndTimer()
			self:GameEndEventSend()
			self:ResultPanelFunc()
			--timer,hp UI 초기화 (enable = false)
			_TimerService:ClearTimer(gameTimerId)
		end
	end
end
gameTimerId = _TimerService:SetTimerRepeat(GameTimer, 1, 10) --DelaySecond 나중에 10으로 돌려놓기
self:PlayerStop()
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=Client
void SetTimerUI()
{
self.minute:SetEnable(true)
self.second:SetEnable(true)
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=Server
void GoalGameStop()
{
self.gameStopFlag = true
local timer = function()
	self:PlayerSpeedInit()
	self.gameStopFlag = false
end
_TimerService:SetTimerOnce(timer, 5)
}
--@ EndMethod

--@ BeginMethod
--@ MethodExecSpace=All
void GameEndEventSend()
{
local event = GameEndEvent()
event.Player1 = self.Player1
event.Player2 = self.Player2
event.Player3 = self.Player3
event.Player4 = self.Player4
event.Player5 = self.Player5
event.Player6 = self.Player6
self.Entity:SendEvent(event) --InGamePlaying맵에 게임 시작 이벤트 동시 처리
}
--@ EndMethod


--@ BeginMethod
--@ MethodExecSpace=ClientOnly
void OnUpdate(number delta)
{
local ScoreA = _EntityService:GetEntityByPath("/ui/DefaultGroup/ScoreA")
local ScoreB = _EntityService:GetEntityByPath("/ui/DefaultGroup/ScoreB")

ScoreA.TextComponent.Text = tostring(math.floor(self.leftTeamScore))
ScoreB.TextComponent.Text = tostring(math.floor(self.rightTeamScore))
}
--@ EndMethod


--@ BeginEntityEventHandler
--@ Scope=Server
--@ Target=entity:e00acbe7-8173-4ae8-9cea-ea5873737b6f
--@ EventName=GameStartEvent
HandleGameStartEvent
{
-- Parameters
self.Player1 = event.Player1
self.Player2 = event.Player2
self.Player3 = event.Player3
self.Player4 = event.Player4
self.Player5 = event.Player5
self.Player6 = event.Player6
--------------------------------------------------------
self:Timer()
self:etcUI()
}
--@ EndEntityEventHandler

--@ BeginEntityEventHandler
--@ Scope=Server
--@ Target=entity:e00acbe7-8173-4ae8-9cea-ea5873737b6f
--@ EventName=GoalEvent
HandleGoalEvent
{
-- Parameters
--------------------------------------------------------
-- 터치다운 UI 보여주기 여기에 추가

-- 까만 화면 스크린 전환
-- 공 위치 초기화 여기에 추가
self:PlayerStop()
self:TouchDown()
self:GoalGameStop()
self:GoalInit()
}
--@ EndEntityEventHandler


--@ BeginMethod
--@ MethodExecSpace=Client
void TouchDown()
{
local timer = 0
local logo = _EntityService:GetEntityByPath("/ui/DefaultGroup/TouchdownUI")
local num = 0
local logoMove = function()
	logo.UITransformComponent.Position.x = logo.UITransformComponent.Position.x - 16
	num = num + 1
	if(num == 100) then
		_TimerService:ClearTimer(timer)
		num = 0
	end
end
timer = _TimerService:SetTimerRepeat(logoMove, 0.01, 0)
wait(1)
local timer2 = 0
local logoMove2 = function()
	logo.UITransformComponent.Position.x = logo.UITransformComponent.Position.x - 16
	num = num + 1
	if(num == 100) then
		_TimerService:ClearTimer(timer2)
		num = 0
		logo.UITransformComponent.Position.x = 1600
	end
end
timer2 = _TimerService:SetTimerRepeat(logoMove2, 0.01, 3)
wait(5)
}
--@ EndMethod

--@ BeginEntityEventHandler
--@ Scope=Client
--@ Target=entity:e00acbe7-8173-4ae8-9cea-ea5873737b6f
--@ EventName=GameEndEvent
HandleGameEndEvent
{
-- Parameters
local Player1 = event.Player1
local Player2 = event.Player2
local Player3 = event.Player3
local Player4 = event.Player4
local Player5 = event.Player5
local Player6 = event.Player6
--------------------------------------------------------
self:GameEndTimer()
}
--@ EndEntityEventHandler

--@ BeginEntityEventHandler
--@ Scope=All
--@ Target=service:UserService
--@ EventName=UserLeaveEvent
HandleUserLeaveEvent
{
-- Parameters
local UserId = event.UserId
--------------------------------------------------------
log("Player Leave! : "..UserId) 
}
--@ EndEntityEventHandler