# Auto Weekly Cap

Auto Weekly Cap (AWC) is a plugin for Dalamud that will use [Lifestream](https://github.com/NightmareXIV/Lifestream), [AutoDuty](https://github.com/erdelf/AutoDuty) and [vnavmesh](https://github.com/awgil/ffxiv_navmesh/tree/master) to automatically switch between your characters and cap their weekly tomestones.

## Features

* Automatically cap all your characters with a single click
* Automatic materia extraction from gear between duty runs
* Repairing gear below a configurable durability threshold
  + Will automatically repair at an NPC when self-repair using crafters is not possible
* Automatically spend uncapped tomestones on crafter materials or relic upgrades
* Unlimited mode, to continue running duties after all characters have reached the weekly tomestone cap
* Safezone support, so AutoDuty only starts from designated safe locations such as your house, apartment, etc
* Disconnect recovery, allowing the plugin to resume automatically after a disconnection or network interruption
* Per-character configuration, including preferred job, which items to spend tomestones on, and more
* Support for optional third-party plugins, including AutoRetainer, Deliveroo, Notification Master, and more

## Installation

To use Auto Weekly Cap you'll need to add the plugin repository to Dalamud by following the steps below:

* Open Dalamud's settings (`/xlsettings`)
* Click on the Experimental tab
* Scroll Down to Custom Plugin Repositories
* Add the following URL, then click on the Plus next to it
  + https://dalamud-plugins.senither.com/plugin/AutoWeeklyCap.json
* Click on the Save icon in the bottom right corner

Once you've added the repository to Dalamud, you can now install the plugin by searching for "Auto Weekly Cap" or "Senither" in the Dalamud plugin list.

## How to use

Once you've installed the plugin, you can configure it by opening the Dalamud plugins list, finding the plugin and clicking on the settings button, alternatively you can open the main character window by typing `/awc` in the chat and then clicking on the cog icon in the top right.

From there you can customize the general settings, plugin themes, UI elements, network options, and much more. You can also use the tab selector to configure your characters, runner options, and stop actions.

Characters are automatically registered with the plugin when you login to them while the plugin is enabled, alternatively you can import characters directly from AutoRetainer from the Characters settings tab.

If you're unsure about any option within the control panel, you can hover over the question mark (?) to get hints and descriptions of what each individual option does.

> **Note:** The plugin requires [Lifestream](https://github.com/NightmareXIV/Lifestream), [AutoDuty](https://github.com/erdelf/AutoDuty), and [vnavmesh](https://github.com/awgil/ffxiv_navmesh/tree/master) to be installed and enabled.

## License

Auto Weekly Cap is open-sourced software licensed under the [AGPL-3.0 license](LICENSE.md).

## Third Party Licenses

Auto Weekly Cap relies on the following projects:

| Name | License  |
|:---|:---|
| [Dalamud](https://github.com/goatcorp/Dalamud) | [GNU Affero General Public License v3.0](https://github.com/goatcorp/Dalamud/blob/master/LICENSE) |
| [ECommons](https://github.com/NightmareXIV/ECommons) | [MIT License](https://github.com/NightmareXIV/ECommons/blob/master/LICENSE.md) |
