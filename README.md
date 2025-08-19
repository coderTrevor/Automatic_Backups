# Automatic Backups For Schedule I
Mod for the game `Schedule I` that automatically backs-up your save files every time you save.  
  
Allows you to recover from disaster!
  

There are builds for both the main (mono) and alternate (il2cpp) branches of the game.
  
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
  
## Acknowledgements
Mod boilerplate was created with the [MelonLoader.VSWizard](https://github.com/TrevTV/MelonLoader.VSWizard) wizard by TrevTV with additions from the [S1MONO_IL2CPP_Template](https://github.com/weedeej/S1MONO_IL2CPP_Template) template by weedeej.