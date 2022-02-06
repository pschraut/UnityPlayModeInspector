# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.6.0] - 2022-02-06
### Fixed
 - Fixed that overridden virtual methods, decorated with [PlayModeInspectorMethod], show up twice in the PlayMode Inspector window.

## [1.5.0] - 2022-01-12
### Fixed
 - Fixed that methods in base classes, decorated with [PlayModeInspectorMethod], don't show up in the PlayMode Inspector window.

## [1.4.0] - 2021-12-31
### Fixed
 - Fixed compile errors that occurred in Unity 2021.2 and newer.
 - Fixed that ExitGUIException caused the PlayMode Inspector window to display "An error occurred" until the GameObject selection was changed.

## [1.3.0] - 2021-05-14
### Changed
 - Improved message when the current selection doesn't contain a Component or ScriptableObject with a [PlayModeInspectorMethod] attribute.

### Fixed
 - Opening the PlayMode Inspector window, while an object is selected already, not correctly displays the selected object in PlayMode Inspector, without the need to deselect and then select the object again.

## [1.2.0] - 2020-08-13
### Changed
 - Removed "sealed" keyword from the PlayModeInspectorMethodAttribute class. This allows to derive from it and use your own attribute in your code. In case you want to get rid of the PlayMode Inspector package, you only need to change your own attribute and everything still compiles.
 
## [1.1.0] - 2020-07-18
### Added
 - Functionality to override the default display name shown in the PlayMode Inspector item header. Use the "displayName" property found in the PlayModeInspectorMethod attribute for this.
 - Functionality to expand/collapse an item by clicking anywhere in the header, rather than on the toggle only.

### Fixed
 - Window icon barely visible with Professional Editor Theme (Issue #1).
 - Clicking the "Add PlayMode Inspector" button to create a new window, displays the current selected object now, rather than an empty window.
 - Do not call PlayMode Inspector method on prefabs in the project. Call the method only, when the object is located in a scene. This is necessary, because all the Awake/Start/OnEnable/etc methods are only called on Components in a scene. And if we would call the PlayMode Inspector method on a prefab in the project, it most likely is in an undefined state.
 - Do not call PlayMode Inspector method on inactive components. This is necessary, because the Component perhaps hasn't initialized and thus is in an undefined state.
 - Do not call PlayMode Inspector method on Components in the prefab stage, because all the Awake/Start/OnEnable/etc methods are only called on Components in a scene. And if we would call the PlayMode Inspector method on a prefab in the project, it most likely is in an undefined state.

## [1.0.0] - 2020-05-31
 - First public release
