# window-from-point

Gets the native window handle at signed 32-bit screen coordinates on Windows using .NET 10.

```csharp
nint window = SevenKeyboard.WindowFromPoint.Get(x, y);
```

Returns zero when no window is found. Preserves native WindowFromPoint behavior,
including hidden/disabled window exclusion and static text control handling.
Coordinates use the calling thread's Windows DPI context.

Version 1.0.0 (initial C# version). Supports x64, x86 and NativeAOT.
