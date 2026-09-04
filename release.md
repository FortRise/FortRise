# 5.4.0
+ Added descriptions for all option buttons in settings. #20
+ Added OptionDescriptionBox. #20
+ Added SpriteFont.WrapText. #20
+ Added ReadOnlySpan overload for SpriteFont.MeasureString.
+ Added OptionsButton.Description as a property extension. #20
+ Added UIAlert.
+ Added Events.Level.LevelEntered events.
+ Added Registry.Towers.RegisterTowerType and towerTypeData.xml.
+ Added Registry.Towers.RegisteredDarkWorldTowers, RegisteredQuestTowers, Registered TrialsTowers, RegisteredVersusTowers.
+ Added Registry.Towers.RegisteredDarkWorldTowerSets, Registry.Towers.RegisteredQuestTowerSets, Registry.Towers.RegisteredVersusTowerSets, Registry.Towers.RegisteredTrialsTowerSets.
+ Added LevelEntityConfiguration.IsHazard.
+ Added multiple node support for Editor.
+ Added modifiable attribute support for Editor.
+ Added Dummy, Orb, FloorMiasma, Cobwebs, BGCrystal, RainDrops, GhostShipWindow, SnowClump, BGMushroom, BGBigMushroom, KingIntro, and PrismBlock to the actor layer on Editor.
+ Added Fixed Time Step options into settings.
+ Added custom co-op gamemode API.

+ Replaced GameStats fields that are directly connected to Steamworks into a properties for compatiblity.
+ All events inside of context.Events are now restructured, all previous events are deprecated.
+ Content image path now requires on an element type tring.

+ Fixed option buttons being out of view. #21
+ Fixed workshop level cannot be loaded on Windows due to System.Windows.Forms missing.
+ Fixed Arrows HUD crashes when a custom arrows does not supply HUD texture.
+ Fixed empty saves crashes on startup.
+ Fixed levels being unsorted in version 1.3.3.1 on Linux.
