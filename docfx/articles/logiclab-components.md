# LogicLab Component Library

`LogicComponentLibrary` defines the default logic components available in the toolbox. Components expose:

- `Id` and `Title`
- `Category`
- `PropagationDelay`
- Input and output pin labels
- Evaluation function

## Gate definitions

The library includes common gates:

- AND, OR, NOT
- NAND, NOR
- XOR, XNOR

## Components

- Half Adder
- Full Adder
- 2:1 Mux
- 2:4 Decoder

## IC Library

A selection of IC-style components are included (e.g., 74HC00, 74HC04, 74HC08, 74HC32, 74HC86, 74HC157, 74HC138, 74HC283).

Each component defines its inputs and outputs and is evaluated using `LogicValue` operations.

## Adding custom components

`LogicComponentLibrary` is sealed and its builder methods are private, so extending it requires either:

- **Forking/modifying** the library source to add definitions, or
- **Creating a custom library** and updating `LogicNodeFactory` to use it.

The easiest approach is to copy `LogicComponentLibrary` into your project and add or remove definitions there, then pass that instance into `LogicNodeFactory`.
