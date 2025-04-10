# Visionaries 777 Foundation (forked from [AYip's Foundation](https://github.com/adrianyip-1222/ayip-package-foundation))
It is a foundational package for all the other packages. It includes some useful shared, common features and types.

## Features
### RefCount & UnityRefCount
#### Why we need this?
RefCount type is designed for managing runtime resources like Texture2D or RenderTexture when they have been passing around through an event bus.

Let's say we keep creating a Texture2D object or RenderTexture during runtime. It is still manageable if we know where is the process ends, then we manually release the resource.
```C#
while (true)
{
    myTexture2D = new Texture2D(bla bla bla);

    // Release the resource manually after use to prevent memory leaking.
    Object.Destroy(myTexture2D);
    myTexture2D = null;
}
```

But when we do not know where or how long this texture will be used elsewhere, it becomes unmanageable and easily causing memory leak.
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
    // Assume that one of the subscribed handlers is asynchronous or a Unity's coroutine
    // It waits for unknown number of frames then accessing the resource, we will then have two outcomes:
    
    // 1. Releasing the resource here will throw an exception after a few frames when it accesses the resource. 
    // 2. Not releasing the resource here will cause memory leaking.
}
```
That's how RefCount comes into play.
#### Solution (Usage Example)
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
// It will possible release the resource if there is no ref count left.
completeRefUse.Invoke();
```
#### Difference Lifetime
It is IMPORTANT to know that the ref count checking will delay `1 frame` by default, as it preserves a piece of time for any functions to register to use the reference in case of any function uses and releases it right away.
You can also set a different lifetime for a RefCounter.
```C#
var myTexture2D = new Texture2D (bla bla bla);
var ref1 = new UnityRefCounter<Texture2D>(myTexture2D, 2);
var ref2 = new UnityRefCounter<Texture2D>(myTexture2D, lifeTime: 5);
var ref3 = new UnityRefCounter<Texture2D>(myTexture2D, lifeTime: 10);
```
I guess it is pretty much like the [Memory Allocator](https://docs.unity3d.com/Packages/com.unity.collections@2.6/manual/allocator-overview.html) of `Unity's JobSystem` if you are familiar with it, where the `Allocator.TempJob` lives for 4 frames before it gets cleaned up.  
