using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FunqTypes.Tests
{
    [TestFixture]
    public class ResultAsyncExtensionsTests
    {
        #region MapAsync() Tests

        [Test]
        public async Task MapAsync_ShouldTransformValue_WhenSuccess()
        {
            var result = Task.FromResult(Result<int, string>.Ok(42));

            var mappedResult = await result.MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });

            Assert.True(mappedResult.IsSuccess);
            Assert.AreEqual(84, mappedResult.Value);
        }

        [Test]
        public async Task MapAsync_ShouldNotTransform_WhenFailure()
        {
            var result = Task.FromResult(Result<int, string>.Fail("Original Error"));

            var mappedResult = await result.MapAsync(async x =>
            {
                await Task.Delay(10);
                return x * 2;
            });

            Assert.False(mappedResult.IsSuccess);
            Assert.AreEqual(1, mappedResult.Errors.Count);
            Assert.AreEqual("Original Error", mappedResult.Errors.First());
        }

        #endregion

        #region MapErrorAsync() Tests

        [Test]
        public async Task MapErrorAsync_ShouldTransformError_WhenFailure()
        {
            var result = Task.FromResult(Result<int, string>.Fail("Critical Failure"));

            var mappedResult = await result.MapErrorAsync(async error =>
            {
                await Task.Delay(10);
                return new ErrorDetails(error, DateTime.UtcNow);
            });

            Assert.False(mappedResult.IsSuccess);
            Assert.AreEqual(1, mappedResult.Errors.Count);
            StringAssert.Contains("Critical Failure", mappedResult.Errors.First().Message);
        }

        [Test]
        public async Task MapErrorAsync_ShouldNotTransform_WhenSuccess()
        {
            var result = Task.FromResult(Result<int, string>.Ok(42));

            var mappedResult = await result.MapErrorAsync(async error =>
            {
                await Task.Delay(10);
                return new ErrorDetails(error, DateTime.UtcNow);
            });

            Assert.True(mappedResult.IsSuccess);
            Assert.AreEqual(42, mappedResult.Value);
            Assert.IsEmpty(mappedResult.Errors);
        }

        #endregion

        #region BindAsync() Tests

        [Test]
        public async Task BindAsync_ShouldFlattenAndChainResults_WhenSuccess()
        {
            var result = Task.FromResult(Result<int, string>.Ok(5));

            async Task<Result<string, string>> AppendText(int value)
            {
                await Task.Delay(10);
                return Result<string, string>.Ok($"Processed-{value}");
            }

            var chainedResult = await result.BindAsync(AppendText);

            Assert.True(chainedResult.IsSuccess);
            Assert.AreEqual("Processed-5", chainedResult.Value);
        }

        [Test]
        public async Task BindAsync_ShouldPropagateFailure_WithoutCallingBinder()
        {
            var result = Task.FromResult(Result<int, string>.Fail("Failed Input"));

            async Task<Result<string, string>> AppendText(int value)
            {
                await Task.Delay(10);
                return Result<string, string>.Fail($"Processed-{value}");
            }

            var chainedResult = await result.BindAsync(AppendText);

            Assert.False(chainedResult.IsSuccess);
            Assert.AreEqual(1, chainedResult.Errors.Count);
            Assert.AreEqual("Failed Input", chainedResult.Errors.First());
        }

        #endregion

        #region TapAsync() Tests

        [Test]
        public async Task TapAsync_ShouldExecuteAction_WhenResultIsSuccess()
        {
            var result = Task.FromResult(Result<int, string>.Ok(42));
            int capturedValue = 0;

            var finalResult = await result.TapAsync(async value =>
            {
                await Task.Delay(10);
                capturedValue = value;
            });

            Assert.True(finalResult.IsGucci);
            Assert.AreEqual(42, finalResult.Value);
            Assert.AreEqual(42, capturedValue);
        }

        [Test]
        public async Task TapAsync_ShouldNotExecuteAction_WhenResultIsFailure()
        {
            var result = Task.FromResult(Result<int, string>.Fail("Error"));
            int capturedValue = 0;

            var finalResult = await result.TapAsync(async value =>
            {
                await Task.Delay(10);
                capturedValue = value;
            });

            Assert.False(finalResult.IsGucci);
            Assert.IsNotEmpty(finalResult.Errors);
            Assert.AreEqual(0, capturedValue);
        }

        #endregion
    }
}
