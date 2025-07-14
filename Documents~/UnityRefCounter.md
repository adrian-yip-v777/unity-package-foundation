# UnityRefCounter
Unity's Objects are not managed by .NET. Therefore, GC collector will not work for them.

`UnityRefCounter`'s designed for managing runtime resources like `Texture2D` or `RenderTexture` when they have been passing around through an event bus.

Let's say we keep creating a `Texture2D` object or `RenderTexture` during runtime. It is still manageable if we know where the process will end, then we manually release the resource in case of memory leaking.
```C#
while (true)
{
    myTexture2D = new Texture2D(bla bla bla);

    // Release the resource manually after use to prevent memory leaking.
    Object.Destroy(myTexture2D);
    myTexture2D = null;
}
```
However, if we do not know where or how long this texture will be used elsewhere, it becomes unmanageable and easily causing memory leaking.
```C#
while (true)
{
    myTexture2D = new Texture2D(bla bla bla);
    eventBus.Publish (new FooEvent (myTexture2D));
    
    // Still safe!
    // With all synchronized handlers, the publish function will only ends if all handlers are executed.
    Object.Destroy(myTexture2D);
    myTexture2D = null;
    
    // Memory leak!
    // If anywhere else uses this resource in a coroutine or async function
    // An error will present because the resource has been destoryed.
}
```
That's how `UnityRefCounter` comes into play.
## Solution (Usage Example)
```C#
var myTexture2D = new Texture2D (bla bla bla);

// Create a wrapper for myTexture2D
var myTexture2DRef = new UnityRefCounter<Texture2D>(myTexture2D);

// Publish the event with the wrapper instead.
eventBus.Publish (new FooEvent(myTexture2DRef));

// Say goodbye to your myTexture2D, because it's unmanageable now after publishing the event.
```
In any event handler:
```C#
// Get the ref counter from the event data.
var myTexture2DRefCounter = eventData.MyTexture2DRef;

// Register to use the reference
// It increments the ref count, returns you the reference and the complete action.
var (myTexture2D, completeRefUse) = myTexture2DRefCounter.RegisterToUse();

// Doing stuff to the myTexture2D

// Invoke the complete action to reduce the ref count.
// It will possibly release the resource if there is no ref count left.
completeRefUse.Invoke();
```
## Difference Lifetime
It is IMPORTANT to know that the ref count checking will delay `1 frame` by default, as it preserves a piece of time for any functions to register to use the reference in case of any function uses and releases it right away.
You can also set a different lifetime for a `RefCounter`.
```C#
var myTexture2D = new Texture2D (bla bla bla);
var surviveFor2Frames = new UnityRefCounter<Texture2D>(myTexture2D, 2);
var surviveFor5Frames = new UnityRefCounter<Texture2D>(myTexture2D, lifeTime: 5);
var surviveFor10Frames = new UnityRefCounter<Texture2D>(myTexture2D, lifeTime: 10);
```
I guess it is pretty much like the [Memory Allocator](https://docs.unity3d.com/Packages/com.unity.collections@2.6/manual/allocator-overview.html) of `Unity's JobSystem` if you are familiar with it, where the `Allocator.TempJob` lives for 4 frames before it gets cleaned up.
# RefCounter
`RefCounter` is the base class of `UnityRefCounter`, you can also use it for other objects which are not managed by .NET.

I can't think of any at the moment. Please feel free to provide any examples of it.