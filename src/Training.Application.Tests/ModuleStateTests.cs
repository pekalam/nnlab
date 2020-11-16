using Common.Domain;
using FluentAssertions;
using TestUtils;
using Xunit;

namespace Training.Application.Tests
{
    public class ModuleStateTests
    {
        AppState _appState;
        ModuleState _moduleState;

        public ModuleStateTests()
        {
            _appState = new AppState();
            _moduleState = new ModuleState(_appState);
        }



    }
}
