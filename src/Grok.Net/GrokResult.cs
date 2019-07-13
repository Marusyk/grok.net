using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GrokNet
{
    public sealed class GrokResult : ReadOnlyCollection<GrokItem>
    {
        public GrokResult(IList<GrokItem> grokItems)
            : base(grokItems ?? new List<GrokItem>())
        {
        }
    }
}
