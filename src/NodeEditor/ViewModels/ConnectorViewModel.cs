using System;
using NodeEditor.Model;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public class ConnectorViewModel : ReactiveObject
    {
        private DrawingNodeViewModel? _parent;
        private ConnectorOrientation _orientation;
        private PinViewModel? _start;
        private PinViewModel? _end;

        public ConnectorViewModel()
        {
            this.WhenAnyValue(x => x.Start)
                .Subscribe(start =>
                {
                    if (start?.Parent is { })
                    {
                        start.Parent.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                        start.Parent.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                    }
                    else
                    {
                        if (start is { })
                        {
                            start.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                            start.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                        }
                    }

                    if (start is { })
                    {
                        start.WhenAnyValue(x => x.Alignment).Subscribe(_ => this.RaisePropertyChanged(nameof(Start)));
                    }
                });

            this.WhenAnyValue(x => x.End)
                .Subscribe(end =>
                {
                    if (end?.Parent is { })
                    {
                        end.Parent.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                        end.Parent.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                    }
                    else
                    {
                        if (end is { })
                        {
                            end.WhenAnyValue(x => x.X).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                            end.WhenAnyValue(x => x.Y).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                        }
                    }

                    if (end is { })
                    {
                        end.WhenAnyValue(x => x.Alignment).Subscribe(_ => this.RaisePropertyChanged(nameof(End)));
                    }
                });
        }

        public DrawingNodeViewModel? Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
        }

        public ConnectorOrientation Orientation
        {
            get => _orientation;
            set => this.RaiseAndSetIfChanged(ref _orientation, value);
        }

        public PinViewModel? Start
        {
            get => _start;
            set => this.RaiseAndSetIfChanged(ref _start, value);
        }

        public PinViewModel? End
        {
            get => _end;
            set => this.RaiseAndSetIfChanged(ref _end, value);
        }

        public void GetControlPoints(
            ConnectorOrientation orientation, 
            double offset, 
            PinAlignment p1A, 
            PinAlignment p2A, 
            ref double p1X, 
            ref double p1Y, 
            ref double p2X, 
            ref double p2Y)
        {
            switch (orientation)
            {
                case ConnectorOrientation.Auto:
                    switch (p1A)
                    {
                        case PinAlignment.None:
                        {
                            switch (p2A)
                            {
                                case PinAlignment.None:
                                    break;
                                case PinAlignment.Left:
                                    p2X -= offset;
                                    p1X += offset;
                                    break;
                                case PinAlignment.Right:
                                    p2X += offset;
                                    p1X -= offset;
                                    break;
                                case PinAlignment.Top:
                                    p2Y -= offset;
                                    p1Y += offset;
                                    break;
                                case PinAlignment.Bottom:
                                    p2Y += offset;
                                    p1Y -= offset;
                                    break;
                            }
                        }
                            break;
                        case PinAlignment.Left:
                            switch (p2A)
                            {
                                case PinAlignment.None:
                                    p1X -= offset;
                                    p2X += offset;
                                    break;
                                case PinAlignment.Left:
                                    p1X -= offset;
                                    p2X -= offset;
                                    break;
                                case PinAlignment.Right:
                                    p1X -= offset;
                                    p2X += offset;
                                    break;
                                case PinAlignment.Top:
                                    p1X -= offset;
                                    break;
                                case PinAlignment.Bottom:
                                    p2Y += offset;
                                    break;
                            }
                            break;
                        case PinAlignment.Right:
                            switch (p2A)
                            {
                                case PinAlignment.None:
                                    p1X += offset;
                                    p2X -= offset;
                                    break;
                                case PinAlignment.Left:
                                    p1X += offset;
                                    p2X -= offset;
                                    break;
                                case PinAlignment.Right:
                                    p1X += offset;
                                    p2X += offset;
                                    break;
                                case PinAlignment.Top:
                                    p1X += offset;
                                    break;
                                case PinAlignment.Bottom:
                                    p2Y += offset;
                                    break;
                            }
                            break;
                        case PinAlignment.Top:
                            switch (p2A)
                            {
                                case PinAlignment.None:
                                    p1Y -= offset;
                                    p2Y += offset;
                                    break;
                                case PinAlignment.Left:
                                    p2X -= offset;
                                    break;
                                case PinAlignment.Right:
                                    p2X += offset;
                                    break;
                                case PinAlignment.Top:
                                    p1Y -= offset;
                                    p2Y -= offset;
                                    break;
                                case PinAlignment.Bottom:
                                    p1Y -= offset;
                                    p2Y += offset;
                                    break;
                            }
                            break;
                        case PinAlignment.Bottom:
                            switch (p2A)
                            {
                                case PinAlignment.None:
                                    p1Y += offset;
                                    p2Y -= offset;
                                    break;
                                case PinAlignment.Left:
                                    p1Y += offset;
                                    break;
                                case PinAlignment.Right:
                                    p1Y += offset;
                                    break;
                                case PinAlignment.Top:
                                    p1Y += offset;
                                    p2Y -= offset;
                                    break;
                                case PinAlignment.Bottom:
                                    p1Y += offset;
                                    p2Y += offset;
                                    break;
                            }
                            break;
                    }

                    break;
                case ConnectorOrientation.Horizontal:
                    p1X += offset;
                    p2X -= offset;
                    break;
                case ConnectorOrientation.Vertical:
                    p1Y += offset;
                    p2Y -= offset;
                    break;
            }
        }
    }
}
