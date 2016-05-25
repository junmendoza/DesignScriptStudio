using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoCore.Lang;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Associative
{
    class SSATransformTest
    {
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void UpdateMember01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 10);
        }

        [Test]
        public void UpdateMember02()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("i");
            Assert.IsTrue((Int64)o.Payload == 1600);
            o = mirror.GetValue("j");
            Assert.IsTrue((Int64)o.Payload == 3200);
        }

        [Test]
        public void ArrayAssignmentNoCycle1()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("y");
            Assert.IsTrue((Int64)o.Payload == 1);
        }

        [Test]
        public void ArrayAssignmentNoCycle2()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("y");
            Assert.IsTrue((Int64)o.Payload == 1);
        }

        [Test]
        public void UpdateMemberArray1()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 30);
        }


        [Test]
        public void UpdateMemberArray2()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("a");
            Assert.IsTrue((Int64)o.Payload == 302);
        }

        [Test]
        public void UpdateMemberArray3()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            Obj o = mirror.GetValue("b");
            Assert.IsTrue((Int64)o.Payload == 100);
        }

        [Test]
        public void TestReplicationGuide01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuide02()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new object[] { 2, 3 });
            thisTest.Verify("y", new object[] { 3, 4 });
        }

        [Test]
        public void TestReplicationGuideOnProperty01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("c", new Object[] { new object[] { 2, 3 }, new object[] { 3, 4 } });
        }

        [Test]
        public void TestReplicationGuideOnProperty02()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { new object[] { 10, 10 }, new object[] { 10, 10 } });
        }

        [Test]
        public void TestReplicationGuideOnFunction01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", new Object[] { new object[] { 4, 5 }, new object[] { 5, 6 } });
        }

        [Test]
        public void TestArrayIndexingFromFunction01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("x", 1);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition02()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
            thisTest.Verify("a", 10);
        }

        [Test]
        public void TestRecursiveAssociativeImperativeCondition03()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("y", 55);
            thisTest.Verify("a", 20);
        }
    }
}