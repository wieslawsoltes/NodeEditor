using System.Collections.Generic;
using System.ComponentModel;

namespace NodeEditor.Model;

public interface INodeTemplatesHost
{
    IList<INodeTemplate>? Templates { get; set; }
}
