# LogicLab Simulation

`LogicSimulationService` evaluates logic graphs and updates pin values.

## Simulation modes

- `Evaluate`: resolves steady state without advancing clocks.
- `Step`: advances clocks, applies propagation delay, and updates outputs.
- `Reset`: clears internal state and restores default values.

## Propagation delay

Each logic node has a `PropagationDelay`. During `Step`, outputs are deferred until the delay expires. The service tracks pending outputs per node.

## Stability checks

The simulator iterates until signals settle or a maximum iteration count is reached. It reports:

- Iteration count
- Stability flag
- Warnings (e.g., unresolved loops)

## Clocks and flip-flops

- Clock nodes toggle based on period and high-tick settings.
- Flip-flops sample the input on rising edge.

## Contention and validation

Multiple drivers on the same input can cause contention. The simulator detects contention and marks connectors with a status message.

## Usage example

```csharp
var result = simulator.Evaluate(drawing);
if (!result.IsStable)
{
    // Inspect result.Messages
}
```
