using System.Linq;
using NUnit.Framework;

namespace FunqTypes.Tests
{

    [TestFixture]
    public class ResultLinqExtensionsTests
    {
        #region Where Tests

        [Test]
        public void Where_ShouldReturnSuccess_IfConditionIsMet()
        {
            var result = Result<int, FunqError>.Ok(10);

            var filteredResult = result.Where(x => x > 5, new FunqError("TOO_SMALL", "Number must be greater than 5"));

            Assert.True(filteredResult.IsSuccess);
            Assert.AreEqual(10, filteredResult.Value);
        }

        [Test]
        public void Where_ShouldReturnFailure_IfConditionIsNotMet()
        {
            var result = Result<int, FunqError>.Ok(3);

            var filteredResult = result.Where(x => x > 5, new FunqError("TOO_SMALL", "Number must be greater than 5"));

            Assert.False(filteredResult.IsSuccess);
            Assert.AreEqual(1, filteredResult.Errors.Count);
            Assert.AreEqual("TOO_SMALL", filteredResult.Errors.First().Code);
        }

        [Test]
        public void Where_ShouldNotAffectFailureResults()
        {
            var error = new FunqError("INVALID", "Initial failure");
            var result = Result<int, FunqError>.Fail(error);

            var filteredResult = result.Where(x => x > 5, new FunqError("TOO_SMALL", "Number must be greater than 5"));

            Assert.False(filteredResult.IsSuccess);
            Assert.AreEqual(1, filteredResult.Errors.Count);
            Assert.AreEqual(error, filteredResult.Errors.First());
        }

        #endregion

        #region Select Tests

        [Test]
        public void Select_ShouldTransformSuccessValue()
        {
            var result = Result<int, FunqError>.Ok(5);

            var mappedResult = result.Select(x => x * 2);

            Assert.True(mappedResult.IsSuccess);
            Assert.AreEqual(10, mappedResult.Value);
        }

        [Test]
        public void Select_ShouldPreserveErrors()
        {
            var error = new FunqError("INVALID", "Cannot process");
            var result = Result<int, FunqError>.Fail(error);

            var mappedResult = result.Select(x => x * 2);

            Assert.False(mappedResult.IsSuccess);
            Assert.AreEqual(1, mappedResult.Errors.Count);
            Assert.AreEqual(error, mappedResult.Errors.First());
        }

        #endregion

        #region SelectMany Tests

        private static Result<int, FunqError> DoubleIfPositive(int number) =>
            number > 0
                ? Result<int, FunqError>.Ok(number * 2)
                : Result<int, FunqError>.Fail(new FunqError("NEGATIVE", "Number must be positive"));

        [Test]
        public void SelectMany_ShouldFlattenNestedResults_OnSuccess()
        {
            var result = Result<int, FunqError>.Ok(10);

            var flattenedResult = result.SelectMany(DoubleIfPositive);

            Assert.True(flattenedResult.IsSuccess);
            Assert.AreEqual(20, flattenedResult.Value);
        }

        [Test]
        public void SelectMany_ShouldPropagateErrors_IfInitialResultIsFailure()
        {
            var error = new FunqError("INVALID", "Initial failure");
            var result = Result<int, FunqError>.Fail(error);

            var flattenedResult = result.SelectMany(DoubleIfPositive);

            Assert.False(flattenedResult.IsSuccess);
            Assert.AreEqual(1, flattenedResult.Errors.Count);
            Assert.AreEqual(error, flattenedResult.Errors.First());
        }

        [Test]
        public void SelectMany_ShouldPropagateErrors_IfSecondOperationFails()
        {
            var result = Result<int, FunqError>.Ok(-5);

            var flattenedResult = result.SelectMany(DoubleIfPositive);

            Assert.False(flattenedResult.IsSuccess);
            Assert.AreEqual(1, flattenedResult.Errors.Count);
            Assert.AreEqual("NEGATIVE", flattenedResult.Errors.First().Code);
        }

        #endregion

        #region LINQ Query Syntax Tests

        [Test]
        public void LinqQuery_ShouldWorkWithResultType()
        {
            Result<User, FunqError> GetUserById(int id) =>
                id == 1
                    ? Result<User, FunqError>.Ok(new User("John", "john@example.com", true))
                    : Result<User, FunqError>.Fail(new FunqError("NOT_FOUND", "User not found."));

            var accountResult = from user in GetUserById(1)
                where user.IsActive
                select new Account(user.Name, user.Email);

            Assert.True(accountResult.IsSuccess);
            Assert.AreEqual("John", accountResult.Value.Name);
        }

        [Test]
        public void LinqQuery_ShouldHandleFailurePaths()
        {
            Result<User, FunqError> GetUserById(int id) =>
                id == 1
                    ? Result<User, FunqError>.Ok(new User("John", "john@example.com", false))
                    : Result<User, FunqError>.Fail(new FunqError("NOT_FOUND", "User not found."));

            var accountResult = GetUserById(1)
                .Where(user => user.IsActive, new FunqError("INACTIVE", "User is inactive."))
                .Select(user => new Account(user.Name, user.Email));

            Assert.False(accountResult.IsSuccess);
            Assert.AreEqual(1, accountResult.Errors.Count);
            Assert.AreEqual("User is inactive.", accountResult.Errors.First().Message);
        }

        #endregion
    }

    public record User(string Name, string Email, bool IsActive);

    public record Account(string Name, string Email);
}
