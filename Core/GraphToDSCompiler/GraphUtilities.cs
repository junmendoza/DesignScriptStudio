﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using ProtoCore.DSASM.Mirror;
using System.Text.RegularExpressions;
using ProtoCore.Utils;
using System.IO;
using ProtoCore.DSASM;

namespace GraphToDSCompiler
{
    public struct kw
    {
        public const string tempPrefix = "temp";
    }

    public struct Constants
    {
        public const uint TempMask = 0x80000000;
        public const uint UIDStart = 10000;
        public const char ReplicationGuideDelimiter = '¡';

        public const string kwTempNull = "temp_NULL";
    }

    /* Variable Line Struct
     * Used to present the pair value of Line number and Variable Name.
     * 
     * @variable    : string, name of the variable refered to
     * @line        : int, line number of the variable in the code block
     */
    public struct VariableLine
    {
        #region Struct Data Member

        public int line;
        public string variable;

        #endregion

        #region Struct Public Methods

        public VariableLine(string variable, int line)
        {
            if (line < 0)
                throw new ArgumentException("Argument cannot be negative", "line");
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentException("Invalid variable name", "variable");

            this.variable = variable;
            this.line = line;
        }

        public VariableLine(int line) // Only used for NUnit purposes (with temp names).
        {
            if (line < 0)
                throw new ArgumentException("Argument cannot be negative", "line");

            this.variable = string.Empty; // In NUnit temp names are ignored.
            this.line = line;
        }

        public static bool operator ==(VariableLine variableLine1, VariableLine variableLine2)
        {
            return variableLine1.Equals(variableLine2);
        }

        public static bool operator !=(VariableLine variableLine1, VariableLine variableLine2)
        {
            return !variableLine1.Equals(variableLine2);
        }

        /*
         * Check if two objects have same Variable Name and Line Number.
         * @return true if they have same Variable Name and Line Number or both are null
         *         false if otherwise
         */
        public override bool Equals(object obj)
        {
            if (obj is VariableLine)
            {
                return this.Equals((VariableLine)obj);
            }
            return false;
        }

        /*
         * Get Hashcode from combining Hashcode of two attribute @variable and @line
         */
        public override int GetHashCode()
        {
            return variable.GetHashCode() ^ line;
        }

        #endregion

        #region Struct Private Methods

        private bool Equals(VariableLine variableLine)
        {
            return (this.variable == variableLine.variable) && (this.line == variableLine.line);
        }

        #endregion
    }
    public class GraphUtilities
    {
        // TODO Jun: is it better to have GraphUtils as a singleton rather than checking for core? 
        // We only need core to be instantiated once
        private static ProtoLanguage.CompileStateTracker compileState = null;
        private static string rootModulePath = string.Empty;

        public static ProtoLanguage.CompileStateTracker GetCompilationState() { return compileState; }

        private static int numBuiltInMethods = 0;

        private static List<ProcedureNode> builtInMethods = null;

        private static Dictionary<string, string> importedAssemblies = new Dictionary<string,string>();
        /// <summary>
        /// List of built-in methods available in Core
        /// </summary>
        public static List<ProcedureNode> BuiltInMethods
        {
            get
            {
                return GetBuiltInMethods();
            }
        }

        /// <summary>
        /// The ClassTable of imported class libraries
        /// </summary>
        public static ProtoCore.DSASM.ClassTable ClassTable
        {
            get
            {
                if (compileState != null)
                    return compileState.ClassTable;
                else
                    return null;
            }

        }

        public static ProtoCore.AST.AssociativeAST.CodeBlockNode ImportNodes
        {
            get
            {
                if (compileState != null)
                    return compileState.ImportNodes;
                else
                    return null;
            }
        }

        public static ProtoCore.BuildStatus BuildStatus { get { return compileState.BuildStatus; } }

        public static uint runningUID = Constants.UIDStart;
        public static uint GenerateUID() { return runningUID++; }

        private static void BuildCompileState(bool isCodeBlockNode = false, bool isPreloadedAssembly = false)
        {
            if (compileState == null)
            {
                ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();
                options.RootModulePathName = rootModulePath;
                compileState = new ProtoLanguage.CompileStateTracker(options);
                compileState.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive());
                compileState.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive());

            }
            else
            {
                compileState.ResetForPrecompilation();
            }

