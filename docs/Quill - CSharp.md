# Quill
## Getting Started

Setting up is dead easy.

```csharp
public class AppStartup : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();
    }
}
```

Quill does not require any resource or prefab. It initialize its canvas and checks for `EventSystem`. Any default object will be a child of this canvas.

## Static Fields
| Type | Name       | get | set |
|:---- |:-----------|:----|:----|
|Canvas |mainCanvas|yes|no|
|QuillElement |mainCanvasElement|yes|no|
|Message |message|yes|no|
|Font |defaultFont|yes|yes|

## Static Methods
```csharp
public static   void              Init()

public static   QuillElement      CreateEmpty()

public static   QuillLabel        CreateLabel(string text)

public static   QuillBox          CreateBox(Color color)

public static   QuillButton       CreateButton(string text)

public static   Vector2           MousePosition()
```

## Components
Every quill component actually a thin layer of its original unity class. That way you'll get the advantage of the base functionality.

# QuillElement

Empty object. 

```csharp
var empty = Quill.CreateEmpty();
empty.SetSize(100, 100);
empty.SetAnchoredPosition(300, 300);
```

`QuillElement` and `ElementRoot` are base components of the quill. It's more like `MonoBehaviour` and `Transform` if you familiar with unity. Since `RectTransform` is a sealead class `ElementRoot` come in handy.

```csharp
public class QuillElement : MonoBehaviour, IQuillElement
{
    public ElementRoot root { get; set; }
    ...
}
```

```csharp
public class ElementRoot
{
    public RectTransform rectTransform;

    public void AddChild(IQuillElement element) 
    { 
        //  make the target as child of the rectTransform
        
        //  element is child
        //  recTransform is parent
    }
}
```

# QuillLabel
Text object. [Unity Manual](https://docs.unity3d.com/2017.3/Documentation/ScriptReference/UI.Text.html)
```csharp
var label = Quill.CreateLabel("my label");
label.fontSize = 20;
```

```csharp
public class QuillLabel : Text, IQuillElement
{    
    ...
}
```

# QuillBox
Image object. [Unity Manual](https://docs.unity3d.com/2017.3/Documentation/ScriptReference/UI.Image.html)
```csharp
var box = Quill.CreateBox(Color.green);
box.SetSize(300, 300);
box.SetAnchoredPosition(200, 200);
```

```csharp
public class QuillBox : Image, IQuillElement
{
    ...
}
```

# QuillButton
Button object. [Unity Manual](https://docs.unity3d.com/2017.3/Documentation/ScriptReference/UI.Button.html)
```csharp
var button = Quill.CreateButton("my awesome button");
button.box.color = Color.blue;
button.label.color = Color.yellow;
```

```csharp
public class QuillButton : Button, IQuillElement
{
    ...
    public QuillLabel label;
    public QuillBox box;   
    ...
}
```

# Message

Provides a signal hub functionality.

```csharp
//  dispatch message 
var data = new MessageData("ON_SCORE_EVENT");
data.container.Add("score", 3);
data.container.Add("player", "cemuka");
Quill.message.Post(data);
```

```csharp
//  listen message 
private void OnEnable()
{
    Quill.message.Register(OnScore);
}

private void OnDestroy()
{
    Quill.message.Unregister(OnScore);
}

private void OnScore(MessageData data)
{
    Debug.Log(data.id);
    if(data.id == "ON_SCORE_EVENT")
    {
        //  handle score
    }
}
```
