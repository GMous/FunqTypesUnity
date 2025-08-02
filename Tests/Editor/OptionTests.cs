using System.Linq;
using NUnit.Framework;

namespace FunqTypes.Tests
{
    [TestFixture]
    public class OptionTests
    {
        #region Creation & Implicit Conversion

        [Test]
        public void Some_ShouldCreateOptionWithValue()
        {
            var option = Option<int>.Some(42);

            Assert.True(option.IsSome);
            Assert.False(option.IsNone);
            Assert.AreEqual(42, option.GetValueOrDefault());
        }

        [Test]
        public void None_ShouldCreateEmptyOption()
        {
            var option = Option<int>.None();

            Assert.False(option.IsSome);
            Assert.True(option.IsNone);
            Assert.AreEqual(0, option.GetValueOrDefault());
        }

        [Test]
        public void ImplicitConversionFromValue_ShouldCreateSome()
        {
            Option<string> option = "FunqTypes";

            Assert.True(option.IsSome);
            Assert.AreEqual("FunqTypes", option.GetValueOrDefault());
        }

        [Test]
        public void ImplicitConversionFromNull_ShouldCreateNone()
        {
            Option<string> option = null!;

            Assert.True(option.IsNone);
            Assert.Null(option.GetValueOrDefault());
        }

        [Test]
        public void ImplicitConversionFromNoneType_ShouldCreateNone()
        {
            Option<int> option = Option.None;

            Assert.True(option.IsNone);
        }

        #endregion

        #region Funq Aliases (Yeah & Nope)

        [Test]
        public void Yeah_ShouldCreateOptionWithValue()
        {
            var option = Option<int>.Yeah(42);

            Assert.True(option.IsSome);
            Assert.False(option.IsNone);
            Assert.AreEqual(42, option.GetValueOrDefault());

            var expected = Option<int>.Some(42);
            Assert.AreEqual(expected, option);
        }

        [Test]
        public void Nah_ShouldCreateNoneOption()
        {
            var option = Option<int>.Nah();

            Assert.False(option.IsSome);
            Assert.True(option.IsNone);
            Assert.AreEqual(0, option.GetValueOrDefault());

            var expected = Option<int>.None();
            Assert.AreEqual(expected, option);
        }

        #endregion

        #region GetValueOrDefault

        [Test]
        public void GetValueOrDefault_ShouldReturnContainedValue()
        {
            var option = Option<int>.Some(99);

            Assert.AreEqual(99, option.GetValueOrDefault());
        }

        [Test]
        public void GetValueOrDefault_ShouldReturnDefaultValueForNone()
        {
            var option = Option<int>.None();

            Assert.AreEqual(0, option.GetValueOrDefault());
        }

        [Test]
        public void GetValueOrDefault_WithFallback_ShouldReturnValueIfSome()
        {
            var option = Option<int>.Some(7);

            Assert.AreEqual(7, option.GetValueOrDefault(100));
        }

        [Test]
        public void GetValueOrDefault_WithFallback_ShouldReturnFallbackIfNone()
        {
            var option = Option<int>.None();

            Assert.AreEqual(100, option.GetValueOrDefault(100));
        }

        [Test]
        public void GetValueOrDefault_WithFactory_ShouldReturnValueIfSome()
        {
            var option = Option<int>.Some(25);

            Assert.AreEqual(25, option.GetValueOrDefault(() => 999));
        }

        [Test]
        public void GetValueOrDefault_WithFactory_ShouldCallFactoryIfNone()
        {
            var option = Option<int>.None();
            var factoryCalled = false;

            var value = option.GetValueOrDefault(() =>
            {
                factoryCalled = true;
                return 888;
            });

            Assert.AreEqual(888, value);
            Assert.True(factoryCalled);
        }

        #endregion

        #region Map & Bind

        [Test]
        public void Map_ShouldTransformSomeValue()
        {
            var option = Option<int>.Some(5);
            var result = option.Map(x => x * 2);

            Assert.True(result.IsSome);
            Assert.AreEqual(10, result.GetValueOrDefault());
        }

