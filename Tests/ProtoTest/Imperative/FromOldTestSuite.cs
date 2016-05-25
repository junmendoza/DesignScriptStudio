using System;
using NUnit.Framework;
using ProtoCore.DSASM.Mirror;
using ProtoTest.TD;
using ProtoTestFx.TD;
namespace ProtoTest.Imperative
{
    [TestFixture]
    class FromOldTestSuite
    {
        public ProtoCore.Core core;
        public TestFrameWork thisTest = new TestFrameWork();
        [SetUp]
        public void Setup()
        {
            core = new ProtoCore.Core(new ProtoCore.Options());
            core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(core));
            core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(core));
        }

        [Test]
        public void TestConds01()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f0", false);
            thisTest.Verify("f1", false);
            thisTest.Verify("f2", false);
            thisTest.Verify("f3", false);
            thisTest.Verify("t0", true);
            thisTest.Verify("t1", true);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", true);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
        }

        [Test]
        public void TestConds02()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f0", false);
            thisTest.Verify("f1", false);
            thisTest.Verify("f2", false);
            thisTest.Verify("f3", false);
            thisTest.Verify("t0", true);
            thisTest.Verify("t1", true);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", true);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
        }

        [Test]
        public void TestConds03()
        {
            String code =
@"
            ExecutionMirror mirror = thisTest.RunScriptSource(code);
            thisTest.Verify("f0", false);
            thisTest.Verify("f1", false);
            thisTest.Verify("f2", false);
            thisTest.Verify("f3", false);
            thisTest.Verify("t0", true);
            thisTest.Verify("t1", true);
            thisTest.Verify("t2", true);
            thisTest.Verify("t3", true);
            thisTest.Verify("t4", true);
            thisTest.Verify("t5", true);
            thisTest.Verify("t6", true);
            thisTest.Verify("t7", true);
        }

        [Test]
        public void FuncWithDec()
        {
            String code =
@"y;[Imperative]
            ProtoScript.Runners.ProtoScriptTestRunner fsr = new ProtoScript.Runners.ProtoScriptTestRunner();

            ProtoLanguage.CompileStateTracker compileState = null;
            ExecutionMirror mirror = fsr.Execute(code, core, out compileState);


            Assert.IsTrue((Int64)mirror.GetValue("y").Payload == 57);
        }


    }
}