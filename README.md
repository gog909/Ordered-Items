# OrderedItems
#### by gog909

Risk of Rain 2 Mod.
___

## What is Ordered Items
Ordered Items is a completely clientside quality of life mod. It organizes the items in the item bar by their in game ID and their Tier. This removes the cluttered and unintuitive system implemented by the game, where the items are placed in the order that they are aqcuired. The Tier ordering can be modified in the configuration file. In addition, the mod does not affect anyone without it installed and does not require you to be host or any other players to have the mod installed for it to work.


### Install
Navigate to your Risk of Rain 2 directory. Put the `OrderedItems` folder in `BepInEx/plugins`,the `OrderedItems.dll` file should be located at `BepInEx/plugins/OrderedItems/OrderedItems.dll`. The same effect can be achieved with a mod manager.


### Uninstall
Navigate to your Risk of Rain 2 directory. Remove the `OrderedItems` folder from `BepInEx/plugins`, there should no longer be an `OrderedItems.dll` file anywhere in your plugins folder. The same effect can be achieved with a mod manager


## Configuration File
![config](https://i.imgur.com/VvgNzRq.png "Configuration file")


### Version Log

```
Version 2.1.0: Changed misleading description, updated information page and added configuration for chat debugging.

Version 2.0.0: Now Hooks UI update instead of modifying inventories, completely clientside.
               Configuration file now updates whenever an item is picked up, allowing config
               changes mid game.

Version 1.1.0: Added configuration file for tier ordering at 
               BepInEx/config/com.gog909.ordereditems.cfg.

Version 1.0.1: Fixed bug relating to UI updating one event late.

Version 1.0.0: First Release.
```

#### Contact Me
My discord is Atlas_#5809, you can find me on the Risk of Rain 2 Modding server.