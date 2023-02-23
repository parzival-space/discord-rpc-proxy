# Unsupported Games

Discord has a list of Games, that do not support DiscordRPC by themselves,
with process identifiers that can be used to check if the game is running.

It is possible to use this list to detect if such a game is running.

Discords "official" API endpoint for this is:

> https://discord.com/api/applications/detectable

## Format

A Game in this list is formatted as following:

```json
{
  "bot_public": false,
  "bot_require_code_grant": true,
  "cover_image": "4f0361f7957490d61da116ef6f5ae07e",
  "description": "Cut-throat multiplayer running ... play.",
  "developers": [
    {
      "id": "521816574047420417",
      "name": "DoubleDutch Games"
    }
  ],
  "executables": [
    {
      "is_launcher": false,
      "name": "speedrunners.exe",
      "os": "win32"
    }
  ],
  "flags": 0,
  "guild_id": "233680583853604864",
  "hook": true,
  "icon": "cb48279ea90e86fb4f71c709d3236395",
  "id": "259392830932254721",
  "name": "SpeedRunners",
  "publishers": [
    {
      "id": "521816524952961034",
      "name": "tinyBuild"
    }
  ],
  "rpc_origins": ["http://discord.speedrunners.doubledutchgames.com"],
  "splash": "94e91cac9509fee1eb80a69b9503878a",
  "summary": "",
  "third_party_skus": [
    {
      "distributor": "steam",
      "id": "207140",
      "sku": "207140"
    }
  ],
  "type": 1,
  "verify_key": "541b46756f2ce27781ea9...e20"
}
```
