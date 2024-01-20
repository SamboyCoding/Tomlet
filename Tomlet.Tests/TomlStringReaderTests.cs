using System;
using Xunit;

namespace Tomlet.Tests
{
    public class TomletStringReaderTest
    {
        [Fact]
        public void PeekWhenPositionIsAtEndShouldReturnMinusOne()
        {
            var reader = new TomletStringReader("t");
            reader.Read(); // Move to the end
            var result = reader.Peek();
            Assert.Equal(-1, result);
        }

        [Fact]
        public void ReadWhenPositionIsAtEndShouldReturnMinusOne()
        {
            var reader = new TomletStringReader("t");
            reader.Read(); // Move to the end
            var result = reader.Read();
            Assert.Equal(-1, result);
        }

        [Fact]
        public void ReadBlockWhenPositionIsAtEndShouldReturnZero()
        {
            var reader = new TomletStringReader("test");
            reader.Read(); // Move to the end
            reader.Read();
            reader.Read();
            reader.Read();
            var buffer = new char[10];
            var result = reader.ReadBlock(buffer, 0, 5);
            Assert.Equal(0, result);
        }

        [Fact]
        public void BacktrackWhenAmountIsGreaterThanPositionShouldThrowException()
        {
            var reader = new TomletStringReader("test");
            Assert.Throws<Exception>(() => reader.Backtrack(10));
        }

        [Fact]
        public void ReadBlockShouldReadCorrectly()
        {
            var reader = new TomletStringReader("test");

            var buffer = new char[10];
            var result = reader.ReadBlock(buffer, 0, 4);

            Assert.Equal(4, result);
            Assert.Equal("test", new string(buffer, 0, result));
        }
    }
}
