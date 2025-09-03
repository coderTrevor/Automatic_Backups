# Automatic Backups For Schedule I
Mod for the game `Schedule I` that automatically backs-up your save files every time you save.  
  
Allows you to recover from disaster!
  

There are builds for both the main (IL2Cpp) and alternate (Mono) branches of the game.
  
# Installation
1. Requires [MelonLoader](https://melonwiki.xyz/), so install that if you haven't already.
1. Download the [latest release](https://github.com/coderTrevor/Automatic_Backups/releases/latest). Extract the .zip and copy the appropriate .dll file to the `Mods` folder in your Schedule I installation.
	1. If you're on the main branch (haven't changed to "alternate" in Steam) you'll want the IL2Cpp version.
	1. If you're on the alternate branch, you'll want the Mono version.

# Usage
Every time you save your game, this mod exports your save to a timestamped .zip file.
These files can be found alongside your other saves in a folder called `Backups`. To load a previous save, just
select `Import` on the `Continue` screen in Schedule I and navigate to that folder.

Notes:
- Backups are local only and not synced with the Steam cloud.
- Untested in Mac or Linux

# Settings (Limit Number of Backups)
You can change the settings for this mod through the settings menu when you first load the game up. Just click the `Backups` tab.

![Settings Screenshot](/PublicRelease/Images/Settings.jpg)
  
You can configure the mod to automatically delete the oldest backups once a certain limit has been reached.
You can turn this feature on or off and the available range is between 25 - 250 in increments of 5. I recommend leaving this setting off or setting the limit very high; it's not always obvious right-away if the game has bugged your save.

When you first load a save slot, you can check the MelonLoader console to see how much disk space your backups are taking up. It's pretty negligible in my experience.
![MelonLoader Log](/PublicRelease/Images/MelonLog.png)

## Like it?
Consider giving this mod a thumbs-up on [Thunderstore](https://thunderstore.io/c/schedule-i/p/coderTrevor/Automatic_Backups/) or [Nexus](https://www.nexusmods.com/schedule1/mods/1168). That will help other players find it and hopefully prevent more save-related disasters! Most importantly, it helps my ego.
  
## Acknowledgements
Mod boilerplate was created with the [MelonLoader.VSWizard](https://github.com/TrevTV/MelonLoader.VSWizard) wizard by TrevTV with additions from the [S1MONO_IL2CPP_Template](https://github.com/weedeej/S1MONO_IL2CPP_Template) template by weedeej.

Thanks to [ifBars](https://github.com/ifBars) who helped me diagnose an Il2Cpp-specific issue.