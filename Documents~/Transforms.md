# Transform Extensions

## GetScenePath
Returns the scene path of a transform.
```C#
// returns a string
var myScenePath = myTransform.GetScenePath();
Debug.Log (myScenePath);
```
## GetWorldTransformFromOtherParent
Return a `TrsData`(world transform data) of a transform being a child of another parent without setting the parent. It helps when you need to calculate the correct transform when you can't set the parent of the transform for any reasons.
```C#
private Transform _targetParent;

private void SomeFunction () 
{
    var trsData = transform.GetWorldTransformFromOtherParent(_targetParent);
    // TrsData should be the world transform data of "transform" being a child of "_targetParent".
    Debug.Log (trsData);
}
```

# Transform Utilities
## GetLocalScaleFromOtherParent
Calculate the local scale of a transform from parent A to parent B
```C#
private Transform _targetParent;

private void SomeFunction () 
{
    var scale = TransformUtils.GetLocalScaleFromOtherParent(transform.parent, _targetParent);
    // Scale will be the local scale of the _targetParent.
    // Please note that non-uniform scale might not be working at the moment.
    Debug.Log (scale);
}
```