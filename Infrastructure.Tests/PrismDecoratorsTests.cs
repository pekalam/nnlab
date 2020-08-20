using System;
using FluentAssertions;
using Infrastructure.Messaging;
using Infrastructure.PrismDecorators;
using Moq;
using Prism.Events;
using Prism.Regions;
using Xunit;

namespace Infrastructure.Tests
{
    class TestRegionCollectionDecorator : RegionCollectionDecorator
    {
        private Action<Uri, ContentRegionNavigationParameters> _navigationAction;

        public TestRegionCollectionDecorator(IRegionCollection regionCollection, Action<Uri, ContentRegionNavigationParameters> navigationAction) : base(
            regionCollection, navigationAction)
        {
            _navigationAction = navigationAction;
        }

        public Mock<IRegion> ContentRegionDecoratorMock { get; } = new Mock<IRegion>();

        protected override IRegion GetRegionDecorator(IRegion region)
        {
            return new ContentRegionDecorator(ContentRegionDecoratorMock.Object, _navigationAction);
        }
    }

    class TestRegionManagerDecorator : RegionManagerNavigationDecorator
    {
        public TestRegionManagerDecorator(RegionManager regionManager, IEventAggregator ea) : base(regionManager, ea)
        {
        }

        public TestRegionCollectionDecorator TestRegionCollection { get; set; }

        protected override IRegionCollection GetDecoratedRegionCollection(IRegionCollection regions)
        {
            TestRegionCollection = new TestRegionCollectionDecorator(regions, SendNavigationEvent);
            return TestRegionCollection;
        }
    }

    public class PrismDecoratorsTests
    {
        [Fact]
        public void RegionManagerNavigationDecorator_when_content_region_navigated_sends_event()
        {
            //arrange
            int called = 0;

            var rm = new RegionManager();
            rm.Regions.Add(AppRegions.ContentRegion, new Region());

            var ea = new EventAggregator();
            ea.GetEvent<ContentRegionViewChanged>().Subscribe(s => called++);

            var regionManager = new TestRegionManagerDecorator(rm, ea);

            //act
            regionManager.Regions[AppRegions.ContentRegion]
                .RequestNavigate(new Uri("x2", UriKind.RelativeOrAbsolute), new ContentRegionNavigationParameters("x2"));


            //assert
            called.Should().Be(1);
        }
    }
}