            compileState.IsParsingPreloadedAssembly = isPreloadedAssembly;
            compileState.IsParsingCodeBlockNode = isCodeBlockNode;
            compileState.ParsingMode = ProtoCore.ParseMode.AllowNonAssignment;
        }

        public static void CleanUp()
        {
            if (compileState != null)
            {
                compileState.Cleanup();
            }
            compileState = null;

            numBuiltInMethods = 0;
            builtInMethods = null;
            importedAssemblies.Clear();
            rootModulePath = string.Empty;
        }

        public static void SetRootModulePath(string rootModulePath)
        {
            if (null == rootModulePath)
                rootModulePath = string.Empty;
            if (null == compileState)
                GraphUtilities.rootModulePath = rootModulePath;
            else
                compileState.Options.RootModulePathName = rootModulePath;
        }

        private static bool LocateAssembly(string assembly, out string assemblyPath)
        {
            ProtoLanguage.CompileOptions options = null;
            if (null == compileState)
                options = new ProtoLanguage.CompileOptions() { RootModulePathName = GraphUtilities.rootModulePath };
            else
                options = compileState.Options;

            assemblyPath = FileUtils.GetDSFullPathName(assembly, options);
            string dirName = Path.GetDirectoryName(assemblyPath);
            if (!string.IsNullOrEmpty(dirName) && !compileState.Options.IncludeDirectories.Contains(dirName))
            {
                compileState.Options.IncludeDirectories.Add(dirName);
            }

            return File.Exists(assemblyPath);
        }

        /// <summary>
        /// TODO: Deprecate
        /// This is for Graph UI to preload assemblies as and when they are loaded from Library viewer
        /// to pre-populate class tables with imported classes and methods
        /// </summary>
        /// <param name="assemblies"></param>
        public static void PreloadAssembly(List<string> assemblies)
        {
            string expression = string.Empty;
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

            BuildCompileState(false, true);

            foreach (string assembly in assemblies)
            {
                string assemblyPath = string.Empty;
                if (!LocateAssembly(assembly, out assemblyPath))
                {
                    BuildStatus.LogWarning(ProtoCore.BuildData.WarningID.kFileNotFound, string.Format(ProtoCore.BuildData.WarningMessage.kFileNotFound, assembly));
                    continue;
                }
                
                if(importedAssemblies.ContainsValue(assemblyPath))
                    continue;

                string key = MD5Hash(assembly);
                importedAssemblies[MD5Hash(assembly)] = assemblyPath;

                string importStatement = "import (" + '"'.ToString() + assemblyPath + '"'.ToString() + ")";
                expression = expression + importStatement + ';'.ToString();

                ProtoCore.BuildStatus status = null;
                if (!string.IsNullOrEmpty(expression))
                {
                    status = PreCompile(expression, compileState, out blockId);
                }

                if (status != null && status.ErrorCount > 0)
                {
                    importedAssemblies.Remove(key);
                }

            }

            GetBuiltInMethods();
        }

        /// <summary>
        /// This is for Graph UI to preload assemblies as and when they are loaded from Library viewer
        /// to pre-populate class tables with imported classes and methods
        /// </summary>
        /// <param name="assemblies"></param>
        public static void PreloadAssembly(string assembly)
        {
            List<string> assemblies = new List<string>();
            assemblies.Add(assembly);
            PreloadAssembly(assemblies);
        }

        /// <summary>
        /// This method iterates over each imported assembly in the importedAssemblies
        /// dictionary and performs the given action with the hash and path of the 
        /// imported assembly.
        /// </summary>
        /// <param name="action">Action to perform with first argument as hash and the 
        /// second as path</param>
        public static void ForEachImportNodes(Action<string, string> action)
        {
            foreach (var item in importedAssemblies)
            {
                action(item.Key, item.Value);
            }
        }

        public static bool TryGetImportLibraryPath(string library, out string path, out string hashKey)
        {
            hashKey = GraphUtilities.MD5Hash(library);
            if (!importedAssemblies.TryGetValue(hashKey, out path))
            {
                if (!LocateAssembly(library, out path))
                    return false;
                importedAssemblies[hashKey] = path;
            }

            return true;
        }

        /// <summary>
        /// Given an assembly or DS file, it precompiles it and returns a list of the newly imported classes
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IList<ClassNode> GetClassesForAssembly(string assembly)
        {
            int currentCI = ClassTable.ClassNodes.Count;

            List<string> assemblies = new List<string>();
            assemblies.Add(assembly);
            PreloadAssembly(assemblies);

            int postCI = ClassTable.ClassNodes.Count;

            IList<ClassNode> classNodes = new List<ClassNode>();
            for (int i = currentCI; i < postCI; ++i)
            {
                classNodes.Add(ClassTable.ClassNodes[i]);
            }

            return classNodes;
        }

        /// <summary>
        /// Returns the LibraryMirror for reflection for each assembly that is imported into the UI
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static ProtoCore.Mirror.LibraryMirror GetLibraryMirror(string assembly)
        {
            IList<ClassNode> classNodes = GetClassesForAssembly(assembly);

            ProtoCore.Mirror.LibraryMirror libraryMirror = new ProtoCore.Mirror.LibraryMirror(compileState, assembly, classNodes);

            return libraryMirror;
        }


        /// <summary>
        /// Given a class name, this returns the list of base classes for that class
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static List<string> GetParentClasses(string className)
        {
            List<string> parentClass = new List<string>();

            if (string.IsNullOrEmpty(className))
                return parentClass;

            Validity.Assert(compileState != null);
            ProtoCore.DSASM.ClassTable classTable = compileState.ClassTable;
            int ci = classTable.IndexOf(className);

            if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
            {

                ProtoCore.DSASM.ClassNode classNode = classTable.ClassNodes[ci];
                while (classNode.baseList.Count > 0)
                {

                    ci = classNode.baseList[0];
                    Validity.Assert(ci != ProtoCore.DSASM.Constants.kInvalidIndex);

                    parentClass.Add(classTable.ClassNodes[ci].name);

                    classNode = classTable.ClassNodes[ci];
                }

            }

            return parentClass;
        }

        /// <summary>
        /// Given a class, this returns the assembly name it is defined in
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string GetAssemblyFromClassName(string className)
        {
            string assembly = "";

            if (string.IsNullOrEmpty(className))
                return assembly;

            Validity.Assert(compileState != null);
            ProtoCore.DSASM.ClassTable classTable = compileState.ClassTable;
            int ci = classTable.IndexOf(className);

            if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                ProtoCore.DSASM.ClassNode classNode = classTable.ClassNodes[ci];
                assembly = classNode.ExternLib;
            }

            return assembly;
        }

        /// <summary>
        /// Returns the list of constructors for a given class
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static List<string> GetConstructors(string className)
        {
            List<string> constructors = new List<string>();
            Validity.Assert(compileState != null);
            ProtoCore.DSASM.ClassTable classTable = compileState.ClassTable;
            int ci = classTable.IndexOf(className);

            if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                ClassNode classNode = classTable.ClassNodes[ci];
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                foreach (ProcedureNode pNode in procList)
                {
                    if (pNode.isConstructor == true)
                        constructors.Add(pNode.name);
                }
            }
            return constructors;
        }

        /// <summary>
        /// Returns the list of properties for a given class
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static List<string> GetProperties(string className)
        {
            List<string> properties = new List<string>();
            Validity.Assert(compileState != null);
            ProtoCore.DSASM.ClassTable classTable = compileState.ClassTable;
            int ci = classTable.IndexOf(className);
            string name = string.Empty;
            if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                ClassNode classNode = classTable.ClassNodes[ci];
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                string prefix = ProtoCore.DSASM.Constants.kGetterPrefix;
                ArgumentInfo aInfo = new ArgumentInfo();
                aInfo.isDefault = false;
                aInfo.Name = ProtoCore.DSASM.Constants.kThisPointerArgName;
                foreach (ProcedureNode pNode in procList)
                {
                    name = pNode.name;
                    if (name.Contains(prefix) && pNode.argInfoList.Count == 0 && !pNode.argInfoList.Contains(aInfo))
                    {
                        int prefixLength = prefix.Length;
                        name = name.Substring(prefixLength);
                        properties.Add(name);
                    }
                }
            }
            return properties;
        }

        /// <summary>
        /// Returns a list of member functions for a given class
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static List<string> GetMethods(string className)
        {
            List<string> methods = new List<string>();
            Validity.Assert(compileState != null);
            ProtoCore.DSASM.ClassTable classTable = compileState.ClassTable;
            int ci = classTable.IndexOf(className);
            string name = string.Empty;
            if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                ClassNode classNode = classTable.ClassNodes[ci];
                ProcedureTable procedureTable = classNode.vtable;
                List<ProcedureNode> procList = procedureTable.procList;
                string prefix = ProtoCore.DSASM.Constants.kGetterPrefix;
                ArgumentInfo aInfo = new ArgumentInfo();
                aInfo.isDefault = false;
                aInfo.Name = ProtoCore.DSASM.Constants.kThisPointerArgName;
                foreach (ProcedureNode pNode in procList)
                {
                    name = pNode.name;
                    if (name.Contains(prefix) == false && pNode.isConstructor == false && !pNode.argInfoList.Contains(aInfo))
                    {
                        methods.Add(name);
                    }
                }
            }
            return methods;
        }

        private static List<ProcedureNode> GetBuiltInMethods()
        {
            Validity.Assert(compileState != null);

            Validity.Assert(compileState.CodeBlockList.Count > 0);

            if (builtInMethods == null)
            {
                List<ProcedureNode> procNodes = compileState.CodeBlockList[0].procedureTable.procList;
                numBuiltInMethods = procNodes.Count;
                List<ProcedureNode> builtIns = new List<ProcedureNode>();

                foreach (ProcedureNode procNode in procNodes)
                {
                    if (!procNode.name.StartsWith(ProtoCore.DSASM.Constants.kInternalNamePrefix) && !procNode.name.Equals("Break"))
                        builtIns.Add(procNode);
                }
                builtInMethods = builtIns;

            }
            Validity.Assert(builtInMethods.Count > 0);

            return builtInMethods;
        }

        /// <summary>
        ///  Given a DS file, this returns the list of global functions defined in the file
        /// </summary>
        /// <param name="dsFile"></param>
        /// <returns></returns>
        public static List<ProcedureNode> GetGlobalMethods(string dsFile)
        {
            List<ProcedureNode> methods = new List<ProcedureNode>();
            Validity.Assert(compileState != null);

            Validity.Assert(compileState.CodeBlockList.Count > 0);

            List<ProcedureNode> procNodes = compileState.CodeBlockList[0].procedureTable.procList;

            int numNewMethods = procNodes.Count - numBuiltInMethods;
            Validity.Assert(numNewMethods >= 0);

            for (int i = numBuiltInMethods; i < procNodes.Count; ++i)
            {
                methods.Add(procNodes[i]);
            }

            numBuiltInMethods = procNodes.Count;
            return methods;
        }

        private static void InsertCommentsInCode(string stmt, ProtoCore.AST.Node node, 
            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode, ref int cNodeNum, ref List<string> compiled)
        {
            if (node == null)
            {
                for (int i = cNodeNum; i < commentNode.Body.Count; ++i)
                {
                    ProtoCore.AST.AssociativeAST.CommentNode cnode = commentNode.Body[i] as ProtoCore.AST.AssociativeAST.CommentNode;
                    if (cnode == null)
                        continue;

                    compiled.Add(cnode.Value);
                }
                return;
            }

            string[] lines = stmt.Split('\n');
            int numLines = lines.Length;
            int nodeEndLine = node.line + numLines - 1;
            int nodeEndCol = lines[numLines - 1].Length + 1;

            //int commentCount = 0;
            for (int i = cNodeNum; i < commentNode.Body.Count; ++i)
            {
                ProtoCore.AST.AssociativeAST.CommentNode cnode = commentNode.Body[i] as ProtoCore.AST.AssociativeAST.CommentNode;
                if (cnode == null)
                    continue;

                // If comment appears on line before statement
                if (cnode.line < node.line)
                {
                    //compiled.Insert(nodeNum + commentCount++, cnode.Value);
                    compiled.Insert(compiled.Count - 1, cnode.Value);
                    cNodeNum = i + 1;
                }
                // If comment is on the same lines as the statement
                else if (cnode.line >= node.line && cnode.line <= nodeEndLine)
                {
                    bool commentBeforeStmt = cnode.line == node.line && cnode.col < node.col;   // can only be comment block
                    bool commentAfterStmt = cnode.line == nodeEndLine && cnode.col >= nodeEndCol;

                    // If comment appears either before or after the statement
                    if (commentBeforeStmt)
                    {
                        //compiled.Insert(nodeNum + commentCount++, cnode.Value);
                        compiled.Insert(compiled.Count - 1, cnode.Value);
                    }
                    else if (commentAfterStmt)
                    {
                        compiled.Add(cnode.Value); 
                    }
                    // Ignore the case where the comment is embedded within a statement
                    cNodeNum = i + 1;
                }
                else // By this point we know the comment appears on a line after the statement
                    break;
            }
        }

        private static List<string> ParseCore(string expression, ref bool parseSuccess)
        {
            List<string> compiled = new List<string>();

            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode = null;
            ProtoCore.AST.Node codeBlockNode = Parse(expression, out commentNode);
            parseSuccess = true;
            List<ProtoCore.AST.Node> nodes = ParserUtils.GetAstNodes(codeBlockNode);
            Validity.Assert(nodes != null);

            int cNodeNum = 0;
            if (nodes.Count == 0)
            {
                InsertCommentsInCode(null, null, commentNode, ref cNodeNum, ref compiled);
                return compiled;
            }
            
            foreach (var node in nodes)
            {
                ProtoCore.AST.AssociativeAST.AssociativeNode n = node as ProtoCore.AST.AssociativeAST.AssociativeNode;
                ProtoCore.Utils.Validity.Assert(n != null);

                if (n is ProtoCore.AST.AssociativeAST.FunctionDefinitionNode ||
                    n is ProtoCore.AST.AssociativeAST.ClassDeclNode)
                {
                    compileState.BuildStatus.LogSemanticError("Class and function definitions are not supported");

                }
                else if (n is ProtoCore.AST.AssociativeAST.ModifierStackNode)
                {
                    compileState.BuildStatus.LogSemanticError("Modifier Blocks are not supported currently");
                }
                //else if (n is ProtoCore.AST.AssociativeAST.ImportNode)
                //{
                //    core.BuildStatus.LogSemanticError("Import statements are not supported in CodeBlock Nodes. To import a file, please use the Library utility");

                //}

                string stmt = ProtoCore.Utils.ParserUtils.ExtractStatementFromCode(expression, node);

                ProtoCore.AST.AssociativeAST.BinaryExpressionNode ben = node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                if (ben != null && ben.Optr == ProtoCore.DSASM.Operator.assign)
                {
                    ProtoCore.AST.AssociativeAST.IdentifierNode lNode = ben.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                    if (lNode != null && lNode.Value == ProtoCore.DSASM.Constants.kTempProcLeftVar)
                    {
                        stmt = "%t =" + stmt;
                    }                    
                }
                else
                {
                    // These nodes are non-assignment nodes
                    stmt = "%t =" + stmt;                
                }
                compiled.Add(stmt);
                
                InsertCommentsInCode(stmt, node, commentNode, ref cNodeNum, ref compiled);
                
            }
            InsertCommentsInCode(null, null, commentNode, ref cNodeNum, ref compiled);

            return compiled;
        }

        // TODO:DEPRECATE
        /*public static void CompileExpression(string expression, List<string> compiled, out List<string> errors)
        {
            errors = CompileExpression(expression, compiled);
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="compiled"></param>
        /// <param name="errors"></param>
        public static void CompileExpression(string expression, out List<string> compiled)
        {
            //expression = expression.Replace("\r\n", "\n");
            int oldIndex = 0;
            compiled = new List<string>();

            bool parseSuccess = false;

            if (expression == null)
                return;

            try
            {
                //List<string> statements = new List<string>();
                compiled = ParseCore(expression, ref parseSuccess);
            }
            catch 
            {
                // For function and class declaration nodes
                if(parseSuccess)
                    return;

                // For invalid functional associative statement errors like for "a+b;"
                // Reset core above as we don't wish to propagate these errors - pratapa
                compileState.ResetForPrecompilation();

                compiled = GenerateStatements(oldIndex, expression);
                /*string code = ParseNonAssignment(expression);
                bool success = false;
                try
                {
                    compiled = ParseCore(code, ref success);
                }
                catch { }*/                
                return;
            }

            // TODO: Aparajit: Check with Jun if this is required for handling function definitions
            // Currently it's not required due to the above checks for function and class definition nodes
            /*do
            {
                index = expression.IndexOf("def ", index);
                if (index != -1)
                {
                    string sub = expression.Substring(oldIndex, index - oldIndex);
                    sub.TrimEnd(' ');
                    string rest = expression.Substring(index);
                    rest.Trim();
                    if (sub.EndsWith("\n")) 
                    {
                        sub = sub.Substring(0, sub.Length - 1); 
                        rest = "\n" + rest; 
                    }
                    string[] expr = GetStatementsString(sub);//.Split(';');
                    foreach (string s in expr)
                        compiled.Add(s);
                    string func = GetFunctionString(rest);
                    compiled.Add(func);
                    index += func.Length;
                    oldIndex = index + 1;
                }
            } 
            while (index != -1);*/           
            
        }

        public static void RemoveGlobalSymbols(string symbolName)
        {
            compileState.CodeBlockList[0].symbolTable.RemoveGlobalSymbols(symbolName);
        }

        private static List<string> GenerateStatements(int oldIndex, string expression)
        {

            List<string> compiled = new List<string>();
            if (oldIndex > expression.Length) ;
            else
            {
                string sub = expression.Substring(oldIndex);
                string[] expr = GetStatementsString(sub);
                foreach (string s in expr)
                    compiled.Add(s);
            }
            for (int i = 0; i < compiled.Count(); i++)
            {
                if (compiled[i].StartsWith("\n"))
                {
                    string newlines = string.Empty;
                    int lastPosButOne = 0;
                    string original = compiled[i];
                    for (int j = 0; j < original.Length; j++)
                    {
                        if (!original[j].Equals('\n')) { lastPosButOne = j; break; } else newlines += original[j];
                    }
                    string newStatement = original.Substring(lastPosButOne);
                    if (!IsNotAssigned(newStatement)) newStatement = "%t =" + newStatement;
                    compiled[i] = newlines + newStatement;
                }
                else
                    if (!IsNotAssigned(compiled[i])) compiled[i] = "%t =" + compiled[i];
            }
            return compiled;
        }

        private static List<string> ParseNonAssignment(string expression)
        {
            //string code = "";
            List<string> compiled = new List<string>();
            string[] stmts = expression.Split(';');

            for (int i = 0; i < stmts.Length; ++i )
            {
                string stmt = stmts[i].Trim();
                if (!string.IsNullOrWhiteSpace(stmt))
                    stmt += ";";
                else
                    continue;

                int index = stmt.IndexOf('=');
                if (index == -1)
                    stmt = "%t =" + stmt;

                //code += stmt + "\n";
                compiled.Add(stmt);
            }
            //return code;
            return compiled;
        }

        private static bool IsNotAssigned(string code)
        {
            code = code.Trim(';', ' ');
            if (string.IsNullOrEmpty(code)) 
                return true;
            bool hasLHS = code.Contains("=");
            return hasLHS;
        }

        /*Given a block of code that has only usual statements and Modifier Stacks*/
        private static string[] GetStatementsString(string input)
        {
            var expr = new List<string>();
            //int index = 0;
            //int oldIndex = 0;
            //do
            //{
            //    index = input.IndexOf("{", index);
            //    if (index != -1)
            //    {                    
            //        string sub;
            //        if (index < input.Length - 1) { if (input[index + 1].Equals('\n')) index += 1; }
            //        sub = input.Substring(0, index);                    
            //        int equalIndex = sub.LastIndexOf('=');
            //        sub = input.Substring(0, equalIndex);                           //enable the open brace to be on the next line of the identifier
            //        int newLineIndex = sub.LastIndexOf('\n');
            //        if (newLineIndex != -1)
            //        {
            //            sub = input.Substring(oldIndex, newLineIndex - oldIndex + 1);
            //        }
            //        //expr.Add(sub);
            //        expr.AddRange(GetBinaryStatementsList(sub));
            //        oldIndex = newLineIndex != -1? newLineIndex : 0;
            //        index = input.IndexOf('}', index);
            //        expr.Add(input.Substring(oldIndex, index - oldIndex + 1));
            //        //index++;
            //        oldIndex = index+1;
            //    }
            //} while (index != -1);
            //if (oldIndex != input.Length - 1)
            //    expr.AddRange(GetBinaryStatementsList(input.Substring(oldIndex)));
            expr.AddRange(GetBinaryStatementsList(input));
            return expr.ToArray();
        }
        /*attempt*/
        /*Given a block of code that has only usual binary statements*/
        private static List<string> GetBinaryStatementsList(string input)
        {
            var expr = new List<string>();
            int index = 0;
            int oldIndex = 0;
            do
            {
                index = input.IndexOf(";", oldIndex);
                if (index != -1)
                {
                    string sub;
                    if (index < input.Length - 1)
                    {
                        if (input[index + 1].Equals('\n'))
                            index += 1;
                    }
                    sub = input.Substring(oldIndex, index - oldIndex + 1);
                    expr.Add(sub);
                    //index++;
                    oldIndex = index + 1;
                }
            } while (index != -1);
            return expr;
        }
        /*attempt ends*/
        private static string GetFunctionString(string input)
        {
            Stack<int> openingBraces = new Stack<int>();
            string output = "";
            int index = input.IndexOf("{");
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '{')
                {
                    openingBraces.Push(i);
                }
                else if (input[i] == '}')
                {
                    int closer = openingBraces.Pop();
                    output += string.Format("{0} - {1}\n", closer, i);
                    if (closer == index) index = i;
                }
            }
            if (index < input.Length - 1) { if (input[index + 1].Equals('\n')) index += 1; }
            return input.Substring(0, index + 1);
        }

        /// <summary>
        /// TODO: DEPRECATE - Currently used in ProtoTest.GraphCompiler.MicroFeatureTests
        /// Takes in a string and returns a list of unbound variables and the line numbers for output lines.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="inputVariables"></param>
        /// <param name="outputLines"></param>
        /// <returns>Returns true if compilation succeeded, or false otherwise. Returning true may still 
        /// result in warnings, which suggests that the compilation was successful with warning.</returns>
        public static bool GetInputOutputInfo(string code, out List<ProtoCore.BuildData.WarningEntry> warnings, out List<string> errors,
            Dictionary<string, int> inputVariables, Dictionary<int, string> outputLines)
        {
            warnings = null;
            errors = new List<string>();

            if (string.IsNullOrEmpty(code))
                throw new ArgumentNullException("code", "8686D4A8");

            if (null != inputVariables)
                inputVariables.Clear();
            if (null != outputLines)
                outputLines.Clear();

            try
            {
                BuildCompileState(true);
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                ProtoCore.BuildStatus status = PreCompile(code, compileState, out blockId);
                foreach (var err in status.Errors)
                {
                    errors.Add(err.Message);
                }
                warnings = status.Warnings;

                if (null != inputVariables)
                    GetInputVariables(warnings, inputVariables);
                if (null != outputLines)
                    GetOutputLines(code, compileState, outputLines);

                return true;
            }
            catch (Exception exception)
            {
                //ProtoCore.BuildData.WarningEntry warning = new ProtoCore.BuildData.WarningEntry();
                //warning.msg = exception.Message;
                //warnings = new List<ProtoCore.BuildData.WarningEntry>();
                //warnings.Add(warning);
                return false;
            }
        }


        /// <summary>
        /// Takes in a string and returns a list of unbound variables and the line numbers for output lines.
        /// </summary>
        /// <param name="compilableText"></param>
        /// <param name="inputLines"></param>
        /// <param name="outputLines"></param>
        /// <returns>Returns true if compilation succeeded, or false otherwise. Returning true may still 
        /// result in warnings, which suggests that the compilation was successful with warning.</returns>
        public static bool GetInputOutputInfo(string compilableText, Dictionary<int, List<VariableLine>> inputLines, HashSet<VariableLine> outputLines)
        {
            List<ProtoCore.BuildData.WarningEntry> warnings = null;
            
            if (string.IsNullOrEmpty(compilableText))
                throw new ArgumentNullException("code", "8686D4A8");

            if (null != inputLines)
                inputLines.Clear();
            if (null != outputLines)
                outputLines.Clear();

            try
            {
                BuildCompileState(true);
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                ProtoCore.BuildStatus status = PreCompile(compilableText, compileState, out blockId);

                if( status.Errors.Count > 0)
                   return false;

                warnings = status.Warnings;

                if (null != inputLines)
                    GetInputLines(compilableText, warnings, inputLines);
                if (null != outputLines)
                    GetOutputLines(compilableText, compileState, outputLines);
                
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Takes in a string and returns a list of AST nodes and a list of unbound variables
        /// </summary>
        /// <param name="compilableText"></param>
        /// <param name="unboundIdentifiers"></param>
        /// <param name="astNodes"></param>
        /// <returns>Returns true if compilation succeeded, or false otherwise. Returning true may still 
        /// result in warnings, which suggests that the compilation was successful with warning.</returns>             
        public static bool ParseCodeBlockNodeStatements(string compilableText, Dictionary<int, List<VariableLine>> unboundIdentifiers, out List<ProtoCore.AST.Node> astNodes)
        {
            List<ProtoCore.BuildData.WarningEntry> warnings = null;

            if (string.IsNullOrEmpty(compilableText))
                throw new ArgumentNullException("code", "8686D4A8");

            if (null != unboundIdentifiers)
                unboundIdentifiers.Clear();
            try
            {
                BuildCompileState(true);
                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
                ProtoCore.BuildStatus status = PreCompile(compilableText, compileState, out blockId);

                if (status.Errors.Count > 0)
                {
                    astNodes = null;
                    return false;
                }

                warnings = status.Warnings;

                if (null != unboundIdentifiers)
                    GetInputLines(compilableText, warnings, unboundIdentifiers);

                astNodes = compileState.AstNodeList;
                return true;
            }
            catch (Exception exception)
            {
                astNodes = null;
                return false;
            }
        }


        public static List<SynchronizeData> GetSDList(string filepath)
        {
            BuildCompileState(true);
            List<SynchronizeData> sdList = new List<SynchronizeData>();
            string readContents = filepath;
            //using (StreamReader streamReader = new StreamReader(filepath, Encoding.UTF8))
            //{
            //    readContents = streamReader.ReadToEnd();
            //}
            sdList = (SynchronizeDataCollection.Deserialize(readContents)).sdList;
            return sdList;
        }

        /// <summary>
        /// Checks if the string in code block node is a literal or an identifier or has multiple lines of code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static SnapshotNodeType AnalyzeString(string code)
        {
            SnapshotNodeType type = SnapshotNodeType.None;
            if (!code.EndsWith(";")) code += ";";
            List<ProtoCore.AST.Node> n = new List<ProtoCore.AST.Node>();
            try
            {
                BuildCompileState(true);
                System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(code));
                ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
                ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, compileState);

                p.Parse();
                ProtoCore.AST.AssociativeAST.CodeBlockNode codeBlockNode = p.root as ProtoCore.AST.AssociativeAST.CodeBlockNode;
                n = p.GetParsedASTList(codeBlockNode);
            }
            catch (Exception exception)
            {
                //if syntax return SnapshotNodeType as None
                return SnapshotNodeType.CodeBlock;
            }
            /* 
             string LiteralPatternInt = @"^\s*-?\d+\s*$";
             string LiteralPatternDouble = @"^\d+([\.]\d+)?$";
             string LiteralPatternString = "^\"([^\"\\\\]|\\\\.)*\"$";
             string IdentifierPattern = @"^\s*[_a-zA-Z][_a-z0-9A-Z@]*\s*$";
             Match matchInt = Regex.Match(code, LiteralPatternInt);
             Match matchDouble = Regex.Match(code, LiteralPatternDouble);
             Match matchString = Regex.Match(code, LiteralPatternString);
             Match matchIdentifier = Regex.Match(code, IdentifierPattern);

            if (matchInt.Success || matchString.Success || matchDouble.Success)
                type = StringExpressionType.Literal;
            else if (matchIdentifier.Success)
                type = StringExpressionType.Identifier;
            else
                type = StringExpressionType.CodeBlock;*/
            if (n.Count > 1 || n.Count == 0)
                type = SnapshotNodeType.CodeBlock;
            else if (n[0] is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                type = SnapshotNodeType.CodeBlock;
            else if (n[0] is ProtoCore.AST.AssociativeAST.IdentifierNode)
                type = SnapshotNodeType.Identifier;
            else if (n[0] is ProtoCore.AST.AssociativeAST.IntNode || n[0] is ProtoCore.AST.AssociativeAST.DoubleNode || n[0] is ProtoCore.AST.AssociativeAST.StringNode || n[0] is ProtoCore.AST.AssociativeAST.FunctionCallNode || n[0] is ProtoCore.AST.AssociativeAST.RangeExprNode)
                type = SnapshotNodeType.Literal;
            else type = SnapshotNodeType.CodeBlock;
            return type;
        }



        /// <summary>
        /// Does the first pass of compilation and returns a list of wanrnings in compilation
        /// </summary>
        /// <param name="code"></param>
        /// <param name="compileState"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        private static ProtoCore.BuildStatus PreCompile(string code, ProtoLanguage.CompileStateTracker compileState, out int blockId)
        {
            bool buildSucceeded = false;

            compileState.ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                globalBlock.body = code;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.language;

                compileState.Executives[id].Compile(compileState, out blockId, null, globalBlock, context);

                compileState.BuildStatus.ReportBuildResult();

                int errors = 0;
                int warnings = 0;
                buildSucceeded = compileState.BuildStatus.GetBuildResult(out errors, out warnings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if (!(ex is ProtoCore.BuildHaltException))
                {
                    throw ex;
                }
            }

            return compileState.BuildStatus;
        }

        // TODO: Deprecate as it's used in the deprecated version of GetInputOutputInfo
        private static void GetInputVariables(List<ProtoCore.BuildData.WarningEntry> warnings,
            Dictionary<string, int> inputLines)
        {
            IEnumerable<ProtoCore.BuildData.WarningEntry> iter1 = warnings;
            int count = 0;
            foreach (ProtoCore.BuildData.WarningEntry warning in iter1)
            {
                if (warning.id == ProtoCore.BuildData.WarningID.kIdUnboundIdentifier)
                {
                    int start = warning.msg.IndexOf("'");
                    int end = warning.msg.IndexOf("'", start + 1);
                    ++count;
                    inputLines.Add(warning.msg.Substring(start + 1, end - start - 1), warning.line);
                }
            }
        }

        private static List<VariableLine> DfsTraverse(ProtoCore.AST.AssociativeAST.AssociativeNode node)
        {
            List<VariableLine> result = new List<VariableLine>();
            if (node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                foreach (ProtoCore.AST.AssociativeAST.AssociativeNode argNode in
                    (node as ProtoCore.AST.AssociativeAST.FunctionCallNode).FormalArguments)
                {
                    if (argNode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                    {
                        result.Add(new VariableLine()
                        {
                            variable = argNode.Name,
                            line = argNode.line
                        });
                    }

                    if (argNode is ProtoCore.AST.AssociativeAST.FunctionCallNode)
                    {
                        result.AddRange(DfsTraverse(argNode));
                    }
                }
            }
            else if (node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                result.Add(new VariableLine()
                {
                    variable = node.Name,
                    line = node.line
                });
            }
            else if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
            {
                //result.AddRange(DfsTraverse((node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).LeftNode));
                result.AddRange(DfsTraverse((node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode).RightNode));
            }

            return result;
        }

        /*private static void GetInputLines(string compilableText, List<ProtoCore.BuildData.WarningEntry> warnings,
            Dictionary<int, List<VariableLine>> inputLines)
        {
            string plainCode = compilableText.Replace("\n", "");
            string[] statementsList = plainCode.Split(';');

            List<VariableLine> warningVLList = GetVarLineListFromWarning(warnings);

            int codeLineNumber = 1;             //last line number of the current statement
            int newLineIndex = 0;               //not for use now
            int oldStmtLine = 0;                //last line number of the last statement
            int stmtNumber = 0;                 //the current statement number

            List<VariableLine> variableLineList = new List<VariableLine>();
            for (int k = newLineIndex; k < compilableText.Length; k++)
            {
                if (compilableText[k] == '\n')
                {
                    codeLineNumber++;
                }
                if (compilableText[k] == ';')
                {
                    stmtNumber++;
                    variableLineList.Clear();

                    variableLineList.AddRange(warningVLList);
                    foreach (VariableLine varLinePair in warningVLList)
                    {
                        if (varLinePair.line > codeLineNumber || varLinePair.line <= oldStmtLine)
                        {
                            variableLineList.Remove(varLinePair);
                        }
                    }
                    if (variableLineList.Count != 0)
                    {

                        inputLines.Add(stmtNumber, new List<VariableLine>(variableLineList));

                    }
                    oldStmtLine = codeLineNumber;
                }
            }
        }*/

        private static void GetInputLines(string compilableText, List<ProtoCore.BuildData.WarningEntry> warnings,
            Dictionary<int, List<VariableLine>> inputLines)
        {
            List<VariableLine> warningVLList = GetVarLineListFromWarning(warnings);

            if (warningVLList.Count == 0)
                return;

            int stmtNumber = 1;
            //List<VariableLine> variableLineList = new List<VariableLine>(warningVLList);
            foreach (var node in compileState.AstNodeList)
            {
                Validity.Assert(node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode);

                List<VariableLine> variableLineList = new List<VariableLine>();
                foreach (var warning in warningVLList)
                {
                    if (warning.line >= node.line && warning.line <= node.endLine)
                        variableLineList.Add(warning);
                }
                if(variableLineList.Count > 0)
                    inputLines.Add(stmtNumber, variableLineList);

                stmtNumber++;
            }
        }

        private static List<VariableLine> GetVarLineListFromWarning(List<ProtoCore.BuildData.WarningEntry> warnings)
        {
            List<VariableLine> result = new List<VariableLine>();
            foreach (ProtoCore.BuildData.WarningEntry warningEntry in warnings)
            {
                if (warningEntry.id == ProtoCore.BuildData.WarningID.kIdUnboundIdentifier)
                {
                    result.Add(new VariableLine()
                    {
                        variable = warningEntry.msg.Split(' ')[1].Replace("'", ""),
                        line = warningEntry.line
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// This is called to Parse individual assignment statements in Codeblock nodes in GraphUI
        /// and return the resulting ProtoAST node
        /// </summary>
        /// <param name="statement"></param>
        public static ProtoCore.AST.Node Parse(string statement, out ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode)
        {
            commentNode = null;
            BuildCompileState(true);

            Validity.Assert(compileState != null);
            Validity.Assert(statement != null);

            if (string.IsNullOrEmpty(statement))
                return null;

            System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(statement));
            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
            ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, compileState);

            p.Parse();
            commentNode = p.commentNode;

            return p.root;
        }

        public static List<ProtoCore.AST.Node> ParseCodeBlock(string code)
        {
            Validity.Assert(code != null);

            if (string.IsNullOrEmpty(code))
                return null;

            // TODO: Change the logic to ignore Import statements in this case using parser - pratapa
            // Check if this will work with modifier blocks as well
            /*string[] stmts = code.Split(';');
            string source = "";
            for (int i=0; i < stmts.Length; ++i)
            {
                if (!stmts[i].Contains("import"))
                    source += stmts[i] + ";";
            }*/


            ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();
            ProtoLanguage.CompileStateTracker compileState = new ProtoLanguage.CompileStateTracker(options);
            
            //compileState.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(compileState));
            //compileState.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(compileState));

            System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(code));
            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
            ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, compileState);

            p.Parse();
            ProtoCore.AST.AssociativeAST.CodeBlockNode cbn = p.root as ProtoCore.AST.AssociativeAST.CodeBlockNode;

            Validity.Assert(cbn != null);
            return p.GetParsedASTList(cbn);
        }

        // TODO: To Deprecate: Currently used in GetInputOutputInfo() overload used in ProtoTest.GraphCompiler.MicroFeatureTests
        // 
        
        private static void GetOutputLines(string code, ProtoLanguage.CompileStateTracker compileState, Dictionary<int, string> outputLines)
        {
            Validity.Assert(code != null);
            if (!String.IsNullOrEmpty(code))
            {

                System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(code));
                ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
                ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, compileState);

                p.Parse();

                foreach (var astNode in compileState.AstNodeList)
                {
                    var binaryExpr = astNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                    while (binaryExpr != null)
                    {
                        var lhsVariable = binaryExpr.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                        if (lhsVariable != null)
                        {
                            outputLines[lhsVariable.line] = lhsVariable.Name;
                        }

                        // deal with the case of continuous assingment
                        binaryExpr = binaryExpr.RightNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                    }
                }
            }
        }
        

        private static void GetOutputLines(string code, ProtoLanguage.CompileStateTracker compileState, HashSet<VariableLine> outputLines)
        {

            foreach (var astNode in compileState.AstNodeList)
            {
                var binaryExpr = astNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                while (binaryExpr != null)
                {
                    var lhsVariable = binaryExpr.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                    if (lhsVariable != null)
                    {
                        var varlinePair = new VariableLine(lhsVariable.Name, lhsVariable.line);
                        outputLines.Add(varlinePair);
                    }
                    else 
                    {
                        var lhs = binaryExpr.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierListNode;
                        Validity.Assert(lhs != null);

                        // TODO: Get identifier list name by DFS Traversing the node
                        /*List<ProtoCore.AST.AssociativeAST.AssociativeNode> astList = new List<ProtoCore.AST.AssociativeAST.AssociativeNode>();
                        astList.Add(lhs as ProtoCore.AST.AssociativeAST.AssociativeNode);
                        ProtoCore.SourceGen codegen = new ProtoCore.SourceGen(astList);
                        codegen.GenerateCode();*/

                        string stmt = ProtoCore.Utils.ParserUtils.ExtractStatementFromCode(code, lhs);
                        int equalIndex = stmt.IndexOf('=');
                        string identListName = ProtoCore.Utils.ParserUtils.GetLHSatAssignment(stmt, equalIndex)[0]; // codegen.Code;
                        
                        string varname = identListName.Split('.')[0];
                        var varlinePair = new VariableLine(varname, lhs.line);
                        //var varlinePair = new VariableLine(identListName, lhs.line);
                        outputLines.Add(varlinePair);
                    }

                    // deal with the case of continuous assingment
                    binaryExpr = binaryExpr.RightNode as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                }
            }            
        }
        /// <summary>
        /// Temporary implementation for importing external libraries
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string MD5Hash(string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }
        public static List<SnapshotNode> NodeToCodeBlocks(List<SnapshotNode> inputs, GraphToDSCompiler.GraphCompiler originalGC)
        {
            List<SnapshotNode> codeBlocks = new List<SnapshotNode>();
            GraphToDSCompiler.GraphCompiler newGC = GraphCompiler.CreateInstance();

            newGC.SetCore(compileState);
            GraphToDSCompiler.SynchronizeData newSyncData = new SynchronizeData();
            newSyncData.AddedNodes = inputs;
            newSyncData.ModifiedNodes = new List<SnapshotNode>();
            newSyncData.RemovedNodes = new List<uint>();
            GraphToDSCompiler.GraphBuilder GB = new GraphBuilder(newSyncData, newGC);

            #region fix connection for multi-line CBN
            /*for multi-line code blocks*/
            List<Node> completeList = originalGC.Graph.nodeList;
            List<uint> originalNodeUIDList = new List<uint>();
            foreach (Node oriGcNode in completeList)
            {
                originalNodeUIDList.Add(oriGcNode.Guid);
            }

            GB.AddNodesToAST();
            
            //List<SnapshotNode> inputsCopy = new List<SnapshotNode>(inputs);
            for (int i = 0; i < inputs.Count; i++)
            {
                SnapshotNode inputSnapshotNode = inputs[i];
                for (int j = 0; j < inputSnapshotNode.InputList.Count; j++)
                {
                    Connection inputConnection = inputSnapshotNode.InputList[j];
                    if (!originalNodeUIDList.Contains(inputConnection.OtherNode))
                    {
                        Connection correctedInputConnection = new Connection();
                        correctedInputConnection.LocalName = inputConnection.LocalName;
                        correctedInputConnection.LocalIndex = inputConnection.LocalIndex;
                        correctedInputConnection.IsImplicit = inputConnection.IsImplicit;
                        correctedInputConnection.OtherIndex = inputConnection.OtherIndex;
                        if (newGC.codeBlockUIDMap.ContainsKey(inputConnection.OtherNode))
                        {
                            correctedInputConnection.OtherNode = newGC.codeBlockUIDMap[inputConnection.OtherNode][inputConnection.OtherIndex];
                        }
                        else 
                        {
                            correctedInputConnection.OtherNode = originalGC.codeBlockUIDMap[inputConnection.OtherNode][inputConnection.OtherIndex];
                        }
                        inputSnapshotNode.InputList.Remove(inputConnection);
                        inputSnapshotNode.InputList.Insert(j, correctedInputConnection);
                    }
                }
                for (int j = 0; j < inputSnapshotNode.OutputList.Count; j++)
                {
                    Connection outputConnection = inputSnapshotNode.OutputList[j];
                    if (!originalNodeUIDList.Contains(outputConnection.OtherNode))                       // if the other node is split
                    {
                        Connection correctedInputConnection = new Connection();
                        correctedInputConnection.LocalName = outputConnection.LocalName;
                        correctedInputConnection.LocalIndex = outputConnection.LocalIndex;
                        correctedInputConnection.IsImplicit = outputConnection.IsImplicit;
                        correctedInputConnection.OtherIndex = outputConnection.OtherIndex;
                        if (newGC.codeBlockUIDMap.ContainsKey(outputConnection.OtherNode))
                        {
                            correctedInputConnection.OtherNode = newGC.GetUidOfRHSIdentifierInCodeBlock(
                                outputConnection.OtherNode, outputConnection.OtherIndex, outputConnection.LocalName);
                            //correctedInputConnection.OtherNode = newGC.codeBlockUIDMap[outputConnection.OtherNode][outputConnection.OtherIndex];
                        }
                        else
                        {
                            correctedInputConnection.OtherNode = originalGC.codeBlockUIDMap[outputConnection.OtherNode][outputConnection.OtherIndex];
                        }
                        inputSnapshotNode.OutputList.Remove(outputConnection);
                        inputSnapshotNode.OutputList.Insert(j, correctedInputConnection);
                    }
                }
            }
            GB.nodesToAdd = inputs;
            GB.MakeConnectionsForAddedNodes_NodeToCode(GB.nodesToAdd);
            newGC.PrintGraph();
            #endregion

            //GB.BuildGraphForCodeBlock();
            
            List<uint> nodesToBeAdded = new List<uint>();
            List<Node> nodesToBeReplaced = new List<Node>();

            //adding children node from the originalGC to the newGC needed for the newGC to generate code
            foreach (Node n in completeList)
            {
                foreach (Node child in n.GetChildren())
                {
                    if (newGC.Graph.GetNode(child.Guid) != null)
                    {
                        if (child.Name != newGC.Graph.GetNode(child.Guid).Name)
                        {
                            nodesToBeReplaced.Add(child);
                        }
                    }
                }
            }

            foreach (uint n in nodesToBeAdded)
            {
                Node n1 = completeList.FirstOrDefault(q => q.Guid == n);
                //n1.children.Clear();
                nodesToBeReplaced.Add(n1);
                //newSyncData.RemovedNodes.Add(n);
            }

            List<uint> nodeToCodeUIDs = new List<uint>();
            foreach (SnapshotNode ssn in inputs)
                nodeToCodeUIDs.Add(ssn.Id);
            newGC.nodeToCodeUIDs = nodeToCodeUIDs;

            /*create output snapshot nodes*/
            List<Connection> inputNodeInputConnections = new List<Connection>();
            List<Connection> inputNodeOutputConnections = new List<Connection>();
            foreach (SnapshotNode ssn in inputs)
            {
                foreach (Connection inputConnection in ssn.InputList)
                {
                    if (!nodeToCodeUIDs.Contains(inputConnection.OtherNode))
                        inputNodeInputConnections.Add(inputConnection);
                }
                foreach (Connection outputConnection in ssn.OutputList)
                {
                    if (!nodeToCodeUIDs.Contains(outputConnection.OtherNode))
                        inputNodeOutputConnections.Add(outputConnection);
                }
            }

            newGC.ReplaceNodesFromAList(nodesToBeReplaced);
            newSyncData.AddedNodes = new List<SnapshotNode>();
            newSyncData.ModifiedNodes = new List<SnapshotNode>();
            newSyncData.RemovedNodes = new List<uint>();
            GB = new GraphBuilder(newSyncData, newGC);
            //string result = GB.BuildGraphDAG();           
            List<SnapshotNode> nodeToCodeBlocks = GB.PrintCodeForSelectedNodes(originalGC, inputs);

            

            /*for now, only connected nodes are supported: return all connections that are not internal connections (connections between the selected nodes)*/
            //uint id = 0;
            //foreach (string content in toCode)
            //{
            //    SnapshotNode ssn = new SnapshotNode();
            //    ssn.Type = SnapshotNodeType.CodeBlock;
            //    ssn.Content = content;
            //    ssn.Id = id++;
            //    ssn.InputList = new List<Connection>();
            //    //stupid stub
            //    foreach (Connection inputConnection in inputNodeInputConnections)
            //    {
            //        Connection newInputConnection = new Connection();
            //        newInputConnection.OtherNode = inputConnection.OtherNode;
            //        newInputConnection.OtherIndex = inputConnection.OtherIndex;
            //        newInputConnection.IsImplicit = inputConnection.IsImplicit;
            //        string[] tokens = newGC.Graph.GetNode(inputConnection.OtherNode).Name.Split('=');
            //        newInputConnection.LocalName = tokens[0];
            //        ssn.InputList.Add(newInputConnection);
            //    }
            //    //ssn.InputList = inputNodeInputConnections; 
            //    ssn.OutputList = new List<Connection>();
            //    foreach (Connection outputConnection in inputNodeOutputConnections)
            //    {
            //        Connection newOutputConnection = new Connection();
            //        newOutputConnection.OtherNode = outputConnection.OtherNode;
            //        newOutputConnection.OtherIndex = outputConnection.OtherIndex;
            //        newOutputConnection.IsImplicit = outputConnection.IsImplicit;
            //        //string[] tokens = originalGC.Graph.GetNode(outputConnection.OtherNode).Name.Split('=');
            //        newOutputConnection.LocalName = outputConnection.LocalName;
            //        ssn.OutputList.Add(newOutputConnection);
            //    }
            //    //ssn.OutputList = inputNodeOutputConnections;
            //    codeBlocks.Add(ssn);
            //}
            
            /*update the original GC*/
            foreach (SnapshotNode inputNode in inputs)
            {
                if (originalNodeUIDList.Contains(inputNode.Id))
                    originalGC.RemoveNodes(inputNode.Id, false);
                else 
                {
                    foreach (KeyValuePair<uint, Dictionary<int, uint>> kvp in originalGC.codeBlockUIDMap)
                    {
                        if (kvp.Value.ContainsValue(inputNode.Id))
                        {
                            originalGC.RemoveNodes(kvp.Key, false);
                        }
                    }
                }
            }
            foreach (Node node in newGC.Graph.nodeList)
            {
                node.Name = node.Name.TrimEnd(';') + ";";
            }
            originalGC.Graph.nodeList.Union<Node>(newGC.Graph.nodeList);
            //originalGC = newGC;
            /**/

            return nodeToCodeBlocks;
            //return codeBlocks;
        }

        public static bool CompareCode(string s1, string s2)
        {
            Validity.Assert(!string.IsNullOrEmpty(s1));
            Validity.Assert(!string.IsNullOrEmpty(s2));

            ProtoCore.AST.AssociativeAST.CodeBlockNode commentNode = null;
            ProtoCore.AST.Node s1Root = Parse(s1, out commentNode);
            ProtoCore.AST.Node s2Root = Parse(s2, out commentNode);
            return s1Root.Compare(s2Root);
        }

        public static string ConvertAbsoluteToRelative(string absolutePath)
        {
            if (!Path.IsPathRooted(absolutePath))
                return absolutePath; //it's already a relative path

            Uri current = new Uri(Directory.GetCurrentDirectory());
            string currentPath = current.ToString()+@"\";
            Uri correctCurrent = new Uri(currentPath);          //the function MakeRelativeUri doesn't work correctly if current doesn't end with a "\"
            Uri path = new Uri(absolutePath);

            return correctCurrent.MakeRelativeUri(path).ToString();
        }

        public static string ConvertRelativeToAbsolute(string relative)
        {
            return Path.GetFullPath(relative);
        }
    }
}
