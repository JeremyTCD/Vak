using Xunit;

namespace Jering.Etl.Rdb2Es.Tests
{
    [Collection("DataPipelineCollection")]
    public class DataPipelineTests
    {
        private VakUnit[] _vakUnits { get; }

        public DataPipelineTests(DataPipelineFixture fixture)
        {
            _vakUnits = fixture.VakUnits;
        }

        [Fact]
        public void Check()
        {
            // Arrange
            Assert.NotNull(_vakUnits);
        }
    }
}
