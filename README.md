# JungAnTagen-Programming-Interaction

This is a set of three artistic applications for visualizing audio as 3D stereoscopic video, and in turn, converting 3D videos into sound. These applications allow artists to create highly integrated audio-visual work, strongly aligned with the phenomenon of synaesthesia.

The basic process:
1. Convert stereo audio to a set of XYZ points in 3D space (Max).
2. Visualize the XYZ points as customized meshes and render them to stereoscopic video (Unity).
3. Convert stereoscopic video to stereo audio (Max).

Requirements:
* [Max](https://cycling74.com/) 7 or later.
  * The *VIDDLL* and *zsa.descriptors* packages must be installed via the Package Manager.
  * The trial version of Max will work if you don't intend to change the patches.
* Optional: [Unity](https://unity.com/) 2018.1 or later.
  * Unity is not needed if the standalone app is used (OSX and Linux builds provided).
  * The Unity project uses .NET 4.0 to leverage the 'dynamic' data type.

Programmed by [Half/theory](http://halftheory.com/). Commissioned by [Jung An Tagen](https://jungantagen.bandcamp.com/).
These applications were used to create parts of the 3D stereoscopic film [DYAD](http://www.sixpackfilm.com/en/catalogue/show/2553).

# 1. Stereo Audio to XYZ-OSC

Open this file in Max: */1_StereoAudio_to_XYZOSC/_StereoAudio_to_XYZOSC.maxpat*

![1_StereoAudio_to_XYZOSC](1_StereoAudio_to_XYZOSC/screenshot.png?raw=true)

Usage:
- Use the live audio input or select an audio file.
- Set the parameters for frequency and volume to be translated into 3D space.
- There are 4 meshes. Set the number of audio peaks (level of detail) and size relating to each mesh.
- Press space to start/stop the file.

# 2. XYZ-OSC to 3D Video

Open the standalone app (Unity not needed).
OSX: */2_XYZOSC_to_3DVideo/build/osx/_XYZOSC_to_3DVideo.app*
Linux: */2_XYZOSC_to_3DVideo/build/linux/_XYZOSC_to_3DVideo.x86_64*

![2_XYZOSC_to_3DVideo](2_XYZOSC_to_3DVideo/screenshot.png?raw=true)

Press escape to open the settings menu.

## Main Settings
![Main Settings](2_XYZOSC_to_3DVideo/screenshot_menu1.png?raw=true)
## Animation Settings
![Animation Settings](2_XYZOSC_to_3DVideo/screenshot_menu2.png?raw=true)
## Mesh Settings
![Mesh Settings](2_XYZOSC_to_3DVideo/screenshot_menu3.png?raw=true)

Usage:
* Select a play mode:
  * *Live* - Receive live OSC input from Max application #1.
  * *Record points* - Record all OSC points from Max to Unity.
  * *Record animation* - Record all real-time changes to Mesh Settings.
  * *Play* - Playback the current animation in progress.
  * *Render* - Render the current animation to single channel video, 3D side-by-side, or both.
* Press space to start/stop the animation.
* Click the "x" on the top right to quit.

Notes:
- If you are unable to open the app, check folders for .sh files and run these commands in Terminal.
- If any of your meshes use 'No Clear Time' or a very long Clear Time you must stop the render manually.
- It is not recommended to delete the 'testObject' animation.
- Applications #1 and #2 must run on the same machine for rendered videos to contain sound.

## Unity Editor
To run the app in Unity Editor open this project in Unity: */2_XYZOSC_to_3DVideo/_XYZOSC_to_3DVideo/*

# 3. 3D Video to Stereo Audio

Open this file in Max: */3_3DVideo_to_StereoAudio/_3DVideo_to_StereoAudio.maxpat*

![3_3DVideo_to_StereoAudio](3_3DVideo_to_StereoAudio/screenshot.png?raw=true)

Usage:
- Select a video file.
- If the video is 3D select Side-By-Side (works with both).
- Set the parameters for resolution and color threshold.
- Set the frequency limits and smoothing for each RGB channel.
- Press space to start/stop the file.
