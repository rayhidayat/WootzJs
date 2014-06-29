﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WootzJs.Compiler
{
    public class AsyncState
    {
        public int Index { get; set; }
        public AsyncState Parent { get; set; }
        public AsyncState CurrentState { get; set; }
        public IEnumerable<StatementSyntax> Statements { get; private set; }
        public AsyncState Next { get; set; }
        public List<AsyncState> Substates { get; set; }
        public Func<SwitchStatementSyntax, StatementSyntax> Wrap { get; set; }
            
        public AsyncState()
        {
            Statements = new List<StatementSyntax>();
            Substates = new List<AsyncState>();
            Wrap = switchStatement => switchStatement;
        }

        public IEnumerable<AsyncState> GetAllSubstates()
        {
            foreach (var state in Substates)
            {
                yield return state;
                foreach (var current in state.GetAllSubstates())
                {
                    yield return current;
                }
            }
        }

        public IEnumerable<int> GetAllIndices()
        {
            yield return Index;
            foreach (var substate in Substates)
            {
                foreach (var index in substate.GetAllIndices())
                {
                    yield return index;
                }
            }
        }

        public void Add(StatementSyntax statement)
        {
            if (Substates.Any())
                throw new Exception("Cannot add a statement to a state that contains substates");

            InternalAdd(statement);
        }

        internal void InternalAdd(StatementSyntax statement)
        {
            ((List<StatementSyntax>)Statements).Add(statement);        
        }

        public override string ToString()
        {
            return Index + ": " + string.Join(" ", Statements);
        }
    }
}