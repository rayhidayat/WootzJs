#region License
//-----------------------------------------------------------------------
// <copyright>
// The MIT License (MIT)
// 
// Copyright (c) 2014 Kirk S Woll
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;
using WootzJs.Testing;

namespace WootzJs.Compiler.Tests.Linq.Expressions
{
    public class ConditionalExpressionTests : TestFixture
    {
        [Test]
        public void Conditional()
        {
            Expression<Func<bool, string>> lambda = x => x ? "yes" : "no";
            var conditional = (ConditionalExpression)lambda.Body;

            AssertEquals(conditional.Test, lambda.Parameters[0]);

            var ifTrue = (ConstantExpression)conditional.IfTrue;
            var ifFalse = (ConstantExpression)conditional.IfFalse;

            AssertEquals(ifTrue.Value, "yes");
            AssertEquals(ifFalse.Value, "no");
        }
    }
}
