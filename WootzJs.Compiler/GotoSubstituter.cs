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

using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace WootzJs.Compiler
{
    public class GotoSubstituter : SyntaxRewriter
    {
        private Compilation compilation;
        private Dictionary<object, State> labelStates;
        private State currentState;

        public GotoSubstituter(Compilation compilation, Dictionary<object, State> labelStates) 
        {
            this.compilation = compilation;
            this.labelStates = labelStates;
        }

        public override SyntaxNode VisitGotoStatement(GotoStatementSyntax node)
        {
            var label = node.Expression.ToString();
            if (label.StartsWith("$"))
                return node;

            return AsyncStateGenerator.ChangeState(labelStates[label]);
        }

        public void GenerateStates()
        {
            var lastState = new State(this);
            lastState.Statements.Add(Cs.Return(Cs.False()));

            currentState = new State(this) { NextState = lastState };
            node.Accept(this);

            // Post-process goto statements
            if (labelStates.Any())
            {
                var gotoSubstituter = new GotoSubstituter(compilation, labelStates);
                foreach (var state in states)
                {
                    state.Statements = state.Statements.Select(x => (StatementSyntax)x.Accept(gotoSubstituter)).ToList();
                }
            }
        }

    }
}
