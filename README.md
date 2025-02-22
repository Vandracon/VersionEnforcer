# Version Enforcer
Enforces and lists missing mod versions for 7 Days to Die (7D2D)

This mod was developed against 7D2D Alpha 21

Confirmed to also work for 1.0, 1.1, 1.2 via the Rebirth overhaul mod.

***Works with In-Game hosting and Dedicated servers.***

***Must exist on Server AND Client (can't connect otherwise).***

## Getting Started

Copy `Config` `Resources` `ModInfo.xml` `VersionEnforcer.dll` and `VersionEnforcer.pdb (optional)` to a folder
named `VersionEnforcer` and place in your game install's mod location.

## Now what?

The mod will scan for all other installed mods and take note of their names and versions. When a client connects to you,
they will send you their installed list of mods and their versions. Anything that deviates from the server's data will
disconnect that user and tell them what mods are missing and/or versions that aren't matching.

### But it's blocking clients not having a server side only mod!

Edit `Resources/IgnoreList.xml` and list out the names of mods you want to skip enforcement on. Also, if a client
has a mod the server does not, there is no issue as this mod only enforces clients to satisfy the mods that 
the server has.

*IgnoreList.xml Example:*

```asxx
<Config>
    <mod name="ServerTools"/>
    <mod name="SomeOtherServerSideOnlyMod"/>
</Config>
```