        [Test]
        public void Map_ShouldReturnNoneIfNone()
        {
            var option = Option<int>.None();
            var result = option.Map(x => x * 2);

            Assert.True(result.IsNone);
        }

        [Test]
        public void Bind_ShouldFlattenOptionResult()
        {
            Option<int> SomeTransform(int x) => Option<int>.Some(x * 2);

            var option = Option<int>.Some(3);
            var result = option.Bind(SomeTransform);

            Assert.True(result.IsSome);
            Assert.AreEqual(6, result.GetValueOrDefault());
        }

        [Test]
        public void Bind_ShouldReturnNoneIfNone()
        {
            Option<int> SomeTransform(int x) => Option<int>.Some(x * 2);

            var option = Option<int>.None();
            var result = option.Bind(SomeTransform);

            Assert.True(result.IsNone);
        }

        #endregion

        #region IfSome & IfNone

        [Test]
        public void IfSome_ShouldExecuteActionIfSome()
        {
            var option = Option<string>.Some("Hello");
            var called = false;

            option.IfSome(value =>
            {
                Assert.AreEqual("Hello", value);
                called = true;
            });

            Assert.True(called);
        }

        [Test]
        public void IfNone_ShouldExecuteActionIfNone()
        {
            var option = Option<string>.None();
            var called = false;

            option.IfNone(() =>
            {
                called = true;
            });

            Assert.True(called);
        }

        [Test]
        public void IfYeah_ShouldBehaveExecuteLike_IfSome()
        {
            var option = Option<int>.Yeah(42);
            var yeahCalled = false;
            var someCalled = false;

            option.IfYeah(_ => yeahCalled = true);
            option.IfSome(_ => someCalled = true);

            Assert.AreEqual(yeahCalled, someCalled);
        }

        [Test]
        public void IfNah_ShouldBehaveExecuteLike_IfNone()
        {
            var option = Option<int>.Nah();
            var nahCalled = false;
            var noneCalled = false;

            option.IfNah(() => nahCalled = true);
            option.IfNone(() => noneCalled = true);

            Assert.AreEqual(nahCalled, noneCalled);
        }

        #endregion

        #region Where

        [Test]
        public void Where_ShouldReturnSameOptionIfPredicateIsTrue()
        {
            var option = Option<int>.Some(10);
            var result = option.Where(x => x > 5);

            Assert.True(result.IsSome);
            Assert.AreEqual(10, result.GetValueOrDefault());
        }

        [Test]
        public void Where_ShouldReturnNoneIfPredicateIsFalse()
        {
            var option = Option<int>.Some(3);
            var result = option.Where(x => x > 5);

            Assert.True(result.IsNone);
        }

        [Test]
        public void Where_ShouldNotAffectNone()
        {
            var option = Option<int>.None();
            var result = option.Where(x => x > 5);

            Assert.True(result.IsNone);
        }

        #endregion

        #region ToResult

        [Test]
        public void ToResult_ShouldConvertSomeToSuccess()
        {
            var option = Option<int>.Some(42);
            var result = option.ToResult("No value available");

            Assert.True(result.IsSuccess);
            Assert.AreEqual(42, result.Value);
        }

        [Test]
        public void ToResult_ShouldConvertNoneToFailure()
        {
            var option = Option<int>.None();
            var result = option.ToResult("No value available");

            Assert.False(result.IsSuccess);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("No value available", result.Errors.First());
        }

        #endregion

        #region ToEnumerable

        [Test]
        public void ToEnumerable_ShouldReturnSingleElementForSome()
        {
            var option = Option<int>.Some(7);
            var list = option.ToEnumerable().ToList();

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(7, list.First());
        }

        [Test]
        public void ToEnumerable_ShouldReturnEmptyForNone()
        {
            var option = Option<int>.None();
            var list = option.ToEnumerable().ToList();

            Assert.IsEmpty(list);
        }

        #endregion
    }
}
