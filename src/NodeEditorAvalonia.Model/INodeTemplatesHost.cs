using System.Collections.Generic;

namespace NodeEditor.Model;

public interface INodeTemplatesHost
{
    IList<INodeTemplate>? Templates { get; set; }
}
