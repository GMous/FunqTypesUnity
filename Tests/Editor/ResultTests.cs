using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FunqTypes.Tests
{

    public record FunqError(string Code, string Message);

    [TestFixture]
    public class ResultTests
    {
        #region Success and Failure Tests

        [Test]
        public void Ok_ShouldCreateSuccessResult()
        {
            const int Value = 42;
            var result = Result<int, FunqError>.Ok(Value);

            Assert.True(result.IsSuccess);
            Assert.AreEqual(Value, result.Value);
            Assert.IsEmpty(result.Errors);
        }

        [Test]
        public void Fail_ShouldCreateFailureResult()
        {
            var error = new FunqError("ERROR", "Something went wrong");
            var result = Result<int, FunqError>.Fail(error);

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(error, result.Errors.First());
        }

        [Test]
        public void Gotcha_ShouldBehaveExactlyLike_Ok()
        {
            const string Value = "FunqTypes Rocks!";

            var gotchaResult = Result<string, FunqError>.Gotcha(Value);
            var okResult = Result<string, FunqError>.Ok(Value);

            Assert.True(gotchaResult.IsSuccess, "Gotcha should be successful.");
            Assert.True(okResult.IsSuccess, "Success should be successful.");

            Assert.AreEqual(okResult.IsSuccess, gotchaResult.IsSuccess);
            Assert.AreEqual(okResult.Value, gotchaResult.Value);
            CollectionAssert.AreEqual(okResult.Errors, gotchaResult.Errors);        }

        [Test]
        public void Oops_ShouldBehaveExactlyLike_Fail()
        {
            var error = new FunqError("FAIL", "Test-driven sadness");

            var oopsResult = Result<int, FunqError>.Oops(error);
            var failureResult = Result<int, FunqError>.Fail(error);

            Assert.False(oopsResult.IsSuccess, "Oops should be a failure.");
            Assert.False(failureResult.IsSuccess, "Failure should be a failure.");

            Assert.AreEqual(failureResult.IsSuccess, oopsResult.IsSuccess);
            Assert.AreEqual(failureResult.Value, oopsResult.Value);
            CollectionAssert.AreEqual(failureResult.Errors, oopsResult.Errors);
        }

        [Test]
        public void IsGucci_ShouldBeTrueFor_Gotcha()
        {
            var result = Result<int, FunqError>.Gotcha(42);

            Assert.True(result.IsGucci, "IsGucci should return true for a successful result.");
        }

        [Test]
        public void IsGucci_ShouldBeFalseFor_Oops()
        {
            var result = Result<int, FunqError>.Oops(new FunqError("Oops", "Not today!"));

            Assert.False(result.IsGucci, "IsGucci should return false for a failure result.");
        }

        [Test]
        public void IsGucci_ShouldBeAliasFor_IsSuccess()
        {
            var gotchaResult = Result<int, FunqError>.Gotcha(100);
            var oopsResult = Result<int, FunqError>.Oops(new FunqError("FAIL", "Oops'd"));

            Assert.AreEqual(gotchaResult.IsSuccess, gotchaResult.IsGucci);
            Assert.AreEqual(oopsResult.IsSuccess, oopsResult.IsGucci);
        }

        [Test]
        public void Unit_Value_ShouldBeEqual()
        {
            var unit1 = Unit.Value;
            var unit2 = Unit.Value;

            Assert.AreEqual(unit1, unit2);
            Assert.True(unit1.Equals(unit2));
        }

        [Test]
        public void Result_WithUnit_ShouldMatchSuccess()
        {
            var result = Result<Unit, FunqError>.Ok(Unit.Value);

            var matched = result.Match(
                ok => true,
                error => false
            );

            Assert.True(matched);
        }

        [Test]
        public void Result_WithError_ShouldNotAccessUnit()
        {
            var error = new FunqError("test", "Something failed");
            var result = Result<Unit, FunqError>.Fail(error);

            Assert.False(result.IsSuccess);
            Assert.AreEqual(error, result.Errors.First());
        }

        [Test]
        public void Unit_ShouldSignalCompletion()
        {
            var sideEffect = false;

            Result<Unit, FunqError>.Ok(Unit.Value)
                .Match(
                    ok =>
                    {
                        sideEffect = true;
                        return Unit.Value;
                    },
                    error => Unit.Value
                );

            Assert.True(sideEffect);
        }

        #endregion

        #region GetDefaultValue tests

        [Test]
        public void GetValueOrDefault_ShouldReturnSuccessValue_WhenResultIsSuccess()
        {
            var result = Result<int, string>.Ok(42);

            var value = result.GetValueOrDefault();

            Assert.AreEqual(42, value);
        }

        [Test]
        public void GetValueOrDefault_ShouldReturnDefaultValue_WhenResultIsFailure()
        {
            var result = Result<int, string>.Fail("Error!");

            var value = result.GetValueOrDefault();

            Assert.AreEqual(0, value);
        }

        [Test]
        public void GetValueOrDefault_ShouldReturnProvidedDefaultValue_WhenResultIsFailure()
        {
            var result = Result<string, string>.Fail("Error!");

            var value = result.GetValueOrDefault("Fallback Value");

            Assert.AreEqual("Fallback Value", value);
        }

        [Test]
        public void GetValueOrDefault_ShouldReturnSuccessValue_WhenResultIsSuccess_AndCustomDefaultIsProvided()
        {
            var result = Result<string, string>.Ok("Success!");

            var value = result.GetValueOrDefault("Fallback Value");

            Assert.AreEqual("Success!", value);
        }

        [Test]
        public void GetValueOrDefault_ShouldReturnFactoryValue_WhenResultIsFailure()
        {
            var result = Result<DateTime, string>.Oops("Error!");

            var value = result.GetValueOrDefault(() => new DateTime(2024, 1, 1));

            Assert.AreEqual(new DateTime(2024, 1, 1), value);
        }

        [Test]
        public void GetValueOrDefault_ShouldNotCallFactory_WhenResultIsSuccess()
        {
            var result = Result<int, string>.Ok(100);
            var factoryCalled = false;

            var value = result.GetValueOrDefault(() =>
            {
                factoryCalled = true;
                return -1;
            });

            Assert.AreEqual(100, value);
            Assert.False(factoryCalled, "Factory should not be called for success results.");
        }

        #endregion

        #region Bind Tests

        [Test]
        public void Bind_ShouldTransformSuccessResult()
        {
            var result = Result<int, FunqError>.Ok(5)
                .Bind(x => Result<int, FunqError>.Ok(x * 2));

            Assert.True(result.IsSuccess);
            Assert.AreEqual(10, result.Value);
        }

        [Test]
        public void Bind_ShouldPropagateFailure()
        {
            var error = new FunqError("INVALID", "Invalid input");
            var result = Result<int, FunqError>.Fail(error)
                .Bind(x => Result<int, FunqError>.Ok(x * 2));

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(error, result.Errors.First());
        }

        #endregion

        #region Map Tests

        [Test]
        public void Map_ShouldTransformSuccessResult()
        {
            var result = Result<int, FunqError>.Ok(10)
                .Map(x => x * 3);

            Assert.True(result.IsSuccess);
            Assert.AreEqual(30, result.Value);
        }

        [Test]
        public void Map_ShouldPropagateFailure()
        {
            var error = new FunqError("FAIL", "Mapping failed");
            var result = Result<int, FunqError>.Fail(error)
                .Map(x => x * 2);

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(error, result.Errors.First());
        }

        #endregion

        #region MapError() Tests

        [Test]
        public void MapError_ShouldTransformError_WhenFailure()
        {
            var result = Result<int, string>.Fail("Database Error");

            var transformedResult = result.MapError(error => new ErrorDetails(error, DateTime.UtcNow));

            Assert.False(transformedResult.IsSuccess);
            Assert.AreEqual(1, transformedResult.Errors.Count);
            StringAssert.Contains("Database Error", transformedResult.Errors.First().Message);
        }

        [Test]
        public void MapError_ShouldNotTransform_WhenSuccess()
        {
            var result = Result<int, string>.Ok(42);

            var transformedResult = result.MapError(error => new ErrorDetails(error, DateTime.UtcNow));

            Assert.True(transformedResult.IsSuccess);
            Assert.AreEqual(42, transformedResult.Value);
            Assert.IsEmpty(transformedResult.Errors);
        }

        [Test]
        public void MapError_ShouldTransformMultipleErrors_WhenFailure()
        {
            var result = Result<int, string>.Fail("Error1", "Error2");

            var transformedResult = result.MapError(error => new ErrorDetails(error, DateTime.UtcNow));

            Assert.False(transformedResult.IsSuccess);
            Assert.AreEqual(2, transformedResult.Errors.Count);
            StringAssert.Contains("Error1", transformedResult.Errors.First().Message);
            StringAssert.Contains("Error2", transformedResult.Errors.Last().Message);
        }

        [Test]
        public void MapError_ShouldReturnSameValue_WhenSuccess()
        {
            var result = Result<string, string>.Ok("Success");

            var transformedResult = result.MapError(error => new ErrorDetails(error, DateTime.UtcNow));

            Assert.True(transformedResult.IsSuccess);
            Assert.AreEqual("Success", transformedResult.Value);
            Assert.IsEmpty(transformedResult.Errors);
        }

        #endregion

        #region Ensure Tests

        [Test]
        public void Ensure_ShouldPassWhenConditionIsMet()
        {
            var result = Result<int, FunqError>.Ok(25)
                .Ensure(x => x >= 18, new FunqError("AGE_INVALID", "Age must be 18 or older"));

            Assert.True(result.IsSuccess);
            Assert.AreEqual(25, result.Value);
        }

        [Test]
        public void Ensure_ShouldFailWhenConditionIsNotMet()
        {
            var error = new FunqError("AGE_INVALID", "Age must be 18 or older");
            var result = Result<int, FunqError>.Ok(16)
                .Ensure(x => x >= 18, error);

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(error, result.Errors.First());
        }

        #endregion

        #region Combine Tests

        [Test]
        public void Combine_ShouldReturnFirstSuccessResultIfAllAreValid()
        {
            var result = Result<int, FunqError>.Combine(
                Result<int, FunqError>.Ok(1),
                Result<int, FunqError>.Ok(2),
                Result<int, FunqError>.Ok(3)
            );

            Assert.True(result.IsSuccess);
            Assert.AreEqual(1, result.Value);
        }

        [Test]
        public void Combine_ShouldAccumulateErrorsIfAnyFail()
        {
            var error1 = new FunqError("ERR1", "First error");
            var error2 = new FunqError("ERR2", "Second error");

            var result = Result<int, FunqError>.Combine(
                Result<int, FunqError>.Ok(1),
                Result<int, FunqError>.Fail(error1),
                Result<int, FunqError>.Fail(error2)
            );

            Assert.False(result.IsSuccess);
            Assert.AreEqual(2, result.Errors.Count);
            Assert.Contains(error1, result.Errors);
            Assert.Contains(error2, result.Errors);
        }

        #endregion

        #region Match Tests

        [Test]
        public void Match_ShouldExecuteOnSuccessFunction()
        {
            var result = Result<int, FunqError>.Ok(100);

            var output = result.Match(
                success => $"Success: {success}",
                errors => $"Errors: {string.Join(", ", errors.Select(e => e.Message))}"
            );

            Assert.AreEqual("Success: 100", output);
        }

        [Test]
        public void Match_ShouldExecuteOnFailureFunction()
        {
            var error = new FunqError("ERROR", "Something went wrong");
            var result = Result<int, FunqError>.Fail(error);

            var output = result.Match(
                success => $"Success: {success}",
                errors => $"Errors: {string.Join(", ", errors.Select(e => e.Message))}"
            );

            Assert.AreEqual("Errors: Something went wrong", output);
        }

        #endregion

        #region Tap() (Sync)

        [Test]
        public void Tap_ShouldExecuteAction_WhenResultIsSuccess()
        {
            var result = Result<int, string>.Gotcha(42);
            var capturedValue = 0;

            result = result.Tap(value => capturedValue = value);

            Assert.True(result.IsGucci);
            Assert.AreEqual(42, result.Value);
            Assert.AreEqual(42, capturedValue);
        }

        [Test]
        public void Tap_ShouldNotExecuteAction_WhenResultIsFailure()
        {
            var result = Result<int, string>.Oops("Error occurred");
            var capturedValue = 0;

            result = result.Tap(value => capturedValue = value);

            Assert.False(result.IsGucci);
            Assert.IsNotEmpty(result.Errors);
            Assert.AreEqual(0, capturedValue);
        }

        #endregion

        #region TapError() (Sync)

        [Test]
        public void TapError_ShouldExecuteAction_WhenResultIsFailure()
        {
            var result = Result<int, string>.Oops("Something went wrong");
            List<string> capturedErrors = new();

            result = result.TapError(errors => capturedErrors.AddRange(errors));

            Assert.False(result.IsGucci);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Something went wrong", capturedErrors.First());
        }

        [Test]
        public void TapError_ShouldNotExecuteAction_WhenResultIsSuccess()
        {
            var result = Result<int, string>.Gotcha(42);
            List<string> capturedErrors = new();

            result = result.TapError(errors => capturedErrors.AddRange(errors));

            Assert.True(result.IsGucci);
            Assert.AreEqual(42, result.Value);
            Assert.IsEmpty(capturedErrors);
        }

        #endregion

        #region Async Tests

        [Test]
        public async Task BindAsync_ShouldTransformSuccessResult()
        {
            var result = await Result<int, FunqError>.Ok(10)
                .BindAsync(async x => await Task.FromResult(Result<int, FunqError>.Ok(x * 2)));

            Assert.True(result.IsSuccess);
            Assert.AreEqual(20, result.Value);
        }

        [Test]
        public async Task BindAsync_ShouldPropagateFailure()
        {
            var error = new FunqError("ASYNC_ERROR", "Async operation failed");
            var result = await Result<int, FunqError>.Fail(error)
                .BindAsync(async x => await Task.FromResult(Result<int, FunqError>.Ok(x * 2)));

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(error, result.Errors.First());
        }

        [Test]
        public async Task EnsureAsync_ShouldValidateSuccessfully()
        {
            var result = await Result<int, FunqError>.Ok(25)
                .EnsureAsync(async x => await Task.FromResult(x >= 18),
                    new FunqError("AGE_FAIL", "Age must be 18 or older"));

            Assert.True(result.IsSuccess);
            Assert.AreEqual(25, result.Value);
        }

        [Test]
        public async Task EnsureAsync_ShouldFailWhenConditionIsNotMet()
        {
            var error = new FunqError("AGE_FAIL", "Age must be 18 or older");
            var result = await Result<int, FunqError>.Ok(16)
                .EnsureAsync(async x => await Task.FromResult(x >= 18), error);

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual(error, result.Errors.First());
        }

        #endregion
    }

    public record ErrorDetails(string Message, DateTime Timestamp);
}
