﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.CodeFixes
Imports Microsoft.CodeAnalysis.Diagnostics
Imports Microsoft.CodeAnalysis.Editor.UnitTests.Diagnostics.NamingStyles
Imports Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.Diagnostics
Imports Microsoft.CodeAnalysis.VisualBasic.GenerateConstructor

Namespace Microsoft.CodeAnalysis.Editor.VisualBasic.UnitTests.GenerateConstructor
    Public Class GenerateConstructorTests
        Inherits AbstractVisualBasicDiagnosticProviderBasedUserDiagnosticTest

        Friend Overrides Function CreateDiagnosticProviderAndFixer(workspace As Workspace) As (DiagnosticAnalyzer, CodeFixProvider)
            Return (Nothing, New GenerateConstructorCodeFixProvider())
        End Function

        Private ReadOnly options As NamingStylesTestOptionSets = New NamingStylesTestOptionSets(LanguageNames.VisualBasic)

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateIntoContainingType() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub Main()
        Dim f = New C([|4|], 5, 6)
    End Sub
End Class",
"Class C
    Private v1 As Integer
    Private v2 As Integer
    Private v3 As Integer

    Public Sub New(v1 As Integer, v2 As Integer, v3 As Integer)
        Me.v1 = v1
        Me.v2 = v2
        Me.v3 = v3
    End Sub

    Sub Main()
        Dim f = New C(4, 5, 6)
    End Sub
End Class")
        End Function

        <WorkItem(44537, "https://github.com/dotnet/roslyn/issues/44537")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateIntoContainingType_WithProperties() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub Main()
        Dim f = New C([|4|], 5, 6)
    End Sub
End Class",
"Class C
    Public Sub New(v1 As Integer, v2 As Integer, v3 As Integer)
        Me.V1 = v1
        Me.V2 = v2
        Me.V3 = v3
    End Sub

    Public ReadOnly Property V1 As Integer
    Public ReadOnly Property V2 As Integer
    Public ReadOnly Property V3 As Integer

    Sub Main()
        Dim f = New C(4, 5, 6)
    End Sub
End Class", index:=1)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateIntoContainingType_NoMembers() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub Main()
        Dim f = New C([|4|], 5, 6)
    End Sub
End Class",
"Class C
    Public Sub New(v1 As Integer, v2 As Integer, v3 As Integer)
    End Sub

    Sub Main()
        Dim f = New C(4, 5, 6)
    End Sub
