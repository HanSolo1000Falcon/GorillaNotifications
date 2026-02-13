# GorillaNotifications

A simple Gorilla Tag mod library meant to centralise notification handling that displays notifications both in world space (VR HUD) and on the PC screen with automatic text wrapping and timed removal.

---

## Overview

GorillaNotifications provides a simple API for sending temporary notifications to players.  
It supports:

- World-space UI attached to the player’s head (VR)
- PC screen overlay for desktop users
- Automatic text wrapping based on screen width
- Timed removal using coroutines
- Runtime updating of notifications

---

## Features

- Simple static API for sending notifications  
- Dual rendering targets (VR + PC)  
- Automatic cleanup after duration expires  
- Update existing notifications instead of re-sending  
- AssetBundle-based UI loading  

---

## Requirements

- BepInEx
- Gorilla Tag

---

## Installation

1. Put the `GorillaNotifications.dll` file in your `BepInEx/plugins` folder.
2. Reference it in you project as a dependency.

---

## Quick Start

The very basics is this:

```csharp
using GorillaNotifications;

NotificationController.SendNotification("System", "Notification system initialized", 3f);
```

Calling `NotificationController`'s `SendNotification` renders a notification on the users screen.<br></br>This is all you need to know for very surface level usages, for a more in depth explanation read the "API" section.

---

## API

### SendNotification

Signature:

```csharp
public static NotificationEntry SendNotification(string source, string notification, float duration, FontType fontType)
```

Creates and displays a new notification.

Parameters:

- source (string): Displayed source label  
- notification (string): Message content  
- duration (float): Time in seconds before auto-removal
- fontType (FontType): The font you want your notification to be

Returns:
A NotificationEntry instance.

---

### NotificationEntry

Represents a single active notification.

Properties:

- string Source
- string Notification  
- float Duration  

Update Methods:

```csharp
entry.UpdateNotification("New Source", "Updated message", 5f);  
entry.UpdateNotification("Updated message");  
entry.UpdateDuration(10f);  
entry.UpdateSource("System");
```

Remove:

```csharp
entry.RemoveNotification();
```

---

## Example: Persistent Status Notification

```csharp
NotificationEntry status = NotificationController.SendNotification("Loader", "Loading assets...", 999f);  
status.UpdateNotification("Loader", "Loading complete", 2f);
```
