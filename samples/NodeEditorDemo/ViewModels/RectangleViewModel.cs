using System.Runtime.Serialization;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels
{
    [DataContract(IsReference = true)]
    public class RectangleViewModel : ViewModelBase
    {
        private object? _label;

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public object? Label
        {
            get => _label;
            set => this.RaiseAndSetIfChanged(ref _label, value);
        }
    }
}
