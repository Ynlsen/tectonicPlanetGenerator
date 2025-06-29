# Tectonic Planet Generator

This repository contains the source code for my tectonic plate based planet generator, built in Godot with C#. The generator creates 3D planets with simulated tectonic plates and calculates stress at their borders.

The project is heavily work in progress and currently has many missing features.

## Features

- **Planet Mesh**: Generates a sphere mesh from six faces, each with customizable resolution and shape.
- **Tectonic Plate Simulation**: Simulates large and small tectonic plates, their boundaries, and movement.
- **Coloring**: Uses plate data and tectonic stress to determine vertex color for easy visualization.
- **Editor Integration**: All settings are exposed in the Godot editor, and the scripts are coded as tools with build buttons integrated in the editor for rapid iterating.

## Planned Features

- **Oceanic/Continental Plates**: Differentiate between plate types to determine elevation and apply different interactions at the borders.
- **Mountain Noise**: Use a different mountain like noise that gradually blends in depending on the stress value.
- **Rivers**: Generate rivers that flow down mountains and get wider.
- **Biomes**: Determine biomes based on latitude and elevation to color the terrain.
