# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
 - Added CrestronSPlus Project
 - Added SPlusPartitionSensorDevice
 - Added CalendarManager property to ICommercialRoom
 - Added CalendarManager Callbacks to AbstractCommercialRoom.
 - Added RoomType property to ICommercialRoom and AbstractCommercialRoom
 - Added OperationalHours to ICommercialRoom and AbstractCommercialRoom
 - Added CallRatingManager to ICommercialRoom and AbstractCommercailRoom

## [15.3.0] - 2020-09-28
### Added
 - Added room activity for awake/idle state

## [15.2.2] - 2020-09-24
### Changed
 - Fixed room activity priorities
 - Fixed a bug where default room activities were not being initialized

## [15.2.1] - 2020-08-24
### Changed
 - Logging when a room is unable to register conference and volume points with the conference manager

## [15.2.0] - 2020-08-13
### Added
 - Added telemetry for room IsAwake features
 - Added telemetry for room mute state

## [15.1.1] - 2020-08-11
### Changed
 - Fixed a bug where Volume control would disable when in a conference call and hide after meeting was ended. 

## [15.1.0] - 2020-07-14
### Added
 - Added activity for commercial room call state

### Removed
 - Removed commercial room Awake activity

## [15.0.0] - 2020-06-19
### Added
 - Added SeatCount to commercial rooms
 - Added telemetry for pushing a list of the originators contained in a room
 - Added telemetry for commercial room privacy mute, volume, active conference device, call-in information
 - Added telemetry for room bookings
 - Moved occupancy features from ICD.Connect.Misc
 - Commercial rooms have occupancy points
 - Commercial rooms have an occupancy state

### Changed
 - MockPartitionDevice now implements IMockDevice
 - Using new logging context
 - Fixed a bug where conference points were not always being registered with the conference manager

## [14.3.0] - 2020-03-20
### Added
 - Added an ICommercialRoom extensions class which has a method for determing the contextual volume point the room should be using.
 
### Changed
 - Fixed issue where rooms would add non-serialized originators to their room collection in CopySettings

## [14.2.0] - 2020-02-27
### Added
 - Added HandlePreUncombine method to IRoom that is called before combined spaces are torn down

## [14.1.1] - 2019-11-20
### Changed
 - More reliable WakeSchedule logic for determining if the system should be awake or asleep

## [14.1.0] - 2019-11-19
### Added
 - Added IsAwake property and event to ICommercialRoom and abstraction

## [14.0.0] - 2019-10-08
### Changed
 - Renamed partition state change events for clarification

## [13.1.0] - 2019-09-16
### Added
 - Added SourceGroups and DestinationGroups to rooms

### Changed
 - RoomOriginatorIdCollection pre-caches originators for faster lookup

## [13.0.0] - 2019-08-15
### Added
 - Added room groups
 - Rooms contain ISourceBase

## [12.1.0] - 2019-08-15
### Added
 - Exposing GetRoomAdjacentPartitions in IPartitionsCollection interface
 - Added method for getting sibling partitions for a given partition

## [12.0.0] - 2019-06-25
### Added
 - PartitionControls expose a mask for supporting get/set state operations
 - Partitions can be configured with a mask for get/set state operations

### Changed
 - Significant performance improvements when opening/closing multiple partitions
 - Partitions and rooms are aligned to a grid for visual representation
 - No longer trying to store room partitions in the settings

## [11.1.0] - 2019-04-23
### Changed
 - Moved partition control open state and event handling into abstraction
 - Logging partition state changes

## [11.0.0] - 2019-02-11
### Added
 - Added ConferencePoint ids under Room in configuration

### Changed
 - DialingPlan configuration is now a single path
 - Added VolumePoints and Partitions to the room console

## [10.6.2] - 2020-03-03
### Changed
 - AbstractCommercailRoom - fixed null reference exceptions when clearing settings without WakeSchedule or ConferenceManager

## [10.6.1] - 2020-02-10
### Changed
 - Fixed a bug where an empty dialing plan in the room config would ignore video dialers

## [10.6.0] - 2019-10-22
### Added
 - Added helper method to determine if the wake schedule is enabled today

## [10.5.0] - 2019-10-01
### Added
 - Added wake and sleep console commands to commercial room

### Changed
 - Moved commercial room abstractions and interfaces into Rooms subdirectory

## [10.4.0] - 2019-09-17
### Added
 - Added commercial room abstractions and interfaces
 - Added WakeSchedule features
 - Added DialingPlanInfo

## [10.3.0] - 2019-08-02
### Added
 - Added console command to print child rooms for a given room

### Changed
 - Fixed a potential bug where a combine space could be made without any child rooms
 - GetInstancesRecursive only returns distinct items
 - Room console commands show devices, sources, etc recursively

## [10.2.0] - 2019-05-16
### Changed
 - RoomOriginatorIdCollection specifies which children have been added/removed to/from the collection

## [10.1.0] - 2019-04-05
### Added
 - Added features for determining if a given room is a master

### Changed
 - Fixed feedback issue when opening/closing partitions with multiple controls

## [10.0.1] - 2019-01-16
### Changed
 - Fixed issue with >1 partition
 - Failing more gracefully when trying to load partitions with missing controls

## [10.0.0] - 2019-01-02
### Added
 - Added Source and Destination items to the Room console

### Changed
 - Re-ordered Devices/Ports/Panels in Room configuration for consistency
 - Better combine state tracking of rooms
 
### Removed
 - Removed AudioDestinations collection on the Room

## [9.0.0] - 2018-11-08
### Changed
 - Fail gracefully when a referenced port/device is not present in the configuration

### Removed
 - Removed DestinationGroups

## [8.0.0] - 2018-10-18
### Added
 - Added RoomLayout features for aligning rooms to a grid
 
### Changed
 - Small optimizations to room originator collection
 - Logging improvements

## [7.0.0] - 2018-09-14
### Changed
 - General performance improvements
 - Originator and control types constrained to class types

## [6.0.1] - 2018-07-02
### Changed
 - Fixed bug where not all items would be added to the room originator collection in a batch

## [6.0.0] - 2018-05-24
### Removed
 - Element name removed from settings classes

## [5.0.0] - 2018-05-03
### Removed
 - Volume points moved into Audio project

## [4.0.0] - 2018-04-27
### Added
 - Added DialingPlanInfo to standardize room dialing plans for DeployAV
 - Adding dialing plan entry to abstract room settings

## [3.1.0] - 2018-04-23
### Added
 - Audio destinations are now specifically listed as part of the room configuration

### Changed
 - Using new API event args

## [3.0.0] - 2018-04-11
### Added
 - VolumePoints are now Originators and are instantiated like devices, sources, etc

### Changed
 - _SimplSharp and _NetStandard suffixes removed from assembly names