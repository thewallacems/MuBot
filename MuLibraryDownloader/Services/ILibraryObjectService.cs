using System.Collections.Generic;
using System.Threading.Tasks;

namespace MuLibraryDownloader.Services
{
    public interface ILibraryObjectService<T>
    {
        Task<List<T>> GetObjects();
    }
}
