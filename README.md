# emulator-DolphinBridge
This is a Snowflake plugin that wraps the Dolphin Wii/Gamecube emulator on Windows for Snowflake.

Currently targets 5.0-RC, expects `dolphin-5.emulatordef@5.0`. May work for Dolphin 4 as well. 

Flags to implement 
---
Out of all the settings in Dolphin, Snowflake will only expose the following settings. This is to ensure ease of use; any other settings that are critical for the game to run will have been set using Dolphin's GameINIs.

* Video Backend Selector
* Internal Resolution Selector
* HLE/LLE/Threaded LLE Audio
* Fullscreen/borderless/window toggle
* Show FPS Counter
* Enable Per Pixel Lighting
* MSAA level
* CPU Overclock 
