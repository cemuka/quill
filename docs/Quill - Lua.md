# Quill Lua api
## Getting Started

Enable lua features by calling extra methods below.
Quill will look into `StreamingAssets/LUA` directory for the `main.lua` file.

```csharp
public class AppStartup : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();
        QuillLua.Run(); 
    }

    private void Update()
    {
        QuillLua.Update();
    }

    private void OnDestroy()
    {
        QuillLua.Exit();
    }
}
```

Here is the structure of the `StreamingAssets/LUA/main.lua`
```lua
function OnInit()

end

function OnUpdate(dt)

end

function OnExit()

end
```

## Static properties
`quill.screenHeight`  
`quill.screenWidth`  

## Static Methods
`quill.mainRoot()`  
`quill.empty()`  
`quill.label()`  
`quill.box()`  
`quill.button()`  
`quill.loadFont(name, size)`  
`quill.setDefaultFont(name)`  
`quill.mousePosition()`  
`quill.log(text)`  

## Components

### Empty
`element:show()`  
`element:hide()`  
`element:destroy()`  
`element:addChild(element)`  
`element:setPivot(x,y)`  
`element:setSize(x,y)`  
`element:setAnchoredPosition(x,y)`  
`element:setAnchorsMin(x,y)`  
`element:setAnchorsMax(x,y)`  
`element:resetTransform()`  
`element:stretchToParent()`  
### Label
`label:getText()`  
`label:setText(text)`  
`label:getFont()`  
`label:setFont(fontName)`  
`label:getColor()`  
`label:setColor(r,g,b)`  
`label:setColor(r,g,b,a)`  
`label:setColor(table)`  
`label:setTextAnchor(textAnchor)`  
### Box
`box:getColor()`  
`box:setColor(r,g,b)`  
`box:setColor(r,g,b,a)`  
`box:setColor(table)`  
`box:sprite(path)`  
`box:sprite(path, options)`  
`box:setImageType(type)`  
### Button
`button.box`  
`button.label`  
`button.onClick:add(function)`  
`button.onClick:remove(function)`  