using System;
using System.Linq;
using NUnit.Framework;

namespace FunqTypes.Tests
{
    [TestFixture]
    public class ResultExtensionsTTests
    {
        [Test]
        public void ToError_ShouldThrow_WhenResultIsSuccess()
        {
            var result = Result<int, string>.Ok(42);
            Assert.Throws<InvalidOperationException>(() => result.ToError<int, string, string>());
        }

        [Test]
        public void ToError_ShouldReturnError_WhenResultIsFail()
        {
            var result = Result<int, string>.Fail("err");
            var converted = result.ToError<int, string, string>();
            Assert.False(converted.IsSuccess);
            Assert.AreEqual("err", converted.Errors.Single());
        }

        [Test]
        public void Combine2_ShouldReturnOk_WhenAllSuccess()
        {
            var result = ResultExtensions.Combine(Result<int, string>.Ok(1), Result<string, string>.Ok("A"));
            Assert.True(result.IsSuccess);
            Assert.AreEqual((1, "A"), result.Value);
        }

        [Test]
        public void Combine2_ShouldReturnErrors_WhenFailures()
        {
            var result = ResultExtensions.Combine(Result<int, string>.Fail("err1"), Result<string, string>.Fail("err2"));
            Assert.False(result.IsSuccess);
            Assert.AreEqual(new[] {"err1", "err2"}, result.Errors);
        }

        [Test]
        public void Combine3_ShouldReturnOk_WhenAllSuccess()
        {
            var result = ResultExtensions.Combine(Result<int, string>.Ok(1), Result<string, string>.Ok("A"),
                Result<double, string>.Ok(3.0));
            Assert.True(result.IsSuccess);
            Assert.AreEqual((1, "A", 3.0), result.Value);
        }

        [Test]
        public void Combine4_ShouldReturnOk_WhenAllSuccess()
        {
            var result = ResultExtensions.Combine(
                Result<int, string>.Ok(1),
                Result<string, string>.Ok("A"),
                Result<double, string>.Ok(3.0),
                Result<bool, string>.Ok(true));
            Assert.True(result.IsSuccess);
            Assert.AreEqual((1, "A", 3.0, true), result.Value);
        }

        [Test]
        public void Combine5_ShouldReturnErrors_WhenSomeFail()
        {
            var result = ResultExtensions.Combine(
                Result<int, string>.Ok(1),
                Result<string, string>.Fail("e2"),
                Result<int, string>.Fail("e3"),
                Result<bool, string>.Ok(true),
                Result<double, string>.Fail("e5")
            );

            Assert.False(result.IsSuccess);
            Assert.AreEqual(new[] {"e2", "e3", "e5"}, result.Errors);
        }
    }
}
