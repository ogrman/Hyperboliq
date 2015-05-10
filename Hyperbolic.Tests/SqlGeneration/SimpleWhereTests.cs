﻿using Xunit;
using static Hyperboliq.Tests.SqlStreamExtensions;
using Hyperboliq.Tests.Model;
using static Hyperboliq.Domain.Types;
using static Hyperboliq.Domain.Stream;
using static Hyperboliq.Domain.SqlGen;
using FluentAssertions;

namespace Hyperboliq.Tests.SqlGeneration
{
    /*
    [Trait("SqlGeneration", "Where")]
    public class SqlGeneration_SimpleWhereTests
    {
        [Fact]
        public void ItShouldBePossibleToSqlifyAWhere()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42))));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42");
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyAWhereWithAndAndOr()
        {
            var stream = 
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                       BinExp(
                           BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(42)),
                           BinaryOperation.Or,
                           BinExp(
                               BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(10)),
                               BinaryOperation.And,
                               BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'"))
                           )
                    )));

            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age > 42 OR PersonRef.Age < 10 AND PersonRef.Name = 'Karl'");
        }

        [Fact]
        public void ItShouldBePossibleToSqlifyAWhereWithAndOrsThatIsNotInBinaryExpressions()
        {
            var stream =
                SelectNode(
                    Select(Col<Person>("*")),
                    From<Person>(),
                    Where(
                        BinExp(Col<Person>("Age"), BinaryOperation.LessThan, Const(42)),
                        And(BinExp(Col<Person>("Age"), BinaryOperation.GreaterThan, Const(12))),
                        Or(BinExp(Col<Person>("Name"), BinaryOperation.Equal, Const("'Karl'")))));
            var result = SqlifyExpression(AnsiSql.Dialect, stream);
            result.Should().Be(@"SELECT PersonRef.* FROM Person PersonRef WHERE PersonRef.Age < 42 AND PersonRef.Age > 12 OR PersonRef.Name = 'Karl'");
        }
    }
    */
}
