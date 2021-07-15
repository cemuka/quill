
-- local timePassed    = 0
-- local timeLabel     = nil

-- local timer         = 0
-- local timerStarted  = false

-- local inform        = nil

-- local test  = require("lib/test")
local root          = nil

-- function clickEvent()
--     inform = quill.label("you have clicked!")
--     root.addChild(inform)

--     inform.setPosition(500,-300)

--     timerStarted = true
-- end

-- function handler(id, data)
--     if id == "space" then
--         quill.log("msg id: " .. id .. " handled.")
--     end
-- end

function OnInit()
    root    = quill.mainRoot()
    local firaCode = "Fira Code"
    quill.loadFont(firaCode, 24);
    
    local label1 = quill.label("label1")
    local label2 = quill.label("label2")
    label2.setFont(firaCode)
    
    quill.setDefaultFont(firaCode)
    
    
    local label3 = quill.label("label3")

    root.addChild(label1)
    root.addChild(label2)
    root.addChild(label3)


    -- quill.log(quill.screenHeight)
    -- quill.log(quill.screenWidth)

    -- local button = quill.button("this is a button")
    -- button.onClick.add(clickEvent)
    
    -- root.addChild(button)
    
    -- button.setPosition(20,-200)
    
    -- color = {}
    -- color.r = 0.4
    -- color.g = 0.8
    -- color.b = 0.3
    -- color.a = 0.1

    -- options = {}
    -- options.filterMode = "Point"
    -- options.pivotX = 0.5
    -- options.pivotY = 0.5
    -- options.extrude = 0
    -- options.pixelsPerUnit = 100
    -- options.borderX = 3
    -- options.borderY = 3
    -- options.borderZ = 3
    -- options.borderW = 3

    
    -- local box = quill.box()
    -- box.setColor(color)
    -- box.setSize(300, 100)
    -- box.sprite("body.png", options);
    -- box.setImageType("Sliced");

    -- root.addChild(box)

    -- timeLabel = quill.label("time")
    -- timeLabel.setSize(300, 100)
    
    -- root.addChild(timeLabel)
end

function OnMessage(data)
    if data.id == "space" then

        quill.log(data.container.player)
        quill.log(data.container.score)
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