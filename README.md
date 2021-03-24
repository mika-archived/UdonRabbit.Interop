# UdonRabbit Interop

UdonRabbit Interop is a package for interoperability between products.  
This is a framework that implemented on top of Udon/UdonSharp, which allows to you gimmicks implements using this to be combined with each other.

## How to use

In UdonRabbit Interop, there are two kinds of senders (triggers in SDK2) and receivers (actions in SDK2).

The sender will be raised/emitted some event such as `Interact`, `OnDrop`and others.  
This is usually a button or something similer.  

The receiver will take some actions such as play a particle effect, an animation and others.  
This usually falls under the category of gimmicks.

### The Sender

You create the gimmick as usual using `UdonSharpBehaviour`.  
And then, implement it to receive the `EventListener` in UdonRabbit Interop.  
Finally, you notify event to the receiver when an event occurs.

Sample Code (Sender) : 

```csharp
using Mochizuki.VRChat.Interop;

using UdonSharp;

using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SomeSender : UdonSharpBehaviour
{
    [SerializeField]
    private EventListener listener;

    public override void Interact()
    {
        // some logic to here...
        if (listener)
            listener.EmitInteract();
    }   
}
```

### The Receiver

You create the gimmick as usual using `UdonSharpBehaviour`.  
And then, implement it to receive the `EventListener` in UdonRabbit Interop.  
Finally, execute some logic to trigger the event reception.

Code Sample (Receiver) :

```csharp
using Mochizuki.VRChat.Interop;

using UdonSharp;

using UnityEngine;

[DefaultExecutionOrder(1)]
public class SomeSender : UdonSharpBehaviour
{
    [SerializeField]
    private EventListener listener;

    private void Update()
    {
        if (listener.IsInteract())
        {
            SomeStuff();
        }
    }
}
```

## Documentation

### For Users

https://docs.mochizuki.moe/udon-rabbit/packages/interop/users/how-to-use/

### For Udon Gimmick/Logic Developers

https://docs.mochizuki.moe/udon-rabbit/packages/interop/developers/getting-started/

## Requirements

- Unity 2018.4.20f1
- VRCSDK3 that supports Udon Networking
    - Note; I recommended to use an SDK that supports Udon Networking, but there is no problem if you do not. The same is true for UdonSharp.
- UdonSharp v0.19.6 or higher, but excludes v0.19.7
- Harmony (In many cases, this is included in UdonSharp)
- Roslyn (In many cases, this is included in UdonSharp)

## License

MIT by [@6jz](https://twitter.com/6jz)

## Third Party Notices

The Font Asset (`Assets/Mochizuki/VRChat/Interop/Fonts`) is licensed under the SIL Open Font License 1.1.  
For more information, please access [googlefonts/noto-cjk](https://github.com/googlefonts/noto-cjk/) repository and see LICENSE file.
