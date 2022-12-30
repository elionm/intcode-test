using FluentAssertions;

namespace Intcode1.UnitTests
{
    public class IntcodeTests
    {
        [Theory]
        [InlineData("1,9,10,3,2,3,11,0,99,30,40,50",new int[] {3500, 9, 10, 70, 2, 3, 11, 0, 99, 30, 40, 50})]
        [InlineData("1,0,0,0,99", new int[] { 2, 0, 0, 0, 99 })]
        [InlineData("2,3,0,3,99", new int[] { 2, 3, 0, 6, 99 })]
        [InlineData("2,4,4,5,99,0", new int[] { 2, 4, 4, 5, 99, 9801 })]
        [InlineData("1,1,1,4,99,5,6,0,99", new int[] { 30, 1, 1, 4, 2, 5, 6, 0, 99 })]
        public void TestStates(string input, int[] expectedState)
        {
            var intcode = CreateIntCode.FromString(input);
            var exitCode = intcode.Execute();
            var state = intcode.GetState();

            exitCode.Should().Be(0);
            state.Should().BeEquivalentTo(expectedState);
        }
    }
}