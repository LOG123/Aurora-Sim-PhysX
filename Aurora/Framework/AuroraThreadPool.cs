/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using OpenSim.Framework;
using Aurora.Framework;

namespace Aurora.Framework
{
    public class AuroraThreadPoolStartInfo
    {
        public ThreadPriority priority;
        public int Threads = 0;
        public int InitialSleepTime = 10;
        public int MaxSleepTime = 100;
        public int SleepIncrementTime = 10;
        public bool KillThreadAfterQueueClear = false;
        public string Name = "";
    }

    public class AuroraThreadPool
    {
        public delegate void QueueItem ();
        
        AuroraThreadPoolStartInfo m_info = null;
        Thread[] Threads = null;
        int[] Sleeping;
        Queue<QueueItem> queue = new Queue<QueueItem> ();
        public long nthreads;
        public long nSleepingthreads;

        public AuroraThreadPool(AuroraThreadPoolStartInfo info)
        {
            m_info = info;
            Threads = new Thread[m_info.Threads];
            Sleeping = new int[m_info.Threads];
            nthreads = 0;
            nSleepingthreads = 0;
            // lets threads check for work a bit faster in case we have all sleeping and awake interrupt fails
        }

        private void ThreadStart(object number)
        {
            Culture.SetCurrentCulture ();
            int OurSleepTime = 0;

            int[] numbers = number as int[];
            int ThreadNumber = numbers[0];

            while (true)
            {
                try
                {
                    QueueItem item = null;
                    lock (queue)
                    {
                        if (queue.Count != 0)
                            item = queue.Dequeue ();
                    }

                    if (item == null)
                    {
                        OurSleepTime += m_info.SleepIncrementTime;
                        if (m_info.KillThreadAfterQueueClear || OurSleepTime > m_info.MaxSleepTime)
                        {
                            Threads[ThreadNumber] = null;
                            Interlocked.Decrement(ref nthreads);
                            break;
                        }
                        else
                        {
                            Interlocked.Exchange(ref Sleeping[ThreadNumber], 1);
                            Interlocked.Increment(ref nSleepingthreads);
                            try { Thread.Sleep (OurSleepTime); }
                            catch (ThreadInterruptedException) { }
                            Interlocked.Decrement(ref nSleepingthreads);
                            Interlocked.Exchange(ref Sleeping[ThreadNumber], 0);
                            continue;
                        }
                    }
                    else
                    {
                        // workers have no business on pool waiting times
                        // that whould make interrelations very hard to debug
                        // If a worker wants to delay its requeue, then he should for now sleep before
                        // asking to be requeued.
                        // in future we should add a trigger time delay as parameter to the queue request.
                        // so to release the thread sooner, like .net and mono can now do.
                        // This control loop whould then have to look for those delayed requests.
                        // UBIT
                        OurSleepTime = m_info.InitialSleepTime;
                        item.Invoke();
                    }
                }
                catch { }
                Thread.Sleep(OurSleepTime);
            }
        }

        public void QueueEvent(QueueItem delegat, int Priority)
        {
            if (delegat == null)
                return;
            lock (queue)
            {
                queue.Enqueue(delegat);
            }

            if (nthreads == 0 || (nthreads - nSleepingthreads < queue.Count - 1 && nthreads < Threads.Length))
            {
                lock (Threads)
                {
                    for (int i = 0; i < Threads.Length; i++)
                    {
                        if (Threads[i] == null)
                        {
                            Thread thread = new Thread (ThreadStart);
                            thread.Priority = m_info.priority;
                            thread.Name = (m_info.Name == "" ? "AuroraThreadPool" : m_info.Name) + "#" + i.ToString ();
                            thread.IsBackground = true;
                            try
                            {
                                thread.Start (new int[] { i });
                                Threads[i] = thread;
                                Sleeping[i] = 0;
                                nthreads++;
                            }
                            catch
                            {
                            }
                            return;
                        }
                    }
                }
            }
            else if (nSleepingthreads > 0)
            {
                lock (Threads)
                {
                    for (int i = 0; i < Threads.Length; i++)
                    {
                        if (Sleeping[i] == 1 && Threads[i].ThreadState == ThreadState.WaitSleepJoin)
                        {
                            Threads[i].Interrupt (); // if we have a sleeping one awake it
                            return;
                        }
                    }
                }
            }
        }

        public void AbortThread (Thread thread)
        {
            int i;
            lock (Threads)
            {
                for (i = 0; i < Threads.Length; i++)
                {
                    if (Threads[i] == thread)
                        break;
                }
                if (i == Threads.Length)
                    return;

                Threads[i] = null;
                nthreads--;
            }
            try
            {
                thread.Abort ("Shutdown");
            }
            catch
            {
            }
        }

        public void ClearEvents ()
        {
            lock (queue)
                queue.Clear ();
        }
    }
}
