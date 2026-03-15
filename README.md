# Lab Frame 2023 - Ganglion Plugin
> com.xrlab/labframe_eyetrack

## Vive Wave Setup

This package keeps Wave dependencies optional. If you are targeting Vive Focus devices,
install Wave support from the Unity menu:

1. Open Unity Editor.
2. Click `LabFrame2023/Install Vive Wave Support`.
3. Wait for package installation to finish in Console.

The installer will:

- Add the VIVE scoped registry (`https://npm-registry.vive.com/`) to `Packages/manifest.json`.
- Install `com.htc.upm.wave.xrsdk@6.2.0-r.9`.
- Install `com.htc.upm.wave.native@6.2.0-r.9`.
- Install `com.htc.upm.wave.essence@6.2.0-r.9`.

`USE_VIVE_ANDROID` is enabled automatically through asmdef version defines when
`com.htc.upm.wave.essence` is present.

## CHANGELOG

### 1.0.0
- Supports PICO eyetrack (works on PICO Interaction SDK V212)
### 1.1.1
- Add new supports for htc vive focus3 (wave sdk is 6.1.0-r.8)
### 1.1.3
- 處理引用上的問題

### 1.1.4
- 重要：更新的時候要記得所有的package json 的version 要相同
- 處理UPM跟NPM的偶合