End Class", index:=2)
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestInvokingFromInsideAnotherConstructor() As Task
            Await TestInRegularAndScriptAsync(
"Class A
    Private v As B
    Public Sub New()
        Me.v = New B([|5|])
    End Sub
End Class
Friend Class B
End Class",
"Class A
    Private v As B
    Public Sub New()
        Me.v = New B(5)
    End Sub
End Class
Friend Class B
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMissingGenerateDefaultConstructor() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a = New [|A|]()
    End Sub
End Class
Class A
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMissingGenerateDefaultConstructorInStructure() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a = New [|A|]()
    End Sub
End Class
Structure A
End Structure")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOfferDefaultConstructorWhenOverloadExists() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a = [|New A()|]
    End Sub
End Class
Class A
    Sub New(x As Integer)
    End Sub
End Class",
"Class Test
    Sub Main()
        Dim a = New A()
    End Sub
End Class
Class A
    Public Sub New()
    End Sub

    Sub New(x As Integer)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestParameterizedConstructorOffered1() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a = New A([|1|])
    End Sub
End Class
Class A
End Class",
"Class Test
    Sub Main()
        Dim a = New A(1)
    End Sub
End Class
Class A
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestParameterizedConstructorOffered2() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a = New A([|1|])
    End Sub
End Class
Class A
    Public Sub New()
    End Sub
End Class",
"Class Test
    Sub Main()
        Dim a = New A(1)
    End Sub
End Class
Class A
    Private v As Integer

    Public Sub New()
    End Sub

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(527627, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/527627"), WorkItem(539735, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539735"), WorkItem(539735, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539735")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestInAsNewExpression() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a As New A([|1|])
    End Sub
End Class
Class A
    Public Sub New()
    End Sub
End Class",
"Class Test
    Sub Main()
        Dim a As New A(1)
    End Sub
End Class
Class A
    Private v As Integer

    Public Sub New()
    End Sub

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateInPartialClass1() As Task
            Await TestInRegularAndScriptAsync(
"Public Partial Class Test
    Public Sub S1()
    End Sub
End Class
Public Class Test
    Public Sub S2()
    End Sub
End Class
Public Class A
    Sub Main()
        Dim s = New Test([|5|])
    End Sub
End Class",
"Public Partial Class Test
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub

    Public Sub S1()
    End Sub
End Class
Public Class Test
    Public Sub S2()
    End Sub
End Class
Public Class A
    Sub Main()
        Dim s = New Test(5)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateInPartialClassWhenArityDoesntMatch() As Task
            Await TestInRegularAndScriptAsync(
"Public Partial Class Test
    Public Sub S1()
    End Sub
End Class
Public Class Test(Of T)
    Public Sub S2()
    End Sub
End Class
Public Class A
    Sub Main()
        Dim s = New Test([|5|])
    End Sub
End Class",
"Public Partial Class Test
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub

    Public Sub S1()
    End Sub
End Class
Public Class Test(Of T)
    Public Sub S2()
    End Sub
End Class
Public Class A
    Sub Main()
        Dim s = New Test(5)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateInPartialClassWithConflicts() As Task
            Await TestInRegularAndScriptAsync(
"Public Partial Class Test2
End Class
Private Partial Class Test2
End Class
Public Class A
    Sub Main()
        Dim s = New Test2([|5|])
    End Sub
End Class",
"Public Partial Class Test2
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
Private Partial Class Test2
End Class
Public Class A
    Sub Main()
        Dim s = New Test2(5)
    End Sub
End Class")
        End Function

        <WorkItem(528257, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528257")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateIntoInaccessibleType() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class Goo
    Private Class Bar
    End Class
End Class
Class A
    Sub Main()
        Dim s = New Goo.Bar([|5|])
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOnNestedTypes() As Task
            Await TestInRegularAndScriptAsync(
"Class Goo
    Class Bar
    End Class
End Class
Class A
    Sub Main()
        Dim s = New Goo.Bar([|5|])
    End Sub
End Class",
"Class Goo
    Class Bar
        Private v As Integer

        Public Sub New(v As Integer)
            Me.v = v
        End Sub
    End Class
End Class
Class A
    Sub Main()
        Dim s = New Goo.Bar(5)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOnNestedPartialTypes() As Task
            Await TestInRegularAndScriptAsync(
"Public Partial Class Test
    Public Partial Class NestedTest
        Public Sub S1()
        End Sub
    End Class
End Class
Public Partial Class Test
    Public Partial Class NestedTest
        Public Sub S2()
        End Sub
    End Class
End Class
Class A
    Sub Main()
        Dim s = New Test.NestedTest([|5|])
    End Sub
End Class",
"Public Partial Class Test
    Public Partial Class NestedTest
        Private v As Integer

        Public Sub New(v As Integer)
            Me.v = v
        End Sub

        Public Sub S1()
        End Sub
    End Class
End Class
Public Partial Class Test
    Public Partial Class NestedTest
        Public Sub S2()
        End Sub
    End Class
End Class
Class A
    Sub Main()
        Dim s = New Test.NestedTest(5)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOnNestedGenericType() As Task
            Await TestInRegularAndScriptAsync(
"Class Outer(Of T)
    Public Class Inner
    End Class
    Public i = New Inner([|5|])
End Class",
"Class Outer(Of T)
    Public Class Inner
        Private v As Integer

        Public Sub New(v As Integer)
            Me.v = v
        End Sub
    End Class
    Public i = New Inner(5)
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOnGenericTypes1() As Task
            Await TestInRegularAndScriptAsync(
"Class Base(Of T, V)
End Class
Class Test
    Sub Goo()
        Dim a = New Base(Of Integer, Integer)([|5|], 5)
    End Sub
End Class",
"Class Base(Of T, V)
    Private v1 As Integer
    Private v2 As Integer

    Public Sub New(v1 As Integer, v2 As Integer)
        Me.v1 = v1
        Me.v2 = v2
    End Sub
End Class
Class Test
    Sub Goo()
        Dim a = New Base(Of Integer, Integer)(5, 5)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOnGenericTypes2() As Task
            Await TestInRegularAndScriptAsync(
"Class Base(Of T, V)
End Class
Class Derived(Of V)
    Inherits Base(Of Integer, V)
End Class
Class Test
    Sub Goo()
        Dim a = New Base(Of Integer, Integer)(5, 5)
        Dim b = New Derived(Of Integer)([|5|])
    End Sub
End Class",
"Class Base(Of T, V)
End Class
Class Derived(Of V)
    Inherits Base(Of Integer, V)

    Private v1 As Integer

    Public Sub New(v1 As Integer)
        Me.v1 = v1
    End Sub
End Class
Class Test
    Sub Goo()
        Dim a = New Base(Of Integer, Integer)(5, 5)
        Dim b = New Derived(Of Integer)(5)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOnGenericTypes3() As Task
            Await TestInRegularAndScriptAsync(
"Class Base(Of T, V)
End Class
Class Derived(Of V)
    Inherits Base(Of Integer, V)
End Class
Class MoreDerived
    Inherits Derived(Of Double)
End Class
Class Test
    Sub Goo()
        Dim a = New Base(Of Integer, Integer)(5, 5)
        Dim b = New Derived(Of Integer)(5)
        Dim c = New MoreDerived([|5.5|])
    End Sub
End Class",
"Class Base(Of T, V)
End Class
Class Derived(Of V)
    Inherits Base(Of Integer, V)
End Class
Class MoreDerived
    Inherits Derived(Of Double)

    Private v As Double

    Public Sub New(v As Double)
        Me.v = v
    End Sub
End Class
Class Test
    Sub Goo()
        Dim a = New Base(Of Integer, Integer)(5, 5)
        Dim b = New Derived(Of Integer)(5)
        Dim c = New MoreDerived(5.5)
    End Sub
End Class")
        End Function

        <WorkItem(528244, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528244")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDateTypeForInference() As Task
            Await TestInRegularAndScriptAsync(
"Class Goo
End Class
Class A
    Sub Main()
        Dim s = New Goo([|Date.Now|])
    End Sub
End Class",
"Class Goo
    Private now As Date

    Public Sub New(now As Date)
        Me.now = now
    End Sub
End Class
Class A
    Sub Main()
        Dim s = New Goo(Date.Now)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestBaseConstructor() As Task
            Await TestInRegularAndScriptAsync(
"Class Base
End Class
Class Derived
    Inherits Base
    Private x As Integer
    Public Sub New(x As Integer)
        MyBase.New([|x|])
        Me.x = x
    End Sub
End Class",
"Class Base
    Private x As Integer

    Public Sub New(x As Integer)
        Me.x = x
    End Sub
End Class
Class Derived
    Inherits Base
    Private x As Integer
    Public Sub New(x As Integer)
        MyBase.New(x)
        Me.x = x
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMustInheritBase() As Task
            Await TestInRegularAndScriptAsync(
"MustInherit Class Base
End Class
Class Derived
    Inherits Base
    Shared x As Integer
    Public Sub New(x As Integer)
        MyBase.New([|x|]) 'This should generate a protected ctor in Base 
        Derived.x = x
    End Sub
    Sub Test1()
        Dim a As New Derived(1)
    End Sub
End Class",
"MustInherit Class Base
    Private x As Integer

    Protected Sub New(x As Integer)
        Me.x = x
    End Sub
End Class
Class Derived
    Inherits Base
    Shared x As Integer
    Public Sub New(x As Integer)
        MyBase.New(x) 'This should generate a protected ctor in Base 
        Derived.x = x
    End Sub
    Sub Test1()
        Dim a As New Derived(1)
    End Sub
End Class")
        End Function

        <WorkItem(540586, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540586")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMissingOnNoCloseParen() As Task
            Await TestMissingInRegularAndScriptAsync(
"Imports System
Imports System.Collections.Generic
Imports System.Linq
Module Program
    Sub Main(args As String())
        Dim c = New [|goo|]( 
 End Sub
End Module
Class goo
End Class")
        End Function

        <WorkItem(540545, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540545")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestConversionError() As Task
            Await TestInRegularAndScriptAsync(
"Imports System
Module Program
    Sub Main(args As String())
        Dim i As Char
        Dim cObject As C = New C([|i|])
        Console.WriteLine(cObject.v1)
    End Sub
End Module
Class C
    Public v1 As Integer
    Public Sub New(v1 As Integer)
        Me.v1 = v1
    End Sub
End Class",
"Imports System
Module Program
    Sub Main(args As String())
        Dim i As Char
        Dim cObject As C = New C(i)
        Console.WriteLine(cObject.v1)
    End Sub
End Module
Class C
    Public v1 As Integer
    Private i As Char

    Public Sub New(v1 As Integer)
        Me.v1 = v1
    End Sub

    Public Sub New(i As Char)
        Me.i = i
    End Sub
End Class")
        End Function

        <WorkItem(540642, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540642")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestNotOnNestedConstructor() As Task
            Await TestInRegularAndScriptAsync(
"Module Program
    Sub Main(args As String())
        Dim x As C = New C([|New C()|])
    End Sub
End Module
Friend Class C
End Class",
"Module Program
    Sub Main(args As String())
        Dim x As C = New C(New C())
    End Sub
End Module
Friend Class C
    Private c As C

    Public Sub New(c As C)
        Me.c = c
    End Sub
End Class")
        End Function

        <WorkItem(540607, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540607")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestUnavailableTypeParameters() As Task
            Await TestInRegularAndScriptAsync(
"Class C(Of T1, T2)
    Sub M(x As T1, y As T2)
        Dim a As Test = New Test([|x|], y)
    End Sub
End Class
Friend Class Test
End Class",
"Class C(Of T1, T2)
    Sub M(x As T1, y As T2)
        Dim a As Test = New Test(x, y)
    End Sub
End Class
Friend Class Test
    Private x As Object
    Private y As Object

    Public Sub New(x As Object, y As Object)
        Me.x = x
        Me.y = y
    End Sub
End Class")
        End Function

        <WorkItem(540748, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540748")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestKeywordArgument1() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Private [Class] As Integer = 5
    Sub Main()
        Dim a = New A([|[Class]|])
    End Sub
End Class
Class A
End Class",
"Class Test
    Private [Class] As Integer = 5
    Sub Main()
        Dim a = New A([Class])
    End Sub
End Class
Class A
    Private [class] As Integer

    Public Sub New([class] As Integer)
        Me.class = [class]
    End Sub
End Class")
        End Function

        <WorkItem(540747, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540747")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestKeywordArgument2() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Sub Main()
        Dim a = New A([|Class|])
    End Sub
End Class
Class A
End Class",
"Class Test
    Sub Main()
        Dim a = New A(Class)
    End Sub
End Class
Class A
    Private value As Object

    Public Sub New(value As Object)
        Me.value = value
    End Sub
End Class")
        End Function

        <WorkItem(540746, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540746")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestConflictWithTypeParameterName() As Task
            Await TestInRegularAndScriptAsync(
"Class Test
    Sub Goo()
        Dim a = New Bar(Of Integer)([|5|])
    End Sub
End Class
Class Bar(Of V)
End Class",
"Class Test
    Sub Goo()
        Dim a = New Bar(Of Integer)(5)
    End Sub
End Class
Class Bar(Of V)
    Private v1 As Integer

    Public Sub New(v1 As Integer)
        Me.v1 = v1
    End Sub
End Class")
        End Function

        <WorkItem(541174, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541174")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingReadonlyField() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    ReadOnly x As Integer
    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C([|x|])
    End Sub
End Class",
"Class C
    ReadOnly x As Integer

    Public Sub New(x As Integer)
        Me.x = x
    End Sub

    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C(x)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingProperty() As Task
            Await TestInRegularAndScriptAsync(
"Class Program
    Sub Test()
        Dim x = New A([|P|]:=5)
    End Sub
End Class
Class A
    Public Property P As Integer
End Class",
"Class Program
    Sub Test()
        Dim x = New A(P:=5)
    End Sub
End Class
Class A
    Public Sub New(P As Integer)
        Me.P = P
    End Sub

    Public Property P As Integer
End Class")
        End Function

        <WorkItem(542055, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542055")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingMethod() As Task
            Await TestInRegularAndScriptAsync(
"Class A
    Sub Test()
        Dim t As New C([|u|]:=5)
    End Sub
End Class
Class C
    Public Sub u()
    End Sub
End Class",
"Class A
    Sub Test()
        Dim t As New C(u:=5)
    End Sub
End Class
Class C
    Private u1 As Integer

    Public Sub New(u As Integer)
        u1 = u
    End Sub

    Public Sub u()
    End Sub
End Class")
        End Function

        <WorkItem(542055, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542055")>
        <WorkItem(14077, "https://github.com/dotnet/roslyn/issues/14077")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDetectAssignmentToSharedFieldFromInstanceConstructor() As Task
            Await TestInRegularAndScriptAsync(
"Class Program
    Sub Test()
        Dim x = New A([|P|]:=5)
    End Sub
End Class
Class A
    Shared Property P As Integer
End Class",
"Class Program
    Sub Test()
        Dim x = New A(P:=5)
    End Sub
End Class
Class A
    Private p As Integer

    Public Sub New(P As Integer)
        Me.p = P
    End Sub

    Shared Property P As Integer
End Class")
        End Function

        <WorkItem(542055, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542055")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingFieldWithSameNameButIncompatibleType() As Task
            Await TestInRegularAndScriptAsync(
"Class A
    Sub Test()
        Dim t As New B([|x|]:=5)
    End Sub
End Class
Class B
    Private x As String
End Class",
"Class A
    Sub Test()
        Dim t As New B(x:=5)
    End Sub
End Class
Class B
    Private x As String
    Private x1 As Integer

    Public Sub New(x As Integer)
        x1 = x
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingFieldFromBaseClass() As Task
            Await TestInRegularAndScriptAsync(
"Class A
    Sub Test()
        Dim t As New C([|u|]:=5)
    End Sub
End Class
Class C
    Inherits B
    Private x As String
End Class
Class B
    Protected u As Integer
End Class",
"Class A
    Sub Test()
        Dim t As New C(u:=5)
    End Sub
End Class
Class C
    Inherits B
    Private x As String

    Public Sub New(u As Integer)
        Me.u = u
    End Sub
End Class
Class B
    Protected u As Integer
End Class")
        End Function

        <WorkItem(542098, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542098")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMeConstructorInitializer() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub New
        Me.New([|1|])
    End Sub
End Class",
"Class C
    Private v As Integer

    Sub New
        Me.New(1)
    End Sub

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(542098, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542098")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingMeConstructorInitializer() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class C
    Private v As Integer
    Sub New
        Me.[|New|](1)
    End Sub
    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(542098, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542098")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMyBaseConstructorInitializer() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub New
        MyClass.New([|1|])
    End Sub
End Class",
"Class C
    Private v As Integer

    Sub New
        MyClass.New(1)
    End Sub

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(542098, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542098")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingMyBaseConstructorInitializer() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class C
    Private v As Integer
    Sub New
        MyClass.[|New|](1)
    End Sub
    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(542098, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542098")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMyClassConstructorInitializer() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Inherits B
    Sub New
        MyBase.New([|1|])
    End Sub
End Class
Class B
End Class",
"Class C
    Inherits B
    Sub New
        MyBase.New(1)
    End Sub
End Class
Class B
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(542098, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542098")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestExistingMyClassConstructorInitializer() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class C
    Inherits B
    Sub New
        MyBase.New([|1|])
    End Sub
End Class
Class B
    Protected Sub New(v As Integer)
    End Sub
End Class")
        End Function

        <WorkItem(542056, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542056")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestConflictingFieldNameInSubclass() As Task
            Await TestInRegularAndScriptAsync(
"Class A
    Sub Test()
        Dim t As New C([|u|]:=5)
    End Sub
End Class
Class C
    Inherits B
    Private x As String
End Class
Class B
    Protected u As String
End Class",
"Class A
    Sub Test()
        Dim t As New C(u:=5)
    End Sub
End Class
Class C
    Inherits B
    Private x As String
    Private u1 As Integer

    Public Sub New(u As Integer)
        u1 = u
    End Sub
End Class
Class B
    Protected u As String
End Class")
        End Function

        <WorkItem(542442, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542442")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestNothingArgument() As Task
            Await TestMissingInRegularAndScriptAsync(
"Class C1
    Public Sub New(ByVal accountKey As Integer)
        Me.new()
        Me.[|new|](accountKey, Nothing)
    End Sub
    Public Sub New(ByVal accountKey As Integer, ByVal accountName As String)
        Me.New(accountKey, accountName, Nothing)
    End Sub
    Public Sub New(ByVal accountKey As Integer, ByVal accountName As String, ByVal accountNumber As String)
    End Sub
End Class")
        End Function

        <WorkItem(540641, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540641")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMissingOnExistingConstructor() As Task
            Await TestMissingInRegularAndScriptAsync(
"Module Program
    Sub M()
        Dim x As C = New [|C|](P)
    End Sub
End Module
Class C
    Private v As Object
    Public Sub New(v As Object)
        Me.v = v
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerationIntoVisibleType() As Task
            Await TestInRegularAndScriptAsync(
"#ExternalSource (""Default.aspx"", 1) 
Class C
    Sub Goo()
        Dim x As New D([|5|])
    End Sub
End Class
Class D
End Class
#End ExternalSource",
"#ExternalSource (""Default.aspx"", 1) 
Class C
    Sub Goo()
        Dim x As New D(5)
    End Sub
End Class
Class D
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
#End ExternalSource")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestNoGenerationIntoEntirelyHiddenType() As Task
            Await TestMissingInRegularAndScriptAsync(
<Text>#ExternalSource (""Default.aspx"", 1)
Class C
    Sub Goo()
        Dim x As New D([|5|])
    End Sub
End Class
#End ExternalSource

Class D

End Class
</Text>.Value)
        End Function

        <WorkItem(546030, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546030")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestConflictingDelegatedParameterNameAndNamedArgumentName1() As Task
            Await TestInRegularAndScriptAsync(
"Module Program
    Sub Main(args As String())
        Dim objc As New C(1, [|prop|]:=""Property"")
    End Sub
End Module

Class C
    Private prop As String

    Public Sub New(prop As String)
        Me.prop = prop
    End Sub
End Class",
"Module Program
    Sub Main(args As String())
        Dim objc As New C(1, prop:=""Property"")
    End Sub
End Module

Class C
    Private prop As String
    Private v As Integer

    Public Sub New(prop As String)
        Me.prop = prop
    End Sub

    Public Sub New(v As Integer, prop As String)
        Me.v = v
        Me.prop = prop
    End Sub
End Class")
        End Function

        <Fact(), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestFormattingInGenerateConstructor() As Task
            Await TestInRegularAndScriptAsync(
<Text>Class C
    Sub New()
        MyClass.New([|1|])
    End Sub
End Class</Text>.Value.Replace(vbLf, vbCrLf),
<Text>Class C
    Private v As Integer

    Sub New()
        MyClass.New(1)
    End Sub

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class</Text>.Value.Replace(vbLf, vbCrLf))
        End Function

        <WorkItem(530003, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530003")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestAttributesWithArgument() As Task
            Await TestInRegularAndScriptAsync(
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute
End Class
[|<MyAttribute(123)>|]
Public Class D
End Class",
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute

    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
<MyAttribute(123)>
Public Class D
End Class")
        End Function

        <WorkItem(530003, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530003")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestAttributesWithMultipleArguments() As Task
            Await TestInRegularAndScriptAsync(
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute
End Class
[|<MyAttribute(true, 1, ""hello"")>|]
Public Class D
End Class",
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute

    Private v1 As Boolean
    Private v2 As Integer
    Private v3 As String

    Public Sub New(v1 As Boolean, v2 As Integer, v3 As String)
        Me.v1 = v1
        Me.v2 = v2
        Me.v3 = v3
    End Sub
End Class
<MyAttribute(true, 1, ""hello"")>
Public Class D
End Class")
        End Function

        <WorkItem(530003, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530003")>
        <WorkItem(14077, "https://github.com/dotnet/roslyn/issues/14077")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestAttributesWithNamedArguments() As Task
            Await TestInRegularAndScriptAsync(
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute
End Class
[|<MyAttribute(true, 1, Topic:=""hello"")>|]
Public Class D
End Class",
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute

    Private v1 As Boolean
    Private v2 As Integer
    Private topic As String

    Public Sub New(v1 As Boolean, v2 As Integer, Topic As String)
        Me.v1 = v1
        Me.v2 = v2
        Me.topic = Topic
    End Sub
End Class
<MyAttribute(true, 1, Topic:=""hello"")>
Public Class D
End Class")
        End Function

        <WorkItem(530003, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530003")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestAttributesWithAdditionalConstructors() As Task
            Await TestInRegularAndScriptAsync(
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute
    Private v As Integer
    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
[|<MyAttribute(True, 2)>|]
Public Class D
End Class",
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute
    Private v As Integer
    Private v1 As Boolean
    Private v2 As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub

    Public Sub New(v1 As Boolean, v2 As Integer)
        Me.v1 = v1
        Me.v2 = v2
    End Sub
End Class
<MyAttribute(True, 2)>
Public Class D
End Class")
        End Function

        <WorkItem(530003, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530003")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestAttributesWithAllValidArguments() As Task
            Await TestInRegularAndScriptAsync(
"Enum A
    A1
End Enum
<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute
End Class
[|<MyAttribute(New Short(1) {1, 2, 3}, A.A1, True, 1, ""Z""c, 5S, 1I, 5L, 6.0R, 2.1F, ""abc"")>|]
Public Class D End Class",
"Enum A
    A1
End Enum
<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute

    Private shorts As Short()
    Private a1 As A
    Private v1 As Boolean
    Private v2 As Integer
    Private v3 As Char
    Private v4 As Short
    Private v5 As Integer
    Private v6 As Long
    Private v7 As Double
    Private v8 As Single
    Private v9 As String

    Public Sub New(shorts() As Short, a1 As A, v1 As Boolean, v2 As Integer, v3 As Char, v4 As Short, v5 As Integer, v6 As Long, v7 As Double, v8 As Single, v9 As String)
        Me.shorts = shorts
        Me.a1 = a1
        Me.v1 = v1
        Me.v2 = v2
        Me.v3 = v3
        Me.v4 = v4
        Me.v5 = v5
        Me.v6 = v6
        Me.v7 = v7
        Me.v8 = v8
        Me.v9 = v9
    End Sub
End Class
<MyAttribute(New Short(1) {1, 2, 3}, A.A1, True, 1, ""Z""c, 5S, 1I, 5L, 6.0R, 2.1F, ""abc"")>
Public Class D End Class")
        End Function

        <WorkItem(530003, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/530003")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestAttributesWithLambda() As Task
            Await TestMissingInRegularAndScriptAsync(
"<AttributeUsage(AttributeTargets.Class)>
Public Class MyAttribute
    Inherits System.Attribute End Class 
 [|<MyAttribute(Function(x) x + 1)>|]
    Public Class D
    End Class")
        End Function

        <WorkItem(889349, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/889349")>
        <Fact(), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestConstructorGenerationForDifferentNamedParameter() As Task
            Await TestInRegularAndScriptAsync(
<Text>Class Program
    Sub Main(args As String())
        Dim a = New Program([|y:=4|])
    End Sub

    Sub New(x As Integer)

    End Sub
End Class</Text>.Value.Replace(vbLf, vbCrLf),
<Text>Class Program
    Private y As Integer

    Sub Main(args As String())
        Dim a = New Program(y:=4)
    End Sub

    Sub New(x As Integer)

    End Sub

    Public Sub New(y As Integer)
        Me.y = y
    End Sub
End Class</Text>.Value.Replace(vbLf, vbCrLf))
        End Function

        <WorkItem(897355, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/897355")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestOptionStrictOn() As Task
            Await TestInRegularAndScriptAsync(
"Option Strict On
Module Module1
    Sub Main()
        Dim int As Integer = 3
        Dim obj As Object = New Object()
        Dim c1 As Classic = New Classic(int)
        Dim c2 As Classic = [|New Classic(obj)|]
    End Sub
    Class Classic
        Private int As Integer
        Public Sub New(int As Integer)
            Me.int = int
        End Sub
    End Class
End Module",
"Option Strict On
Module Module1
    Sub Main()
        Dim int As Integer = 3
        Dim obj As Object = New Object()
        Dim c1 As Classic = New Classic(int)
        Dim c2 As Classic = New Classic(obj)
    End Sub
    Class Classic
        Private int As Integer
        Private obj As Object

        Public Sub New(int As Integer)
            Me.int = int
        End Sub

        Public Sub New(obj As Object)
            Me.obj = obj
        End Sub
    End Class
End Module")
        End Function

        <WorkItem(528257, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528257")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateInInaccessibleType() As Task
            Await TestInRegularAndScriptAsync(
"Class Goo
    Private Class Bar
    End Class
End Class
Class A
    Sub Main()
        Dim s = New [|Goo.Bar(5)|]
    End Sub
End Class",
"Class Goo
    Private Class Bar
        Private v As Integer

        Public Sub New(v As Integer)
            Me.v = v
        End Sub
    End Class
End Class
Class A
    Sub Main()
        Dim s = New Goo.Bar(5)
    End Sub
End Class")
        End Function

        <WorkItem(1241, "https://github.com/dotnet/roslyn/issues/1241")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateConstructorInIncompleteLambda() As Task
            Await TestInRegularAndScriptAsync(
"Imports System
Imports System.Linq
Class C
    Sub New()
        Dim s As Action = Sub()
                              Dim a = New C([|0|])",
"Imports System
Imports System.Linq
Class C
    Private v As Integer

    Sub New()
        Dim s As Action = Sub()
                              Dim a = New C(0)Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
")
        End Function

        <WorkItem(5920, "https://github.com/dotnet/roslyn/issues/5920")>
        <Fact(), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateConstructorInIncompleteLambda2() As Task
            Await TestInRegularAndScriptAsync(
"Imports System
Imports System.Linq
Class C
    Private v As Integer
    Public Sub New(v As Integer)
        Me.v = v
    End Sub
    Sub New()
        Dim s As Action = Sub()
                              Dim a = New [|C|](0, 0)",
"Imports System
Imports System.Linq
Class C
    Private v As Integer
    Private v1 As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
    Sub New()
        Dim s As Action = Sub()
                              Dim a = New C(0, 0)Public Sub New(v As Integer, v1 As Integer)
        Me.New(v)
        Me.v1 = v1
    End Sub
End Class
")
        End Function

        <WorkItem(1241, "https://github.com/dotnet/roslyn/issues/1241")>
        <Fact(Skip:="https://github.com/dotnet/roslyn/issues/53238"), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateConstructorInIncompleteLambda_WithoutImport() As Task
            Await TestInRegularAndScriptAsync(
"Imports System.Linq
Class C
    Sub New()
        Dim s As Action = Sub()
                              Dim a = New C([|0|])",
"Imports System.Linq
Class C
    Private v As Integer

    Sub New()
        Dim s As Action = Sub()
                              Dim a = New C(0)Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
")
        End Function

        <WorkItem(5920, "https://github.com/dotnet/roslyn/issues/5920")>
        <Fact(Skip:="https://github.com/dotnet/roslyn/issues/53238"), Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateConstructorInIncompleteLambda2_WithoutImport() As Task
            Await TestInRegularAndScriptAsync(
"Imports System.Linq
Class C
    Private v As Integer
    Public Sub New(v As Integer)
        Me.v = v
    End Sub
    Sub New()
        Dim s As Action = Sub()
                              Dim a = New [|C|](0, 0)",
"Imports System.Linq
Class C
    Private v As Integer
    Private v1 As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
    Sub New()
        Dim s As Action = Sub()
                              Dim a = New C(0, 0)Public Sub New(v As Integer, v1 As Integer)
        Me.New(v)
        Me.v1 = v1
    End Sub
End Class
")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestGenerateConstructorNotOfferedForDuplicate() As Task
            Await TestMissingInRegularAndScriptAsync(
"Imports System

Class X
    Private v As String

    Public Sub New(v As String)
        Me.v = v
    End Sub

    Sub Test()
        Dim x As X = New X(New [|String|]())
    End Sub
End Class")
        End Function

        <WorkItem(9575, "https://github.com/dotnet/roslyn/issues/9575")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestMissingOnMethodCall() As Task
            Await TestMissingInRegularAndScriptAsync(
"class C
    public sub new(int arg)
    end sub

    public function M(s as string, i as integer, b as boolean) as boolean
        return [|M|](i, b)
    end function
end class")
        End Function

        <WorkItem(13749, "https://github.com/dotnet/roslyn/issues/13749")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function Support_Readonly_Properties() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    ReadOnly Property Prop As Integer
End Class
Module P
    Sub M()
        Dim prop = 42
        Dim c = New C([|prop|])
    End Sub
End Module",
"Class C
    Public Sub New(prop As Integer)
        Me.Prop = prop
    End Sub

    ReadOnly Property Prop As Integer
End Class
Module P
    Sub M()
        Dim prop = 42
        Dim c = New C(prop)
    End Sub
End Module")
        End Function

        <WorkItem(21692, "https://github.com/dotnet/roslyn/issues/21692")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructor1() As Task
            Await TestInRegularAndScriptAsync(
"Public Class B
    Public Sub New(a As Integer)
        [|Me.New(a, 1)|]
    End Sub
End Class",
"Public Class B
    Private a As Integer
    Private v As Integer

    Public Sub New(a As Integer)
        Me.New(a, 1)
    End Sub

    Public Sub New(a As Integer, v As Integer)
        Me.a = a
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(21692, "https://github.com/dotnet/roslyn/issues/21692")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructor2() As Task
            Await TestInRegularAndScriptAsync(
"Public Class B
    Public Sub New(x As Integer)
        Me.New(x, 0, 0)
    End Sub

    Public Sub New(x As Integer, y As Integer, z As Integer)
        [|Me.New(x, y)|]
    End Sub
End Class",
"Public Class B
    Private x As Integer
    Private y As Integer

    Public Sub New(x As Integer)
        Me.New(x, 0, 0)
    End Sub

    Public Sub New(x As Integer, y As Integer)
        Me.x = x
        Me.y = y
    End Sub

    Public Sub New(x As Integer, y As Integer, z As Integer)
        Me.New(x, y)
    End Sub
End Class")
        End Function

        <WorkItem(21692, "https://github.com/dotnet/roslyn/issues/21692")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructor3() As Task
            Await TestInRegularAndScriptAsync(
"Public Class B
    Public Sub New(x As Integer)
    End Sub

    Public Sub New(x As Integer, y As Integer, z As Integer)
        [|Me.New(x, y)|]
    End Sub
End Class",
"Public Class B
    Private y As Integer

    Public Sub New(x As Integer)
    End Sub

    Public Sub New(x As Integer, y As Integer)
        Me.New(x)
        Me.y = y
    End Sub

    Public Sub New(x As Integer, y As Integer, z As Integer)
        Me.New(x, y)
    End Sub
End Class")
        End Function

        <WorkItem(21692, "https://github.com/dotnet/roslyn/issues/21692")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructor4() As Task
            Await TestInRegularAndScriptAsync(
"Public Class B
    Public Sub New(x As Integer)
        Me.New(x, 0)
    End Sub

    Public Sub New(x As Integer, y As Integer)
        [|Me.New(x, y, 0)|]
    End Sub
End Class",
"Public Class B
    Private x As Integer
    Private y As Integer
    Private v As Integer

    Public Sub New(x As Integer)
        Me.New(x, 0)
    End Sub

    Public Sub New(x As Integer, y As Integer)
        Me.New(x, y, 0)
    End Sub

    Public Sub New(x As Integer, y As Integer, v As Integer)
        Me.x = x
        Me.y = y
        Me.v = v
    End Sub
End Class")
        End Function

        <WorkItem(21692, "https://github.com/dotnet/roslyn/issues/21692")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructor5() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Public Sub New(a As Integer)
    End Sub

    Public Sub New(a As Integer, b As Integer)
        Me.New(True, True)
    End Sub

    Public Sub New(a As Boolean, b As Boolean)
        Me.New(1, 1)
    End Sub

    Public Sub New(a As Integer, b As Integer, c As Integer, e As Integer)
        [|Me.New(a, b, c)|]
    End Sub
End Class",
"Class C
    Private b As Integer
    Private c As Integer

    Public Sub New(a As Integer)
    End Sub

    Public Sub New(a As Integer, b As Integer)
        Me.New(True, True)
    End Sub

    Public Sub New(a As Boolean, b As Boolean)
        Me.New(1, 1)
    End Sub

    Public Sub New(a As Integer, b As Integer, c As Integer)
        Me.New(a)
        Me.b = b
        Me.c = c
    End Sub

    Public Sub New(a As Integer, b As Integer, c As Integer, e As Integer)
        Me.New(a, b, c)
    End Sub
End Class")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructorCrossLanguageCycleAvoidance() As Task
            Await TestInRegularAndScriptAsync(
<Workspace>
    <Project Language="C#" Name="CSharpProject" CommonReferences="true">
        <Document>
            public class BaseType
            {
                public BaseType(int x, int y) { }
            }
        </Document>
    </Project>
    <Project Language="Visual Basic" CommonReferences="true">
        <ProjectReference>CSharpProject</ProjectReference>
        <Document>
Public Class B
    Inherits BaseType

    Public Sub New(a As Integer)
        [|Me.New(a, 1)|]
    End Sub
End Class</Document>
    </Project>
</Workspace>.ToString(),
"
Public Class B
    Inherits BaseType

    Public Sub New(a As Integer)
        Me.New(a, 1)
    End Sub

    Public Sub New(x As Integer, y As Integer)
        MyBase.New(x, y)
    End Sub
End Class")
        End Function

        <WorkItem(49850, "https://github.com/dotnet/roslyn/issues/49850")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructorCrossLanguage() As Task
            Await TestInRegularAndScriptAsync(
<Workspace>
    <Project Language="C#" Name="CSharpProject" CommonReferences="true">
        <Document>
public class BaseType
{
    public BaseType(string x) { }
}</Document>
    </Project>
    <Project Language="Visual Basic" CommonReferences="true">
        <ProjectReference>CSharpProject</ProjectReference>
        <Document>
Option Strict On

Public Class B
    Public Sub M()
        Dim x = [|New BaseType(42)|]
    End Sub
End Class
        </Document>
    </Project>
</Workspace>.ToString(),
"
public class BaseType
{
    private int v;

    public BaseType(string x) { }

    public BaseType(int v)
    {
        this.v = v;
    }
}")
        End Function

        <WorkItem(50765, "https://github.com/dotnet/roslyn/issues/50765")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestDelegateConstructorCrossLanguageWithMissingType() As Task
            Await TestAsync(
<Workspace>
    <Project Language="C#" Name="CSharpProjectWithExtraType" CommonReferences="true">
        <Document>
public class ExtraType { }
        </Document>
    </Project>
    <Project Language="C#" Name="CSharpProjectGeneratingInto" CommonReferences="true">
        <ProjectReference>CSharpProjectWithExtraType</ProjectReference>
        <Document>
public class C
{
    public C(ExtraType t) { }
    public C(string s, int i) { }
}
        </Document>
    </Project>
    <Project Language="Visual Basic" CommonReferences="true">
        <ProjectReference>CSharpProjectGeneratingInto</ProjectReference>
        <Document>
Option Strict On

Public Class B
    Public Sub M()
        Dim x = [|New C(42, 42)|]
    End Sub
End Class
        </Document>
    </Project>
</Workspace>.ToString(),
"
public class C
{
    private int v1;
    private int v2;

    public C(ExtraType t) { }
    public C(string s, int i) { }

    public C(int v1, int v2)
    {
        this.v1 = v1;
        this.v2 = v2;
    }
}
        ", TestOptions.Regular)
        End Function

        <WorkItem(14077, "https://github.com/dotnet/roslyn/issues/14077")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function CreateFieldDefaultNamingStyle() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C([|x|])
    End Sub
End Class",
"Class C
    Private x As Integer

    Public Sub New(x As Integer)
        Me.x = x
    End Sub

    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C(x)
    End Sub
End Class")
        End Function

        <WorkItem(14077, "https://github.com/dotnet/roslyn/issues/14077")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function CreateFieldSpecifiedNamingStyle() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C([|x|])
    End Sub
End Class",
"Class C
    Private _x As Integer

    Public Sub New(x As Integer)
        _x = x
    End Sub

    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C(x)
    End Sub
End Class", options:=options.FieldNamesAreCamelCaseWithUnderscorePrefix)
        End Function

        <WorkItem(542055, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542055")>
        <WorkItem(14077, "https://github.com/dotnet/roslyn/issues/14077")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestFieldWithNamingStyleAlreadyExists() As Task
            Await TestInRegularAndScriptAsync(
"Class Program
    Sub Test()
        Dim x = New A([|P|]:=5)
    End Sub
End Class
Class A
    Shared Property _p As Integer
End Class",
"Class Program
    Sub Test()
        Dim x = New A(P:=5)
    End Sub
End Class
Class A
    Private _p1 As Integer

    Public Sub New(P As Integer)
        _p1 = P
    End Sub

    Shared Property _p As Integer
End Class", options:=options.FieldNamesAreCamelCaseWithUnderscorePrefix)
        End Function

        <WorkItem(14077, "https://github.com/dotnet/roslyn/issues/14077")>
        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        Public Async Function TestFieldAndPropertyNamingStyles() As Task
            Await TestInRegularAndScriptAsync(
"Class C
    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C([|x|])
    End Sub
End Class",
"Class C
    Private _x As Integer

    Public Sub New(p_x As Integer)
        _x = p_x
    End Sub

    Sub Test()
        Dim x As Integer = 1
        Dim obj As New C(x)
    End Sub
End Class", options:=options.MergeStyles(options.FieldNamesAreCamelCaseWithUnderscorePrefix, options.ParameterNamesAreCamelCaseWithPUnderscorePrefix))
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        <WorkItem(23807, "https://github.com/dotnet/roslyn/issues/23807")>
        Public Async Function TestAsNewClause() As Task
            Await TestInRegularAndScriptAsync(
"
Class Test
    Private field As New Test([|1|])
End Class
",
"
Class Test
    Private field As New Test(1)
    Private v As Integer

    Public Sub New(v As Integer)
        Me.v = v
    End Sub
End Class
")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        <WorkItem(44708, "https://github.com/dotnet/roslyn/issues/44708")>
        Public Async Function TestGenerateNameFromTypeArgument() As Task
            Await TestInRegularAndScriptAsync(
"Imports System.Collections.Generic

Class Frog
End Class

Class C
    Private Function M() As C
        Return New C([||]New List(Of Frog)())
    End Function
End Class
",
"Imports System.Collections.Generic

Class Frog
End Class

Class C
    Private frogs As List(Of Frog)

    Public Sub New(frogs As List(Of Frog))
        Me.frogs = frogs
    End Sub

    Private Function M() As C
        Return New C(New List(Of Frog)())
    End Function
End Class
")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        <WorkItem(44708, "https://github.com/dotnet/roslyn/issues/44708")>
        Public Async Function TestDoNotGenerateNameFromTypeArgumentIfNotEnumerable() As Task
            Await TestInRegularAndScriptAsync(
"Class Frog(Of T)
End Class

Class C
    Private Function M() As C
        Return New C([||]New Frog(Of Integer)())
    End Function
End Class
",
"Class Frog(Of T)
End Class

Class C
    Private frog As Frog(Of Integer)

    Public Sub New(frog As Frog(Of Integer))
        Me.frog = frog
    End Sub

    Private Function M() As C
        Return New C(New Frog(Of Integer)())
    End Function
End Class
")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        <WorkItem(51040, "https://github.com/dotnet/roslyn/issues/51040")>
        Public Async Function TestOmittedParameter() As Task

            Await TestInRegularAndScriptAsync(
"Class C
    Private _a As Integer

    Public Sub New(Optional a As Integer = 1)
        Me._a = a
    End Sub

    Public Function M() As C
        Return New C(, [||]2)
    End Function
End Class
",
"Class C
    Private _a As Integer
    Private v As Integer

    Public Sub New(Optional a As Integer = 1)
        Me._a = a
    End Sub

    Public Sub New(Optional a As Integer = 1, Optional v As Integer = Nothing)
        Me.New(a)
        Me.v = v
    End Sub

    Public Function M() As C
        Return New C(, 2)
    End Function
End Class
")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        <WorkItem(51040, "https://github.com/dotnet/roslyn/issues/51040")>
        Public Async Function TestOmittedParameterAtEnd() As Task

            Await TestInRegularAndScriptAsync(
"Class C
    Private _a As Integer

    Public Sub New(Optional a As Integer = 1)
        Me._a = a
    End Sub

    Public Function M() As C
        Return New C(1,[||])
    End Function
End Class
",
"Class C
    Private _a As Integer
    Private value As Object

    Public Sub New(Optional a As Integer = 1)
        Me._a = a
    End Sub

    Public Sub New(Optional a As Integer = 1, Optional value As Object = Nothing)
        Me.New(a)
        Me.value = value
    End Sub

    Public Function M() As C
        Return New C(1,)
    End Function
End Class
")
        End Function

        <Fact, Trait(Traits.Feature, Traits.Features.CodeActionsGenerateConstructor)>
        <WorkItem(51040, "https://github.com/dotnet/roslyn/issues/51040")>
        Public Async Function TestOmittedParameterAtStartAndEnd() As Task

            Await TestInRegularAndScriptAsync(
"Class C
    Private _a As Integer

    Public Sub New(Optional a As Integer = 1)
        Me._a = a
    End Sub

    Public Function M() As C
        Return New C(,[||])
    End Function
End Class
",
"Class C
    Private _a As Integer
    Private value As Object

    Public Sub New(Optional a As Integer = 1)
        Me._a = a
    End Sub

    Public Sub New(Optional a As Integer = 1, Optional value As Object = Nothing)
        Me.New(a)
        Me.value = value
    End Sub

    Public Function M() As C
        Return New C(,)
    End Function
End Class
")
        End Function

    End Class
End Namespace
