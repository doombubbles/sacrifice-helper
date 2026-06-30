# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.4.7] - 2026-06-14

- Fixed a bug with Auto Sacrificed temples losing their sacrificed state when saving/loading in recent BTD6 versions

## [2.4.6] - 2026-05-26

- Updated HonoraryParagons integration

## [2.4.5] - 2026-04-09

- Fixed for BTD6 v54

## [2.4.4] - 2026-01-25

- Added an Auto Sacrifice mod setting for Temples
    - Mode for automatically applying sacrifice benefits to temples via increasing the upgrade cost a corresponding amount instead of sacrificing nearby towers.
    - Temple syntax is for example 2221 meaning all sacrifices are 50k except for Support on the Tier 4 sacrifice.
      - Off
      - Sacrifice 1222 (+\$150,000 Tier 4, +\$200,000 Tier 5)
      - Sacrifice 2122 (+\$150,000 Tier 4, +\$200,000 Tier 5)
      - Sacrifice 2212 (+\$150,000 Tier 4, +\$200,000 Tier 5)
      - Sacrifice 2221 (+\$150,000 Tier 4, +\$200,000 Tier 5)
      - Sacrifice 2222 (+\$200,000 Tier 4, +\$200,000 Tier 5) <--- (normally not obtainable)
    - Still works for becoming a Vengeful Temple.

## [2.4.3] - 2025-12-03

- Fixed for v52

## [2.4.2] - 2025-08-27

- Fixed for v50

## [2.4.1] - 2024-12-11

- Fixed for recent BTD6 versions

## [2.4.0] - 2023-10-14

- Updated degree calculations for v39 changes
- Added Degree Indicator to the paragon investment slider
- Added mod setting to control the default Slider Contribution Penalty of 5%

## [2.3.4] - 2023-07-31

- Fixed Paragon costs getting included for Temple Sacrifice UI

## [2.3.3] - 2023-04-04

- Fixed for BTD6 v36

## [2.3.2] - 2022-12-29

- Fixed for BTD6 v34 / ML 0.6.0

## [2.3.1] - 2022-10-20

- Fixed for BTD6 v33

## [2.3.0] - 2022-08-26

- Internal revamp of UI code
- Added paragon bonus power indicator (from those Geraldo items)
- You can now click on the paragon power indicator icons to be told your progress toward the maximum
- Fixed issue with Paragon sacrifices not destroying towers
- Fixed issue with degree calculations for extra tier 5 towers (apparently the upgrade cost of the Paragon starts being included in the sacrifice once you have more than three tier 5s lol)

## [2.2.2] - 2022-08-09

- Updated to Mod Helper 3.0
- Fixed for BTD6 v32.0

[unreleased]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.7...HEAD
[2.4.7]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.6...2.4.7
[2.4.6]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.5...2.4.6
[2.4.5]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.4...2.4.5
[2.4.4]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.3...2.4.4
[2.4.3]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.2...2.4.3
[2.4.2]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.1...2.4.2
[2.4.1]: https://github.com/doombubbles/SacrificeHelper/compare/2.4.0...2.4.1
[2.4.0]: https://github.com/doombubbles/SacrificeHelper/compare/2.3.4...2.4.0
[2.3.4]: https://github.com/doombubbles/SacrificeHelper/compare/2.3.3...2.3.4
[2.3.3]: https://github.com/doombubbles/SacrificeHelper/compare/2.3.2...2.3.3
[2.3.2]: https://github.com/doombubbles/SacrificeHelper/compare/2.3.1...2.3.2
[2.3.1]: https://github.com/doombubbles/SacrificeHelper/compare/2.3.0...2.3.1
[2.3.0]: https://github.com/doombubbles/SacrificeHelper/compare/2.2.2...2.3.0
[2.2.2]: https://github.com/doombubbles/SacrificeHelper/compare/eaefe73b217aaaef9b6043a8b1fb1bc434f8bdb3...2.2.2
