# OmronConverters — DLCM Converter

Converts Omron DLCM tab-delimited test result files (`.txt`) to WATS UUT reports.

## Input Format

Tab-delimited text files produced by Omron DLCM (DC Link Capacitor Module) ATE systems.

Key columns: Date, Time, Station Name, Barcode/Serial, Pass/Fail, Test Time, Temperature, followed by alternating value/pass-fail pairs for each test step.

Header section ends with a row starting with four tabs, which contains the step names. The following row contains the limit values.

## Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `operationTypeCode` | `10` | WATS process/operation code. |
| `sequenceName` | `MainSequence` | Name of the root sequence. |
| `cultureCode` | `en-US` | Culture used for parsing numbers and dates. |
| `utcOffsetHours` | `0` | UTC offset in hours (e.g. `4` if test station is UTC+4). |

## Usage

```xml
<converter name="DLCMConverter" assembly="Virinco.WATS.Converter.Omron" class="Virinco.WATS.Converter.Omron.DLCMConverter">
  <Source type="folder">
    <Parameter name="Path">C:\ProgramData\Virinco\WATS\DLCMData</Parameter>
    <Parameter name="Filter">*.txt</Parameter>
    <Parameter name="PostProcessAction">Zip</Parameter>
  </Source>
  <Destination type="api">
    <Parameter name="operationTypeCode">10</Parameter>
    <Parameter name="sequenceName">MainSequence</Parameter>
    <Parameter name="cultureCode">en-US</Parameter>
    <Parameter name="utcOffsetHours">0</Parameter>
  </Destination>
</converter>
```

## Status

`testing` — see [CHANGELOG.md](../CHANGELOG.md).
