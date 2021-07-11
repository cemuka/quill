
local timePassed = 0
local timeLabel = nil

function Init()

    local root = quill.empty()

    color = {}
    color.r = 0.2
    color.g = 0.3
    color.b = 0.7

    local box = quill.box()
    box.setColor(color.r, color.g, color.b)
    box.setSize(300, 100)
    root.addChild(box)

    timeLabel = quill.label("child label")
    timeLabel.setSize(300, 100)
    
    root.addChild(timeLabel)
end

function OnUpdate(dt)
    timePassed = timePassed + dt
    timeLabel.setText("time: " .. string.format("%.2f", timePassed))

end