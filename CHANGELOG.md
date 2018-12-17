# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Changed
 - Re-ordered Devices/Ports/Panels in Room configuration for consistency

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