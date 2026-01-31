# LogicLab Buses and Signals

LogicLab supports multi-bit buses using `LogicPinViewModel.BusWidth` and `LogicValue[]` signals.

## Bus pins

- Pins with `BusWidth > 1` carry a vector of `LogicValue`.
- Connectors validate that both ends have matching bus width.

## Bus nodes

- **Bus Input**: emits a multi-bit value.
- **Bus Output**: displays a multi-bit value (hex or binary).
- **Bus Split**: splits a bus into individual bits.
- **Bus Merge**: merges individual bits into a bus.

## Signal helpers

`LogicSignalHelper` provides utilities:

- `CreateBusFromInt`
- `ToHexString` / `ToBinaryString`
- `Aggregate` for resolving multiple bits

## Validation rules

LogicLab uses `LogicConnectionValidation.TypeCompatibility` to enforce:

- Clock pins connect to clock pins.
- Bus pins connect to bus pins.
- Signal pins connect to signal pins.
