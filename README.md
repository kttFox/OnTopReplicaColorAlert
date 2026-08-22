# OnTopReplica

**A real-time always-on-top "replica" of a window of your choice, for Windows Vista, 7, 8, or 10.**

This simple utility application shows a blank always-on-top window by default.
Users can pick any other window of the system to have an always up-to-date clone of the target window shown always-on-top.
Very useful for monitoring background processes, wrangling with complex multi-window games or tools, watching YouTube videos while working, and so on.

**📢 Features:**

* Clone any of your windows and keep it *always-on-top* while working with other windows,
* Color-alert feature lets you monitor a target window for a chosen color; open the **Color Alert** side panel, select a color and check "Enable Color Detection" (settings take effect immediately). If the monitored window contains the color, the alarm will sound for 3 seconds and an entry is written to the log.
* Select a subregion of the cloned window, which:
  * Can use relative coordinates from the target window's borders.
* Auto-resizing (fit the original window, half, quarter),
* Position lock on any corner of your screen,
* Adjustable opacity (10% steps),
* "Click-through": makes the replica ignore any mouse interaction (turns **OnTopReplica** into an overlay if set together with partial opacity),
* ~~"Group switch"-mode automatically switches through a group of windows while you use them.~~

## Fork changes

### Multi-panel

* Use **"Add Panel"** in the right-click menu to open any number of panels showing different regions of the same target window.
  * The target window is always kept in sync with the main (primary) panel.
  * Monitored region, color-alert settings, opacity, and window frame visibility are independent per panel.

### Automatic layout save & restore

* The panel layout (target window, position, size, frame visibility, monitored region, color-alert settings) is **saved immediately on every change** and all panels are restored at the next startup (`PanelLayout.txt` in the same folder as the executable).
* A resident watcher monitors the target window even if it is not yet running, and **automatically reconnects when it starts later** (the same applies if the target exits and restarts during a session).

### Color-alert enhancements

* Detection colors: in addition to the red, orange, and gray categories, you can specify a **custom color**. A sampling feature lets you pick a color by clicking any point on the screen (a cursor-following color preview is shown while sampling).
* Added an option to sound the alarm when the detection color disappears.
  You can set the number of consecutive misses required, to avoid false positives.
* Color-alert monitoring can be **paused automatically while the target window is lost** (e.g. minimized or not yet running) and resumed when it comes back, avoiding spurious alarms (setting "Pause color alerts when the window is lost").
* Alarm sounds: in addition to the bundled WAV/MP3 files (just drop files into the `Sounds` folder to add them to the list), some Windows system sounds can be selected.
* Detection runs independently per panel.
* Added the ability to send a single key to the monitored window when the alarm fires.
* A **minimum detection pixel count** can be set per panel: the alarm only fires when at least the specified number of matching pixels is found. The current per-category detection counts are shown in real time on the panel (e.g. "Detecting: Red:12 Gray:340"), and a count-monitoring mode keeps updating the counts even while detection is disabled or paused (without firing alarms).
* The paused state of color-alert monitoring is saved in the panel layout and restored at the next startup.
* A small **status indicator (●)** is shown at the top-right of the preview so you can see at a glance whether color-alert monitoring is running (a different color is used while paused). Visibility, size, and colors are configurable in the settings panel.

### Auto-hide

* Added an option to **show/hide all panels in sync with the target window** (setting "Show/hide in sync with the cloned window", disabled by default): panels are hidden (without stealing focus) while the target window is inactive and shown again when it becomes active. This is also integrated with the target window's exit/start, so panels are hidden when the target exits and restored when it starts again. The taskbar button remains visible while hidden, so you can also restore the panels manually.

### Other

* Removed the auto-update feature.
* Application settings are now saved next to the executable (`OnTopReplica.Settings.xml`) instead of the per-user `user.config`, and are written immediately on every change.
* Removed the MSI installer and made the application **portable** (just place the executable files and run).
* Added a Japanese locale (translation resources).
* Removed legacy settings such as "restore last window" and "restore previous position and size", which have been consolidated into the automatic layout restore described above.
* Updated the target framework to .NET Framework 4.8.

## Requirements

* Microsoft Windows Vista or greater (the application makes use of native DWM Thumbnails to create replicas),
* Microsoft .NET Framework 4.8.
* Desktop Composition (a.k.a. Windows *Aero*) enabled.

## Logging & Troubleshooting

If the executable does not start when you double-click it, check for a log file in the same folder as the executable:

```
<application folder>\lastrun.log.txt
```

The program records startup details (version, command line, OS/CLR, current directory) and any exceptions. A crash dump (`OnTopReplica-dump-*.txt`) is written to the desktop if an unhandled exception occurs.

No log file at all usually means the process failed before the CLR loaded; verify that .NET Framework 4.8 (or later) is installed.

## Installation

This fork is a **portable version**; there is no installer. Place the built executable files in any folder and run `OnTopReplica.exe`. See [BUILD_GUIDE.md](BUILD_GUIDE.md) and `build.ps1` for build instructions.

The original version (MSI installer) is available from the [LorenzCK/OnTopReplica releases](https://github.com/LorenzCK/OnTopReplica/releases).

## Contributions

…are very welcome. Fork away! 🍽️

Submitting [issues](https://github.com/kttFox/OnTopReplica/issues) and other feedback is also appreciated.
