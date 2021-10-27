using System.Runtime.Serialization;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    [DataContract(IsReference = true)]
    public class SignalViewModel : ViewModelBase
    {
        private object? _label;
        private bool? _state;

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public bool? State
        {
            get => _state;
            set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object? Label
        {
            get => _label;
            set => this.RaiseAndSetIfChanged(ref _label, value);
        }
    }
}
