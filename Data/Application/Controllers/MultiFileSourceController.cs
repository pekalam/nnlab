using Data.Application.Services;
using Data.Application.ViewModels;
using Prism.Regions;

namespace Data.Application.Controllers
{
    internal class MultiFileSourceController
    {
        private readonly MultiFileService _multiFileService;
        private readonly IRegionManager _rm;

        public MultiFileSourceController(MultiFileService multiFileService, IRegionManager rm)
        {
            _multiFileService = multiFileService;
            _rm = rm;

            MultiFileSourceViewModel.Created += () => _multiFileService.Reset();
        }



    }
}