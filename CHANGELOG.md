# Changelog — OmronConverters

## [1.0.0] — 2026-06-19

### Added
- Initial public release of `DLCMConverter`.
- Ported from internal `OmronConverters`. Namespace updated to `Virinco.WATS.Converter.Omron`.
- Added default `ConverterParameters` (`operationTypeCode=10`, `sequenceName=MainSequence`, `cultureCode=en-US`, `utcOffsetHours=0`).
- Added `utcOffsetHours` parameter (replaces hardcoded UTC offset).
- Fixed `api.Submit` to v7 API signature.
- Added default constructor.
- Sanitized test data: customer-specific identifiers removed; replaced with generic placeholder values.
