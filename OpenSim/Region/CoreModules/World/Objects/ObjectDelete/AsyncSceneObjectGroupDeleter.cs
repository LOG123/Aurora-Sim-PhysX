/*
 * Copyright (c) Contributors, http://aurora-sim.org/, http://opensimulator.org/
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
using System.Collections.Generic;
using System.Reflection;
using log4net;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Region.CoreModules
{
    public class DeleteToInventoryHolder
    {
        public DeRezAction action;
        public UUID agentId;
        public List<ISceneEntity> objectGroups;
        public UUID folderID;
        public bool permissionToDelete;
        public bool permissionToTake;
    }

    /// <summary>
    /// Asynchronously derez objects.  This is used to derez large number of objects to inventory without holding
    /// up the main client thread.
    /// </summary>
    public class AsyncSceneObjectGroupDeleter : INonSharedRegionModule, IAsyncSceneObjectGroupDeleter
    {
        private static readonly ILog m_log
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <value>
        /// Is the deleter currently enabled?
        /// </value>
        public bool Enabled;

        private readonly Queue<DeleteToInventoryHolder> m_removeFromSimQueue = new Queue<DeleteToInventoryHolder>();
        private bool DeleteLoopInUse = false;
        private Scene m_scene;

        #region INonSharedRegionModule Members

        public string Name
        {
            get { return "AsyncSceneObjectGroupDeleter"; }
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        public void Initialise(Nini.Config.IConfigSource source)
        {
        }

        public void Close()
        {
        }

        public void AddRegion(Scene scene)
        {
            scene.RegisterModuleInterface<IAsyncSceneObjectGroupDeleter>(this);
            m_scene = scene;
        }

        public void RemoveRegion(Scene scene)
        {
            scene.UnregisterModuleInterface<IAsyncSceneObjectGroupDeleter>(this);
        }

        public void RegionLoaded(Scene scene)
        {
        }

        #endregion

        #region Delete To Inventory

        /// <summary>
        /// Delete the given object from the scene
        /// </summary>
        public void DeleteToInventory(DeRezAction action, UUID folderID,
                List<ISceneEntity> objectGroups, UUID AgentId,
                bool permissionToDelete, bool permissionToTake)
        {
            DeleteToInventoryHolder dtis = new DeleteToInventoryHolder();
            dtis.action = action;
            dtis.folderID = folderID;
            dtis.objectGroups = objectGroups;
            dtis.agentId = AgentId;
            dtis.permissionToDelete = permissionToDelete;
            dtis.permissionToTake = permissionToTake;
            //Do this before the locking so that the objects 'appear' gone and the client doesn't think things have gone wrong
            if (permissionToDelete)
            {
                DeleteGroups(objectGroups);
            }

            lock (m_removeFromSimQueue)
            {
                m_removeFromSimQueue.Enqueue(dtis);
            }

            if (!DeleteLoopInUse)
            {
                DeleteLoopInUse = true;
                //m_log.Debug("[SCENE]: Starting delete loop");
                Util.FireAndForget(DoDeleteObject);
            }
        }

        private void DeleteGroups(List<ISceneEntity> objectGroups)
        {
            m_scene.ForEachScenePresence(delegate(IScenePresence avatar)
            {
                lock (objectGroups)
                {
                    foreach (ISceneEntity grp in objectGroups)
                    {
                        avatar.ControllingClient.SendKillObject (m_scene.RegionInfo.RegionHandle, grp.ChildrenEntities().ToArray ());
                    }
                }
            });
        }

        public void DoDeleteObject(object o)
        {
            if (DeleteObject())
            {
                //Requeue us if there is some left
                Util.FireAndForget(DoDeleteObject);
            }
            else
            {
                DeleteLoopInUse = false;
                //m_log.Debug("[SCENE]: Ending delete loop");
            }
        }

        public bool DeleteObject()
        {
            DeleteToInventoryHolder x = null;

            try
            {
                int left = 0;
                lock (m_removeFromSimQueue)
                {
                    left = m_removeFromSimQueue.Count;
                }
                if (left > 0)
                {
                    lock (m_removeFromSimQueue)
                    {
                        x = m_removeFromSimQueue.Dequeue();
                    }

                    if (x.permissionToDelete)
                    {
                        IBackupModule backup = m_scene.RequestModuleInterface<IBackupModule>();
                        if (backup != null)
                            backup.DeleteSceneObjects(x.objectGroups.ToArray(), true);
                    }
                    m_log.DebugFormat(
                        "[SCENE]: Sending object to user's inventory, {0} item(s) remaining.", left);

                    if (x.permissionToTake)
                    {
                        try
                        {
                            IInventoryAccessModule invAccess = m_scene.RequestModuleInterface<IInventoryAccessModule>();
                            UUID itemID;
                            if (invAccess != null)
                                invAccess.DeleteToInventory(x.action, x.folderID, x.objectGroups, x.agentId, out itemID);
                        }
                        catch (Exception e)
                        {
                            m_log.ErrorFormat(
                                "[ASYNC DELETER]: Exception background sending object: {0}{1}", e.Message, e.StackTrace);
                        }
                    }
                    return true;
                }
            }
            catch (Exception e)
            {
                // We can't put the object group details in here since the root part may have disappeared (which is where these sit).
                // FIXME: This needs to be fixed.
                m_log.ErrorFormat(
                    "[SCENE]: Queued sending of scene object to agent {0} {1} failed: {2}",
                    (x != null ? x.agentId.ToString() : "unavailable"), (x != null ? x.agentId.ToString() : "unavailable"), e.ToString());
            }

            //m_log.Debug("[SCENE]: No objects left in delete queue.");
            return false;
        }

        #endregion
    }
}
