# DisposableBase

## Not using DisposableBase
```C#
public class Foo 
{
    public Foo () 
    {
        _texture2D = new Texture2D (bla bla bla);
        someData = new float[100];
    }
    
    // Unmanaged resource
    private Texture2D _texture2D;
    
    // Managed resource
    private float[] someData;
}

var foo = new Foo();
// Memory leaks on _texture2D!!!
foo = null;
```

## Using DisposableBase
```C#
public class Foo : DisposableBase 
{
    public Foo () 
    {
        _texture2D = new Texture2D (bla bla bla);
        someData = new float[100];
    }
    
    // Unmanaged resource
    private Texture2D _texture2D;
    
    // Managed resource
    private float[] someData;
    
    // Optional override, use it when you need it.
    protected override void DisposeUnManagedResources()
    {
        // Manaually dispose it to prevent memory leaking. 
        _texture2D?.Dispose();
        _texture2D = null;
    }
    
    // Optional override, use it when you need it.
    protected override void DisposeManagedResources()
    {
        someData = null;
    }
}

// There is no leaking, no lagginess produced by GC collector anymore.
private void SomeFunction () 
{
    // using "using" will automatically dispose foo after this function ends.
    using var foo = new Foo();
    
    // Or, manually dispose an object.
    var foo1 = new Foo();
    foo1.Dispose();
}
```