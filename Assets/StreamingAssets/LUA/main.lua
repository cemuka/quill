local tooltip = nil

function OnInit()
    tooltip = quill.empty()

    options = {}
    options.filterMode = "Point"
    options.borderX = 3
    options.borderY = 3
    options.borderZ = 3
    options.borderW = 3

    local box = quill.box()
    box:setSize(120, 80)
    tooltip:addChild(box)
    box:sprite("body.png", options)
    box:setImageType("Tiled")
    box:setColor(1,1,1)
    box:setAnchoredPosition(20, -20)

    local label = quill.label("tooltip example")
    box:addChild(label)
    label:stretchToParent()
    label:setTextAnchor("MiddleCenter")
end

function OnUpdate(dt)
    
    local pos = quill.mousePosition()
    -- quill.log("pos: " .. pos.x .. ", " .. pos.y)

    tooltip:setAnchoredPosition(pos.x, pos.y)
end