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
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Region.Physics.Manager;

namespace OpenSim.Region.Physics.BasicPhysicsPlugin
{
    public class BasicScene : PhysicsScene
    {
        private List<PhysicsActor> _actors = new List<PhysicsActor>();
        private short[] _heightMap;
        private RegionInfo m_region;

        //protected internal string sceneIdentifier;

        public BasicScene(string _sceneIdentifier)
        {
            //sceneIdentifier = _sceneIdentifier;
        }

        public override void Initialise (IMesher meshmerizer, RegionInfo region, IRegistryCore registry)
        {
            m_region = region;
        }

        public override void PostInitialise(IConfigSource config)
        {
        }

        public override void Dispose()
        {
        }

        public override PhysicsCharacter AddAvatar (string avName, Vector3 position, Quaternion rotation, Vector3 size, bool isFlying, uint localID, UUID UUID)
        {
            BasicCharacterActor act = new BasicCharacterActor();
            act.Position = position;
            act.Flying = isFlying;
            _actors.Add(act);
            return act;
        }

        public override void RemovePrim(PhysicsObject prim)
        {
        }

        public override void RemoveAvatar(PhysicsCharacter actor)
        {
            BasicCharacterActor act = (BasicCharacterActor)actor;
            if (_actors.Contains(act))
            {
                _actors.Remove(act);
            }
        }

/*
        public override PhysicsActor AddPrim(Vector3 position, Vector3 size, Quaternion rotation)
        {
            return null;
        }
*/

        public override PhysicsObject AddPrimShape(ISceneChildEntity entity)
        {
            return null;
        }

        public override void AddPhysicsActorTaint(PhysicsActor prim)
        {
        }

        public override float Simulate(float timeStep)
        {
            float fps = 0;
            for (int i = 0; i < _actors.Count; ++i)
            {
                PhysicsActor actor = _actors[i];
                Vector3 actorPosition = actor.Position;
                Vector3 actorVelocity = actor.Velocity;

                actorPosition.X += actor.Velocity.X*timeStep;
                actorPosition.Y += actor.Velocity.Y*timeStep;

                if (actor.Position.Y < 0)
                {
                    actorPosition.Y = 0.1F;
                }
                else if (actor.Position.Y >= m_region.RegionSizeY)
                {
                    actorPosition.Y = (m_region.RegionSizeY - 0.1f);
                }

                if (actor.Position.X < 0)
                {
                    actorPosition.X = 0.1F;
                }
                else if (actor.Position.X >= m_region.RegionSizeX)
                {
                    actorPosition.X = (m_region.RegionSizeX - 0.1f);
                }

                float height = _heightMap[(int)actor.Position.Y * m_region.RegionSizeX + (int)actor.Position.X] + actor.Size.Z;
                if (actor.Flying)
                {
                    if (actor.Position.Z + (actor.Velocity.Z*timeStep) <
                        _heightMap[(int)actor.Position.Y * m_region.RegionSizeX + (int)actor.Position.X] + 2)
                    {
                        actorPosition.Z = height;
                        actorVelocity.Z = 0;
                        actor.IsColliding = true;
                    }
                    else
                    {
                        actorPosition.Z += actor.Velocity.Z*timeStep;
                        actor.IsColliding = false;
                    }
                }
                else
                {
                    actorPosition.Z = height;
                    actorVelocity.Z = 0;
                    actor.IsColliding = true;
                }

                actor.Position = actorPosition;
                actor.Velocity = actorVelocity;
            }

            return fps;
        }

        public override void SetWaterLevel(short[] map)
        {
        }

        public override Dictionary<uint, float> GetTopColliders()
        {
            Dictionary<uint, float> returncolliders = new Dictionary<uint, float>();
            return returncolliders;
        }

        public override void SetTerrain (ITerrainChannel channel, short[] heightMap)
        {
            _heightMap = heightMap;
        }

        public override bool DisableCollisions
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public override bool UseUnderWaterPhysics
        {
            get { return false; }
        }
    }
}
