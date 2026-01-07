#Requires AutoHotkey v1.1.0+
;==============================================================
; windowFromPoint — Gets the window handle at the given screen coordinates
;
; GitHub: https://github.com/SevenKeyboard/window-from-point
; Author: SevenKeyboard Ltd. (2026)
; License: The Unlicense
;
; Documentation / References:
;   https://www.autohotkey.com/boards/viewtopic.php?t=36281
;==============================================================
windowFromPoint(x, y)    {
    return dllCall("User32.dll\WindowFromPoint", "Int64",(x & 0xFFFFFFFF) | (y << 32), "Ptr")
}