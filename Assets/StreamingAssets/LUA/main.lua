
local timePassed    = 0
local timeLabel     = nil
local root          = nil

local timer         = 0
local timerStarted  = false

local inform        = nil
local test = require "test"

function clickEvent()
    inform = quill.label("you have clicked!")
    root.addChild(inform)

    inform.setPosition(500,-300)

    timerStarted = true
end

function handler(id, data)
    if id == "space" then
        quill.log("msg id: " .. id .. " handled.")
    end
end

function OnInit()


    test.greeter()


    timer        = 3.0
    timerStarted = false

    root    = quill.mainRoot()


    quill.log(quill.screenHeight)
    quill.log(quill.screenWidth)

    local button = quill.button()
    button.onClick.add(clickEvent)
    button.label.setText("this is a button")
    
    root.addChild(button)
    
    button.setPosition(20,-200)
    
    color = {}
    color.r = 0.4
    color.g = 0.8
    color.b = 0.3
    
    local box = quill.box()
    box.setColor(color.r, color.g, color.b)
    box.setSize(300, 100)
    quill.setSprite(box, "dices/dice-spritesheet.png");
    root.addChild(box)

    timeLabel = quill.label("time")
    timeLabel.setSize(300, 100)
    
    root.addChild(timeLabel)
end

function OnMessage(data)
    if data.id == "space" then
        quill.log("lua handled.")
    end
end

-- function OnUpdate(dt)

--     if timer < 0 then
--         inform.destroy()

--         timerStarted = false;
--         timer = 3
--     end

--     if timerStarted then
--         timer = timer - dt
--     end
-- end

-- function OnExit()
--     quill.log("program exit")
-- end