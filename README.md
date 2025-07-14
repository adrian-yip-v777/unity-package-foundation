# Visionaries 777 Foundation
It is a foundational package for all the other packages. It includes some useful shared, common features and types.

This package covers a series of foundational code or utilities across all vz777 packages. The following list is a category of these codes:
## Categories
### Extensions & Utilities
1. [Transforms](Documents~/Transforms.md) - extension functions or utilities for Transform. 
2. [Vector4](Documents~/Vector4.md) - extension functions of utilities for Vector4.
### Common Interfaces
1. [IInteractable.cs](Runtime/Interfaces/IInteractable.cs) - indicates interactable.
2. [IShowBehavior.cs](Runtime/Interfaces/IShowBehavior.cs) - indicates showable and hid-able with `UnityAction`.
### Common Structs
1. [TrsData.cs](Runtime/Structs/TrsData.cs) - stores transform data like translation, rotation, and local scale.
### Performance Optimization
1. [DisposableBase](Documents~/DisposableBase.md) - a common solution for disposing an object safely. It helps reduce the lagginess produced by the auto GC collection.
### Prevent Memory Leaking
1. [RefCounter](Documents~/UnityRefCounter.md) - manually manage Unity Objects' lifecycle to prevent memory leaking.

## Reports & Issues
Please create issues on GitHub if you want any assistance, or have any questions or advice.

Cheers. 