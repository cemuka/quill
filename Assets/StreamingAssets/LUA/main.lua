local tooltip = nil

function OnInit()
    tooltip = quill.empty()
    quill.mainRoot().addChild(tooltip)

    local box = quill.box()
    tooltip.addChild(box)
    box.setColor(0,0.5,1)
    box.setPosition(20, -20)

    local label = quill.label("tooltip example")
    box.addChild(label)
end

function OnUpdate(dt)
    
    local pos = quill.mousePosition()
    quill.log("pos: " .. pos.x .. ", " .. pos.y)

    tooltip.setPosition(pos.x, pos.y)
end