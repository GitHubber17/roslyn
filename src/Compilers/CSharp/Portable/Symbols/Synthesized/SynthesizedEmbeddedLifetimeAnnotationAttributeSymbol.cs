﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    internal sealed class SynthesizedEmbeddedLifetimeAnnotationAttributeSymbol : SynthesizedEmbeddedAttributeSymbolBase
    {
        private readonly ImmutableArray<FieldSymbol> _fields;
        private readonly ImmutableArray<MethodSymbol> _constructors;

        public SynthesizedEmbeddedLifetimeAnnotationAttributeSymbol(
            string name,
            NamespaceSymbol containingNamespace,
            ModuleSymbol containingModule,
            NamedTypeSymbol systemAttributeType,
            TypeSymbol boolType)
            : base(name, containingNamespace, containingModule, baseType: systemAttributeType)
        {
            _fields = ImmutableArray.Create<FieldSymbol>(
                new SynthesizedFieldSymbol(this, boolType, "IsRefScoped", isPublic: true, isReadOnly: true, isStatic: false),
                new SynthesizedFieldSymbol(this, boolType, "IsValueScoped", isPublic: true, isReadOnly: true, isStatic: false));

            _constructors = ImmutableArray.Create<MethodSymbol>(
                new SynthesizedEmbeddedAttributeConstructorWithBodySymbol(
                    this,
                    m => ImmutableArray.Create(
                        SynthesizedParameterSymbol.Create(m, TypeWithAnnotations.Create(boolType), 0, RefKind.None),
                        SynthesizedParameterSymbol.Create(m, TypeWithAnnotations.Create(boolType), 1, RefKind.None)),
                    (f, s, p) => GenerateConstructorBody(f, s, p)));
        }

        internal override IEnumerable<FieldSymbol> GetFieldsToEmit() => _fields;

        public override ImmutableArray<MethodSymbol> Constructors => _constructors;

        internal override AttributeUsageInfo GetAttributeUsageInfo() =>
            new AttributeUsageInfo(AttributeTargets.Parameter, allowMultiple: false, inherited: false);

        private void GenerateConstructorBody(SyntheticBoundNodeFactory factory, ArrayBuilder<BoundStatement> statements, ImmutableArray<ParameterSymbol> parameters)
        {
            statements.Add(
                factory.ExpressionStatement(
                    factory.AssignmentExpression(
                        factory.Field(factory.This(), _fields[0]),
                        factory.Parameter(parameters[0]))));
            statements.Add(
                factory.ExpressionStatement(
                    factory.AssignmentExpression(
                        factory.Field(factory.This(), _fields[1]),
                        factory.Parameter(parameters[1]))));
        }
    }
}
