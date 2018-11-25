using System.Collections.Generic;

namespace hagen
{
    public interface IActionSourceFactory
    {
        IEnumerable<IActionSource3> CreateActionSources();
    }
}