/*
* Copyright (c) Contributors, http://opensimulator.org/
* See CONTRIBUTORS.TXT for a full list of copyright holders.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of the OpenSim Project nor the
*       names of its contributors may be used to endorse or promote products
*       derived from this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS AS IS AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
* 
*/

using System;
using System.Collections.Generic;

namespace OpenSim.Region.ScriptEngine.LSOEngine.LSO
{
    //public partial class LSL_BaseClass
    //{
    //    /*
    //     * OPCODES
    //     *
    //     * These are internal "assembly" commands,
    //     *  basic operators like "ADD", "PUSH" and "POP"
    //     * 
    //     * It also contains managed stack and keeps track of internal variables, etc.
    //     * 
    //     */


    //    public void StoreToLocal(UInt32 index)
    //    {
    //        // TODO: How to determine local?
    //        LSOEngine.LSO.Common.SendToDebug("::StoreToLocal " + index);
    //        if (LocalVariables.ContainsKey(index))
    //            LocalVariables.Remove(index);
    //        LocalVariables.Add(index, LSLStack.Peek());
    //    }

    //    public void StoreToGlobal(UInt32 index)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::StoreToGlobal " + index);
    //        if (GlobalVariables.ContainsKey(index))
    //            GlobalVariables.Remove(index);
    //        GlobalVariables.Add(index, LSLStack.Peek());
    //    }

    //    public void StoreToStatic(UInt32 index)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::StoreToStatic " + index);
    //        //if (StaticVariables.ContainsKey(index))
    //        //    StaticVariables.Remove(index);
    //        StaticVariables.Add(index, LSLStack.Peek());
    //    }

    //    public void GetFromLocal(UInt32 index)
    //    {
    //        // TODO: How to determine local?
    //        LSOEngine.LSO.Common.SendToDebug("::GetFromLocal " + index);
    //        object ret;
    //        LocalVariables.TryGetValue(index, out ret);
    //        LSLStack.Push(ret);
    //        //return ret;
    //    }

    //    public void GetFromGlobal(UInt32 index)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::GetFromGlobal " + index);
    //        object ret;
    //        GlobalVariables.TryGetValue(index, out ret);
    //        LSLStack.Push(ret);
    //        //return ret;
    //    }

    //    public void GetFromStatic(UInt32 index)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::GetFromStatic " + index);
    //        object ret;
    //        StaticVariables.TryGetValue(index, out ret);
    //        LSOEngine.LSO.Common.SendToDebug("::GetFromStatic - ObjectType: " + ret.GetType().ToString());
    //        LSLStack.Push(ret);
    //        //return ret;
    //    }

    //    public object POPToStack()
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::POPToStack");
    //        //return LSLStack.Pop();
    //        object p = LSLStack.Pop();
    //        if (p.GetType() == typeof (UInt32))
    //            return (UInt32) p;
    //        if (p.GetType() == typeof (string))
    //            return (string) p;
    //        if (p.GetType() == typeof (Int32))
    //            return (Int32) p;
    //        if (p.GetType() == typeof (UInt16))
    //            return (UInt16) p;
    //        if (p.GetType() == typeof (float))
    //            return (float) p;
    //        if (p.GetType() == typeof (LSO_Enums.Vector))
    //            return (LSO_Enums.Vector) p;
    //        if (p.GetType() == typeof (LSO_Enums.Rotation))
    //            return (LSO_Enums.Rotation) p;
    //        if (p.GetType() == typeof (LSO_Enums.Key))
    //            return (LSO_Enums.Key) p;

    //        return p;
    //    }

    //    //public object POPToStack(UInt32 count)
    //    //{
    //    //    // POP NUMBER FROM TOP OF STACK
    //    //    //LSLStack.SetLength(LSLStack.Length - 4);
    //    //    Common.SendToDebug("::POPToStack " + count);
    //    //    if (count < 2)
    //    //        return LSLStack.Pop();

    //    //    Stack<object> s = new Stack<object>();
    //    //    for (int i = 0; i < count; i++)
    //    //    {
    //    //        s.Push(LSLStack.Pop);

    //    //    }

    //    //}

