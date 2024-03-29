﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ParserEngine;

namespace Whorl
{
    public class CSharpCompiledInfo
    {
        public class EvalInstance
        {
            public CSharpSharedCompiledInfo CSharpSharedCompiledInfo { get; }
            public object ClassInstance { get; }
            public object ParamsObj { get; private set; }
            public Action EvalFormula { get; }

            public EvalInstance(CSharpSharedCompiledInfo cSharpCompiledInfo, bool forFormula = true)
            {
                CSharpSharedCompiledInfo = cSharpCompiledInfo;
                ClassInstance = Activator.CreateInstance(CSharpSharedCompiledInfo.EvalClassType);
                if (forFormula)
                {
                    if (CSharpSharedCompiledInfo.ParametersPropertyInfo != null)
                        ParamsObj = CSharpSharedCompiledInfo.ParametersPropertyInfo.GetValue(ClassInstance);
                    try
                    {
                        EvalFormula = (Action)Delegate.CreateDelegate(typeof(Action), ClassInstance,
                                              CSharpSharedCompiledInfo.EvalMethodInfo);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error creating delegate: " + ex.Message);
                    }
                }
            }

            private bool CallMethod(MethodInfo methodInfo)
            {
                bool success = true;
                if (methodInfo != null)
                {
                    try
                    {
                        methodInfo.Invoke(ClassInstance, null);
                    }
                    catch (Exception ex)
                    {
                        Tools.HandleException(ex);
                        success = false;
                    }
                }
                return success;
            }

            public bool PreInitializeForEval()
            {
                return CallMethod(CSharpSharedCompiledInfo.PreInitMethodInfo);
            }

            public bool InitializeForEval()
            {
                return CallMethod(CSharpSharedCompiledInfo.InitMethodInfo);
            }

            public bool Initialize2ForEval()
            {
                return CallMethod(CSharpSharedCompiledInfo.Init2MethodInfo);
            }

            public void SetInfoObject(object info)
            {
                CSharpSharedCompiledInfo.InfoPropertyInfo.SetValue(ClassInstance, info);
            }

            public void GetParametersObject()
            {
                if (ClassInstance != null && CSharpSharedCompiledInfo.ParametersPropertyInfo != null)
                    ParamsObj = CSharpSharedCompiledInfo.ParametersPropertyInfo.GetValue(ClassInstance);
            }

            public void SetParametersObject(object oParams)
            {
                if (ClassInstance != null && CSharpSharedCompiledInfo.ParametersPropertyInfo != null)
                {
                    CSharpSharedCompiledInfo.ParametersPropertyInfo.SetValue(ClassInstance, oParams);
                    ParamsObj = oParams;
                }
            }

            //public void SetParametersObject(object oParams)
            //{
            //    if (ClassInstance != null && CSharpSharedCompiledInfo.ParametersPropertyInfo != null)
            //    {
            //        CSharpSharedCompiledInfo.ParametersPropertyInfo.SetValue(ClassInstance, oParams);
            //        ParamsObj = oParams;
            //    }
            //}

            public bool UpdateParameters()
            {
                if (ParamsObj == null || CSharpSharedCompiledInfo.UpdateParametersMethodInfo == null)
                    return false;
                CSharpSharedCompiledInfo.UpdateParametersMethodInfo.Invoke(ParamsObj, null);
                return true;
            }

            public IEnumerable<PropertyInfo> GetDisplayedParameters(bool allowAllParams = true)
            {
                return CSharpSharedCompiledInfo.GetIncludedParameters(ParamsObj, allowAllParams);
            }

            public List<ParamInfo> GetParamInfos()
            {
                var keys = new List<ParamInfo>();
                if (ParamsObj != null)
                {
                    foreach (var propertyInfo in GetDisplayedParameters(allowAllParams: false))
                    {
                        if (propertyInfo.PropertyType.IsArray)
                        {
                            object oValue = propertyInfo.GetValue(ParamsObj);
                            if (oValue != null)
                            {
                                var array = (Array)oValue;
                                keys.AddRange(Enumerable.Range(0, array.Length).Select(i => new ParamInfo(propertyInfo.Name, i)));
                            }
                        }
                        else
                            keys.Add(new ParamInfo(propertyInfo.Name));
                    }
                }
                return keys;
            }
        }

        public CSharpSharedCompiledInfo CSharpSharedCompiledInfo { get; }

        public CSharpCompiledInfo(CSharpSharedCompiledInfo cSharpSharedCompiledInfo)
        {
            CSharpSharedCompiledInfo = cSharpSharedCompiledInfo;
        }

        public EvalInstance CreateEvalInstance(bool forFormula = true)
        {
            if (CSharpSharedCompiledInfo.ErrorCount != 0)
                return null;
            return new EvalInstance(CSharpSharedCompiledInfo, forFormula);
        }

    }


}
