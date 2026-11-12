# System Architecture

## Overview

SpanishCafe uses a Unity scene containing the restaurant environment, a player character, camera objects, character interaction components, dialogue presentation, and audio objects.

## Interaction flow



flowchart TD
\[Player explores café] --> \[Player selects character]
\[Conversation starts]
\[Spanish dialogue and response options]
\[Player chooses response]
\[Conversation continues or ends]



## Scene-level components

|Component|Role|Evidence|
|-|-|-|
|`Cafe.unity`|Main café environment scene|Supplied scene archive|
|`BoZoPlayerMovement`|Player movement component|Referenced in scene|
|`CafeConversationStarter`|Character conversation entry component|Referenced in scene|
|Cinemachine camera objects|Camera follow/view configuration|Referenced in scene|
|`Cafe Music` and chatter audio objects|Environmental sound|Referenced in scene|

## 

