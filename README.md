# Ndst
NDS toolkit for unpacking and rebuilding ROMs, along with some conversion/compression abilities.
RoadrunnerWMC's Ndspy was used for reference.

Advantages:
* Extracted assets are easier to work with (especially with automatic conversion).
* Patch folder allows for easier mod distribution and source control.
* Can automate building ROMs with a build system.

Extract a ROM with automatically converting files enabled, then rebuild:
```
./Ndst -e ROM.nds Conversions ExtractedROM
./Ndst -n ExtractedROM ROMPatches Conversions NewROM.nds
ninja
```

Extract a ROM without converting files, then rebuild:
```
./Ndst -e ROM.nds ExtractedROM
./Ndst -n ExtractedROM ROMPatches NewROM.nds
ninja
```

Pack a ROM folder:
```
./Ndst -p ExtractedROM NewROM.nds
```

Usage:
    Ndst MODE input1 (input2) (input3) output
        Modes:
            -e Extract a ROM (input1) with optional conversion folder (input2) to a folder (output).
            -n Generate a ninja build system for ROM folder (input1), patch folder (input2), and optional conversion folder (input3) to build ROM (output).
            -p Pack a ROM folder (input1) to a ROM (output).
            -t Use file conversion method (input1) to convert file (input2) and save as (output).
            -c Copy file (input1) to (output).