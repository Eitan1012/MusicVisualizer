# MusicVisualizer
A music visualizer application for windows which reacts to the music being played on the system and shows its title artist and album cover.

## Motivation
I was looking for a fullscreen expirence that I could display on my computer while listening to music, combining real time audio visualization with information about the currently playing song and will work with any music player.

## Features

- Real time system audio capture.
- 64 band circular spectrum visualization.
- Dynamic bass reactive ring.
- Smooth animations whit spring physics.
- Rotating CD displaying album cover.
- Automatic track title, artist and album cover detection.
- Adptive color scheme based on album cover.
- Fullscreen mode.
  ## How it works

MusicVisualizer captures system audio using NAudio and WASAPI. The audio is analyzed using FFT with Hamming window and divided into 64 frequency bands.
The visualizer reacts to bass, frequency energy and volume. Smoothing spring physics and inertia make the animations feel more netural.
Song information are retrieved through Windows Media Session.
I also built a color extraction algorithm that finds the dominant color of the album cover and uses it to adapt the color scheme of the visualizer.

## Technologies
- C# .NET 8
- Windows Forms
- NAudio
- GDI+
- Windows media session
- TagLibSharp

## Download and Usage
[Download MusicVisualizer v1.0.0](https://github.com/Eitan1012/MusicVisualizer/releases/download/v1.0.0/MusicVisualizer.exe)

Available for Windows 10 and 11.

1. Download and run MusicVisualizer.exe
2. Play music on your favorite music player (Spotify, YouTube, etc.).
3. Enjoy the visualizer!


### Keyboard Shortcuts
- F11 - Toggle fullscreen mode
- Esc - Exit fullscreen mode

## Screenshots

![MusicVisualizer Screenshot 1](screenshots/screenshot1.png)

![MusicVisualizer Screenshot 2](screenshots/screenshot2.png)

## Building from Source

## Build From Source

Make sure you have the .NET 8 SDK installed on Windows.

```bash
git clone https://github.com/Eitan1012/MusicVisualizer.git
cd MusicVisualizer
dotnet restore
dotnet run
```