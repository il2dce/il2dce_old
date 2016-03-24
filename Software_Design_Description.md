# Introduction #

This document contains a overview and a description of the software components.

# Details #

## Overview ##

![https://docs.google.com/drawings/pub?id=1Dk3_m9XILOTuZTa_7jI6pWIUrrTEsYu1IeEyvNRqN3Y&w=1342&h=737&nonsense=something_that_ends_with.png](https://docs.google.com/drawings/pub?id=1Dk3_m9XILOTuZTa_7jI6pWIUrrTEsYu1IeEyvNRqN3Y&w=1342&h=737&nonsense=something_that_ends_with.png)

## Components ##

### `IPersistentWorld` ###

The `IPersistentWorld` represents the current campaign state. The state contains different unit types (i.e. `AirUnit`, `GroundUnit`, `NavalUnit` and `SupplyUnit`). The `IPersistentWorld` also contains a list of `IHeadquarters`. Any change of the campaign state has to be propagated to the contained `IHeadquarters`, which then can interact with the `IPersistentWorld` to react on the change.

The `IPersistentWorld` provides a interface to receive a `ICommand` for the contained units. The `ICommand` will then be executed by the unit in the persistent world.

There are two different implementations for the `IPersistentWorld`. The `Mission` represents the real-time progress the campaign. The real-time progress is provided by the engine of _IL-2 Sturmovik: Cliffs of Dover_. The `Mission` listens to the events that are fired by the _IL-2 Sturmovik: Cliffs of Dover_ and applies them on the campaign state. To execute a `ICommand` it will be translated into calls to the engine.

The `Simulator` represents the time-compressed progress of the campaign that is used to skip time spans that should not progress in real-time. The time-compressed progress is not provided by the engine of _IL-2 Sturmovik: Cliffs of Dover_. Hence a simplified simulation the `ICommand` calls must be used that will change the campaign state appropriately.

### `IHeadquarters` ###

## Component details ##

See source code documentation.