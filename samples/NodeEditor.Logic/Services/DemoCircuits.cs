using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorLogic.ViewModels.Nodes;

namespace NodeEditorLogic.Services;

internal static class DemoCircuits
{
    public static IDrawingNode CreateDemoDrawing(LogicNodeFactory factory)
    {
        var drawing = factory.CreateDrawing("Logic Demo");
        drawing.Width = 1650;
        drawing.Height = 1000;

        var intro = factory.CreateNoteNode(40, 20);
        if (intro.Content is LogicNoteNodeViewModel introContent)
        {
            introContent.Text = "LogicLab demo: a clocked 2-bit counter feeds a decoder and MUX select. Toggle A/B/Cin and bus values to see gates, MUX, and the 4-bit adder update. Run the simulation to watch waveforms.";
        }
        AddNode(drawing, intro);

        var inputA = factory.CreateInputNode(40, 140, "A");
        var inputB = factory.CreateInputNode(40, 210, "B");
        var inputCin = factory.CreateInputNode(40, 280, "Cin");

        if (inputA.Content is LogicInputNodeViewModel inputAContent)
        {
            inputAContent.IsOn = true;
        }

        AddNode(drawing, inputA);
        AddNode(drawing, inputB);
        AddNode(drawing, inputCin);

        var xor = factory.CreateGateNode(220, 140, factory.GetDefinition("xor"), "XOR");
        var andGate = factory.CreateGateNode(220, 220, factory.GetDefinition("and"), "AND");
        var orGate = factory.CreateGateNode(220, 300, factory.GetDefinition("or"), "OR");
        var notGate = factory.CreateGateNode(220, 380, factory.GetDefinition("not"), "NOT");

        var outputXor = factory.CreateOutputNode(420, 140, "A XOR B");
        var outputAnd = factory.CreateOutputNode(420, 220, "A AND B");
        var outputOr = factory.CreateOutputNode(420, 300, "A OR B");
        var outputNot = factory.CreateOutputNode(420, 380, "NOT A");

        AddNode(drawing, xor);
        AddNode(drawing, andGate);
        AddNode(drawing, orGate);
        AddNode(drawing, notGate);
        AddNode(drawing, outputXor);
        AddNode(drawing, outputAnd);
        AddNode(drawing, outputOr);
        AddNode(drawing, outputNot);

        Connect(drawing, inputA, 0, xor, 0);
        Connect(drawing, inputB, 0, xor, 1);
        Connect(drawing, inputA, 0, andGate, 0);
        Connect(drawing, inputB, 0, andGate, 1);
        Connect(drawing, inputA, 0, orGate, 0);
        Connect(drawing, inputB, 0, orGate, 1);
        Connect(drawing, inputA, 0, notGate, 0);
        Connect(drawing, xor, 0, outputXor, 0);
        Connect(drawing, andGate, 0, outputAnd, 0);
        Connect(drawing, orGate, 0, outputOr, 0);
        Connect(drawing, notGate, 0, outputNot, 0);

        var mux = factory.CreateGateNode(560, 200, factory.GetDefinition("mux2"), "2:1 Mux");
        var outputMux = factory.CreateOutputNode(760, 200, "Mux Out");
        AddNode(drawing, mux);
        AddNode(drawing, outputMux);

        Connect(drawing, inputA, 0, mux, 0, "A");
        Connect(drawing, inputB, 0, mux, 1, "B");
        Connect(drawing, mux, 0, outputMux, 0);

        var clock = factory.CreateClockNode(40, 520);
        if (clock.Content is LogicClockNodeViewModel clockContent)
        {
            clockContent.IsRunning = true;
            clockContent.Period = 4;
            clockContent.HighTicks = 2;
        }
        AddNode(drawing, clock);

        var flipFlop0 = factory.CreateFlipFlopNode(220, 500);
        var flipFlop1 = factory.CreateFlipFlopNode(220, 640);
        var counterBus = factory.CreateBusMergeNode(460, 540, 2);
        var counterOut = factory.CreateBusOutputNode(680, 540, 2, "Counter");
        var decoder = factory.CreateGateNode(460, 700, factory.GetDefinition("decoder2"), "2:4 Decoder");
        var decoderD0 = factory.CreateOutputNode(680, 660, "D0");
        var decoderD1 = factory.CreateOutputNode(680, 700, "D1");
        var decoderD2 = factory.CreateOutputNode(680, 740, "D2");
        var decoderD3 = factory.CreateOutputNode(680, 780, "D3");

        AddNode(drawing, flipFlop0);
        AddNode(drawing, flipFlop1);
        AddNode(drawing, counterBus);
        AddNode(drawing, counterOut);
        AddNode(drawing, decoder);
        AddNode(drawing, decoderD0);
        AddNode(drawing, decoderD1);
        AddNode(drawing, decoderD2);
        AddNode(drawing, decoderD3);

        Connect(drawing, clock, 0, flipFlop0, 1, "CLK");
        Connect(drawing, flipFlop0, 1, flipFlop0, 0, "Qn->D");
        Connect(drawing, flipFlop0, 0, flipFlop1, 1, "Q0");
        Connect(drawing, flipFlop1, 1, flipFlop1, 0, "Qn->D");
        Connect(drawing, flipFlop0, 0, counterBus, 0, "Q0");
        Connect(drawing, flipFlop1, 0, counterBus, 1, "Q1");
        Connect(drawing, counterBus, 0, counterOut, 0, "BUS");
        Connect(drawing, flipFlop0, 0, decoder, 0, "A");
        Connect(drawing, flipFlop1, 0, decoder, 1, "B");
        Connect(drawing, decoder, 0, decoderD0, 0);
        Connect(drawing, decoder, 1, decoderD1, 0);
        Connect(drawing, decoder, 2, decoderD2, 0);
        Connect(drawing, decoder, 3, decoderD3, 0);
        Connect(drawing, flipFlop0, 0, mux, 2, "Sel");

        var busInputA = factory.CreateBusInputNode(980, 120, 4, "A[3:0]");
        var busInputB = factory.CreateBusInputNode(980, 260, 4, "B[3:0]");
        var busSplitA = factory.CreateBusSplitNode(1200, 120, 4);
        var busSplitB = factory.CreateBusSplitNode(1200, 260, 4);
        var adder = factory.CreateGateNode(1420, 180, factory.GetDefinition("ic-74283"), "4-bit Adder");
        var sumMerge = factory.CreateBusMergeNode(1400, 420, 4);
        var sumOut = factory.CreateBusOutputNode(1400, 580, 4, "Sum");
        var carryOut = factory.CreateOutputNode(1400, 720, "Cout");

        if (busInputA.Content is LogicBusInputNodeViewModel busAContent)
        {
            busAContent.BusValue = 3;
        }

        if (busInputB.Content is LogicBusInputNodeViewModel busBContent)
        {
            busBContent.BusValue = 5;
        }

        AddNode(drawing, busInputA);
        AddNode(drawing, busInputB);
        AddNode(drawing, busSplitA);
        AddNode(drawing, busSplitB);
        AddNode(drawing, adder);
        AddNode(drawing, sumMerge);
        AddNode(drawing, sumOut);
        AddNode(drawing, carryOut);

        Connect(drawing, busInputA, 0, busSplitA, 0, "BUS");
        Connect(drawing, busInputB, 0, busSplitB, 0, "BUS");

        for (var i = 0; i < 4; i++)
        {
            Connect(drawing, busSplitA, i, adder, i);
            Connect(drawing, busSplitB, i, adder, 4 + i);
            Connect(drawing, adder, i, sumMerge, i);
        }

        Connect(drawing, inputCin, 0, adder, 8, "Cin");
        Connect(drawing, adder, 4, carryOut, 0, "Cout");
        Connect(drawing, sumMerge, 0, sumOut, 0, "SUM");

        return drawing;
    }

    private static void AddNode(IDrawingNode drawing, NodeViewModel node)
    {
        node.Parent = drawing;
        drawing.Nodes ??= new ObservableCollection<INode>();
        drawing.Nodes.Add(node);
    }

    private static void Connect(
        IDrawingNode drawing,
        NodeViewModel source,
        int sourceIndex,
        NodeViewModel target,
        int targetIndex,
        string? name = null)
    {
        var sourceContent = source.Content as LogicNodeContentViewModel;
        var targetContent = target.Content as LogicNodeContentViewModel;

        if (sourceContent is null || targetContent is null)
        {
            return;
        }

        var outputPins = sourceContent.OutputPins;
        var inputPins = targetContent.InputPins;

        if (sourceIndex >= outputPins.Count || targetIndex >= inputPins.Count)
        {
            return;
        }

        var connector = LogicNodeFactory.CreateConnector(outputPins[sourceIndex], inputPins[targetIndex]);
        if (!string.IsNullOrWhiteSpace(name))
        {
            connector.Name = name;
        }
        connector.Parent = drawing;
        drawing.Connectors ??= new ObservableCollection<IConnector>();
        drawing.Connectors.Add(connector);
    }
}
