# Ndst
NDS toolkit for unpacking and rebuilding ROMs, along with some conversion/compression abilities.
RoadrunnerWMC's Ndspy was used for reference.

Advantages:
    * Extracted assets are easier to work with (especially with automatic conversion).
    * Patch folder allows for easier mod distribution and source control.
    * Can automate building ROMs with a build system.

Usage:
    Ndst MODE input1 (input2) output
        Modes:
            -e Extract a ROM (input1) to a folder (output).
            -p Pack a ROM folder (input1) and patch folder (input2) to a ROM (output).