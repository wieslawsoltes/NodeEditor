using System;
using ReactiveUI;

namespace NodeEditor.ViewModels
{
    public class ConnectorViewModel : ReactiveObject
    {
        private DrawingNodeViewModel? _parent;
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
                });
        }

        public DrawingNodeViewModel? Parent
        {
            get => _parent;
            set => this.RaiseAndSetIfChanged(ref _parent, value);
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
    }
}