    //    public void POP()
    //    {
    //        // POP NUMBER FROM TOP OF STACK
    //        //LSLStack.SetLength(LSLStack.Length - 4);
    //        LSOEngine.LSO.Common.SendToDebug("::POP");
    //        if (LSLStack.Count < 1)
    //        {
    //            //TODO: Temporary fix
    //            LSOEngine.LSO.Common.SendToDebug("ERROR: TRYING TO POP EMPTY STACK!");
    //        }
    //        else
    //        {
    //            LSLStack.Pop();
    //        }
    //    }

    //    public void PUSH(object Param)
    //    {
    //        if (Param == null)
    //        {
    //            LSOEngine.LSO.Common.SendToDebug("::PUSH: <null>");
    //        }
    //        else
    //        {
    //            //Common.SendToDebug("::PUSH: " + Param.GetType());
    //        }

    //        LSLStack.Push(Param);
    //    }

    //    public void ADD(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::ADD: " + Param);
    //        object o2 = LSLStack.Pop();
    //        object o1 = LSLStack.Pop();
    //        LSOEngine.LSO.Common.SendToDebug("::ADD: Debug: o1: " + o1.GetType() + " (" + o1.ToString() + "), o2: " + o2.GetType() +
    //                           " (" + o2.ToString() + ")");
    //        if (o2.GetType() == typeof (string))
    //        {
    //            LSLStack.Push((string) o1 + (string) o2);
    //            return;
    //        }
    //        if (o2.GetType() == typeof (UInt32))
    //        {
    //            LSLStack.Push((UInt32) o1 + (UInt32) o2);
    //            return;
    //        }
    //    }

    //    public void SUB(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::SUB: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1 - i2));
    //    }

    //    public void MUL(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::SUB: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1*i2));
    //    }

    //    public void DIV(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::DIV: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1/i2));
    //    }


    //    public void MOD(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::MOD: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1%i2));
    //    }

    //    public void EQ(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::EQ: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        if (i1 == i2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void NEQ(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::NEQ: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        if (i1 != i2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void LEQ(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::LEQ: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        if (i1 <= i2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void GEQ(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::GEQ: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        if (i1 >= i2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void LESS(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::LESS: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        if (i1 < i2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void GREATER(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::GREATER: " + Param);
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        if (i1 > i2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }


    //    public void BITAND()
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::BITAND");
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1 & i2));
    //    }

    //    public void BITOR()
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::BITOR");
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1 | i2));
    //    }

    //    public void BITXOR()
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::BITXOR");
    //        UInt32 i2 = (UInt32) LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1 ^ i2));
    //    }

    //    public void BOOLAND()
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::BOOLAND");
    //        bool b2 = bool.Parse((string) LSLStack.Pop());
    //        bool b1 = bool.Parse((string) LSLStack.Pop());
    //        if (b1 && b2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void BOOLOR()
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::BOOLOR");
    //        bool b2 = bool.Parse((string) LSLStack.Pop());
    //        bool b1 = bool.Parse((string) LSLStack.Pop());

    //        if (b1 || b2)
    //        {
    //            LSLStack.Push((UInt32) 1);
    //        }
    //        else
    //        {
    //            LSLStack.Push((UInt32) 0);
    //        }
    //    }

    //    public void NEG(UInt32 Param)
    //    {
    //        LSOEngine.LSO.Common.SendToDebug("::NEG: " + Param);
    //        //UInt32 i2 = (UInt32)LSLStack.Pop();
    //        UInt32 i1 = (UInt32) LSLStack.Pop();
    //        LSLStack.Push((UInt32) (i1*-1));
    //    }

    //    public void BITNOT()
    //    {
    //        //Common.SendToDebug("::BITNOT");
    //        //UInt32 i2 = (UInt32)LSLStack.Pop();
    //        //UInt32 i1 = (UInt32)LSLStack.Pop();
    //        //LSLStack.Push((UInt32)(i1 / i2));
    //    }

    //    public void BOOLNOT()
    //    {
    //        //Common.SendToDebug("::BOOLNOT");
    //        ////UInt32 i2 = (UInt32)LSLStack.Pop();
    //        //UInt32 i1 = (UInt32)LSLStack.Pop();
    //        //LSLStack.Push((UInt32)(i1));
    //    }
    //}
}