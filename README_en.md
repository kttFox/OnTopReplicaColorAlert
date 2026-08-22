# OnTopReplicaColorAlert

[日本語](README.md)

**A window monitor for Windows: keeps a live, always-on-top replica of any window and raises a color alert when a chosen color shows up in it.**

Pick any window of the system and OnTopReplicaColorAlert shows an always up-to-date clone of it, always-on-top.
Its main purpose is unattended monitoring: watch a target window for a specific color (a warning light, an error banner, a status LED in a control panel) and get an audible alarm the moment it appears — or disappears.

OnTopReplicaColorAlert started as a fork of [OnTopReplica](https://github.com/LorenzCK/OnTopReplica) and has since diverged into its own application: the feature set is centered on color-alert monitoring, and several features of the original have been removed (see *Removed features*). See *Origin and license* at the bottom.

**📢 Features:**

* Clone any of your windows and keep it *always-on-top* while working with other windows,
* Color-alert feature lets you monitor a target window for a chosen color; open the **Color Alert** side panel, select a color and check "Enable Color Detection" (settings take effect immediately). If the monitored window contains the color, the alarm will sound for 3 seconds and an entry is written to the log.
* Select a subregion of the cloned window, which:
  * Can use relative coordinates from the target window's borders.
* Adjustable opacity (10% steps),
* "Click-through": makes the replica ignore any mouse interaction (turns **OnTopReplicaColorAlert** into an overlay if set together with partial opacity).

**🖱️ Mouse operations on a panel:**

| Operation | Action |
| --- | --- |
| Right click | Open the context menu |
| Double click | Pause / resume color alerts on **all** panels |
| Middle click | Enable / disable color alerts on **that** panel |
| Ctrl + drag | Select a subregion of the cloned window |

## Own features

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

* Application settings are now saved next to the executable (`OnTopReplicaColorAlert.Settings.xml`) instead of the per-user `user.config`, and are written immediately on every change.
* Added a Japanese locale (translation resources).
* Removed legacy settings such as "restore last window" and "restore previous position and size", which have been consolidated into the automatic layout restore described above.
* Updated the target framework to .NET Framework 4.8.

## Removed features

Features of the original OnTopReplica that this application no longer has:

* Fullscreen mode (and its dedicated context menu),
* "Click forwarding" (forwarding clicks on the replica to the target window),
* Global hotkeys (show/hide, clone current window) and the hotkey settings,
* The "Resize" menu (fit original / half / quarter) and resizing by mouse wheel — resize by dragging the window border instead,
* Position lock on a screen corner,
* "Group switch" mode,
* The auto-update feature and the MSI installer (the application is portable).

## Requirements

* Microsoft Windows Vista or greater (the application makes use of native DWM Thumbnails to create replicas),
* Microsoft .NET Framework 4.8.
* Desktop Composition (a.k.a. Windows *Aero*) enabled.

## Upgrading from an older version

To carry over the settings and panel layout of an older version (a fork based on OnTopReplica), copy these two files from the old folder next to this application's executable before starting it:

| Old file | Contents |
| --- | --- |
| `OnTopReplica.Settings.xml` | Application settings (language, indicator, auto-hide, …) |
| `PanelLayout v3.txt` | Panel layout (target window, position, size, monitored region, color-alert settings) |

They are read on startup and migrated to the current file names (`OnTopReplicaColorAlert.Settings.xml` and `PanelLayout.txt`). Once the migration succeeds, the two original files are deleted — the new file is always written first, so the originals stay in place if the migration fails.

Settings of removed features (position lock, hotkeys, click forwarding, …) are not carried over.

## Origin and license

OnTopReplicaColorAlert is derived from [OnTopReplica](https://github.com/LorenzCK/OnTopReplica) by Lorenz Cuno Klopfenstein and is distributed under the **Microsoft Reciprocal License (Ms-RL)** — see [LICENSE](LICENSE). Portions of the source code are Copyright © Lorenz Cuno Klopfenstein.

This is an unofficial application and is not affiliated with, nor endorsed by, the authors of OnTopReplica.
