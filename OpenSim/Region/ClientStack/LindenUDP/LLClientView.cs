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
 *     * Neither the name of the OpenSimulator Project nor the
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
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using log4net;
using OpenMetaverse;
using OpenMetaverse.Packets;
using OpenMetaverse.Messages.Linden;
using OpenMetaverse.StructuredData;
using OpenSim.Framework;
using OpenSim.Framework.Client;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Services.Interfaces;
using Timer = System.Timers.Timer;
using AssetLandmark = OpenSim.Framework.AssetLandmark;
using Nini.Config;
using Mischel.Collections;

namespace OpenSim.Region.ClientStack.LindenUDP
{
    public delegate bool PacketMethod(IClientAPI simClient, Packet packet);

    /// <summary>
    /// Handles new client connections
    /// Constructor takes a single Packet and authenticates everything
    /// </summary>
    public class LLClientView : IClientAPI, IClientCore, IClientIM, IClientChat, IClientIPEndpoint, IStatsCollector
    {
        /// <value>
        /// Debug packet level.  See OpenSim.RegisterConsoleCommands() for more details.
        /// </value>
        protected int m_debugPacketLevel = 0;

        #region Events

        public event BinaryGenericMessage OnBinaryGenericMessage;
        public event Action<IClientAPI> OnLogout;
        public event ObjectPermissions OnObjectPermissions;
        public event Action<IClientAPI> OnConnectionClosed;
        public event ViewerEffectEventHandler OnViewerEffect;
        public event ImprovedInstantMessage OnInstantMessage;
        public event ChatMessage OnChatFromClient;
        public event RezObject OnRezObject;
        public event DeRezObject OnDeRezObject;
        public event ModifyTerrain OnModifyTerrain;
        public event Action<IClientAPI> OnRegionHandShakeReply;
        public event GenericCall1 OnRequestWearables;
        public event SetAppearance OnSetAppearance;
        public event AvatarNowWearing OnAvatarNowWearing;
        public event RezSingleAttachmentFromInv OnRezSingleAttachmentFromInv;
        public event RezMultipleAttachmentsFromInv OnRezMultipleAttachmentsFromInv;
        public event UUIDNameRequest OnDetachAttachmentIntoInv;
        public event ObjectAttach OnObjectAttach;
        public event ObjectDeselect OnObjectDetach;
        public event ObjectDrop OnObjectDrop;
        public event GenericCall1 OnCompleteMovementToRegion;
        public event UpdateAgent OnAgentUpdate;
        public event AgentRequestSit OnAgentRequestSit;
        public event AgentSit OnAgentSit;
        public event AvatarPickerRequest OnAvatarPickerRequest;
        public event StartAnim OnStartAnim;
        public event StopAnim OnStopAnim;
        public event Action<IClientAPI> OnRequestAvatarsData;
        public event LinkObjects OnLinkObjects;
        public event DelinkObjects OnDelinkObjects;
        public event GrabObject OnGrabObject;
        public event DeGrabObject OnDeGrabObject;
        public event SpinStart OnSpinStart;
        public event SpinStop OnSpinStop;
        public event ObjectDuplicate OnObjectDuplicate;
        public event ObjectDuplicateOnRay OnObjectDuplicateOnRay;
        public event MoveObject OnGrabUpdate;
        public event SpinObject OnSpinUpdate;
        public event AddNewPrim OnAddPrim;
        public event RequestGodlikePowers OnRequestGodlikePowers;
        public event GodKickUser OnGodKickUser;
        public event ObjectExtraParams OnUpdateExtraParams;
        public event UpdateShape OnUpdatePrimShape;
        public event ObjectRequest OnObjectRequest;
        public event ObjectSelect OnObjectSelect;
        public event ObjectDeselect OnObjectDeselect;
        public event GenericCall7 OnObjectDescription;
        public event GenericCall7 OnObjectName;
        public event GenericCall7 OnObjectClickAction;
        public event GenericCall7 OnObjectMaterial;
        public event ObjectIncludeInSearch OnObjectIncludeInSearch;
        public event RequestObjectPropertiesFamily OnRequestObjectPropertiesFamily;
        public event UpdatePrimFlags OnUpdatePrimFlags;
        public event UpdatePrimTexture OnUpdatePrimTexture;
        public event UpdateVectorWithUpdate OnUpdatePrimGroupPosition;
        public event UpdateVectorWithUpdate OnUpdatePrimSinglePosition;
        public event UpdatePrimRotation OnUpdatePrimGroupRotation;
        public event UpdatePrimSingleRotation OnUpdatePrimSingleRotation;
        public event UpdatePrimSingleRotationPosition OnUpdatePrimSingleRotationPosition;
        public event UpdatePrimGroupRotation OnUpdatePrimGroupMouseRotation;
        public event UpdateVector OnUpdatePrimScale;
        public event UpdateVector OnUpdatePrimGroupScale;

        #pragma warning disable 67

        public event StatusChange OnChildAgentStatus;
        public event GenericMessage OnGenericMessage;
        public event BuyObjectInventory OnBuyObjectInventory;
        public event TerrainUnacked OnUnackedTerrain;
        public event SetEstateTerrainBaseTexture OnSetEstateTerrainBaseTexture;

        #pragma warning restore 67

        public event RequestMapBlocks OnRequestMapBlocks;
        public event RequestMapName OnMapNameRequest;
        public event TeleportLocationRequest OnTeleportLocationRequest;
        public event TeleportLandmarkRequest OnTeleportLandmarkRequest;
        public event RequestAvatarProperties OnRequestAvatarProperties;
        public event SetAlwaysRun OnSetAlwaysRun;
        public event FetchInventory OnAgentDataUpdateRequest;
        public event TeleportLocationRequest OnSetStartLocationRequest;
        public event UpdateAvatarProperties OnUpdateAvatarProperties;
        public event CreateNewInventoryItem OnCreateNewInventoryItem;
        public event LinkInventoryItem OnLinkInventoryItem;
        public event CreateInventoryFolder OnCreateNewInventoryFolder;
        public event UpdateInventoryFolder OnUpdateInventoryFolder;
        public event MoveInventoryFolder OnMoveInventoryFolder;
        public event FetchInventoryDescendents OnFetchInventoryDescendents;
        public event PurgeInventoryDescendents OnPurgeInventoryDescendents;
        public event FetchInventory OnFetchInventory;
        public event RequestTaskInventory OnRequestTaskInventory;
        public event UpdateInventoryItem OnUpdateInventoryItem;
        public event ChangeInventoryItemFlags OnChangeInventoryItemFlags;
        public event CopyInventoryItem OnCopyInventoryItem;
        public event MoveInventoryItem OnMoveInventoryItem;
        public event RemoveInventoryItem OnRemoveInventoryItem;
        public event RemoveInventoryFolder OnRemoveInventoryFolder;
        public event UDPAssetUploadRequest OnAssetUploadRequest;
        public event XferReceive OnXferReceive;
        public event RequestXfer OnRequestXfer;
        public event ConfirmXfer OnConfirmXfer;
        public event AbortXfer OnAbortXfer;
        public event RequestTerrain OnRequestTerrain;
        public event RezScript OnRezScript;
        public event UpdateTaskInventory OnUpdateTaskInventory;
        public event MoveTaskInventory OnMoveTaskItem;
        public event RemoveTaskInventory OnRemoveTaskItem;
        public event UUIDNameRequest OnNameFromUUIDRequest;
        public event ParcelAccessListRequest OnParcelAccessListRequest;
        public event ParcelAccessListUpdateRequest OnParcelAccessListUpdateRequest;
        public event ParcelPropertiesRequest OnParcelPropertiesRequest;
        public event ParcelDivideRequest OnParcelDivideRequest;
        public event ParcelJoinRequest OnParcelJoinRequest;
        public event ParcelPropertiesUpdateRequest OnParcelPropertiesUpdateRequest;
        public event ParcelSelectObjects OnParcelSelectObjects;
        public event ParcelObjectOwnerRequest OnParcelObjectOwnerRequest;
        public event ParcelAbandonRequest OnParcelAbandonRequest;
        public event ParcelGodForceOwner OnParcelGodForceOwner;
        public event ParcelReclaim OnParcelReclaim;
        public event ParcelReturnObjectsRequest OnParcelReturnObjectsRequest;
        public event ParcelReturnObjectsRequest OnParcelDisableObjectsRequest;
        public event ParcelDeedToGroup OnParcelDeedToGroup;
        public event RegionInfoRequest OnRegionInfoRequest;
        public event EstateCovenantRequest OnEstateCovenantRequest;
        public event FriendActionDelegate OnApproveFriendRequest;
        public event FriendActionDelegate OnDenyFriendRequest;
        public event FriendshipTermination OnTerminateFriendship;
        public event GrantUserFriendRights OnGrantUserRights;
        public event MoneyTransferRequest OnMoneyTransferRequest;
        public event EconomyDataRequest OnEconomyDataRequest;
        public event MoneyBalanceRequest OnMoneyBalanceRequest;
        public event ParcelBuy OnParcelBuy;
        public event UUIDNameRequest OnTeleportHomeRequest;
        public event UUIDNameRequest OnUUIDGroupNameRequest;
        public event ScriptAnswer OnScriptAnswer;
        public event RequestPayPrice OnRequestPayPrice;
        public event ObjectSaleInfo OnObjectSaleInfo;
        public event ObjectBuy OnObjectBuy;
        public event AgentSit OnUndo;
        public event AgentSit OnRedo;
        public event LandUndo OnLandUndo;
        public event ForceReleaseControls OnForceReleaseControls;
        public event GodLandStatRequest OnLandStatRequest;
        public event RequestObjectPropertiesFamily OnObjectGroupRequest;
        public event DetailedEstateDataRequest OnDetailedEstateDataRequest;
        public event SetEstateFlagsRequest OnSetEstateFlagsRequest;
        public event SetEstateTerrainDetailTexture OnSetEstateTerrainDetailTexture;
        public event SetEstateTerrainTextureHeights OnSetEstateTerrainTextureHeights;
        public event CommitEstateTerrainTextureRequest OnCommitEstateTerrainTextureRequest;
        public event SetRegionTerrainSettings OnSetRegionTerrainSettings;
        public event BakeTerrain OnBakeTerrain;
        public event RequestTerrain OnUploadTerrain;
        public event EstateChangeInfo OnEstateChangeInfo;
        public event EstateRestartSimRequest OnEstateRestartSimRequest;
        public event EstateChangeCovenantRequest OnEstateChangeCovenantRequest;
        public event UpdateEstateAccessDeltaRequest OnUpdateEstateAccessDeltaRequest;
        public event SimulatorBlueBoxMessageRequest OnSimulatorBlueBoxMessageRequest;
        public event EstateBlueBoxMessageRequest OnEstateBlueBoxMessageRequest;
        public event EstateDebugRegionRequest OnEstateDebugRegionRequest;
        public event EstateTeleportOneUserHomeRequest OnEstateTeleportOneUserHomeRequest;
        public event EstateTeleportAllUsersHomeRequest OnEstateTeleportAllUsersHomeRequest;
        public event RegionHandleRequest OnRegionHandleRequest;
        public event ParcelInfoRequest OnParcelInfoRequest;
        public event ScriptReset OnScriptReset;
        public event GetScriptRunning OnGetScriptRunning;
        public event SetScriptRunning OnSetScriptRunning;
        public event UpdateVector OnAutoPilotGo;
        public event ActivateGesture OnActivateGesture;
        public event DeactivateGesture OnDeactivateGesture;
        public event ObjectOwner OnObjectOwner;
        public event DirPlacesQuery OnDirPlacesQuery;
        public event DirFindQuery OnDirFindQuery;
        public event DirLandQuery OnDirLandQuery;
        public event DirPopularQuery OnDirPopularQuery;
        public event DirClassifiedQuery OnDirClassifiedQuery;
        public event EventInfoRequest OnEventInfoRequest;
        public event ParcelSetOtherCleanTime OnParcelSetOtherCleanTime;
        public event MapItemRequest OnMapItemRequest;
        public event OfferCallingCard OnOfferCallingCard;
        public event AcceptCallingCard OnAcceptCallingCard;
        public event DeclineCallingCard OnDeclineCallingCard;
        public event SoundTrigger OnSoundTrigger;
        public event StartLure OnStartLure;
        public event TeleportLureRequest OnTeleportLureRequest;
        public event NetworkStats OnNetworkStatsUpdate;
        public event ClassifiedInfoRequest OnClassifiedInfoRequest;
        public event ClassifiedInfoUpdate OnClassifiedInfoUpdate;
        public event ClassifiedDelete OnClassifiedDelete;
        public event ClassifiedDelete OnClassifiedGodDelete;
        public event EventNotificationAddRequest OnEventNotificationAddRequest;
        public event EventNotificationRemoveRequest OnEventNotificationRemoveRequest;
        public event EventGodDelete OnEventGodDelete;
        public event ParcelDwellRequest OnParcelDwellRequest;
        public event UserInfoRequest OnUserInfoRequest;
        public event UpdateUserInfo OnUpdateUserInfo;
        public event RetrieveInstantMessages OnRetrieveInstantMessages;
        public event PickDelete OnPickDelete;
        public event PickGodDelete OnPickGodDelete;
        public event PickInfoUpdate OnPickInfoUpdate;
        public event AvatarNotesUpdate OnAvatarNotesUpdate;
        public event MuteListRequest OnMuteListRequest;
        public event AvatarInterestUpdate OnAvatarInterestUpdate;
        public event PlacesQuery OnPlacesQuery;
        public event AgentFOV OnAgentFOV;
        public event FindAgentUpdate OnFindAgent;
        public event TrackAgentUpdate OnTrackAgent;
        public event NewUserReport OnUserReport;
        public event SaveStateHandler OnSaveState;
        public event GroupAccountSummaryRequest OnGroupAccountSummaryRequest;
        public event GroupAccountDetailsRequest OnGroupAccountDetailsRequest;
        public event GroupAccountTransactionsRequest OnGroupAccountTransactionsRequest;
        public event FreezeUserUpdate OnParcelFreezeUser;
        public event EjectUserUpdate OnParcelEjectUser;
        public event ParcelBuyPass OnParcelBuyPass;
        public event ParcelGodMark OnParcelGodMark;
        public event GroupActiveProposalsRequest OnGroupActiveProposalsRequest;
        public event GroupVoteHistoryRequest OnGroupVoteHistoryRequest;
        public event SimWideDeletesDelegate OnSimWideDeletes;
        public event SendPostcard OnSendPostcard;
        public event TeleportCancel OnTeleportCancel;
        public event MuteListEntryUpdate OnUpdateMuteListEntry;
        public event MuteListEntryRemove OnRemoveMuteListEntry;
        public event GodlikeMessage OnGodlikeMessage;
        public event GodUpdateRegionInfoUpdate OnGodUpdateRegionInfoUpdate;
        public event GodlikeMessage OnEstateTelehubRequest;
        public event ViewerStartAuction OnViewerStartAuction;
        public event GroupProposalBallotRequest OnGroupProposalBallotRequest;
        public event AgentCachedTextureRequest OnAgentCachedTextureRequest;
        

        #endregion Events

        #region Enums

        public enum TransferPacketStatus : int
        {
            MorePacketsToCome = 0,
            Done = 1,
            AssetSkip = 2,
            AssetAbort = 3,
            AssetRequestFailed = -1,
            AssetUnknownSource = -2, // Equivalent of a 404
            InsufficientPermissions = -3
        }

        #endregion

        #region Class Members

        // LLClientView Only
        public delegate void BinaryGenericMessage(Object sender, string method, byte[][] args);

        /// <summary>Used to adjust Sun Orbit values so Linden based viewers properly position sun</summary>
        private const float m_sunPainDaHalfOrbitalCutoff = 4.712388980384689858f;

        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected static Dictionary<PacketType, PacketMethod> PacketHandlers = new Dictionary<PacketType, PacketMethod>(); //Global/static handlers for all clients

        private readonly LLUDPServer m_udpServer;
        private readonly LLUDPClient m_udpClient;
        private readonly UUID m_sessionId;
        private readonly UUID m_secureSessionId;
        protected readonly UUID m_agentId;
        private readonly uint m_circuitCode;
        private readonly byte[] m_channelVersion = Utils.EmptyBytes;
        private readonly Dictionary<string, UUID> m_defaultAnimations = new Dictionary<string, UUID>();
        private readonly IGroupsModule m_GroupsModule;

        private int m_cachedTextureSerial;
        private PriorityQueue<EntityUpdate, double> m_entityUpdates;
        private Prioritizer m_prioritizer;
        private bool m_disableFacelights = false;

        /// <value>
        /// Maintain a record of all the objects killed.  This allows us to stop an update being sent from the
        /// thread servicing the m_primFullUpdates queue after a kill.  If this happens the object persists as an
        /// ownerless phantom.
        ///
        /// All manipulation of this set has to occur under an m_entityUpdates.SyncRoot lock
        ///
        /// </value>
        protected HashSet<uint> m_killRecord = new HashSet<uint>();

//        protected HashSet<uint> m_attachmentsSent;

        private int m_animationSequenceNumber = 1;
        private bool m_SendLogoutPacketWhenClosing = true;
        private AgentUpdateArgs lastarg;
        private bool m_IsActive = true;
        private bool m_IsLoggingOut = false;

        protected Dictionary<PacketType, PacketProcessor> m_packetHandlers = new Dictionary<PacketType, PacketProcessor>();
        protected Dictionary<string, GenericMessage> m_genericPacketHandlers = new Dictionary<string, GenericMessage>(); //PauPaw:Local Generic Message handlers
        protected Scene m_scene;
        protected LLImageManager m_imageManager;
        protected string m_firstName;
        protected string m_lastName;
        protected Thread m_clientThread;
        protected Vector3 m_startpos;
        protected EndPoint m_userEndPoint;
        protected UUID m_activeGroupID;
        protected string m_activeGroupName = String.Empty;
        protected ulong m_activeGroupPowers;
        protected int m_terrainCheckerCount;
        protected uint m_agentFOVCounter;

        protected IAssetService m_assetService;
        private bool m_checkPackets = true;

        private Timer m_propertiesPacketTimer;
        private List<ObjectPropertiesPacket.ObjectDataBlock> m_propertiesBlocks = new List<ObjectPropertiesPacket.ObjectDataBlock>();

        #endregion Class Members

        #region Properties

        public LLUDPClient UDPClient { get { return m_udpClient; } }
        public IPEndPoint RemoteEndPoint { get { return m_udpClient.RemoteEndPoint; } }
        public UUID SecureSessionId { get { return m_secureSessionId; } }
        public IScene Scene { get { return m_scene; } }
        public UUID SessionId { get { return m_sessionId; } }
        public Vector3 StartPos
        {
            get { return m_startpos; }
            set { m_startpos = value; }
        }
        public UUID AgentId { get { return m_agentId; } }
        public UUID ActiveGroupId { get { return m_activeGroupID; } }
        public string ActiveGroupName { get { return m_activeGroupName; } }
        public ulong ActiveGroupPowers { get { return m_activeGroupPowers; } }
        
        /// <summary>
        /// First name of the agent/avatar represented by the client
        /// </summary>
        public string FirstName { get { return m_firstName; } }

        /// <summary>
        /// Last name of the agent/avatar represented by the client
        /// </summary>
        public string LastName { get { return m_lastName; } }

        /// <summary>
        /// Full name of the client (first name and last name)
        /// </summary>
        public string Name { get { return FirstName + " " + LastName; } }

        public uint CircuitCode { get { return m_circuitCode; } }
        public int NextAnimationSequenceNumber { get { return m_animationSequenceNumber; } }
        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; }
        }
        public bool IsLoggingOut
        {
            get { return m_IsLoggingOut; }
            set { m_IsLoggingOut = value; }
        }

        public bool DisableFacelights
        {
            get { return m_disableFacelights; }
            set { m_disableFacelights = value; }
        }

        public bool SendLogoutPacketWhenClosing { set { m_SendLogoutPacketWhenClosing = value; } }

        #endregion Properties

        /// <summary>
        /// Constructor
        /// </summary>
        public LLClientView(EndPoint remoteEP, Scene scene, LLUDPServer udpServer, LLUDPClient udpClient, AgentCircuitData sessionInfo,
            UUID agentId, UUID sessionId, uint circuitCode)
        {
            RegisterInterface<IClientIM>(this);
            RegisterInterface<IClientChat>(this);
            RegisterInterface<IClientIPEndpoint>(this);

            InitDefaultAnimations();

            m_scene = scene;

            m_entityUpdates = new PriorityQueue<EntityUpdate, double>(m_scene.Entities.Count > 1000 ? m_scene.Entities.Count : 1000);
            m_killRecord = new HashSet<uint>();
//            m_attachmentsSent = new HashSet<uint>();

            m_assetService = m_scene.RequestModuleInterface<IAssetService>();
            m_GroupsModule = scene.RequestModuleInterface<IGroupsModule>();
            m_imageManager = new LLImageManager(this, m_assetService, Scene.RequestModuleInterface<IJ2KDecoder>());
            ISimulationBase simulationBase = m_scene.RequestModuleInterface<ISimulationBase>();
            if (simulationBase != null)
                m_channelVersion = Util.StringToBytes256(simulationBase.Version);
            m_agentId = agentId;
            m_sessionId = sessionId;
            m_secureSessionId = sessionInfo.SecureSessionID;
            m_circuitCode = circuitCode;
            m_userEndPoint = remoteEP;
            UserAccount account = m_scene.UserAccountService.GetUserAccount(m_scene.RegionInfo.ScopeID, m_agentId);
            if (account != null)
            {
                m_firstName = account.FirstName;
                m_lastName = account.LastName;
            }
            m_startpos = sessionInfo.startpos;

            m_udpServer = udpServer;
            m_udpClient = udpClient;
            m_udpClient.OnQueueEmpty += HandleQueueEmpty;
            m_udpClient.OnPacketStats += PopulateStats;

            m_propertiesPacketTimer = new Timer(10);
            m_propertiesPacketTimer.Elapsed += ProcessObjectPropertiesPacket;

            m_prioritizer = new Prioritizer(m_scene);

            RegisterLocalPacketHandlers();
        }

        public void Reset()
        {
            //Reset the killObjectUpdate packet stats
            m_killRecord = new HashSet<uint>();
        }

        public void SetDebugPacketLevel(int newDebug)
        {
            m_debugPacketLevel = newDebug;
        }

        #region Client Methods

        /// <summary>
        /// Shut down the client view
        /// </summary>
        public void Close()
        {
            //m_log.DebugFormat(
            //    "[CLIENT]: Close has been called for {0} attached to scene {1}",
            //    Name, m_scene.RegionInfo.RegionName);

            // Send the STOP packet
            //DisableSimulatorPacket disable = (DisableSimulatorPacket)PacketPool.Instance.GetPacket(PacketType.DisableSimulator);
            //OutPacket(disable, ThrottleOutPacketType.Unknown);

            IsActive = false;

            // Shutdown the image manager
            if (m_imageManager != null)
                m_imageManager.Close();

            // Fire the callback for this connection closing
            if (OnConnectionClosed != null)
                OnConnectionClosed(this);

            // Flush all of the packets out of the UDP server for this client
            if (m_udpServer != null)
                m_udpServer.Flush(m_udpClient);

            // Disable UDP handling for this client
            m_udpClient.Shutdown();
            
            //m_log.InfoFormat("[CLIENTVIEW] Memory pre  GC {0}", System.GC.GetTotalMemory(false));
            //GC.Collect();
            //m_log.InfoFormat("[CLIENTVIEW] Memory post GC {0}", System.GC.GetTotalMemory(true));
        }

        public void Kick(string message)
        {
            if (!ChildAgentStatus())
            {
                KickUserPacket kupack = (KickUserPacket)PacketPool.Instance.GetPacket(PacketType.KickUser);
                kupack.UserInfo.AgentID = AgentId;
                kupack.UserInfo.SessionID = SessionId;
                kupack.TargetBlock.TargetIP = 0;
                kupack.TargetBlock.TargetPort = 0;
                kupack.UserInfo.Reason = Util.StringToBytes256(message);
                OutPacket(kupack, ThrottleOutPacketType.Unknown);
                // You must sleep here or users get no message!
                Thread.Sleep(500);
            }
        }

        public void Stop()
        {

        }

        #endregion Client Methods

        #region Packet Handling

        public void PopulateStats(int inPackets, int outPackets, int unAckedBytes)
        {
            NetworkStats handlerNetworkStatsUpdate = OnNetworkStatsUpdate;
            if (handlerNetworkStatsUpdate != null)
            {
                handlerNetworkStatsUpdate(inPackets, outPackets, unAckedBytes);
            }
        }

        public static bool AddPacketHandler(PacketType packetType, PacketMethod handler)
        {
            bool result = false;
            lock (PacketHandlers)
            {
                if (!PacketHandlers.ContainsKey(packetType))
                {
                    PacketHandlers.Add(packetType, handler);
                    result = true;
                }
            }
            return result;
        }

        public bool AddLocalPacketHandler(PacketType packetType, PacketMethod handler)
        {
            return AddLocalPacketHandler(packetType, handler, true);
        }

        public bool AddLocalPacketHandler(PacketType packetType, PacketMethod handler, bool async)
        {
            bool result = false;
            lock (m_packetHandlers)
            {
                if (!m_packetHandlers.ContainsKey(packetType))
                {
                    m_packetHandlers.Add(packetType, new PacketProcessor() { method = handler, Async = async });
                    result = true;
                }
            }
            return result;
        }

        public bool AddGenericPacketHandler(string MethodName, GenericMessage handler)
        {
            MethodName = MethodName.ToLower().Trim();

            bool result = false;
            lock (m_genericPacketHandlers)
            {
                if (!m_genericPacketHandlers.ContainsKey(MethodName))
                {
                    m_genericPacketHandlers.Add(MethodName, handler);
                    result = true;
                }
            }
            return result;
        }

        public bool RemoveGenericPacketHandler(string MethodName)
        {
            MethodName = MethodName.ToLower().Trim();

            bool result = false;
            lock (m_genericPacketHandlers)
            {
                if (m_genericPacketHandlers.ContainsKey(MethodName))
                {
                    m_genericPacketHandlers.Remove(MethodName);
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Try to process a packet using registered packet handlers
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>True if a handler was found which successfully processed the packet.</returns>
        protected virtual bool ProcessPacketMethod(Packet packet)
        {
            bool result = false;
            PacketProcessor pprocessor;
            if (m_packetHandlers.TryGetValue(packet.Type, out pprocessor))
            {
                //there is a local handler for this packet type
                if (pprocessor.Async)
                {
                    object obj = new AsyncPacketProcess(this, pprocessor.method, packet);
                    Util.FireAndForget(ProcessSpecificPacketAsync, obj);
                    result = true;
                }
                else
                {
                    result = pprocessor.method(this, packet);
                }
            }
            else
            {
                //there is not a local handler so see if there is a Global handler
                PacketMethod method = null;
                bool found;
                lock (PacketHandlers)
                {
                    found = PacketHandlers.TryGetValue(packet.Type, out method);
                }
                if (found)
                {
                    result = method(this, packet);
                }
            }
            return result;
        }

        public void ProcessSpecificPacketAsync(object state)
        {
            AsyncPacketProcess packetObject = (AsyncPacketProcess)state;

            try
            {
                packetObject.result = packetObject.Method(packetObject.ClientView, packetObject.Pack);
            }
            catch (Exception e)
            {
                // Make sure that we see any exception caused by the asynchronous operation.
                m_log.ErrorFormat(
                    "[LLCLIENTVIEW]: Caught exception while processing {0} for {1}, {2} {3}",
                    packetObject.Pack, Name, e.Message, e.StackTrace);
            }
        }

        #endregion Packet Handling

        public virtual void Start()
        {
            m_scene.AddNewClient(this);
        }

        public void ActivateGesture(UUID assetId, UUID gestureId)
        {
        }

        public void DeactivateGesture(UUID assetId, UUID gestureId)
        {
        }

        // Sound
        public void SoundTrigger(UUID soundId, UUID owerid, UUID Objectid, UUID ParentId, float Gain, Vector3 Position, UInt64 Handle)
        {
        }

        #region Scene/Avatar to Client

        public void SendRegionHandshake(RegionInfo regionInfo, RegionHandshakeArgs args)
        {
            RegionHandshakePacket handshake = (RegionHandshakePacket)PacketPool.Instance.GetPacket(PacketType.RegionHandshake);
            handshake.RegionInfo = new RegionHandshakePacket.RegionInfoBlock();
            handshake.RegionInfo.BillableFactor = args.billableFactor;
            handshake.RegionInfo.IsEstateManager = args.isEstateManager;
            handshake.RegionInfo.TerrainHeightRange00 = args.terrainHeightRange0;
            handshake.RegionInfo.TerrainHeightRange01 = args.terrainHeightRange1;
            handshake.RegionInfo.TerrainHeightRange10 = args.terrainHeightRange2;
            handshake.RegionInfo.TerrainHeightRange11 = args.terrainHeightRange3;
            handshake.RegionInfo.TerrainStartHeight00 = args.terrainStartHeight0;
            handshake.RegionInfo.TerrainStartHeight01 = args.terrainStartHeight1;
            handshake.RegionInfo.TerrainStartHeight10 = args.terrainStartHeight2;
            handshake.RegionInfo.TerrainStartHeight11 = args.terrainStartHeight3;
            handshake.RegionInfo.SimAccess = args.simAccess;
            handshake.RegionInfo.WaterHeight = args.waterHeight;

            handshake.RegionInfo.RegionFlags = args.regionFlags;
            handshake.RegionInfo.SimName = Util.StringToBytes256(args.regionName);
            handshake.RegionInfo.SimOwner = args.SimOwner;
            handshake.RegionInfo.TerrainBase0 = args.terrainBase0;
            handshake.RegionInfo.TerrainBase1 = args.terrainBase1;
            handshake.RegionInfo.TerrainBase2 = args.terrainBase2;
            handshake.RegionInfo.TerrainBase3 = args.terrainBase3;
            handshake.RegionInfo.TerrainDetail0 = args.terrainDetail0;
            handshake.RegionInfo.TerrainDetail1 = args.terrainDetail1;
            handshake.RegionInfo.TerrainDetail2 = args.terrainDetail2;
            handshake.RegionInfo.TerrainDetail3 = args.terrainDetail3;
            handshake.RegionInfo.CacheID = UUID.Random(); //I guess this is for the client to remember an old setting?
            handshake.RegionInfo2 = new RegionHandshakePacket.RegionInfo2Block();
            handshake.RegionInfo2.RegionID = regionInfo.RegionID;

            handshake.RegionInfo3 = new RegionHandshakePacket.RegionInfo3Block();
            handshake.RegionInfo3.CPUClassID = 9;
            handshake.RegionInfo3.CPURatio = 1;

            handshake.RegionInfo3.ColoName = Utils.EmptyBytes;
            handshake.RegionInfo3.ProductName = Util.StringToBytes256(regionInfo.RegionType);
            handshake.RegionInfo3.ProductSKU = Utils.EmptyBytes;

            OutPacket(handshake, ThrottleOutPacketType.Unknown);
        }

        /// <summary>
        ///
        /// </summary>
        public void MoveAgentIntoRegion(RegionInfo regInfo, Vector3 pos, Vector3 look)
        {
            AgentMovementCompletePacket mov = (AgentMovementCompletePacket)PacketPool.Instance.GetPacket(PacketType.AgentMovementComplete);
            mov.SimData.ChannelVersion = m_channelVersion;
            mov.AgentData.SessionID = m_sessionId;
            mov.AgentData.AgentID = AgentId;
            mov.Data.RegionHandle = regInfo.RegionHandle;
            mov.Data.Timestamp = (uint)Util.UnixTimeSinceEpoch();

            if ((pos.X == 0) && (pos.Y == 0) && (pos.Z == 0))
            {
                mov.Data.Position = m_startpos;
            }
            else
            {
                mov.Data.Position = pos;
            }
            mov.Data.LookAt = look;

            // Hack to get this out immediately and skip the throttles
            OutPacket(mov, ThrottleOutPacketType.Unknown);
        }

        public void SendChatMessage(string message, byte type, Vector3 fromPos, string fromName,
                                    UUID fromAgentID, byte source, byte audible)
        {
            ChatFromSimulatorPacket reply = (ChatFromSimulatorPacket)PacketPool.Instance.GetPacket(PacketType.ChatFromSimulator);
            reply.ChatData.Audible = audible;
            reply.ChatData.Message = Util.StringToBytes1024(message);
            reply.ChatData.ChatType = type;
            reply.ChatData.SourceType = source;
            reply.ChatData.Position = fromPos;
            reply.ChatData.FromName = Util.StringToBytes256(fromName);
            reply.ChatData.OwnerID = fromAgentID;
            reply.ChatData.SourceID = fromAgentID;

            //Don't split me up!
            reply.HasVariableBlocks = false;
            // Hack to get this out immediately and skip throttles
            OutPacket(reply, ThrottleOutPacketType.Unknown);
        }

        public void SendTelehubInfo(Vector3 TelehubPos, Quaternion TelehubRot, List<Vector3> SpawnPoint, UUID ObjectID, string Name)
        {
            TelehubInfoPacket packet = (TelehubInfoPacket)PacketPool.Instance.GetPacket(PacketType.TelehubInfo);
            packet.SpawnPointBlock = new TelehubInfoPacket.SpawnPointBlockBlock[SpawnPoint.Count];
            int i = 0;
            foreach(Vector3 pos in SpawnPoint)
            {
                packet.SpawnPointBlock[i] = new TelehubInfoPacket.SpawnPointBlockBlock();
                packet.SpawnPointBlock[i].SpawnPointPos = pos;
                i++;
            }
            packet.TelehubBlock.ObjectID = ObjectID;
            packet.TelehubBlock.ObjectName = Utils.StringToBytes(Name);
            packet.TelehubBlock.TelehubPos = TelehubPos;
            packet.TelehubBlock.TelehubRot = TelehubRot;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        /// <summary>
        /// Send an instant message to this client
        /// </summary>
        //
        // Don't remove transaction ID! Groups and item gives need to set it!
        public void SendInstantMessage(GridInstantMessage im)
        {
            if (((Scene)(m_scene)).Permissions.CanInstantMessage(new UUID(im.fromAgentID), new UUID(im.toAgentID)))
            {
                ImprovedInstantMessagePacket msg
                    = (ImprovedInstantMessagePacket)PacketPool.Instance.GetPacket(PacketType.ImprovedInstantMessage);

                msg.AgentData.AgentID = new UUID(im.fromAgentID);
                msg.AgentData.SessionID = UUID.Zero;
                msg.MessageBlock.FromAgentName = Util.StringToBytes256(im.fromAgentName);
                msg.MessageBlock.Dialog = im.dialog;
                msg.MessageBlock.FromGroup = im.fromGroup;
                if (im.imSessionID == UUID.Zero.Guid)
                    msg.MessageBlock.ID = new UUID(im.fromAgentID) ^ new UUID(im.toAgentID);
                else
                    msg.MessageBlock.ID = new UUID(im.imSessionID);
                msg.MessageBlock.Offline = im.offline;
                msg.MessageBlock.ParentEstateID = im.ParentEstateID;
                msg.MessageBlock.Position = im.Position;
                msg.MessageBlock.RegionID = new UUID(im.RegionID);
                msg.MessageBlock.Timestamp = im.timestamp;
                msg.MessageBlock.ToAgentID = new UUID(im.toAgentID);
                msg.MessageBlock.Message = Util.StringToBytes1024(im.message);
                msg.MessageBlock.BinaryBucket = im.binaryBucket;

                OutPacket(msg, ThrottleOutPacketType.Unknown);
            }
        }

        public void SendGenericMessage(string method, List<string> message)
        {
            List<byte[]> convertedmessage = message.ConvertAll<byte[]>(delegate(string item)
            {
                return Util.StringToBytes256(item);
            });
            SendGenericMessage(method, convertedmessage);
        }

        public void SendGenericMessage(string method, List<byte[]> message)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();
            gmp.MethodData.Method = Util.StringToBytes256(method);
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[message.Count];
            int i = 0;
            foreach (byte[] val in message)
            {
                gmp.ParamList[i] = new GenericMessagePacket.ParamListBlock();
                gmp.ParamList[i++].Parameter = val;
            }

            OutPacket(gmp, ThrottleOutPacketType.Task);
        }

        public void SendGroupActiveProposals(UUID groupID, UUID transactionID, GroupActiveProposals[] Proposals)
        {
            GroupActiveProposalItemReplyPacket GAPIRP = new GroupActiveProposalItemReplyPacket();

            GAPIRP.AgentData.AgentID = AgentId;
            GAPIRP.AgentData.GroupID = groupID;
            GAPIRP.TransactionData.TransactionID = transactionID;
            GAPIRP.TransactionData.TotalNumItems = (uint)Proposals.Length;
            GAPIRP.ProposalData = new GroupActiveProposalItemReplyPacket.ProposalDataBlock[Proposals.Length];
            
            int i = 0;
            foreach (GroupActiveProposals Proposal in Proposals)
            {
                GroupActiveProposalItemReplyPacket.ProposalDataBlock ProposalData = new GroupActiveProposalItemReplyPacket.ProposalDataBlock();
                ProposalData.VoteCast = Utils.StringToBytes("false");
                ProposalData.VoteID = new UUID(Proposal.VoteID);
                ProposalData.VoteInitiator = new UUID(Proposal.VoteInitiator);
                ProposalData.Majority = (float)Convert.ToInt32(Proposal.Majority);
                ProposalData.Quorum = Convert.ToInt32(Proposal.Quorum);
                ProposalData.TerseDateID = Utils.StringToBytes(Proposal.TerseDateID);
                ProposalData.StartDateTime = Utils.StringToBytes(Proposal.StartDateTime);
                ProposalData.EndDateTime = Utils.StringToBytes(Proposal.EndDateTime);
                ProposalData.ProposalText = Utils.StringToBytes(Proposal.ProposalText);
                ProposalData.AlreadyVoted = false;
                GAPIRP.ProposalData[i] = ProposalData;
                i++;
            }
            OutPacket(GAPIRP, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendGroupVoteHistory(UUID groupID, UUID transactionID, GroupVoteHistory Vote, GroupVoteHistoryItem[] VoteItems)
        {
            GroupVoteHistoryItemReplyPacket GVHIRP = new GroupVoteHistoryItemReplyPacket();
            GVHIRP.AgentData.AgentID = AgentId;
            GVHIRP.AgentData.GroupID = groupID;
            GVHIRP.TransactionData.TransactionID = transactionID;
            GVHIRP.TransactionData.TotalNumItems = (uint)VoteItems.Length;

            GVHIRP.HistoryItemData.VoteID = new UUID(Vote.VoteID);
            GVHIRP.HistoryItemData.VoteInitiator = new UUID(Vote.VoteInitiator);
            GVHIRP.HistoryItemData.Majority = (float)Convert.ToInt32(Vote.Majority);
            GVHIRP.HistoryItemData.Quorum = Convert.ToInt32(Vote.Quorum);
            GVHIRP.HistoryItemData.TerseDateID = Utils.StringToBytes(Vote.TerseDateID);
            GVHIRP.HistoryItemData.StartDateTime = Utils.StringToBytes(Vote.StartDateTime);
            GVHIRP.HistoryItemData.EndDateTime = Utils.StringToBytes(Vote.EndDateTime);
            GVHIRP.HistoryItemData.VoteType = Utils.StringToBytes(Vote.VoteType);
            GVHIRP.HistoryItemData.VoteResult = Utils.StringToBytes(Vote.VoteResult);
            GVHIRP.HistoryItemData.ProposalText = Utils.StringToBytes(Vote.ProposalText);

            int i = 0;
            GVHIRP.VoteItem = new GroupVoteHistoryItemReplyPacket.VoteItemBlock[VoteItems.Length];
            foreach (GroupVoteHistoryItem item in VoteItems)
            {
                GroupVoteHistoryItemReplyPacket.VoteItemBlock VoteItem = new GroupVoteHistoryItemReplyPacket.VoteItemBlock();
                VoteItem.CandidateID = item.CandidateID;
                VoteItem.NumVotes = item.NumVotes;
                VoteItem.VoteCast = Utils.StringToBytes(item.VoteCast);
                GVHIRP.VoteItem[i] = VoteItem;
                i++;
            }

            OutPacket(GVHIRP, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendGroupAccountingDetails(IClientAPI sender,UUID groupID, UUID transactionID, UUID sessionID, int amt)
        {
            GroupAccountDetailsReplyPacket GADRP = new GroupAccountDetailsReplyPacket();
            GADRP.AgentData = new GroupAccountDetailsReplyPacket.AgentDataBlock();
            GADRP.AgentData.AgentID = sender.AgentId;
            GADRP.AgentData.GroupID = groupID;
            GADRP.HistoryData = new GroupAccountDetailsReplyPacket.HistoryDataBlock[1];
            GroupAccountDetailsReplyPacket.HistoryDataBlock History = new GroupAccountDetailsReplyPacket.HistoryDataBlock();
            GADRP.MoneyData = new GroupAccountDetailsReplyPacket.MoneyDataBlock();
            GADRP.MoneyData.CurrentInterval = 0;
            GADRP.MoneyData.IntervalDays = 7;
            GADRP.MoneyData.RequestID = transactionID;
            GADRP.MoneyData.StartDate = Utils.StringToBytes(DateTime.Today.ToString());
            History.Amount = amt;
            History.Description = Utils.StringToBytes("");
            GADRP.HistoryData[0] = History;
            OutPacket(GADRP, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendGroupAccountingSummary(IClientAPI sender,UUID groupID, uint moneyAmt, int totalTier, int usedTier)
        {
            GroupAccountSummaryReplyPacket GASRP =
                    (GroupAccountSummaryReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.GroupAccountSummaryReply);

            GASRP.AgentData = new GroupAccountSummaryReplyPacket.AgentDataBlock();
            GASRP.AgentData.AgentID = sender.AgentId;
            GASRP.AgentData.GroupID = groupID;
            GASRP.MoneyData = new GroupAccountSummaryReplyPacket.MoneyDataBlock();
            GASRP.MoneyData.Balance = (int)moneyAmt;
            GASRP.MoneyData.TotalCredits = totalTier;
            GASRP.MoneyData.TotalDebits = usedTier;
            GASRP.MoneyData.StartDate = new byte[1];
            GASRP.MoneyData.CurrentInterval = 1;
            GASRP.MoneyData.GroupTaxCurrent = 0;
            GASRP.MoneyData.GroupTaxEstimate = 0;
            GASRP.MoneyData.IntervalDays = 0;
            GASRP.MoneyData.LandTaxCurrent = 0;
            GASRP.MoneyData.LandTaxEstimate = 0;
            GASRP.MoneyData.LastTaxDate = new byte[1];
            GASRP.MoneyData.LightTaxCurrent = 0;
            GASRP.MoneyData.TaxDate = new byte[1];
            GASRP.MoneyData.RequestID = sender.AgentId;
            GASRP.MoneyData.ParcelDirFeeEstimate = 0;
            GASRP.MoneyData.ParcelDirFeeCurrent = 0;
            GASRP.MoneyData.ObjectTaxEstimate = 0;
            GASRP.MoneyData.NonExemptMembers = 0;
            GASRP.MoneyData.ObjectTaxCurrent = 0;
            GASRP.MoneyData.LightTaxEstimate = 0;
            OutPacket(GASRP, ThrottleOutPacketType.Asset);
        }

        public void SendGroupTransactionsSummaryDetails(IClientAPI sender,UUID groupID, UUID transactionID, UUID sessionID, int amt)
        {
            GroupAccountTransactionsReplyPacket GATRP =
                    (GroupAccountTransactionsReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.GroupAccountTransactionsReply);

            GATRP.AgentData = new GroupAccountTransactionsReplyPacket.AgentDataBlock();
            GATRP.AgentData.AgentID = sender.AgentId;
            GATRP.AgentData.GroupID = groupID;
            GATRP.MoneyData = new GroupAccountTransactionsReplyPacket.MoneyDataBlock();
            GATRP.MoneyData.CurrentInterval = 0;
            GATRP.MoneyData.IntervalDays = 7;
            GATRP.MoneyData.RequestID = transactionID;
            GATRP.MoneyData.StartDate = Utils.StringToBytes(DateTime.Today.ToString());
            GATRP.HistoryData = new GroupAccountTransactionsReplyPacket.HistoryDataBlock[1];
            GroupAccountTransactionsReplyPacket.HistoryDataBlock History = new GroupAccountTransactionsReplyPacket.HistoryDataBlock();
            History.Amount = 0;
            History.Item = Utils.StringToBytes("");
            History.Time = Utils.StringToBytes("");
            History.Type = 0;
            History.User = Utils.StringToBytes("");
            GATRP.HistoryData[0] = History;
            OutPacket(GATRP, ThrottleOutPacketType.Asset);
        }

        /// <summary>
        ///  Send the region heightmap to the client
        /// </summary>
        /// <param name="map">heightmap</param>
        public virtual void SendLayerData(float[] map)
        {
            DoSendLayerData((object)map);
            Util.FireAndForget(DoSendLayerData, map);
        }

        /// <summary>
        /// Send terrain layer information to the client.
        /// </summary>
        /// <param name="o"></param>
        private void DoSendLayerData(object o)
        {
            float[] map = (float[])o;
            map = LLHeightFieldMoronize(map);
            try
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x += 4)
                    {
                        SendLayerPacket(map, y, x);
                        //Thread.Sleep(35);
                    }
                }
            }
            catch (Exception e)
            {
                m_log.Warn("[CLIENT]: ClientView.API.cs: SendLayerData() - Failed with exception " + e);
            }
        }

        private void SendLayerTopRight(float[] map, int x1, int y1, int x2, int y2)
        {
            // Row
            for (int i = x1; i <= x2; i++)
                SendLayerData(i, y1, map);

            // Column
            for (int j = y1 + 1; j <= y2; j++)
                SendLayerData(x2, j, map);

            if (x2 - x1 > 0)
                SendLayerBottomLeft(map, x1, y1 + 1, x2 - 1, y2);
        }

        void SendLayerBottomLeft(float[] map, int x1, int y1, int x2, int y2)
        {
            // Row in reverse
            for (int i = x2; i >= x1; i--)
                SendLayerData(i, y2, map);

            // Column in reverse
            for (int j = y2 - 1; j >= y1; j--)
                SendLayerData(x1, j, map);

            if (x2 - x1 > 0)
                SendLayerTopRight(map, x1 + 1, y1, x2, y2 - 1);
        }

        /*/// <summary>
        /// Sends a set of four patches (x, x+1, ..., x+3) to the client
        /// </summary>
        /// <param name="map">heightmap</param>
        /// <param name="px">X coordinate for patches 0..12</param>
        /// <param name="py">Y coordinate for patches 0..15</param>
        public void SendLayerPacket(float[] map, int x, int y)
        {
            float[] heightmap = (map.Length == 65536) ?
                 map :
                 LLHeightFieldMoronize(map);

                try
            {
                int[] patches = new int[4];
                patches[0] = x + 0 + y * 16;
                patches[1] = x + 1 + y * 16;
                patches[2] = x + 2 + y * 16;
                patches[3] = x + 3 + y * 16;

                LayerDataPacket layerpack = TerrainCompressor.CreateLandPacket(heightmap, patches);
                layerpack.Header.Reliable = false;
                OutPacket(layerpack, ThrottleOutPacketType.Texture);
            }
            catch (Exception e)
            {
                try
                {
                    SendLayerData(x, y, heightmap);
                    SendLayerData(x + 1, y, heightmap);
                    SendLayerData(x + 2, y, heightmap);
                    SendLayerData(x + 3, y, heightmap);
                }
                catch (Exception ex)
                {
                    m_log.Error("[CLIENT]: SendLayerData() Failed with exception: " + ex.Message, ex);
                }
            }
        }*/

        /// <summary>
        /// Sends a set of four patches (x, x+1, ..., x+3) to the client
        /// </summary>
        /// <param name="map">heightmap</param>
        /// <param name="px">X coordinate for patches 0..12</param>
        /// <param name="py">Y coordinate for patches 0..15</param>
        public void SendLayerPacket(float[] map, int y, int x)
        {
            int[] patches = new int[4];
            patches[0] = x + 0 + y * 16;
            patches[1] = x + 1 + y * 16;
            patches[2] = x + 2 + y * 16;
            patches[3] = x + 3 + y * 16;

            LayerDataPacket layerpack;
            try
            {
                layerpack = TerrainCompressor.CreateLandPacket(map, patches);
                layerpack.Header.Zerocoded = true;  
                layerpack.Header.Reliable = true;

                if (layerpack.Length > 1000) // Oversize packet was created
                {
                    for (int xa = 0; xa < 4; xa++)
                    {
                        // Send oversize packet in individual patches
                        //
                        SendLayerData(x + xa, y, map);
                    }
                }
                else
                {
                    OutPacket(layerpack, ThrottleOutPacketType.Unknown);
                }
            }
            catch (OverflowException)
            {
                for (int xa = 0; xa < 4; xa++)
                {
                    // Send oversize packet in individual patches
                    //
                    SendLayerData(x + xa, y, map);
                }
            }
            catch (IndexOutOfRangeException)
            {
                for (int xa = 0; xa < 4; xa++)
                {
                    // Bad terrain, send individual chunks
                    //
                    SendLayerData(x + xa, y, map);
                }
            }
        }

        /// <summary>
        /// Sends a specified patch to a client
        /// </summary>
        /// <param name="px">Patch coordinate (x) 0..15</param>
        /// <param name="py">Patch coordinate (y) 0..15</param>
        /// <param name="map">heightmap</param>
        public void SendLayerData(int px, int py, float[] map)
        {
            try
            {
                int[] patches = new int[] { py * 16 + px };
                float[] heightmap = (map.Length == 65536) ?
                    map :
                    LLHeightFieldMoronize(map);

                LayerDataPacket layerpack = TerrainCompressor.CreateLandPacket(heightmap, patches);
                
                OutPacket(layerpack, ThrottleOutPacketType.Unknown);
            }
            catch (Exception e)
            {
                m_log.Error("[CLIENT]: SendLayerData() Failed with exception: " + e.Message, e);
            }
        }

        /// <summary>
        /// Munges heightfield into the LLUDP backed in restricted heightfield.
        /// </summary>
        /// <param name="map">float array in the base; Constants.RegionSize</param>
        /// <returns>float array in the base 256</returns>
        internal float[] LLHeightFieldMoronize(float[] map)
        {
            //if (map.Length == 65536)
                return map;
            /*else
            {
                float[] returnmap = new float[65536];

                if (map.Length < 65535)
                {
                    // rebase the vector stride to 256
                    for (int i = 0; i < Constants.RegionSize; i++)
                        Array.Copy(map, i * (int)Constants.RegionSize, returnmap, i * 256, (int)Constants.RegionSize);
                }
                else
                {
                    for (int i = 0; i < 256; i++)
                        Array.Copy(map, i * (int)Constants.RegionSize, returnmap, i * 256, 256);
                }

                //Array.Copy(map,0,returnmap,0,(map.Length < 65536)? map.Length : 65536);

                return returnmap;
            }*/

        }

        /// <summary>
        ///  Send the wind matrix to the client
        /// </summary>
        /// <param name="windSpeeds">16x16 array of wind speeds</param>
        public virtual void SendWindData(Vector2[] windSpeeds)
        {
            Util.FireAndForget(DoSendWindData, windSpeeds);
        }

        /// <summary>
        ///  Send the cloud matrix to the client
        /// </summary>
        /// <param name="windSpeeds">16x16 array of cloud densities</param>
        public virtual void SendCloudData(float[] cloudDensity)
        {
            Util.FireAndForget(DoSendCloudData, cloudDensity);
        }

        /// <summary>
        /// Send wind layer information to the client.
        /// </summary>
        /// <param name="o"></param>
        private void DoSendWindData(object o)
        {
            Vector2[] windSpeeds = (Vector2[])o;
            TerrainPatch[] patches = new TerrainPatch[2];
            patches[0] = new TerrainPatch();
            patches[0].Data = new float[16 * 16];
            patches[1] = new TerrainPatch();
            patches[1].Data = new float[16 * 16];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    patches[0].Data[y * 16 + x] = windSpeeds[y * 16 + x].X;
                    patches[1].Data[y * 16 + x] = windSpeeds[y * 16 + x].Y;
                }
            }

            LayerDataPacket layerpack = TerrainCompressor.CreateLayerDataPacket(patches, TerrainPatch.LayerType.Wind);
            layerpack.Header.Zerocoded = true;
            OutPacket(layerpack, ThrottleOutPacketType.Wind);
        }

        /// <summary>
        /// Send cloud layer information to the client.
        /// </summary>
        /// <param name="o"></param>
        private void DoSendCloudData(object o)
        {
            float[] cloudCover = (float[])o;
            TerrainPatch[] patches = new TerrainPatch[1];
            patches[0] = new TerrainPatch();
            patches[0].Data = new float[16 * 16];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    patches[0].Data[y * 16 + x] = cloudCover[y * 16 + x];
                }
            }

            LayerDataPacket layerpack = TerrainCompressor.CreateLayerDataPacket(patches, TerrainPatch.LayerType.Cloud);
            layerpack.Header.Zerocoded = true;
            OutPacket(layerpack, ThrottleOutPacketType.Cloud);
        }

        public AgentCircuitData RequestClientInfo()
        {
            AgentCircuitData agentData = new AgentCircuitData();
            agentData.AgentID = AgentId;
            agentData.SessionID = m_sessionId;
            agentData.SecureSessionID = SecureSessionId;
            agentData.circuitcode = m_circuitCode;
            agentData.child = false;

            AgentCircuitData currentAgentCircuit = this.m_udpServer.m_circuitManager.GetAgentCircuitData(CircuitCode);
            if (currentAgentCircuit != null)
            {
                agentData.ServiceURLs = currentAgentCircuit.ServiceURLs;
                agentData.IPAddress = currentAgentCircuit.IPAddress;
            }

            return agentData;
        }

        internal void SendMapBlockSplit(List<MapBlockData> mapBlocks, uint flag)
        {
            MapBlockReplyPacket mapReply = (MapBlockReplyPacket)PacketPool.Instance.GetPacket(PacketType.MapBlockReply);
            // TODO: don't create new blocks if recycling an old packet

            MapBlockData[] mapBlocks2 = mapBlocks.ToArray();

            mapReply.AgentData.AgentID = AgentId;
            mapReply.Data = new MapBlockReplyPacket.DataBlock[mapBlocks2.Length];
            mapReply.AgentData.Flags = flag;

            for (int i = 0; i < mapBlocks2.Length; i++)
            {
                mapReply.Data[i] = new MapBlockReplyPacket.DataBlock();
                mapReply.Data[i].MapImageID = mapBlocks2[i].MapImageID;
                //m_log.Warn(mapBlocks2[i].MapImageId.ToString());
                mapReply.Data[i].X = mapBlocks2[i].X;
                mapReply.Data[i].Y = mapBlocks2[i].Y;
                mapReply.Data[i].WaterHeight = mapBlocks2[i].WaterHeight;
                mapReply.Data[i].Name = Utils.StringToBytes(mapBlocks2[i].Name);
                mapReply.Data[i].RegionFlags = mapBlocks2[i].RegionFlags;
                mapReply.Data[i].Access = mapBlocks2[i].Access;
                mapReply.Data[i].Agents = mapBlocks2[i].Agents;
            }
            OutPacket(mapReply, ThrottleOutPacketType.Land);
        }

        public void SendMapBlock(List<MapBlockData> mapBlocks, uint flag)
        {

            MapBlockData[] mapBlocks2 = mapBlocks.ToArray();

            int maxsend = 10;

            //int packets = Math.Ceiling(mapBlocks2.Length / maxsend);

            List<MapBlockData> sendingBlocks = new List<MapBlockData>();

            for (int i = 0; i < mapBlocks2.Length; i++)
            {
                sendingBlocks.Add(mapBlocks2[i]);
                if (((i + 1) == mapBlocks2.Length) || (((i + 1) % maxsend) == 0))
                {
                    SendMapBlockSplit(sendingBlocks, flag);
                    sendingBlocks = new List<MapBlockData>();
                }
            }
        }

        public void SendLocalTeleport(Vector3 position, Vector3 lookAt, uint flags)
        {
            TeleportLocalPacket tpLocal = (TeleportLocalPacket)PacketPool.Instance.GetPacket(PacketType.TeleportLocal);
            tpLocal.Info.AgentID = AgentId;
            tpLocal.Info.TeleportFlags = flags;
            tpLocal.Info.LocationID = 2;
            tpLocal.Info.LookAt = lookAt;
            tpLocal.Info.Position = position;

            // Hack to get this out immediately and skip throttles
            OutPacket(tpLocal, ThrottleOutPacketType.Unknown);
        }

        public virtual void SendRegionTeleport(ulong regionHandle, byte simAccess, IPEndPoint newRegionEndPoint, uint locationID,
                                       uint flags, string capsURL)
        {
            //TeleportFinishPacket teleport = (TeleportFinishPacket)PacketPool.Instance.GetPacket(PacketType.TeleportFinish);

            TeleportFinishPacket teleport = new TeleportFinishPacket();
            teleport.Info.AgentID = AgentId;
            teleport.Info.RegionHandle = regionHandle;
            teleport.Info.SimAccess = simAccess;

            teleport.Info.SeedCapability = Util.StringToBytes256(capsURL);

            IPAddress oIP = newRegionEndPoint.Address;
            byte[] byteIP = oIP.GetAddressBytes();
            uint ip = (uint)byteIP[3] << 24;
            ip += (uint)byteIP[2] << 16;
            ip += (uint)byteIP[1] << 8;
            ip += (uint)byteIP[0];

            teleport.Info.SimIP = ip;
            teleport.Info.SimPort = (ushort)newRegionEndPoint.Port;
            teleport.Info.LocationID = 4;
            teleport.Info.TeleportFlags = 1 << 4;

            // Hack to get this out immediately and skip throttles.
            OutPacket(teleport, ThrottleOutPacketType.Unknown);
        }

        /// <summary>
        /// Inform the client that a teleport attempt has failed
        /// </summary>
        public void SendTeleportFailed(string reason)
        {
            TeleportFailedPacket tpFailed = (TeleportFailedPacket)PacketPool.Instance.GetPacket(PacketType.TeleportFailed);
            tpFailed.Info.AgentID = AgentId;
            tpFailed.Info.Reason = Util.StringToBytes256(reason);
            tpFailed.AlertInfo = new TeleportFailedPacket.AlertInfoBlock[0];

            // Hack to get this out immediately and skip throttles
            OutPacket(tpFailed, ThrottleOutPacketType.Unknown);
        }

        /// <summary>
        ///
        /// </summary>
        public void SendTeleportStart(uint flags)
        {
            TeleportStartPacket tpStart = (TeleportStartPacket)PacketPool.Instance.GetPacket(PacketType.TeleportStart);
            //TeleportStartPacket tpStart = new TeleportStartPacket();
            tpStart.Info.TeleportFlags = flags; //16; // Teleport via location

            // Hack to get this out immediately and skip throttles
            OutPacket(tpStart, ThrottleOutPacketType.Unknown);
        }

        public void SendTeleportProgress(uint flags, string message)
        {
            TeleportProgressPacket tpProgress = (TeleportProgressPacket)PacketPool.Instance.GetPacket(PacketType.TeleportProgress);
            tpProgress.AgentData.AgentID = this.AgentId;
            tpProgress.Info.TeleportFlags = flags;
            tpProgress.Info.Message = Util.StringToBytes256(message);

            // Hack to get this out immediately and skip throttles
            OutPacket(tpProgress, ThrottleOutPacketType.Unknown);
        }

        public void SendMoneyBalance(UUID transaction, bool success, byte[] description, int balance)
        {
            MoneyBalanceReplyPacket money = (MoneyBalanceReplyPacket)PacketPool.Instance.GetPacket(PacketType.MoneyBalanceReply);
            money.MoneyData.AgentID = AgentId;
            money.MoneyData.TransactionID = transaction;
            money.MoneyData.TransactionSuccess = success;
            money.MoneyData.Description = description;
            money.MoneyData.MoneyBalance = balance;
            OutPacket(money, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendPayPrice(UUID objectID, int[] payPrice)
        {
            if (payPrice[0] == 0 &&
                payPrice[1] == 0 &&
                payPrice[2] == 0 &&
                payPrice[3] == 0 &&
                payPrice[4] == 0)
                return;

            PayPriceReplyPacket payPriceReply = (PayPriceReplyPacket)PacketPool.Instance.GetPacket(PacketType.PayPriceReply);
            payPriceReply.ObjectData.ObjectID = objectID;
            payPriceReply.ObjectData.DefaultPayPrice = payPrice[0];

            payPriceReply.ButtonData = new PayPriceReplyPacket.ButtonDataBlock[4];
            payPriceReply.ButtonData[0] = new PayPriceReplyPacket.ButtonDataBlock();
            payPriceReply.ButtonData[0].PayButton = payPrice[1];
            payPriceReply.ButtonData[1] = new PayPriceReplyPacket.ButtonDataBlock();
            payPriceReply.ButtonData[1].PayButton = payPrice[2];
            payPriceReply.ButtonData[2] = new PayPriceReplyPacket.ButtonDataBlock();
            payPriceReply.ButtonData[2].PayButton = payPrice[3];
            payPriceReply.ButtonData[3] = new PayPriceReplyPacket.ButtonDataBlock();
            payPriceReply.ButtonData[3].PayButton = payPrice[4];

            OutPacket(payPriceReply, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendPlacesQuery(Aurora.Framework.ExtendedLandData[] LandData, UUID queryID, UUID transactionID)
        {
            PlacesReplyPacket PlacesReply = new PlacesReplyPacket();
            PlacesReplyPacket.QueryDataBlock[] Query = new PlacesReplyPacket.QueryDataBlock[LandData.Length];
            int totalarea = 0;
            List<string> RegionTypes = new List<string>();
            for (int i = 0; i < LandData.Length; i++)
            {
                PlacesReplyPacket.QueryDataBlock QueryBlock = new PlacesReplyPacket.QueryDataBlock();
                QueryBlock.ActualArea = LandData[i].LandData.Area;
                QueryBlock.BillableArea = LandData[i].LandData.Area;
                QueryBlock.Desc = Utils.StringToBytes(LandData[i].LandData.Description);
                QueryBlock.Dwell = LandData[i].LandData.Dwell;
                QueryBlock.Flags = 0;
                QueryBlock.GlobalX = LandData[i].GlobalPosX;
                QueryBlock.GlobalY = LandData[i].GlobalPosY;
                QueryBlock.GlobalZ = 0;
                QueryBlock.Name = Utils.StringToBytes(LandData[i].LandData.Name);
                QueryBlock.OwnerID = LandData[i].LandData.OwnerID;
                QueryBlock.Price = (int)LandData[i].LandData.SalePrice;
                QueryBlock.SimName = Utils.StringToBytes(LandData[i].RegionName);
                QueryBlock.SnapshotID = LandData[i].LandData.SnapshotID;
                Query[i] = QueryBlock;
                totalarea += LandData[i].LandData.Area;
                RegionTypes.Add(LandData[i].RegionType);
            }
            PlacesReply.QueryData = Query;
            PlacesReply.AgentData = new PlacesReplyPacket.AgentDataBlock();
            PlacesReply.AgentData.AgentID = AgentId;
            PlacesReply.AgentData.QueryID = queryID;
            PlacesReply.TransactionData.TransactionID = transactionID;
            try
            {
                IEventQueueService eq = Scene.RequestModuleInterface<IEventQueueService>();
                if (eq != null)
                {
                    eq.QueryReply(PlacesReply, AgentId, RegionTypes.ToArray(), Scene.RegionInfo.RegionHandle);
                }
            }
            catch (Exception ex)
            {
                m_log.Error("Unable to send group membership data via eventqueue - exception: " + ex.ToString());
                m_log.Warn("sending places query data via UDP");
                OutPacket(PlacesReply, ThrottleOutPacketType.AvatarInfo);
            }
        }

        public void SendStartPingCheck(byte seq)
        {
            StartPingCheckPacket pc = (StartPingCheckPacket)PacketPool.Instance.GetPacket(PacketType.StartPingCheck);
            pc.Header.Reliable = false;

            pc.PingID.PingID = seq;
            // We *could* get OldestUnacked, but it would hurt performance and not provide any benefit
            pc.PingID.OldestUnacked = 0;

            OutPacket(pc, ThrottleOutPacketType.Unknown);
        }

        public void SendKillObject(ulong regionHandle, ISceneEntity[] entities)
        {
            if (entities.Length == 0)
                return; //........... why!

//            m_log.DebugFormat("[CLIENT]: Sending KillObjectPacket to {0} for {1} in {2}", Name, localID, regionHandle);

            KillObjectPacket kill = (KillObjectPacket)PacketPool.Instance.GetPacket(PacketType.KillObject);
            kill.ObjectData = new KillObjectPacket.ObjectDataBlock[entities.Length];
            int i = 0;
            bool brokenUpdate = false;
            
            foreach (ISceneEntity entity in entities)
            {
                if (entity == null)
                {
                    brokenUpdate = true;
                    continue;
                }

                if (entity is SceneObjectPart &&
                    ((SceneObjectPart)entity).IsAttachment)
                {
                    // Do nothing
                }
                else if(entity is SceneObjectPart)
                    m_killRecord.Add(entity.LocalId);
                KillObjectPacket.ObjectDataBlock block = new KillObjectPacket.ObjectDataBlock();
                block.ID = entity.LocalId;
                kill.ObjectData[i] = block;
                i++;
            }
            //If the # of entities is not correct, we have to rebuild the entire packet
            if (brokenUpdate)
            {
                int count = 0;
                foreach (KillObjectPacket.ObjectDataBlock block in kill.ObjectData)
                {
                    if(block != null)
                        count++;
                }
                i = 0;
                KillObjectPacket.ObjectDataBlock[] bk = new KillObjectPacket.ObjectDataBlock[count];
                foreach (KillObjectPacket.ObjectDataBlock block in kill.ObjectData)
                {
                    if (block != null)
                    {
                        bk[i] = block;
                        i++;
                    }
                }
                kill.ObjectData = bk;
            }

            kill.Header.Reliable = true;
            kill.Header.Zerocoded = true;

            OutPacket(kill, ThrottleOutPacketType.Task);
        }

        /// <summary>
        /// Send information about the items contained in a folder to the client.
        ///
        /// XXX This method needs some refactoring loving
        /// </summary>
        /// <param name="ownerID">The owner of the folder</param>
        /// <param name="folderID">The id of the folder</param>
        /// <param name="items">The items contained in the folder identified by folderID</param>
        /// <param name="folders"></param>
        /// <param name="fetchFolders">Do we need to send folder information?</param>
        /// <param name="fetchItems">Do we need to send item information?</param>
        public void SendInventoryFolderDetails(UUID ownerID, UUID folderID, List<InventoryItemBase> items,
                                               List<InventoryFolderBase> folders, int version,
                                               bool fetchFolders, bool fetchItems)
        {
            // An inventory descendents packet consists of a single agent section and an inventory details
            // section for each inventory item.  The size of each inventory item is approximately 550 bytes.
            // In theory, UDP has a maximum packet size of 64k, so it should be possible to send descendent
            // packets containing metadata for in excess of 100 items.  But in practice, there may be other
            // factors (e.g. firewalls) restraining the maximum UDP packet size.  See,
            //
            // http://opensimulator.org/mantis/view.php?id=226
            //
            // for one example of this kind of thing.  In fact, the Linden servers appear to only send about
            // 6 to 7 items at a time, so let's stick with 6
            int MAX_ITEMS_PER_PACKET = 5;
            int MAX_FOLDERS_PER_PACKET = 6;

            if (items == null || folders == null)
                return; //This DOES happen when things time out!!

            int totalItems = fetchItems ? items.Count : 0;
            int totalFolders = fetchFolders ? folders.Count : 0;
            int itemsSent = 0;
            int foldersSent = 0;
            int foldersToSend = 0;
            int itemsToSend = 0;

            InventoryDescendentsPacket currentPacket = null;

            // Handle empty folders
            //
            if (totalItems == 0 && totalFolders == 0)
                currentPacket = CreateInventoryDescendentsPacket(ownerID, folderID, version, items.Count + folders.Count, 0, 0);

            // To preserve SL compatibility, we will NOT combine folders and items in one packet
            //
            while (itemsSent < totalItems || foldersSent < totalFolders)
            {
                if (currentPacket == null) // Start a new packet
                {
                    foldersToSend = totalFolders - foldersSent;
                    if (foldersToSend > MAX_FOLDERS_PER_PACKET)
                        foldersToSend = MAX_FOLDERS_PER_PACKET;

                    if (foldersToSend == 0)
                    {
                        itemsToSend = totalItems - itemsSent;
                        if (itemsToSend > MAX_ITEMS_PER_PACKET)
                            itemsToSend = MAX_ITEMS_PER_PACKET;
                    }

                    currentPacket = CreateInventoryDescendentsPacket(ownerID, folderID, version, items.Count + folders.Count, foldersToSend, itemsToSend);
                }

                if (foldersToSend-- > 0)
                    currentPacket.FolderData[foldersSent % MAX_FOLDERS_PER_PACKET] = CreateFolderDataBlock(folders[foldersSent++]);
                else if (itemsToSend-- > 0)
                    currentPacket.ItemData[itemsSent % MAX_ITEMS_PER_PACKET] = CreateItemDataBlock(items[itemsSent++]);
                else
                {
                    OutPacket(currentPacket, ThrottleOutPacketType.Asset, false);
                    currentPacket = null;
                }

            }

            if (currentPacket != null)
                OutPacket(currentPacket, ThrottleOutPacketType.Asset, false);
        }

        private InventoryDescendentsPacket.FolderDataBlock CreateFolderDataBlock(InventoryFolderBase folder)
        {
            InventoryDescendentsPacket.FolderDataBlock newBlock = new InventoryDescendentsPacket.FolderDataBlock();
            newBlock.FolderID = folder.ID;
            newBlock.Name = Util.StringToBytes256(folder.Name);
            newBlock.ParentID = folder.ParentID;
            newBlock.Type = (sbyte)folder.Type;

            return newBlock;
        }

        private InventoryDescendentsPacket.ItemDataBlock CreateItemDataBlock(InventoryItemBase item)
        {
            InventoryDescendentsPacket.ItemDataBlock newBlock = new InventoryDescendentsPacket.ItemDataBlock();
            newBlock.ItemID = item.ID;
            newBlock.AssetID = item.AssetID;
            newBlock.CreatorID = item.CreatorIdAsUuid;
            newBlock.BaseMask = item.BasePermissions;
            newBlock.Description = Util.StringToBytes256(item.Description);
            newBlock.EveryoneMask = item.EveryOnePermissions;
            newBlock.OwnerMask = item.CurrentPermissions;
            newBlock.FolderID = item.Folder;
            newBlock.InvType = (sbyte)item.InvType;
            newBlock.Name = Util.StringToBytes256(item.Name);
            newBlock.NextOwnerMask = item.NextPermissions;
            newBlock.OwnerID = item.Owner;
            newBlock.Type = (sbyte)item.AssetType;

            newBlock.GroupID = item.GroupID;
            newBlock.GroupOwned = item.GroupOwned;
            newBlock.GroupMask = item.GroupPermissions;
            newBlock.CreationDate = item.CreationDate;
            newBlock.SalePrice = item.SalePrice;
            newBlock.SaleType = item.SaleType;
            newBlock.Flags = item.Flags;

            newBlock.CRC =
                Helpers.InventoryCRC(newBlock.CreationDate, newBlock.SaleType,
                                     newBlock.InvType, newBlock.Type,
                                     newBlock.AssetID, newBlock.GroupID,
                                     newBlock.SalePrice,
                                     newBlock.OwnerID, newBlock.CreatorID,
                                     newBlock.ItemID, newBlock.FolderID,
                                     newBlock.EveryoneMask,
                                     newBlock.Flags, newBlock.OwnerMask,
                                     newBlock.GroupMask, newBlock.NextOwnerMask);

            return newBlock;
        }

        private void AddNullFolderBlockToDecendentsPacket(ref InventoryDescendentsPacket packet)
        {
            packet.FolderData = new InventoryDescendentsPacket.FolderDataBlock[1];
            packet.FolderData[0] = new InventoryDescendentsPacket.FolderDataBlock();
            packet.FolderData[0].FolderID = UUID.Zero;
            packet.FolderData[0].ParentID = UUID.Zero;
            packet.FolderData[0].Type = -1;
            packet.FolderData[0].Name = new byte[0];
        }

        private void AddNullItemBlockToDescendentsPacket(ref InventoryDescendentsPacket packet)
        {
            packet.ItemData = new InventoryDescendentsPacket.ItemDataBlock[1];
            packet.ItemData[0] = new InventoryDescendentsPacket.ItemDataBlock();
            packet.ItemData[0].ItemID = UUID.Zero;
            packet.ItemData[0].AssetID = UUID.Zero;
            packet.ItemData[0].CreatorID = UUID.Zero;
            packet.ItemData[0].BaseMask = 0;
            packet.ItemData[0].Description = new byte[0];
            packet.ItemData[0].EveryoneMask = 0;
            packet.ItemData[0].OwnerMask = 0;
            packet.ItemData[0].FolderID = UUID.Zero;
            packet.ItemData[0].InvType = (sbyte)0;
            packet.ItemData[0].Name = new byte[0];
            packet.ItemData[0].NextOwnerMask = 0;
            packet.ItemData[0].OwnerID = UUID.Zero;
            packet.ItemData[0].Type = -1;

            packet.ItemData[0].GroupID = UUID.Zero;
            packet.ItemData[0].GroupOwned = false;
            packet.ItemData[0].GroupMask = 0;
            packet.ItemData[0].CreationDate = 0;
            packet.ItemData[0].SalePrice = 0;
            packet.ItemData[0].SaleType = 0;
            packet.ItemData[0].Flags = 0;

            // No need to add CRC
        }

        private InventoryDescendentsPacket CreateInventoryDescendentsPacket(UUID ownerID, UUID folderID, int version, int descendents, int folders, int items)
        {
            InventoryDescendentsPacket descend = (InventoryDescendentsPacket)PacketPool.Instance.GetPacket(PacketType.InventoryDescendents);
            descend.Header.Zerocoded = true;
            descend.AgentData.AgentID = AgentId;
            descend.AgentData.OwnerID = ownerID;
            descend.AgentData.FolderID = folderID;
            descend.AgentData.Version = version;
            descend.AgentData.Descendents = descendents;

            if (folders > 0)
                descend.FolderData = new InventoryDescendentsPacket.FolderDataBlock[folders];
            else
                AddNullFolderBlockToDecendentsPacket(ref descend);

            if (items > 0)
                descend.ItemData = new InventoryDescendentsPacket.ItemDataBlock[items];
            else
                AddNullItemBlockToDescendentsPacket(ref descend);

            return descend;
        }

        public void SendInventoryItemDetails(UUID ownerID, InventoryItemBase item)
        {
            const uint FULL_MASK_PERMISSIONS = (uint)PermissionMask.All;

            FetchInventoryReplyPacket inventoryReply = (FetchInventoryReplyPacket)PacketPool.Instance.GetPacket(PacketType.FetchInventoryReply);
            // TODO: don't create new blocks if recycling an old packet
            inventoryReply.AgentData.AgentID = AgentId;
            inventoryReply.InventoryData = new FetchInventoryReplyPacket.InventoryDataBlock[1];
            inventoryReply.InventoryData[0] = new FetchInventoryReplyPacket.InventoryDataBlock();
            inventoryReply.InventoryData[0].ItemID = item.ID;
            inventoryReply.InventoryData[0].AssetID = item.AssetID;
            inventoryReply.InventoryData[0].CreatorID = item.CreatorIdAsUuid;
            inventoryReply.InventoryData[0].BaseMask = item.BasePermissions;
            inventoryReply.InventoryData[0].CreationDate = item.CreationDate;

            inventoryReply.InventoryData[0].Description = Util.StringToBytes256(item.Description);
            inventoryReply.InventoryData[0].EveryoneMask = item.EveryOnePermissions;
            inventoryReply.InventoryData[0].FolderID = item.Folder;
            inventoryReply.InventoryData[0].InvType = (sbyte)item.InvType;
            inventoryReply.InventoryData[0].Name = Util.StringToBytes256(item.Name);
            inventoryReply.InventoryData[0].NextOwnerMask = item.NextPermissions;
            inventoryReply.InventoryData[0].OwnerID = item.Owner;
            inventoryReply.InventoryData[0].OwnerMask = item.CurrentPermissions;
            inventoryReply.InventoryData[0].Type = (sbyte)item.AssetType;

            inventoryReply.InventoryData[0].GroupID = item.GroupID;
            inventoryReply.InventoryData[0].GroupOwned = item.GroupOwned;
            inventoryReply.InventoryData[0].GroupMask = item.GroupPermissions;
            inventoryReply.InventoryData[0].Flags = item.Flags;
            inventoryReply.InventoryData[0].SalePrice = item.SalePrice;
            inventoryReply.InventoryData[0].SaleType = item.SaleType;

            inventoryReply.InventoryData[0].CRC =
                Helpers.InventoryCRC(
                    1000, 0, inventoryReply.InventoryData[0].InvType,
                    inventoryReply.InventoryData[0].Type, inventoryReply.InventoryData[0].AssetID,
                    inventoryReply.InventoryData[0].GroupID, 100,
                    inventoryReply.InventoryData[0].OwnerID, inventoryReply.InventoryData[0].CreatorID,
                    inventoryReply.InventoryData[0].ItemID, inventoryReply.InventoryData[0].FolderID,
                    FULL_MASK_PERMISSIONS, 1, FULL_MASK_PERMISSIONS, FULL_MASK_PERMISSIONS,
                    FULL_MASK_PERMISSIONS);
            inventoryReply.Header.Zerocoded = true;
            OutPacket(inventoryReply, ThrottleOutPacketType.Asset);
        }

        protected void SendBulkUpdateInventoryFolder(InventoryFolderBase folderBase)
        {
            // We will use the same transaction id for all the separate packets to be sent out in this update.
            UUID transactionId = UUID.Random();

            List<BulkUpdateInventoryPacket.FolderDataBlock> folderDataBlocks
                = new List<BulkUpdateInventoryPacket.FolderDataBlock>();

            SendBulkUpdateInventoryFolderRecursive(folderBase, ref folderDataBlocks, transactionId);

            if (folderDataBlocks.Count > 0)
            {
                // We'll end up with some unsent folder blocks if there were some empty folders at the end of the list
                // Send these now
                BulkUpdateInventoryPacket bulkUpdate
                    = (BulkUpdateInventoryPacket)PacketPool.Instance.GetPacket(PacketType.BulkUpdateInventory);
                bulkUpdate.Header.Zerocoded = true;

                bulkUpdate.AgentData.AgentID = AgentId;
                bulkUpdate.AgentData.TransactionID = transactionId;
                bulkUpdate.FolderData = folderDataBlocks.ToArray();
                List<BulkUpdateInventoryPacket.ItemDataBlock> foo = new List<BulkUpdateInventoryPacket.ItemDataBlock>();
                bulkUpdate.ItemData = foo.ToArray();

                //m_log.Debug("SendBulkUpdateInventory :" + bulkUpdate);
                OutPacket(bulkUpdate, ThrottleOutPacketType.Asset);
            }
        }

        /// <summary>
        /// Recursively construct bulk update packets to send folders and items
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="folderDataBlocks"></param>
        /// <param name="transactionId"></param>
        private void SendBulkUpdateInventoryFolderRecursive(
            InventoryFolderBase folder, ref List<BulkUpdateInventoryPacket.FolderDataBlock> folderDataBlocks,
            UUID transactionId)
        {
            folderDataBlocks.Add(GenerateBulkUpdateFolderDataBlock(folder));

            const int MAX_ITEMS_PER_PACKET = 5;

            IInventoryService invService = m_scene.RequestModuleInterface<IInventoryService>();
            // If there are any items then we have to start sending them off in this packet - the next folder will have
            // to be in its own bulk update packet.  Also, we can only fit 5 items in a packet (at least this was the limit
            // being used on the Linden grid at 20081203).
            InventoryCollection contents = invService.GetFolderContent(AgentId, folder.ID); // folder.RequestListOfItems();
            List<InventoryItemBase> items = contents.Items;
            while (items.Count > 0)
            {
                BulkUpdateInventoryPacket bulkUpdate
                    = (BulkUpdateInventoryPacket)PacketPool.Instance.GetPacket(PacketType.BulkUpdateInventory);
                bulkUpdate.Header.Zerocoded = true;

                bulkUpdate.AgentData.AgentID = AgentId;
                bulkUpdate.AgentData.TransactionID = transactionId;
                bulkUpdate.FolderData = folderDataBlocks.ToArray();

                int itemsToSend = (items.Count > MAX_ITEMS_PER_PACKET ? MAX_ITEMS_PER_PACKET : items.Count);
                bulkUpdate.ItemData = new BulkUpdateInventoryPacket.ItemDataBlock[itemsToSend];

                for (int i = 0; i < itemsToSend; i++)
                {
                    // Remove from the end of the list so that we don't incur a performance penalty
                    bulkUpdate.ItemData[i] = GenerateBulkUpdateItemDataBlock(items[items.Count - 1]);
                    items.RemoveAt(items.Count - 1);
                }

                //m_log.Debug("SendBulkUpdateInventoryRecursive :" + bulkUpdate);
                OutPacket(bulkUpdate, ThrottleOutPacketType.Asset);

                folderDataBlocks = new List<BulkUpdateInventoryPacket.FolderDataBlock>();

                // If we're going to be sending another items packet then it needs to contain just the folder to which those
                // items belong.
                if (items.Count > 0)
                    folderDataBlocks.Add(GenerateBulkUpdateFolderDataBlock(folder));
            }

            List<InventoryFolderBase> subFolders = contents.Folders;
            foreach (InventoryFolderBase subFolder in subFolders)
            {
                SendBulkUpdateInventoryFolderRecursive(subFolder, ref folderDataBlocks, transactionId);
            }
        }

        /// <summary>
        /// Generate a bulk update inventory data block for the given folder
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private BulkUpdateInventoryPacket.FolderDataBlock GenerateBulkUpdateFolderDataBlock(InventoryFolderBase folder)
        {
            BulkUpdateInventoryPacket.FolderDataBlock folderBlock = new BulkUpdateInventoryPacket.FolderDataBlock();

            folderBlock.FolderID = folder.ID;
            folderBlock.ParentID = folder.ParentID;
            folderBlock.Type = -1;
            folderBlock.Name = Util.StringToBytes256(folder.Name);

            return folderBlock;
        }

        /// <summary>
        /// Generate a bulk update inventory data block for the given item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private BulkUpdateInventoryPacket.ItemDataBlock GenerateBulkUpdateItemDataBlock(InventoryItemBase item)
        {
            BulkUpdateInventoryPacket.ItemDataBlock itemBlock = new BulkUpdateInventoryPacket.ItemDataBlock();

            itemBlock.ItemID = item.ID;
            itemBlock.AssetID = item.AssetID;
            itemBlock.CreatorID = item.CreatorIdAsUuid;
            itemBlock.BaseMask = item.BasePermissions;
            itemBlock.Description = Util.StringToBytes256(item.Description);
            itemBlock.EveryoneMask = item.EveryOnePermissions;
            itemBlock.FolderID = item.Folder;
            itemBlock.InvType = (sbyte)item.InvType;
            itemBlock.Name = Util.StringToBytes256(item.Name);
            itemBlock.NextOwnerMask = item.NextPermissions;
            itemBlock.OwnerID = item.Owner;
            itemBlock.OwnerMask = item.CurrentPermissions;
            itemBlock.Type = (sbyte)item.AssetType;
            itemBlock.GroupID = item.GroupID;
            itemBlock.GroupOwned = item.GroupOwned;
            itemBlock.GroupMask = item.GroupPermissions;
            itemBlock.Flags = item.Flags;
            itemBlock.SalePrice = item.SalePrice;
            itemBlock.SaleType = item.SaleType;
            itemBlock.CreationDate = item.CreationDate;

            itemBlock.CRC =
                Helpers.InventoryCRC(
                    1000, 0, itemBlock.InvType,
                    itemBlock.Type, itemBlock.AssetID,
                    itemBlock.GroupID, 100,
                    itemBlock.OwnerID, itemBlock.CreatorID,
                    itemBlock.ItemID, itemBlock.FolderID,
                    (uint)PermissionMask.All, 1, (uint)PermissionMask.All, (uint)PermissionMask.All,
                    (uint)PermissionMask.All);

            return itemBlock;
        }

        public void SendBulkUpdateInventory(InventoryNodeBase node)
        {
            if (node is InventoryItemBase)
                SendBulkUpdateInventoryItem((InventoryItemBase)node);
            else if (node is InventoryFolderBase)
                SendBulkUpdateInventoryFolder((InventoryFolderBase)node);
            else
                m_log.ErrorFormat("[CLIENT]: Client for {0} sent unknown inventory node named {1}", Name, node.Name);
        }

        protected void SendBulkUpdateInventoryItem(InventoryItemBase item)
        {
            const uint FULL_MASK_PERMISSIONS = (uint)PermissionMask.All;

            BulkUpdateInventoryPacket bulkUpdate
                = (BulkUpdateInventoryPacket)PacketPool.Instance.GetPacket(PacketType.BulkUpdateInventory);

            bulkUpdate.AgentData.AgentID = AgentId;
            bulkUpdate.AgentData.TransactionID = UUID.Random();

            bulkUpdate.FolderData = new BulkUpdateInventoryPacket.FolderDataBlock[1];
            bulkUpdate.FolderData[0] = new BulkUpdateInventoryPacket.FolderDataBlock();
            bulkUpdate.FolderData[0].FolderID = UUID.Zero;
            bulkUpdate.FolderData[0].ParentID = UUID.Zero;
            bulkUpdate.FolderData[0].Type = -1;
            bulkUpdate.FolderData[0].Name = new byte[0];

            bulkUpdate.ItemData = new BulkUpdateInventoryPacket.ItemDataBlock[1];
            bulkUpdate.ItemData[0] = new BulkUpdateInventoryPacket.ItemDataBlock();
            bulkUpdate.ItemData[0].ItemID = item.ID;
            bulkUpdate.ItemData[0].AssetID = item.AssetID;
            bulkUpdate.ItemData[0].CreatorID = item.CreatorIdAsUuid;
            bulkUpdate.ItemData[0].BaseMask = item.BasePermissions;
            bulkUpdate.ItemData[0].CreationDate = item.CreationDate;
            bulkUpdate.ItemData[0].Description = Util.StringToBytes256(item.Description);
            bulkUpdate.ItemData[0].EveryoneMask = item.EveryOnePermissions;
            bulkUpdate.ItemData[0].FolderID = item.Folder;
            bulkUpdate.ItemData[0].InvType = (sbyte)item.InvType;
            bulkUpdate.ItemData[0].Name = Util.StringToBytes256(item.Name);
            bulkUpdate.ItemData[0].NextOwnerMask = item.NextPermissions;
            bulkUpdate.ItemData[0].OwnerID = item.Owner;
            bulkUpdate.ItemData[0].OwnerMask = item.CurrentPermissions;
            bulkUpdate.ItemData[0].Type = (sbyte)item.AssetType;

            bulkUpdate.ItemData[0].GroupID = item.GroupID;
            bulkUpdate.ItemData[0].GroupOwned = item.GroupOwned;
            bulkUpdate.ItemData[0].GroupMask = item.GroupPermissions;
            bulkUpdate.ItemData[0].Flags = item.Flags;
            bulkUpdate.ItemData[0].SalePrice = item.SalePrice;
            bulkUpdate.ItemData[0].SaleType = item.SaleType;

            bulkUpdate.ItemData[0].CRC =
                Helpers.InventoryCRC(1000, 0, bulkUpdate.ItemData[0].InvType,
                                     bulkUpdate.ItemData[0].Type, bulkUpdate.ItemData[0].AssetID,
                                     bulkUpdate.ItemData[0].GroupID, 100,
                                     bulkUpdate.ItemData[0].OwnerID, bulkUpdate.ItemData[0].CreatorID,
                                     bulkUpdate.ItemData[0].ItemID, bulkUpdate.ItemData[0].FolderID,
                                     FULL_MASK_PERMISSIONS, 1, FULL_MASK_PERMISSIONS, FULL_MASK_PERMISSIONS,
                                     FULL_MASK_PERMISSIONS);
            bulkUpdate.Header.Zerocoded = true;
            OutPacket(bulkUpdate, ThrottleOutPacketType.Asset);
        }

        /// <see>IClientAPI.SendInventoryItemCreateUpdate(InventoryItemBase)</see>
        public void SendInventoryItemCreateUpdate(InventoryItemBase Item, uint callbackId)
        {
            const uint FULL_MASK_PERMISSIONS = (uint)PermissionMask.All;

            UpdateCreateInventoryItemPacket InventoryReply
                = (UpdateCreateInventoryItemPacket)PacketPool.Instance.GetPacket(
                                                       PacketType.UpdateCreateInventoryItem);

            // TODO: don't create new blocks if recycling an old packet
            InventoryReply.AgentData.AgentID = AgentId;
            InventoryReply.AgentData.SimApproved = true;
            InventoryReply.InventoryData = new UpdateCreateInventoryItemPacket.InventoryDataBlock[1];
            InventoryReply.InventoryData[0] = new UpdateCreateInventoryItemPacket.InventoryDataBlock();
            InventoryReply.InventoryData[0].ItemID = Item.ID;
            InventoryReply.InventoryData[0].AssetID = Item.AssetID;
            InventoryReply.InventoryData[0].CreatorID = Item.CreatorIdAsUuid;
            InventoryReply.InventoryData[0].BaseMask = Item.BasePermissions;
            InventoryReply.InventoryData[0].Description = Util.StringToBytes256(Item.Description);
            InventoryReply.InventoryData[0].EveryoneMask = Item.EveryOnePermissions;
            InventoryReply.InventoryData[0].FolderID = Item.Folder;
            InventoryReply.InventoryData[0].InvType = (sbyte)Item.InvType;
            InventoryReply.InventoryData[0].Name = Util.StringToBytes256(Item.Name);
            InventoryReply.InventoryData[0].NextOwnerMask = Item.NextPermissions;
            InventoryReply.InventoryData[0].OwnerID = Item.Owner;
            InventoryReply.InventoryData[0].OwnerMask = Item.CurrentPermissions;
            InventoryReply.InventoryData[0].Type = (sbyte)Item.AssetType;
            InventoryReply.InventoryData[0].CallbackID = callbackId;

            InventoryReply.InventoryData[0].GroupID = Item.GroupID;
            InventoryReply.InventoryData[0].GroupOwned = Item.GroupOwned;
            InventoryReply.InventoryData[0].GroupMask = Item.GroupPermissions;
            InventoryReply.InventoryData[0].Flags = Item.Flags;
            InventoryReply.InventoryData[0].SalePrice = Item.SalePrice;
            InventoryReply.InventoryData[0].SaleType = Item.SaleType;
            InventoryReply.InventoryData[0].CreationDate = Item.CreationDate;

            InventoryReply.InventoryData[0].CRC =
                Helpers.InventoryCRC(1000, 0, InventoryReply.InventoryData[0].InvType,
                                     InventoryReply.InventoryData[0].Type, InventoryReply.InventoryData[0].AssetID,
                                     InventoryReply.InventoryData[0].GroupID, 100,
                                     InventoryReply.InventoryData[0].OwnerID, InventoryReply.InventoryData[0].CreatorID,
                                     InventoryReply.InventoryData[0].ItemID, InventoryReply.InventoryData[0].FolderID,
                                     FULL_MASK_PERMISSIONS, 1, FULL_MASK_PERMISSIONS, FULL_MASK_PERMISSIONS,
                                     FULL_MASK_PERMISSIONS);
            InventoryReply.Header.Zerocoded = true;
            OutPacket(InventoryReply, ThrottleOutPacketType.Asset);
        }

        public void SendRemoveInventoryItem(UUID itemID)
        {
            RemoveInventoryItemPacket remove = (RemoveInventoryItemPacket)PacketPool.Instance.GetPacket(PacketType.RemoveInventoryItem);
            // TODO: don't create new blocks if recycling an old packet
            remove.AgentData.AgentID = AgentId;
            remove.AgentData.SessionID = m_sessionId;
            remove.InventoryData = new RemoveInventoryItemPacket.InventoryDataBlock[1];
            remove.InventoryData[0] = new RemoveInventoryItemPacket.InventoryDataBlock();
            remove.InventoryData[0].ItemID = itemID;
            remove.Header.Zerocoded = true;
            OutPacket(remove, ThrottleOutPacketType.Asset);
        }

        public void SendTakeControls(int controls, bool passToAgent, bool TakeControls)
        {
            ScriptControlChangePacket scriptcontrol = (ScriptControlChangePacket)PacketPool.Instance.GetPacket(PacketType.ScriptControlChange);
            ScriptControlChangePacket.DataBlock[] data = new ScriptControlChangePacket.DataBlock[1];
            ScriptControlChangePacket.DataBlock ddata = new ScriptControlChangePacket.DataBlock();
            ddata.Controls = (uint)controls;
            ddata.PassToAgent = passToAgent;
            ddata.TakeControls = TakeControls;
            data[0] = ddata;
            scriptcontrol.Data = data;
            OutPacket(scriptcontrol, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendTaskInventory(UUID taskID, short serial, byte[] fileName)
        {
            ReplyTaskInventoryPacket replytask = (ReplyTaskInventoryPacket)PacketPool.Instance.GetPacket(PacketType.ReplyTaskInventory);
            replytask.InventoryData.TaskID = taskID;
            replytask.InventoryData.Serial = serial;
            replytask.InventoryData.Filename = fileName;
            OutPacket(replytask, ThrottleOutPacketType.Asset);
        }

        public void SendXferPacket(ulong xferID, uint packet, byte[] data)
        {
            SendXferPacketPacket sendXfer = (SendXferPacketPacket)PacketPool.Instance.GetPacket(PacketType.SendXferPacket);
            sendXfer.XferID.ID = xferID;
            sendXfer.XferID.Packet = packet;
            sendXfer.DataPacket.Data = data;
            OutPacket(sendXfer, ThrottleOutPacketType.Asset);
        }

        public void SendEconomyData(float EnergyEfficiency, int ObjectCapacity, int ObjectCount, int PriceEnergyUnit,
                                    int PriceGroupCreate, int PriceObjectClaim, float PriceObjectRent, float PriceObjectScaleFactor,
                                    int PriceParcelClaim, float PriceParcelClaimFactor, int PriceParcelRent, int PricePublicObjectDecay,
                                    int PricePublicObjectDelete, int PriceRentLight, int PriceUpload, int TeleportMinPrice, float TeleportPriceExponent)
        {
            EconomyDataPacket economyData = (EconomyDataPacket)PacketPool.Instance.GetPacket(PacketType.EconomyData);
            economyData.Info.EnergyEfficiency = EnergyEfficiency;
            economyData.Info.ObjectCapacity = ObjectCapacity;
            economyData.Info.ObjectCount = ObjectCount;
            economyData.Info.PriceEnergyUnit = PriceEnergyUnit;
            economyData.Info.PriceGroupCreate = PriceGroupCreate;
            economyData.Info.PriceObjectClaim = PriceObjectClaim;
            economyData.Info.PriceObjectRent = PriceObjectRent;
            economyData.Info.PriceObjectScaleFactor = PriceObjectScaleFactor;
            economyData.Info.PriceParcelClaim = PriceParcelClaim;
            economyData.Info.PriceParcelClaimFactor = PriceParcelClaimFactor;
            economyData.Info.PriceParcelRent = PriceParcelRent;
            economyData.Info.PricePublicObjectDecay = PricePublicObjectDecay;
            economyData.Info.PricePublicObjectDelete = PricePublicObjectDelete;
            economyData.Info.PriceRentLight = PriceRentLight;
            economyData.Info.PriceUpload = PriceUpload;
            economyData.Info.TeleportMinPrice = TeleportMinPrice;
            economyData.Info.TeleportPriceExponent = TeleportPriceExponent;
            economyData.Header.Reliable = true;
            OutPacket(economyData, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAvatarPickerReply(AvatarPickerReplyAgentDataArgs AgentData, List<AvatarPickerReplyDataArgs> Data)
        {
            //construct the AvatarPickerReply packet.
            AvatarPickerReplyPacket replyPacket = new AvatarPickerReplyPacket();
            replyPacket.AgentData.AgentID = AgentData.AgentID;
            replyPacket.AgentData.QueryID = AgentData.QueryID;
            //int i = 0;
            List<AvatarPickerReplyPacket.DataBlock> data_block = new List<AvatarPickerReplyPacket.DataBlock>();
            foreach (AvatarPickerReplyDataArgs arg in Data)
            {
                AvatarPickerReplyPacket.DataBlock db = new AvatarPickerReplyPacket.DataBlock();
                db.AvatarID = arg.AvatarID;
                db.FirstName = arg.FirstName;
                db.LastName = arg.LastName;
                data_block.Add(db);
            }
            replyPacket.Data = data_block.ToArray();
            OutPacket(replyPacket, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAgentDataUpdate(UUID agentid, UUID activegroupid, string firstname, string lastname, ulong grouppowers, string groupname, string grouptitle)
        {
            m_activeGroupID = activegroupid;
            m_activeGroupName = groupname;
            m_activeGroupPowers = grouppowers;

            AgentDataUpdatePacket sendAgentDataUpdate = (AgentDataUpdatePacket)PacketPool.Instance.GetPacket(PacketType.AgentDataUpdate);
            sendAgentDataUpdate.AgentData.ActiveGroupID = activegroupid;
            sendAgentDataUpdate.AgentData.AgentID = agentid;
            sendAgentDataUpdate.AgentData.FirstName = Util.StringToBytes256(firstname);
            sendAgentDataUpdate.AgentData.GroupName = Util.StringToBytes256(groupname);
            sendAgentDataUpdate.AgentData.GroupPowers = grouppowers;
            sendAgentDataUpdate.AgentData.GroupTitle = Util.StringToBytes256(grouptitle);
            sendAgentDataUpdate.AgentData.LastName = Util.StringToBytes256(lastname);
            OutPacket(sendAgentDataUpdate, ThrottleOutPacketType.Task);
        }

        /// <summary>
        /// Send an alert message to the client.  On the Linden client (tested 1.19.1.4), this pops up a brief duration
        /// blue information box in the bottom right hand corner.
        /// </summary>
        /// <param name="message"></param>
        public void SendAlertMessage(string message)
        {
            AlertMessagePacket alertPack = (AlertMessagePacket)PacketPool.Instance.GetPacket(PacketType.AlertMessage);
            alertPack.AlertData = new AlertMessagePacket.AlertDataBlock();
            alertPack.AlertData.Message = Util.StringToBytes256(message);
            alertPack.AlertInfo = new AlertMessagePacket.AlertInfoBlock[0];
            OutPacket(alertPack, ThrottleOutPacketType.AvatarInfo);
        }

        /// <summary>
        /// Send an agent alert message to the client.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="modal">On the linden client, if this true then it displays a one button text box placed in the
        /// middle of the window.  If false, the message is displayed in a brief duration blue information box (as for
        /// the AlertMessage packet).</param>
        public void SendAgentAlertMessage(string message, bool modal)
        {
            AgentAlertMessagePacket alertPack = (AgentAlertMessagePacket)PacketPool.Instance.GetPacket(PacketType.AgentAlertMessage);
            alertPack.AgentData.AgentID = AgentId;
            alertPack.AlertData.Message = Util.StringToBytes256(message);
            alertPack.AlertData.Modal = modal;
            OutPacket(alertPack, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendLoadURL(string objectname, UUID objectID, UUID ownerID, bool groupOwned, string message,
                                string url)
        {
            LoadURLPacket loadURL = (LoadURLPacket)PacketPool.Instance.GetPacket(PacketType.LoadURL);
            loadURL.Data.ObjectName = Util.StringToBytes256(objectname);
            loadURL.Data.ObjectID = objectID;
            loadURL.Data.OwnerID = ownerID;
            loadURL.Data.OwnerIsGroup = groupOwned;
            loadURL.Data.Message = Util.StringToBytes256(message);
            loadURL.Data.URL = Util.StringToBytes256(url);
            OutPacket(loadURL, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDialog(string objectname, UUID objectID, string ownerFirstName, string ownerLastName, string msg, UUID textureID, int ch, string[] buttonlabels)
        {
            ScriptDialogPacket dialog = (ScriptDialogPacket)PacketPool.Instance.GetPacket(PacketType.ScriptDialog);
            dialog.Data.ObjectID = objectID;
            dialog.Data.ObjectName = Util.StringToBytes256(objectname);
            // this is the username of the *owner*
            dialog.Data.FirstName = Util.StringToBytes256(ownerFirstName);
            dialog.Data.LastName = Util.StringToBytes256(ownerLastName);
            dialog.Data.Message = Util.StringToBytes1024(msg);
            dialog.Data.ImageID = textureID;
            dialog.Data.ChatChannel = ch;
            ScriptDialogPacket.ButtonsBlock[] buttons = new ScriptDialogPacket.ButtonsBlock[buttonlabels.Length];
            for (int i = 0; i < buttonlabels.Length; i++)
            {
                buttons[i] = new ScriptDialogPacket.ButtonsBlock();
                buttons[i].ButtonLabel = Util.StringToBytes256(buttonlabels[i]);
            }
            dialog.Buttons = buttons;
            OutPacket(dialog, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendPreLoadSound(UUID objectID, UUID ownerID, UUID soundID)
        {
            PreloadSoundPacket preSound = (PreloadSoundPacket)PacketPool.Instance.GetPacket(PacketType.PreloadSound);
            // TODO: don't create new blocks if recycling an old packet
            preSound.DataBlock = new PreloadSoundPacket.DataBlockBlock[1];
            preSound.DataBlock[0] = new PreloadSoundPacket.DataBlockBlock();
            preSound.DataBlock[0].ObjectID = objectID;
            preSound.DataBlock[0].OwnerID = ownerID;
            preSound.DataBlock[0].SoundID = soundID;
            preSound.Header.Zerocoded = true;
            OutPacket(preSound, ThrottleOutPacketType.Asset);
        }

        public void SendPlayAttachedSound(UUID soundID, UUID objectID, UUID ownerID, float gain, byte flags)
        {
            AttachedSoundPacket sound = (AttachedSoundPacket)PacketPool.Instance.GetPacket(PacketType.AttachedSound);
            sound.DataBlock.SoundID = soundID;
            sound.DataBlock.ObjectID = objectID;
            sound.DataBlock.OwnerID = ownerID;
            sound.DataBlock.Gain = gain;
            sound.DataBlock.Flags = flags;

            OutPacket(sound, ThrottleOutPacketType.Asset);
        }

        public void SendTriggeredSound(UUID soundID, UUID ownerID, UUID objectID, UUID parentID, ulong handle, Vector3 position, float gain)
        {
            SoundTriggerPacket sound = (SoundTriggerPacket)PacketPool.Instance.GetPacket(PacketType.SoundTrigger);
            sound.SoundData.SoundID = soundID;
            sound.SoundData.OwnerID = ownerID;
            sound.SoundData.ObjectID = objectID;
            sound.SoundData.ParentID = parentID;
            sound.SoundData.Handle = handle;
            sound.SoundData.Position = position;
            sound.SoundData.Gain = gain;

            OutPacket(sound, ThrottleOutPacketType.Asset);
        }

        public void SendAttachedSoundGainChange(UUID objectID, float gain)
        {
            AttachedSoundGainChangePacket sound = (AttachedSoundGainChangePacket)PacketPool.Instance.GetPacket(PacketType.AttachedSoundGainChange);
            sound.DataBlock.ObjectID = objectID;
            sound.DataBlock.Gain = gain;

            OutPacket(sound, ThrottleOutPacketType.Asset);
        }

        public void SendSunPos(Vector3 Position, Vector3 Velocity, ulong CurrentTime, uint SecondsPerSunCycle, uint SecondsPerYear, float OrbitalPosition)
        {
            // Viewers based on the Linden viwer code, do wacky things for oribital positions from Midnight to Sunrise
            // So adjust for that
            // Contributed by: Godfrey

            if (OrbitalPosition > m_sunPainDaHalfOrbitalCutoff) // things get weird from midnight to sunrise
            {
                OrbitalPosition = (OrbitalPosition - m_sunPainDaHalfOrbitalCutoff) * 0.6666666667f + m_sunPainDaHalfOrbitalCutoff;
            }

            SimulatorViewerTimeMessagePacket viewertime = (SimulatorViewerTimeMessagePacket)PacketPool.Instance.GetPacket(PacketType.SimulatorViewerTimeMessage);
            viewertime.TimeInfo.SunDirection = Position;
            viewertime.TimeInfo.SunAngVelocity = Velocity;

            // Sun module used to add 6 hours to adjust for linden sun hour, adding here
            // to prevent existing code from breaking if it assumed that 6 hours were included.
            // 21600 == 6 hours * 60 minutes * 60 Seconds
            viewertime.TimeInfo.UsecSinceStart = CurrentTime + 21600;

            viewertime.TimeInfo.SecPerDay = SecondsPerSunCycle;
            viewertime.TimeInfo.SecPerYear = SecondsPerYear;
            viewertime.TimeInfo.SunPhase = OrbitalPosition;
            viewertime.Header.Reliable = false;
            viewertime.Header.Zerocoded = true;
            OutPacket(viewertime, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendViewerEffect(ViewerEffectPacket.EffectBlock[] effectBlocks)
        {
            ViewerEffectPacket packet = (ViewerEffectPacket)PacketPool.Instance.GetPacket(PacketType.ViewerEffect);
            packet.Header.Reliable = false;
            packet.Header.Zerocoded = true;

            packet.AgentData.AgentID = AgentId;
            packet.AgentData.SessionID = SessionId;

            packet.Effect = effectBlocks;

            OutPacket(packet, ThrottleOutPacketType.State);
        }

        public void SendAvatarProperties(UUID avatarID, string aboutText, string bornOn, Byte[] charterMember,
                                         string flAbout, uint flags, UUID flImageID, UUID imageID, string profileURL,
                                         UUID partnerID)
        {
            AvatarPropertiesReplyPacket avatarReply = (AvatarPropertiesReplyPacket)PacketPool.Instance.GetPacket(PacketType.AvatarPropertiesReply);
            avatarReply.AgentData.AgentID = AgentId;
            avatarReply.AgentData.AvatarID = avatarID;
            if (aboutText != null)
                avatarReply.PropertiesData.AboutText = Util.StringToBytes1024(aboutText);
            else
                avatarReply.PropertiesData.AboutText = Utils.EmptyBytes;
            avatarReply.PropertiesData.BornOn = Util.StringToBytes256(bornOn);
            avatarReply.PropertiesData.CharterMember = charterMember;
            if (flAbout != null)
                avatarReply.PropertiesData.FLAboutText = Util.StringToBytes256(flAbout);
            else
                avatarReply.PropertiesData.FLAboutText = Utils.EmptyBytes;
            avatarReply.PropertiesData.Flags = flags;
            avatarReply.PropertiesData.FLImageID = flImageID;
            avatarReply.PropertiesData.ImageID = imageID;
            avatarReply.PropertiesData.ProfileURL = Util.StringToBytes256(profileURL);
            avatarReply.PropertiesData.PartnerID = partnerID;
            OutPacket(avatarReply, ThrottleOutPacketType.AvatarInfo);
        }

        /// <summary>
        /// Send the client an Estate message blue box pop-down with a single OK button
        /// </summary>
        /// <param name="FromAvatarID"></param>
        /// <param name="fromSessionID"></param>
        /// <param name="FromAvatarName"></param>
        /// <param name="Message"></param>
        public void SendBlueBoxMessage(UUID FromAvatarID, String FromAvatarName, String Message)
        {
            if (!ChildAgentStatus())
                SendInstantMessage(new GridInstantMessage(null, FromAvatarID, FromAvatarName, AgentId, 1, Message, false, new Vector3()));

            //SendInstantMessage(FromAvatarID, fromSessionID, Message, AgentId, SessionId, FromAvatarName, (byte)21,(uint) Util.UnixTimeSinceEpoch());
        }

        public void SendLogoutPacket()
        {
            // I know this is a bit of a hack, however there are times when you don't
            // want to send this, but still need to do the rest of the shutdown process
            // this method gets called from the packet server..   which makes it practically
            // impossible to do any other way.

            if (m_SendLogoutPacketWhenClosing)
            {
                LogoutReplyPacket logReply = (LogoutReplyPacket)PacketPool.Instance.GetPacket(PacketType.LogoutReply);
                // TODO: don't create new blocks if recycling an old packet
                logReply.AgentData.AgentID = AgentId;
                logReply.AgentData.SessionID = SessionId;
                logReply.InventoryData = new LogoutReplyPacket.InventoryDataBlock[1];
                logReply.InventoryData[0] = new LogoutReplyPacket.InventoryDataBlock();
                logReply.InventoryData[0].ItemID = UUID.Zero;

                OutPacket(logReply, ThrottleOutPacketType.Unknown);
            }
        }

        public void SendHealth(float health)
        {
            HealthMessagePacket healthpacket = (HealthMessagePacket)PacketPool.Instance.GetPacket(PacketType.HealthMessage);
            healthpacket.HealthData.Health = health;
            OutPacket(healthpacket, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAgentOnline(UUID[] agentIDs)
        {
            OnlineNotificationPacket onp = new OnlineNotificationPacket();
            OnlineNotificationPacket.AgentBlockBlock[] onpb = new OnlineNotificationPacket.AgentBlockBlock[agentIDs.Length];
            for (int i = 0; i < agentIDs.Length; i++)
            {
                OnlineNotificationPacket.AgentBlockBlock onpbl = new OnlineNotificationPacket.AgentBlockBlock();
                onpbl.AgentID = agentIDs[i];
                onpb[i] = onpbl;
            }
            onp.AgentBlock = onpb;
            onp.Header.Reliable = true;
            OutPacket(onp, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAgentOffline(UUID[] agentIDs)
        {
            OfflineNotificationPacket offp = new OfflineNotificationPacket();
            OfflineNotificationPacket.AgentBlockBlock[] offpb = new OfflineNotificationPacket.AgentBlockBlock[agentIDs.Length];
            for (int i = 0; i < agentIDs.Length; i++)
            {
                OfflineNotificationPacket.AgentBlockBlock onpbl = new OfflineNotificationPacket.AgentBlockBlock();
                onpbl.AgentID = agentIDs[i];
                offpb[i] = onpbl;
            }
            offp.AgentBlock = offpb;
            offp.Header.Reliable = true;
            OutPacket(offp, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendSitResponse(UUID TargetID, Vector3 OffsetPos, Quaternion SitOrientation, bool autopilot,
                                        Vector3 CameraAtOffset, Vector3 CameraEyeOffset, bool ForceMouseLook)
        {
            AvatarSitResponsePacket avatarSitResponse = new AvatarSitResponsePacket();
            avatarSitResponse.SitObject.ID = TargetID;
            if (CameraAtOffset != Vector3.Zero)
            {
                avatarSitResponse.SitTransform.CameraAtOffset = CameraAtOffset;
                avatarSitResponse.SitTransform.CameraEyeOffset = CameraEyeOffset;
            }
            avatarSitResponse.SitTransform.ForceMouselook = ForceMouseLook;
            avatarSitResponse.SitTransform.AutoPilot = autopilot;
            avatarSitResponse.SitTransform.SitPosition = OffsetPos;
            avatarSitResponse.SitTransform.SitRotation = SitOrientation;

            OutPacket(avatarSitResponse, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAdminResponse(UUID Token, uint AdminLevel)
        {
            GrantGodlikePowersPacket respondPacket = new GrantGodlikePowersPacket();
            GrantGodlikePowersPacket.GrantDataBlock gdb = new GrantGodlikePowersPacket.GrantDataBlock();
            GrantGodlikePowersPacket.AgentDataBlock adb = new GrantGodlikePowersPacket.AgentDataBlock();

            adb.AgentID = AgentId;
            adb.SessionID = SessionId; // More security
            gdb.GodLevel = (byte)AdminLevel;
            gdb.Token = Token;
            //respondPacket.AgentData = (GrantGodlikePowersPacket.AgentDataBlock)ablock;
            respondPacket.GrantData = gdb;
            respondPacket.AgentData = adb;
            OutPacket(respondPacket, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendGroupMembership(GroupMembershipData[] GroupMembership)
        {
            AgentGroupDataUpdatePacket Groupupdate = new AgentGroupDataUpdatePacket();
            AgentGroupDataUpdatePacket.GroupDataBlock[] Groups = new AgentGroupDataUpdatePacket.GroupDataBlock[GroupMembership.Length];
            for (int i = 0; i < GroupMembership.Length; i++)
            {
                AgentGroupDataUpdatePacket.GroupDataBlock Group = new AgentGroupDataUpdatePacket.GroupDataBlock();
                Group.AcceptNotices = GroupMembership[i].AcceptNotices;
                Group.Contribution = GroupMembership[i].Contribution;
                Group.GroupID = GroupMembership[i].GroupID;
                Group.GroupInsigniaID = GroupMembership[i].GroupPicture;
                Group.GroupName = Util.StringToBytes256(GroupMembership[i].GroupName);
                Group.GroupPowers = GroupMembership[i].GroupPowers;
                Groups[i] = Group;


            }
            Groupupdate.GroupData = Groups;
            Groupupdate.AgentData = new AgentGroupDataUpdatePacket.AgentDataBlock();
            Groupupdate.AgentData.AgentID = AgentId;
            OutPacket(Groupupdate, ThrottleOutPacketType.AvatarInfo);

            try
            {
                IEventQueueService eq = Scene.RequestModuleInterface<IEventQueueService>();
                if (eq != null)
                {
                    eq.GroupMembership(Groupupdate, this.AgentId, this.Scene.RegionInfo.RegionHandle);
                }
            }
            catch (Exception ex)
            {
                m_log.Error("Unable to send group membership data via eventqueue - exception: " + ex.ToString());
                m_log.Warn("sending group membership data via UDP");
                OutPacket(Groupupdate, ThrottleOutPacketType.AvatarInfo);
            }
        }


        public void SendGroupNameReply(UUID groupLLUID, string GroupName)
        {
            UUIDGroupNameReplyPacket pack = new UUIDGroupNameReplyPacket();
            UUIDGroupNameReplyPacket.UUIDNameBlockBlock[] uidnameblock = new UUIDGroupNameReplyPacket.UUIDNameBlockBlock[1];
            UUIDGroupNameReplyPacket.UUIDNameBlockBlock uidnamebloc = new UUIDGroupNameReplyPacket.UUIDNameBlockBlock();
            uidnamebloc.ID = groupLLUID;
            uidnamebloc.GroupName = Util.StringToBytes256(GroupName);
            uidnameblock[0] = uidnamebloc;
            pack.UUIDNameBlock = uidnameblock;
            OutPacket(pack, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendLandStatReply(uint reportType, uint requestFlags, uint resultCount, LandStatReportItem[] lsrpia)
        {
            LandStatReplyPacket lsrp = new LandStatReplyPacket();
            // LandStatReplyPacket.RequestDataBlock lsreqdpb = new LandStatReplyPacket.RequestDataBlock();
            LandStatReplyPacket.ReportDataBlock[] lsrepdba = new LandStatReplyPacket.ReportDataBlock[lsrpia.Length];
            //LandStatReplyPacket.ReportDataBlock lsrepdb = new LandStatReplyPacket.ReportDataBlock();
            // lsrepdb.
            lsrp.RequestData.ReportType = reportType;
            lsrp.RequestData.RequestFlags = requestFlags;
            lsrp.RequestData.TotalObjectCount = resultCount;
            for (int i = 0; i < lsrpia.Length; i++)
            {
                LandStatReplyPacket.ReportDataBlock lsrepdb = new LandStatReplyPacket.ReportDataBlock();
                lsrepdb.LocationX = lsrpia[i].LocationX;
                lsrepdb.LocationY = lsrpia[i].LocationY;
                lsrepdb.LocationZ = lsrpia[i].LocationZ;
                lsrepdb.Score = lsrpia[i].Score;
                lsrepdb.TaskID = lsrpia[i].TaskID;
                lsrepdb.TaskLocalID = lsrpia[i].TaskLocalID;
                lsrepdb.TaskName = Util.StringToBytes256(lsrpia[i].TaskName);
                lsrepdb.OwnerName = Util.StringToBytes256(lsrpia[i].OwnerName);
                lsrepdba[i] = lsrepdb;
            }
            lsrp.ReportData = lsrepdba;
            OutPacket(lsrp, ThrottleOutPacketType.Land);
        }

        public void SendScriptRunningReply(UUID objectID, UUID itemID, bool running)
        {
            ScriptRunningReplyPacket scriptRunningReply = new ScriptRunningReplyPacket();
            scriptRunningReply.Script.ObjectID = objectID;
            scriptRunningReply.Script.ItemID = itemID;
            scriptRunningReply.Script.Running = running;

            OutPacket(scriptRunningReply, ThrottleOutPacketType.AvatarInfo);
        }

        private void SendFailedAsset(AssetRequestToClient req, TransferPacketStatus assetErrors)
        {
            TransferInfoPacket Transfer = new TransferInfoPacket();
            Transfer.TransferInfo.ChannelType = (int)ChannelType.Asset;
            Transfer.TransferInfo.Status = (int)assetErrors;
            Transfer.TransferInfo.TargetType = 0;
            Transfer.TransferInfo.Params = req.Params;
            Transfer.TransferInfo.Size = 0;
            Transfer.TransferInfo.TransferID = req.TransferRequestID;
            Transfer.Header.Zerocoded = true;
            OutPacket(Transfer, ThrottleOutPacketType.Asset);
        }

        public void SendAsset(AssetRequestToClient req)
        {
            if (req.AssetInf.Data == null)
            {
                m_log.ErrorFormat("[LLClientView]: Cannot send asset {0} ({1}), asset data is null",
                    req.AssetInf.ID, req.AssetInf.Metadata.ContentType);
                return;
            }
            
            //m_log.Debug("sending asset " + req.RequestAssetID);
            TransferInfoPacket Transfer = new TransferInfoPacket();
            Transfer.TransferInfo.ChannelType = (int)ChannelType.Asset;
            Transfer.TransferInfo.Status = (int)TransferPacketStatus.MorePacketsToCome;
            Transfer.TransferInfo.TargetType = 0;

            if (req.AssetRequestSource == 2)
            {
                Transfer.TransferInfo.Params = new byte[20];
                Array.Copy(req.RequestAssetID.GetBytes(), 0, Transfer.TransferInfo.Params, 0, 16);
                int assType = req.AssetInf.Type;
                Array.Copy(Utils.IntToBytes(assType), 0, Transfer.TransferInfo.Params, 16, 4);
            }
            else if (req.AssetRequestSource == 3)
            {
                Transfer.TransferInfo.Params = req.Params;
                // Transfer.TransferInfo.Params = new byte[100];
                //Array.Copy(req.RequestUser.AgentId.GetBytes(), 0, Transfer.TransferInfo.Params, 0, 16);
                //Array.Copy(req.RequestUser.SessionId.GetBytes(), 0, Transfer.TransferInfo.Params, 16, 16);
            }
            Transfer.TransferInfo.Size = req.AssetInf.Data.Length;
            Transfer.TransferInfo.TransferID = req.TransferRequestID;
            Transfer.Header.Zerocoded = true;
            OutPacket(Transfer, ThrottleOutPacketType.Asset);

            if (req.NumPackets == 1)
            {
                TransferPacketPacket TransferPacket = new TransferPacketPacket();
                TransferPacket.TransferData.Packet = 0;
                TransferPacket.TransferData.ChannelType = (int)ChannelType.Asset;
                TransferPacket.TransferData.TransferID = req.TransferRequestID;
                TransferPacket.TransferData.Data = req.AssetInf.Data;
                TransferPacket.TransferData.Status = (int)TransferPacketStatus.Done;
                TransferPacket.Header.Zerocoded = true;
                OutPacket(TransferPacket, ThrottleOutPacketType.Asset);
            }
            else
            {
                int processedLength = 0;
                int maxChunkSize = 1024;
                int packetNumber = 0;
                int firstPacketSize = 600;

                while (processedLength < req.AssetInf.Data.Length)
                {
                    TransferPacketPacket TransferPacket = new TransferPacketPacket();
                    TransferPacket.TransferData.Packet = packetNumber;
                    TransferPacket.TransferData.ChannelType = (int)ChannelType.Asset;
                    TransferPacket.TransferData.TransferID = req.TransferRequestID;

                    int chunkSize = Math.Min(req.AssetInf.Data.Length - processedLength, packetNumber == 0 ? firstPacketSize : maxChunkSize);

                    byte[] chunk = new byte[chunkSize];
                    Array.Copy(req.AssetInf.Data, processedLength, chunk, 0, chunk.Length);
                    TransferPacket.TransferData.Data = chunk;

                    processedLength += chunkSize;
                    // 0 indicates more packets to come, 1 indicates last packet
                    if (req.AssetInf.Data.Length - processedLength == 0)
                    {
                        TransferPacket.TransferData.Status = (int)TransferPacketStatus.Done;
                    }
                    else
                    {
                        TransferPacket.TransferData.Status = (int)TransferPacketStatus.MorePacketsToCome;
                    }
                    TransferPacket.Header.Zerocoded = true;
                    OutPacket(TransferPacket, ThrottleOutPacketType.Asset);

                    packetNumber++;
                }
            }
        }

        public void SendRegionHandle(UUID regionID, ulong handle)
        {
            RegionIDAndHandleReplyPacket reply = (RegionIDAndHandleReplyPacket)PacketPool.Instance.GetPacket(PacketType.RegionIDAndHandleReply);
            reply.ReplyBlock.RegionID = regionID;
            reply.ReplyBlock.RegionHandle = handle;
            OutPacket(reply, ThrottleOutPacketType.Land);
        }

        public void SendParcelInfo(LandData land, UUID parcelID, uint x, uint y, string SimName)
        {
            ParcelInfoReplyPacket reply = (ParcelInfoReplyPacket)PacketPool.Instance.GetPacket(PacketType.ParcelInfoReply);
            reply.AgentData.AgentID = m_agentId;
            reply.Data.ParcelID = parcelID;
            reply.Data.OwnerID = land.OwnerID;
            reply.Data.Name = Utils.StringToBytes(land.Name);
            reply.Data.Desc = Utils.StringToBytes(land.Description);
            reply.Data.ActualArea = land.Area;
            reply.Data.BillableArea = land.Area; // TODO: what is this?

            // Bit 0: Mature, bit 7: on sale, other bits: no idea
            reply.Data.Flags = (byte)(
                (((Scene)this.Scene).RegionInfo.RegionSettings.Maturity > 0 ? (1 << 0) : 0) +
                ((land.Flags & (uint)ParcelFlags.ForSale) != 0 ? (1 << 7) : 0));

            Vector3 pos = land.UserLocation;
            if (pos.Equals(Vector3.Zero))
            {
                pos = (land.AABBMax + land.AABBMin) * 0.5f;
            }
            reply.Data.GlobalX = x;
            reply.Data.GlobalY = y;
            reply.Data.GlobalZ = pos.Z;
            reply.Data.SimName = Utils.StringToBytes(SimName);
            reply.Data.SnapshotID = land.SnapshotID;
            reply.Data.Dwell = land.Dwell;
            reply.Data.SalePrice = land.SalePrice;
            reply.Data.AuctionID = (int)land.AuctionID;

            OutPacket(reply, ThrottleOutPacketType.Land);
        }

        public void SendScriptTeleportRequest(string objName, string simName, Vector3 pos, Vector3 lookAt)
        {
            ScriptTeleportRequestPacket packet = (ScriptTeleportRequestPacket)PacketPool.Instance.GetPacket(PacketType.ScriptTeleportRequest);

            packet.Data.ObjectName = Utils.StringToBytes(objName);
            packet.Data.SimName = Utils.StringToBytes(simName);
            packet.Data.SimPosition = pos;
            packet.Data.LookAt = lookAt;

            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirPlacesReply(UUID queryID, DirPlacesReplyData[] data)
        {
            DirPlacesReplyPacket packet = (DirPlacesReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirPlacesReply);

            packet.AgentData = new DirPlacesReplyPacket.AgentDataBlock();

            packet.QueryData = new DirPlacesReplyPacket.QueryDataBlock[1];
            packet.QueryData[0] = new DirPlacesReplyPacket.QueryDataBlock();

            packet.AgentData.AgentID = AgentId;

            packet.QueryData[0].QueryID = queryID;

            DirPlacesReplyPacket.QueryRepliesBlock[] replies =
                    new DirPlacesReplyPacket.QueryRepliesBlock[0];
            DirPlacesReplyPacket.StatusDataBlock[] status =
                    new DirPlacesReplyPacket.StatusDataBlock[0];

            packet.QueryReplies = replies;
            packet.StatusData = status;

            foreach (DirPlacesReplyData d in data)
            {
                int idx = replies.Length;
                Array.Resize(ref replies, idx + 1);
                Array.Resize(ref status, idx + 1);

                replies[idx] = new DirPlacesReplyPacket.QueryRepliesBlock();
                status[idx] = new DirPlacesReplyPacket.StatusDataBlock();
                replies[idx].ParcelID = d.parcelID;
                replies[idx].Name = Utils.StringToBytes(d.name);
                replies[idx].ForSale = d.forSale;
                replies[idx].Auction = d.auction;
                replies[idx].Dwell = d.dwell;
                status[idx].Status = d.Status;

                packet.QueryReplies = replies;
                packet.StatusData = status;

                if (packet.Length >= 1000)
                {
                    OutPacket(packet, ThrottleOutPacketType.AvatarInfo);

                    packet = (DirPlacesReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirPlacesReply);

                    packet.AgentData = new DirPlacesReplyPacket.AgentDataBlock();

                    packet.QueryData = new DirPlacesReplyPacket.QueryDataBlock[1];
                    packet.QueryData[0] = new DirPlacesReplyPacket.QueryDataBlock();

                    packet.AgentData.AgentID = AgentId;

                    packet.QueryData[0].QueryID = queryID;

                    replies = new DirPlacesReplyPacket.QueryRepliesBlock[0];
                    status = new DirPlacesReplyPacket.StatusDataBlock[0];
                }
            }

            packet.HasVariableBlocks = false;
			if (replies.Length > 0 || data.Length == 0)
                OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirPeopleReply(UUID queryID, DirPeopleReplyData[] data)
        {
            DirPeopleReplyPacket packet = (DirPeopleReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirPeopleReply);

            packet.AgentData = new DirPeopleReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.QueryData = new DirPeopleReplyPacket.QueryDataBlock();
            packet.QueryData.QueryID = queryID;

            packet.QueryReplies = new DirPeopleReplyPacket.QueryRepliesBlock[
                    data.Length];

            int i = 0;
            foreach (DirPeopleReplyData d in data)
            {
                packet.QueryReplies[i] = new DirPeopleReplyPacket.QueryRepliesBlock();
                packet.QueryReplies[i].AgentID = d.agentID;
                packet.QueryReplies[i].FirstName =
                        Utils.StringToBytes(d.firstName);
                packet.QueryReplies[i].LastName =
                        Utils.StringToBytes(d.lastName);
                packet.QueryReplies[i].Group =
                        Utils.StringToBytes(d.group);
                packet.QueryReplies[i].Online = d.online;
                packet.QueryReplies[i].Reputation = d.reputation;
                i++;
            }

            packet.HasVariableBlocks = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirEventsReply(UUID queryID, DirEventsReplyData[] data)
        {
            DirEventsReplyPacket packet = (DirEventsReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirEventsReply);

            packet.AgentData = new DirEventsReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.QueryData = new DirEventsReplyPacket.QueryDataBlock();
            packet.QueryData.QueryID = queryID;

            packet.QueryReplies = new DirEventsReplyPacket.QueryRepliesBlock[
                    data.Length];

            packet.StatusData = new DirEventsReplyPacket.StatusDataBlock[
                    data.Length];

            int i = 0;
            foreach (DirEventsReplyData d in data)
            {
                packet.QueryReplies[i] = new DirEventsReplyPacket.QueryRepliesBlock();
                packet.StatusData[i] = new DirEventsReplyPacket.StatusDataBlock();
                packet.QueryReplies[i].OwnerID = d.ownerID;
                packet.QueryReplies[i].Name =
                        Utils.StringToBytes(d.name);
                packet.QueryReplies[i].EventID = d.eventID;
                packet.QueryReplies[i].Date =
                        Utils.StringToBytes(d.date);
                packet.QueryReplies[i].UnixTime = d.unixTime;
                packet.QueryReplies[i].EventFlags = d.eventFlags;
                packet.StatusData[i].Status = d.Status;
                i++;
            }

            packet.HasVariableBlocks = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirGroupsReply(UUID queryID, DirGroupsReplyData[] data)
        {
            DirGroupsReplyPacket packet = (DirGroupsReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirGroupsReply);

            packet.AgentData = new DirGroupsReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.QueryData = new DirGroupsReplyPacket.QueryDataBlock();
            packet.QueryData.QueryID = queryID;

            packet.QueryReplies = new DirGroupsReplyPacket.QueryRepliesBlock[
                    data.Length];

            int i = 0;
            foreach (DirGroupsReplyData d in data)
            {
                packet.QueryReplies[i] = new DirGroupsReplyPacket.QueryRepliesBlock();
                packet.QueryReplies[i].GroupID = d.groupID;
                packet.QueryReplies[i].GroupName =
                        Utils.StringToBytes(d.groupName);
                packet.QueryReplies[i].Members = d.members;
                packet.QueryReplies[i].SearchOrder = d.searchOrder;
                i++;
            }

            packet.HasVariableBlocks = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirClassifiedReply(UUID queryID, DirClassifiedReplyData[] data)
        {
            DirClassifiedReplyPacket packet = (DirClassifiedReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirClassifiedReply);

            packet.AgentData = new DirClassifiedReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.QueryData = new DirClassifiedReplyPacket.QueryDataBlock();
            packet.QueryData.QueryID = queryID;

            packet.QueryReplies = new DirClassifiedReplyPacket.QueryRepliesBlock[
                    data.Length];
            packet.StatusData = new DirClassifiedReplyPacket.StatusDataBlock[
                    data.Length];

            int i = 0;
            foreach (DirClassifiedReplyData d in data)
            {
                packet.QueryReplies[i] = new DirClassifiedReplyPacket.QueryRepliesBlock();
                packet.StatusData[i] = new DirClassifiedReplyPacket.StatusDataBlock();
                packet.QueryReplies[i].ClassifiedID = d.classifiedID;
                packet.QueryReplies[i].Name =
                        Utils.StringToBytes(d.name);
                packet.QueryReplies[i].ClassifiedFlags = d.classifiedFlags;
                packet.QueryReplies[i].CreationDate = d.creationDate;
                packet.QueryReplies[i].ExpirationDate = d.expirationDate;
                packet.QueryReplies[i].PriceForListing = d.price;
                packet.StatusData[i].Status = d.Status;
                i++;
            }

            packet.HasVariableBlocks = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirLandReply(UUID queryID, DirLandReplyData[] data)
        {
            DirLandReplyPacket packet = (DirLandReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirLandReply);

            packet.AgentData = new DirLandReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.QueryData = new DirLandReplyPacket.QueryDataBlock();
            packet.QueryData.QueryID = queryID;

            packet.QueryReplies = new DirLandReplyPacket.QueryRepliesBlock[
                    data.Length];

            int i = 0;
            foreach (DirLandReplyData d in data)
            {
                packet.QueryReplies[i] = new DirLandReplyPacket.QueryRepliesBlock();
                packet.QueryReplies[i].ParcelID = d.parcelID;
                packet.QueryReplies[i].Name =
                        Utils.StringToBytes(d.name);
                packet.QueryReplies[i].Auction = d.auction;
                packet.QueryReplies[i].ForSale = d.forSale;
                packet.QueryReplies[i].SalePrice = d.salePrice;
                packet.QueryReplies[i].ActualArea = d.actualArea;
                i++;
            }

            packet.HasVariableBlocks = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDirPopularReply(UUID queryID, DirPopularReplyData[] data)
        {
            DirPopularReplyPacket packet = (DirPopularReplyPacket)PacketPool.Instance.GetPacket(PacketType.DirPopularReply);

            packet.AgentData = new DirPopularReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.QueryData = new DirPopularReplyPacket.QueryDataBlock();
            packet.QueryData.QueryID = queryID;

            packet.QueryReplies = new DirPopularReplyPacket.QueryRepliesBlock[
                    data.Length];

            int i = 0;
            foreach (DirPopularReplyData d in data)
            {
                packet.QueryReplies[i] = new DirPopularReplyPacket.QueryRepliesBlock();
                packet.QueryReplies[i].ParcelID = d.parcelID;
                packet.QueryReplies[i].Name =
                        Utils.StringToBytes(d.name);
                packet.QueryReplies[i].Dwell = d.dwell;
                i++;
            }

            packet.HasVariableBlocks = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendEventInfoReply(EventData data)
        {
            EventInfoReplyPacket packet = (EventInfoReplyPacket)PacketPool.Instance.GetPacket(PacketType.EventInfoReply);

            packet.AgentData = new EventInfoReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;

            packet.EventData = new EventInfoReplyPacket.EventDataBlock();
            packet.EventData.EventID = data.eventID;
            packet.EventData.Creator = Utils.StringToBytes(data.creator);
            packet.EventData.Name = Utils.StringToBytes(data.name);
            packet.EventData.Category = Utils.StringToBytes(data.category);
            packet.EventData.Desc = Utils.StringToBytes(data.description);
            packet.EventData.Date = Utils.StringToBytes(data.date);
            packet.EventData.DateUTC = data.dateUTC;
            packet.EventData.Duration = data.duration;
            packet.EventData.Cover = data.cover;
            packet.EventData.Amount = data.amount;
            packet.EventData.SimName = Utils.StringToBytes(data.simName);
            packet.EventData.GlobalPos = new Vector3d(data.globalPos);
            packet.EventData.EventFlags = data.eventFlags;

            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendMapItemReply(mapItemReply[] replies, uint mapitemtype, uint flags)
        {
            MapItemReplyPacket mirplk = new MapItemReplyPacket();
            mirplk.AgentData.AgentID = AgentId;
            mirplk.RequestData.ItemType = mapitemtype;
            mirplk.Data = new MapItemReplyPacket.DataBlock[replies.Length];
            for (int i = 0; i < replies.Length; i++)
            {
                MapItemReplyPacket.DataBlock mrdata = new MapItemReplyPacket.DataBlock();
                mrdata.X = replies[i].x;
                mrdata.Y = replies[i].y;
                mrdata.ID = replies[i].id;
                mrdata.Extra = replies[i].Extra;
                mrdata.Extra2 = replies[i].Extra2;
                mrdata.Name = Utils.StringToBytes(replies[i].name);
                mirplk.Data[i] = mrdata;
            }
            //m_log.Debug(mirplk.ToString());
            OutPacket(mirplk, ThrottleOutPacketType.Land);

        }

        public void SendOfferCallingCard(UUID srcID, UUID transactionID)
        {
            // a bit special, as this uses AgentID to store the source instead
            // of the destination. The destination (the receiver) goes into destID
            OfferCallingCardPacket p = (OfferCallingCardPacket)PacketPool.Instance.GetPacket(PacketType.OfferCallingCard);
            p.AgentData.AgentID = srcID;
            p.AgentData.SessionID = UUID.Zero;
            p.AgentBlock.DestID = AgentId;
            p.AgentBlock.TransactionID = transactionID;
            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAcceptCallingCard(UUID transactionID)
        {
            AcceptCallingCardPacket p = (AcceptCallingCardPacket)PacketPool.Instance.GetPacket(PacketType.AcceptCallingCard);
            p.AgentData.AgentID = AgentId;
            p.AgentData.SessionID = UUID.Zero;
            p.FolderData = new AcceptCallingCardPacket.FolderDataBlock[1];
            p.FolderData[0] = new AcceptCallingCardPacket.FolderDataBlock();
            p.FolderData[0].FolderID = UUID.Zero;
            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDeclineCallingCard(UUID transactionID)
        {
            DeclineCallingCardPacket p = (DeclineCallingCardPacket)PacketPool.Instance.GetPacket(PacketType.DeclineCallingCard);
            p.AgentData.AgentID = AgentId;
            p.AgentData.SessionID = UUID.Zero;
            p.TransactionBlock.TransactionID = transactionID;
            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendTerminateFriend(UUID exFriendID)
        {
            TerminateFriendshipPacket p = (TerminateFriendshipPacket)PacketPool.Instance.GetPacket(PacketType.TerminateFriendship);
            p.AgentData.AgentID = AgentId;
            p.AgentData.SessionID = SessionId;
            p.ExBlock.OtherID = exFriendID;
            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAvatarGroupsReply(UUID avatarID, GroupMembershipData[] data)
        {
             OSDMap llsd = new OSDMap(3);
             OSDArray AgentData = new OSDArray(1);
             OSDMap AgentDataMap = new OSDMap(1);
             AgentDataMap.Add("AgentID", OSD.FromUUID(this.AgentId));
             AgentDataMap.Add("AvatarID", OSD.FromUUID(avatarID));
             AgentData.Add(AgentDataMap);
             llsd.Add("AgentData", AgentData);
             OSDArray GroupData = new OSDArray(data.Length);
             OSDArray NewGroupData = new OSDArray(data.Length);
             foreach (GroupMembershipData m in data)
             {
                 OSDMap GroupDataMap = new OSDMap(6);
                 OSDMap NewGroupDataMap = new OSDMap(1);
                 GroupDataMap.Add("GroupPowers", OSD.FromULong(m.GroupPowers));
                 GroupDataMap.Add("AcceptNotices", OSD.FromBoolean(m.AcceptNotices));
                 GroupDataMap.Add("GroupTitle", OSD.FromString(m.GroupTitle));
                 GroupDataMap.Add("GroupID", OSD.FromUUID(m.GroupID));
                 GroupDataMap.Add("GroupName", OSD.FromString(m.GroupName));
                 GroupDataMap.Add("GroupInsigniaID", OSD.FromUUID(m.GroupPicture));
                 NewGroupDataMap.Add("ListInProfile", OSD.FromBoolean(m.ListInProfile));
                 GroupData.Add(GroupDataMap);
                 NewGroupData.Add(NewGroupDataMap);
             }
             llsd.Add("GroupData", GroupData);
             llsd.Add("NewGroupData", NewGroupData);

             IEventQueueService eq = this.Scene.RequestModuleInterface<IEventQueueService>();
             if (eq != null)
             {
                 eq.Enqueue(BuildEvent("AvatarGroupsReply", llsd), this.AgentId, Scene.RegionInfo.RegionHandle);
             }
        }

        public void SendJoinGroupReply(UUID groupID, bool success)
        {
            JoinGroupReplyPacket p = (JoinGroupReplyPacket)PacketPool.Instance.GetPacket(PacketType.JoinGroupReply);

            p.AgentData = new JoinGroupReplyPacket.AgentDataBlock();
            p.AgentData.AgentID = AgentId;

            p.GroupData = new JoinGroupReplyPacket.GroupDataBlock();
            p.GroupData.GroupID = groupID;
            p.GroupData.Success = success;

            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendEjectGroupMemberReply(UUID agentID, UUID groupID, bool success)
        {
            EjectGroupMemberReplyPacket p = (EjectGroupMemberReplyPacket)PacketPool.Instance.GetPacket(PacketType.EjectGroupMemberReply);

            p.AgentData = new EjectGroupMemberReplyPacket.AgentDataBlock();
            p.AgentData.AgentID = agentID;

            p.GroupData = new EjectGroupMemberReplyPacket.GroupDataBlock();
            p.GroupData.GroupID = groupID;

            p.EjectData = new EjectGroupMemberReplyPacket.EjectDataBlock();
            p.EjectData.Success = success;

            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendLeaveGroupReply(UUID groupID, bool success)
        {
            LeaveGroupReplyPacket p = (LeaveGroupReplyPacket)PacketPool.Instance.GetPacket(PacketType.LeaveGroupReply);

            p.AgentData = new LeaveGroupReplyPacket.AgentDataBlock();
            p.AgentData.AgentID = AgentId;

            p.GroupData = new LeaveGroupReplyPacket.GroupDataBlock();
            p.GroupData.GroupID = groupID;
            p.GroupData.Success = success;

            OutPacket(p, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAvatarClassifiedReply(UUID targetID, UUID[] classifiedID, string[] name)
        {
            if (classifiedID.Length != name.Length)
                return;

            AvatarClassifiedReplyPacket ac =
                    (AvatarClassifiedReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.AvatarClassifiedReply);

            ac.AgentData = new AvatarClassifiedReplyPacket.AgentDataBlock();
            ac.AgentData.AgentID = AgentId;
            ac.AgentData.TargetID = targetID;

            ac.Data = new AvatarClassifiedReplyPacket.DataBlock[classifiedID.Length];
            int i;
            for (i = 0; i < classifiedID.Length; i++)
            {
                ac.Data[i].ClassifiedID = classifiedID[i];
                ac.Data[i].Name = Utils.StringToBytes(name[i]);
            }

            OutPacket(ac, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendClassifiedInfoReply(UUID classifiedID, UUID creatorID, uint creationDate, uint expirationDate, uint category, string name, string description, UUID parcelID, uint parentEstate, UUID snapshotID, string simName, Vector3 globalPos, string parcelName, byte classifiedFlags, int price)
        {
            ClassifiedInfoReplyPacket cr =
                    (ClassifiedInfoReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.ClassifiedInfoReply);

            cr.AgentData = new ClassifiedInfoReplyPacket.AgentDataBlock();
            cr.AgentData.AgentID = AgentId;

            cr.Data = new ClassifiedInfoReplyPacket.DataBlock();
            cr.Data.ClassifiedID = classifiedID;
            cr.Data.CreatorID = creatorID;
            cr.Data.CreationDate = creationDate;
            cr.Data.ExpirationDate = expirationDate;
            cr.Data.Category = category;
            cr.Data.Name = Utils.StringToBytes(name);
            cr.Data.Desc = Utils.StringToBytes(description);
            cr.Data.ParcelID = parcelID;
            cr.Data.ParentEstate = parentEstate;
            cr.Data.SnapshotID = snapshotID;
            cr.Data.SimName = Utils.StringToBytes(simName);
            cr.Data.PosGlobal = new Vector3d(globalPos);
            cr.Data.ParcelName = Utils.StringToBytes(parcelName);
            cr.Data.ClassifiedFlags = classifiedFlags;
            cr.Data.PriceForListing = price;

            OutPacket(cr, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAgentDropGroup(UUID groupID)
        {
            AgentDropGroupPacket dg =
                    (AgentDropGroupPacket)PacketPool.Instance.GetPacket(
                    PacketType.AgentDropGroup);

            dg.AgentData = new AgentDropGroupPacket.AgentDataBlock();
            dg.AgentData.AgentID = AgentId;
            dg.AgentData.GroupID = groupID;

            OutPacket(dg, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAvatarNotesReply(UUID targetID, string text)
        {
            AvatarNotesReplyPacket an =
                    (AvatarNotesReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.AvatarNotesReply);

            an.AgentData = new AvatarNotesReplyPacket.AgentDataBlock();
            an.AgentData.AgentID = AgentId;

            an.Data = new AvatarNotesReplyPacket.DataBlock();
            an.Data.TargetID = targetID;
            an.Data.Notes = Utils.StringToBytes(text);

            OutPacket(an, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAvatarPicksReply(UUID targetID, Dictionary<UUID, string> picks)
        {
            AvatarPicksReplyPacket ap =
                    (AvatarPicksReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.AvatarPicksReply);

            ap.AgentData = new AvatarPicksReplyPacket.AgentDataBlock();
            ap.AgentData.AgentID = AgentId;
            ap.AgentData.TargetID = targetID;

            ap.Data = new AvatarPicksReplyPacket.DataBlock[picks.Count];

            int i = 0;
            foreach (KeyValuePair<UUID, string> pick in picks)
            {
                ap.Data[i] = new AvatarPicksReplyPacket.DataBlock();
                ap.Data[i].PickID = pick.Key;
                ap.Data[i].PickName = Utils.StringToBytes(pick.Value);
                i++;
            }

            OutPacket(ap, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendAvatarClassifiedReply(UUID targetID, Dictionary<UUID, string> classifieds)
        {
            AvatarClassifiedReplyPacket ac =
                    (AvatarClassifiedReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.AvatarClassifiedReply);

            ac.AgentData = new AvatarClassifiedReplyPacket.AgentDataBlock();
            ac.AgentData.AgentID = AgentId;
            ac.AgentData.TargetID = targetID;

            ac.Data = new AvatarClassifiedReplyPacket.DataBlock[classifieds.Count];

            int i = 0;
            foreach (KeyValuePair<UUID, string> classified in classifieds)
            {
                ac.Data[i] = new AvatarClassifiedReplyPacket.DataBlock();
                ac.Data[i].ClassifiedID = classified.Key;
                ac.Data[i].Name = Utils.StringToBytes(classified.Value);
                i++;
            }

            OutPacket(ac, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendParcelDwellReply(int localID, UUID parcelID, float dwell)
        {
            ParcelDwellReplyPacket pd =
                    (ParcelDwellReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.ParcelDwellReply);

            pd.AgentData = new ParcelDwellReplyPacket.AgentDataBlock();
            pd.AgentData.AgentID = AgentId;

            pd.Data = new ParcelDwellReplyPacket.DataBlock();
            pd.Data.LocalID = localID;
            pd.Data.ParcelID = parcelID;
            pd.Data.Dwell = dwell;

            OutPacket(pd, ThrottleOutPacketType.Land);
        }

        public void SendUserInfoReply(bool imViaEmail, bool visible, string email)
        {
            UserInfoReplyPacket ur =
                    (UserInfoReplyPacket)PacketPool.Instance.GetPacket(
                    PacketType.UserInfoReply);

            string Visible = "hidden";
            if (visible)
                Visible = "default";

            ur.AgentData = new UserInfoReplyPacket.AgentDataBlock();
            ur.AgentData.AgentID = AgentId;

            ur.UserData = new UserInfoReplyPacket.UserDataBlock();
            ur.UserData.IMViaEMail = imViaEmail;
            ur.UserData.DirectoryVisibility = Utils.StringToBytes(Visible);
            ur.UserData.EMail = Utils.StringToBytes(email);

            OutPacket(ur, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendCreateGroupReply(UUID groupID, bool success, string message)
        {
            CreateGroupReplyPacket createGroupReply = (CreateGroupReplyPacket)PacketPool.Instance.GetPacket(PacketType.CreateGroupReply);

            createGroupReply.AgentData =
                new CreateGroupReplyPacket.AgentDataBlock();
            createGroupReply.ReplyData =
                new CreateGroupReplyPacket.ReplyDataBlock();

            createGroupReply.AgentData.AgentID = AgentId;
            createGroupReply.ReplyData.GroupID = groupID;

            createGroupReply.ReplyData.Success = success;
            createGroupReply.ReplyData.Message = Utils.StringToBytes(message);
            OutPacket(createGroupReply, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendUseCachedMuteList()
        {
            UseCachedMuteListPacket useCachedMuteList = (UseCachedMuteListPacket)PacketPool.Instance.GetPacket(PacketType.UseCachedMuteList);

            useCachedMuteList.AgentData = new UseCachedMuteListPacket.AgentDataBlock();
            useCachedMuteList.AgentData.AgentID = AgentId;

            OutPacket(useCachedMuteList, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendMuteListUpdate(string filename)
        {
            MuteListUpdatePacket muteListUpdate = (MuteListUpdatePacket)PacketPool.Instance.GetPacket(PacketType.MuteListUpdate);

            muteListUpdate.MuteData = new MuteListUpdatePacket.MuteDataBlock();
            muteListUpdate.MuteData.AgentID = AgentId;
            muteListUpdate.MuteData.Filename = Utils.StringToBytes(filename);

            OutPacket(muteListUpdate, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendPickInfoReply(UUID pickID, UUID creatorID, bool topPick, UUID parcelID, string name, string desc, UUID snapshotID, string user, string originalName, string simName, Vector3 posGlobal, int sortOrder, bool enabled)
        {
            PickInfoReplyPacket pickInfoReply = (PickInfoReplyPacket)PacketPool.Instance.GetPacket(PacketType.PickInfoReply);

            pickInfoReply.AgentData = new PickInfoReplyPacket.AgentDataBlock();
            pickInfoReply.AgentData.AgentID = AgentId;

            pickInfoReply.Data = new PickInfoReplyPacket.DataBlock();
            pickInfoReply.Data.PickID = pickID;
            pickInfoReply.Data.CreatorID = creatorID;
            pickInfoReply.Data.TopPick = topPick;
            pickInfoReply.Data.ParcelID = parcelID;
            pickInfoReply.Data.Name = Utils.StringToBytes(name);
            pickInfoReply.Data.Desc = Utils.StringToBytes(desc);
            pickInfoReply.Data.SnapshotID = snapshotID;
            pickInfoReply.Data.User = Utils.StringToBytes(user);
            pickInfoReply.Data.OriginalName = Utils.StringToBytes(originalName);
            pickInfoReply.Data.SimName = Utils.StringToBytes(simName);
            pickInfoReply.Data.PosGlobal = new Vector3d(posGlobal);
            pickInfoReply.Data.SortOrder = sortOrder;
            pickInfoReply.Data.Enabled = enabled;

            OutPacket(pickInfoReply, ThrottleOutPacketType.AvatarInfo);
        }

        #endregion Scene/Avatar to Client

        // Gesture

        #region Appearance/ Wearables Methods

        public void SendWearables(AvatarWearable[] wearables, int serial)
        {
            AgentWearablesUpdatePacket aw = (AgentWearablesUpdatePacket)PacketPool.Instance.GetPacket(PacketType.AgentWearablesUpdate);
            aw.AgentData.AgentID = AgentId;
            aw.AgentData.SerialNum = (uint)serial;
            aw.AgentData.SessionID = m_sessionId;

            int count = 0;
            for (int i = 0; i < wearables.Length; i++)
                count += wearables[i].Count;

            // TODO: don't create new blocks if recycling an old packet
            aw.WearableData = new AgentWearablesUpdatePacket.WearableDataBlock[count];
            AgentWearablesUpdatePacket.WearableDataBlock awb;
            int idx = 0;
            for (int i = 0; i < wearables.Length; i++)
            {
                for (int j = 0; j < wearables[i].Count; j++)
                {
                    awb = new AgentWearablesUpdatePacket.WearableDataBlock();
                    awb.WearableType = (byte)i;
                    awb.AssetID = wearables[i][j].AssetID;
                    awb.ItemID = wearables[i][j].ItemID;
                    aw.WearableData[idx] = awb;
                    idx++;

                    //                                m_log.DebugFormat(
                    //                                    "[APPEARANCE]: Sending wearable item/asset {0} {1} (index {2}) for {3}",
                    //                                    awb.ItemID, awb.AssetID, i, Name);
                }
            }

            OutPacket(aw, ThrottleOutPacketType.Texture);
        }

        public void SendAppearance(UUID agentID, byte[] visualParams, byte[] textureEntry)
        {
            AvatarAppearancePacket avp = (AvatarAppearancePacket)PacketPool.Instance.GetPacket(PacketType.AvatarAppearance);
            // TODO: don't create new blocks if recycling an old packet
            avp.VisualParam = new AvatarAppearancePacket.VisualParamBlock[218];
            avp.ObjectData.TextureEntry = textureEntry;

            AvatarAppearancePacket.VisualParamBlock avblock = null;
            for (int i = 0; i < visualParams.Length; i++)
            {
                avblock = new AvatarAppearancePacket.VisualParamBlock();
                avblock.ParamValue = visualParams[i];
                avp.VisualParam[i] = avblock;
            }

            avp.Sender.IsTrial = false;
            avp.Sender.ID = agentID;
            m_log.InfoFormat("[LLClientView]: Sending appearance for {0} to {1}", agentID.ToString(), AgentId.ToString());
            OutPacket(avp, ThrottleOutPacketType.Texture);
        }

        public void SendAnimations(UUID[] animations, int[] seqs, UUID sourceAgentId, UUID[] objectIDs)
        {
            //m_log.DebugFormat("[CLIENT]: Sending animations to {0}", Name);

            AvatarAnimationPacket ani = (AvatarAnimationPacket)PacketPool.Instance.GetPacket(PacketType.AvatarAnimation);
            // TODO: don't create new blocks if recycling an old packet
            ani.AnimationSourceList = new AvatarAnimationPacket.AnimationSourceListBlock[animations.Length];
            
            ani.Sender = new AvatarAnimationPacket.SenderBlock();
            ani.Sender.ID = sourceAgentId;

            ani.AnimationList = new AvatarAnimationPacket.AnimationListBlock[animations.Length];
            
            ani.PhysicalAvatarEventList = new AvatarAnimationPacket.PhysicalAvatarEventListBlock[1];
            ani.PhysicalAvatarEventList[0] = new AvatarAnimationPacket.PhysicalAvatarEventListBlock();
            ani.PhysicalAvatarEventList[0].TypeData = new byte[0];

            for (int i = 0; i < animations.Length; ++i)
            {
                ani.AnimationList[i] = new AvatarAnimationPacket.AnimationListBlock();
                ani.AnimationList[i].AnimID = animations[i];
                ani.AnimationList[i].AnimSequenceID = seqs[i] + ((i + 1) * 2);

                ani.AnimationSourceList[i] = new AvatarAnimationPacket.AnimationSourceListBlock();
                ani.AnimationSourceList[i].ObjectID = objectIDs[i];
                //if (objectIDs[i] == UUID.Zero)
                //    ani.AnimationSourceList[i].ObjectID = sourceAgentId;
            }
            //We do this here to keep the numbers under control
            m_animationSequenceNumber += (animations.Length * 2);

            ani.Header.Reliable = true;
            ani.HasVariableBlocks = false;
            OutPacket(ani, ThrottleOutPacketType.Asset);
        }

        #endregion

        #region Avatar Packet/Data Sending Methods

        /// <summary>
        /// Send an ObjectUpdate packet with information about an avatar
        /// </summary>
        public void SendAvatarDataImmediate(ISceneEntity avatar)
        {
            ScenePresence presence = avatar as ScenePresence;
            if (presence == null)
                return;

            ObjectUpdatePacket objupdate = (ObjectUpdatePacket)PacketPool.Instance.GetPacket(PacketType.ObjectUpdate);
            objupdate.Header.Zerocoded = true;

            objupdate.RegionData.RegionHandle = presence.RegionHandle;
            objupdate.RegionData.TimeDilation = ushort.MaxValue;

            objupdate.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[1];
            objupdate.ObjectData[0] = CreateAvatarUpdateBlock(presence);

            OutPacket(objupdate, ThrottleOutPacketType.Task);

            // We need to record the avatar local id since the root prim of an attachment points to this.
//            m_attachmentsSent.Add(avatar.LocalId);
        }

        public void SendCoarseLocationUpdate(List<UUID> users, List<Vector3> CoarseLocations)
        {
            if (!IsActive) return; // We don't need to update inactive clients.

            CoarseLocationUpdatePacket loc = (CoarseLocationUpdatePacket)PacketPool.Instance.GetPacket(PacketType.CoarseLocationUpdate);
            loc.Header.Reliable = false;

            // Each packet can only hold around 60 avatar positions and the client clears the mini-map each time
            // a CoarseLocationUpdate packet is received. Oh well.
            int total = Math.Min(CoarseLocations.Count, 60);

            CoarseLocationUpdatePacket.IndexBlock ib = new CoarseLocationUpdatePacket.IndexBlock();

            loc.Location = new CoarseLocationUpdatePacket.LocationBlock[total];
            loc.AgentData = new CoarseLocationUpdatePacket.AgentDataBlock[total];

            int selfindex = -1;
            for (int i = 0; i < total; i++)
            {
                CoarseLocationUpdatePacket.LocationBlock lb =
                    new CoarseLocationUpdatePacket.LocationBlock();

                lb.X = (byte)CoarseLocations[i].X;
                lb.Y = (byte)CoarseLocations[i].Y;

                lb.Z = CoarseLocations[i].Z > 1024 ? (byte)0 : (byte)(CoarseLocations[i].Z * 0.25f);
                loc.Location[i] = lb;
                loc.AgentData[i] = new CoarseLocationUpdatePacket.AgentDataBlock();
                loc.AgentData[i].AgentID = users[i];
                if (users[i] == AgentId)
                    selfindex = i;
            }

            ib.You = (short)selfindex;
            ib.Prey = -1;
            loc.Index = ib;

            OutPacket(loc, ThrottleOutPacketType.Land);
        }

        #endregion Avatar Packet/Data Sending Methods

        #region Primitive Packet/Data Sending Methods

        /// <summary>
        /// Generate one of the object update packets based on PrimUpdateFlags
        /// and broadcast the packet to clients
        /// </summary>
        public void SendPrimUpdate(ISceneEntity entity, PrimUpdateFlags updateFlags)
        {
            double priority = m_prioritizer.GetUpdatePriority(this, entity);
            PriorityQueueItem<EntityUpdate, double> item = new PriorityQueueItem<EntityUpdate,double>();
            item.Priority = priority;
            item.Value = new EntityUpdate(entity, updateFlags);
            lock (m_entityUpdates.SyncRoot)
                m_entityUpdates.Enqueue(item);
        }

        public void SendPrimUpdate(ISceneEntity entity, PrimUpdateFlags updateFlags, double priority)
        {
            PriorityQueueItem<EntityUpdate, double> item = new PriorityQueueItem<EntityUpdate, double>();
            item.Priority = priority;
            item.Value = new EntityUpdate(entity, updateFlags);
            lock (m_entityUpdates.SyncRoot)
                m_entityUpdates.Enqueue(item);
        }

        private void ProcessEntityUpdates(int maxUpdates)
        {
            OpenSim.Framework.Lazy<List<ObjectUpdatePacket.ObjectDataBlock>> objectUpdateBlocks = new OpenSim.Framework.Lazy<List<ObjectUpdatePacket.ObjectDataBlock>>();
            OpenSim.Framework.Lazy<List<ObjectUpdateCompressedPacket.ObjectDataBlock>> compressedUpdateBlocks = new OpenSim.Framework.Lazy<List<ObjectUpdateCompressedPacket.ObjectDataBlock>>();
            OpenSim.Framework.Lazy<List<ImprovedTerseObjectUpdatePacket.ObjectDataBlock>> terseUpdateBlocks = new OpenSim.Framework.Lazy<List<ImprovedTerseObjectUpdatePacket.ObjectDataBlock>>();
            OpenSim.Framework.Lazy<List<ObjectUpdateCachedPacket.ObjectDataBlock>> cachedUpdateBlocks = new OpenSim.Framework.Lazy<List<ObjectUpdateCachedPacket.ObjectDataBlock>>();

            if (maxUpdates <= 0) maxUpdates = Int32.MaxValue;
            int updatesThisCall = 0;

            lock (m_entityUpdates.SyncRoot)
            {
                PriorityQueueItem<EntityUpdate, double> update;
                while (updatesThisCall < maxUpdates && m_entityUpdates.TryDequeue(out update))
                {
                    if (update.Value.Entity is SceneObjectPart)
                    {
                        SceneObjectPart part = (SceneObjectPart)update.Value.Entity;

                        if (part.ParentGroup.IsAttachment && m_disableFacelights)
                        {
                            if (part.ParentGroup.RootPart.Shape.State != (byte)AttachmentPoint.LeftHand &&
                                part.ParentGroup.RootPart.Shape.State != (byte)AttachmentPoint.RightHand)
                            {
                                part.Shape.LightEntry = false;
                            }
                        }
                    }

                    ++updatesThisCall;

                    #region UpdateFlags to packet type conversion

                    PrimUpdateFlags updateFlags = update.Value.Flags;

                    bool canUseCompressed = true;
                    bool canUseImproved = true;
                    bool canUseCached = true;

                    // Compressed and cached object updates only make sense for LL primitives
                    if (!(update.Value.Entity is SceneObjectPart))
                    {
                        canUseCompressed = false;
                        canUseCached = false;
                    }
                    if (update.Value.Entity is SceneObjectPart)
                    {
                        // Please do not remove this unless you can demonstrate on the OpenSim mailing list that a client
                        // will never receive an update after a prim kill.  Even then, keeping the kill record may be a good
                        // safety measure.
                        //
                        // If a Linden Lab 1.23.5 client (and possibly later and earlier) receives an object update
                        // after a kill, it will keep displaying the deleted object until relog.  OpenSim currently performs
                        // updates and kills on different threads with different scheduling strategies, hence this protection.
                        //
                        // This doesn't appear to apply to child prims - a client will happily ignore these updates
                        // after the root prim has been deleted.
                        if (m_killRecord.Contains(update.Value.Entity.LocalId))
                        {
                                m_log.ErrorFormat(
                                    "[CLIENT]: Preventing update for prim with local id {0} after client for user {1} told it was deleted. JIRA this at http://jira.openmetaverse.org/secure/CreateIssue!default.jspa !",
                                    update.Value.Entity.LocalId, Name);
                            continue;
                        }

                        IObjectCache module = Scene.RequestModuleInterface<IObjectCache>();
                        if (module != null)
                        {
                            canUseCached = module.UseCachedObject(AgentId, update.Value.Entity.LocalId, ((SceneObjectPart)update.Value.Entity).CRC);
                        }
                        else
                        {
                            //No cache module? Don't use cached then, or it won't stop sending ObjectUpdateCached even when the client requests prims
                            canUseCached = false;
                        }
                    }

                    if (updateFlags.HasFlag(PrimUpdateFlags.FullUpdate))
                    {
                        canUseCompressed = false;
                        canUseImproved = false;
                        //If a full update has been requested, DO THE FULL UPDATE.
                        // Don't try to get out of this.... the monster called RepeatObjectUpdateCachedFromTheServer will occur and eat all your prims!
                        canUseCached = false;
                    }
                    else
                    {
                        if (updateFlags.HasFlag(PrimUpdateFlags.Velocity) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Acceleration) ||
                            updateFlags.HasFlag(PrimUpdateFlags.CollisionPlane) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Joint))
                        {
                            canUseCompressed = false;
                        }

                        if (updateFlags.HasFlag(PrimUpdateFlags.PrimFlags) ||
                            updateFlags.HasFlag(PrimUpdateFlags.ParentID) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Shape) ||
                            updateFlags.HasFlag(PrimUpdateFlags.PrimData) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Text) ||
                            updateFlags.HasFlag(PrimUpdateFlags.NameValue) ||
                            updateFlags.HasFlag(PrimUpdateFlags.ExtraData) ||
                            updateFlags.HasFlag(PrimUpdateFlags.TextureAnim) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Sound) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Particles) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Material) ||
                            updateFlags.HasFlag(PrimUpdateFlags.ClickAction) ||
                            updateFlags.HasFlag(PrimUpdateFlags.MediaURL) ||
                            updateFlags.HasFlag(PrimUpdateFlags.Joint))
                        {
                            canUseImproved = false;
                        }
                    }

                    #endregion UpdateFlags to packet type conversion

                    #region Block Construction

                    // UNTODO: Remove this once we can build compressed updates
                    // UPDATE: WE CAN!
                    //canUseCompressed = false;

                    // UNTODO: Remove this once we can build cached updates
                    // UPDATE: WE CAN!
                    //canUseCached = false;

                    try
                    {
                        //Do NOT send cached updates for terse updates
                        bool isTerse = updateFlags.HasFlag((PrimUpdateFlags.Position | PrimUpdateFlags.Rotation | PrimUpdateFlags.Velocity | PrimUpdateFlags.Acceleration | PrimUpdateFlags.AngularVelocity));
                        if (canUseCached && !isTerse)
                        {
                            cachedUpdateBlocks.Value.Add(CreatePrimCachedUpdateBlock((SceneObjectPart)update.Value.Entity, this.m_agentId));
                        }
                        else if (!canUseImproved && !canUseCompressed)
                        {
                            if (update.Value.Entity is ScenePresence)
                            {
                                objectUpdateBlocks.Value.Add(CreateAvatarUpdateBlock((ScenePresence)update.Value.Entity));
                            }
                            else
                            {
                                objectUpdateBlocks.Value.Add(CreatePrimUpdateBlock((SceneObjectPart)update.Value.Entity, this.m_agentId));
                            }
                        }
                        else if (!canUseImproved)
                        {
                            CompressedFlags Flags = CompressedFlags.None;
                            if (updateFlags.HasFlag(PrimUpdateFlags.AngularVelocity))
                                Flags |= CompressedFlags.HasAngularVelocity;
                            if (updateFlags.HasFlag(PrimUpdateFlags.MediaURL))
                                Flags |= CompressedFlags.MediaURL;
                            if (updateFlags.HasFlag(PrimUpdateFlags.ParentID))
                                Flags |= CompressedFlags.HasParent;
                            if (updateFlags.HasFlag(PrimUpdateFlags.Particles))
                                Flags |= CompressedFlags.HasParticles;
                            if (updateFlags.HasFlag(PrimUpdateFlags.Sound))
                                Flags |= CompressedFlags.HasSound;
                            if (updateFlags.HasFlag(PrimUpdateFlags.Text))
                                Flags |= CompressedFlags.HasText;
                            if (updateFlags.HasFlag(PrimUpdateFlags.TextureAnim))
                                Flags |= CompressedFlags.TextureAnimation;
                            if (updateFlags.HasFlag(PrimUpdateFlags.NameValue) || ((SceneObjectPart)update.Value.Entity).IsAttachment)
                                Flags |= CompressedFlags.HasNameValues;

                            compressedUpdateBlocks.Value.Add(CreateCompressedUpdateBlock((SceneObjectPart)update.Value.Entity, Flags, updateFlags));
                        }
                        else
                        {
                            terseUpdateBlocks.Value.Add(CreateImprovedTerseBlock(update.Value.Entity, updateFlags.HasFlag(PrimUpdateFlags.Textures)));
                        }
                    }
                    catch (Exception ex)
                    {
                        m_log.Warn("[LLCLIENTVIEW]: Issue creating an update block " + ex.ToString());
                    }

                    #endregion Block Construction
                }
            }

            #region Packet Sending

            float TIME_DILATION = m_scene.TimeDilation;
            ushort timeDilation = Utils.FloatToUInt16(TIME_DILATION, 0.0f, 1.0f);

            if (objectUpdateBlocks.IsValueCreated)
            {
                List<ObjectUpdatePacket.ObjectDataBlock> blocks = objectUpdateBlocks.Value;

                ObjectUpdatePacket packet = (ObjectUpdatePacket)PacketPool.Instance.GetPacket(PacketType.ObjectUpdate);
                packet.RegionData.RegionHandle = m_scene.RegionInfo.RegionHandle;
                packet.RegionData.TimeDilation = timeDilation;
                packet.ObjectData = new ObjectUpdatePacket.ObjectDataBlock[blocks.Count];

                for (int i = 0; i < blocks.Count; i++)
                    packet.ObjectData[i] = blocks[i];

                OutPacket(packet, ThrottleOutPacketType.Unknown, true);
            }

            if (compressedUpdateBlocks.IsValueCreated)
            {
                List<ObjectUpdateCompressedPacket.ObjectDataBlock> blocks = compressedUpdateBlocks.Value;

                ObjectUpdateCompressedPacket packet = (ObjectUpdateCompressedPacket)PacketPool.Instance.GetPacket(PacketType.ObjectUpdateCompressed);
                packet.RegionData.RegionHandle = m_scene.RegionInfo.RegionHandle;
                packet.RegionData.TimeDilation = timeDilation;
                packet.ObjectData = new ObjectUpdateCompressedPacket.ObjectDataBlock[blocks.Count];

                for (int i = 0; i < blocks.Count; i++)
                    packet.ObjectData[i] = blocks[i];

                OutPacket(packet, ThrottleOutPacketType.Unknown, true);
            }

            if (cachedUpdateBlocks.IsValueCreated)
            {
                List<ObjectUpdateCachedPacket.ObjectDataBlock> blocks = cachedUpdateBlocks.Value;

                ObjectUpdateCachedPacket packet = (ObjectUpdateCachedPacket)PacketPool.Instance.GetPacket(PacketType.ObjectUpdateCached);
                packet.RegionData.RegionHandle = m_scene.RegionInfo.RegionHandle;
                packet.RegionData.TimeDilation = timeDilation;
                packet.ObjectData = new ObjectUpdateCachedPacket.ObjectDataBlock[blocks.Count];

                for (int i = 0; i < blocks.Count; i++)
                    packet.ObjectData[i] = blocks[i];

                OutPacket(packet, ThrottleOutPacketType.Unknown, true);
            }

            if (terseUpdateBlocks.IsValueCreated)
            {
                List<ImprovedTerseObjectUpdatePacket.ObjectDataBlock> blocks = terseUpdateBlocks.Value;

                ImprovedTerseObjectUpdatePacket packet = new ImprovedTerseObjectUpdatePacket();
                packet.RegionData.RegionHandle = m_scene.RegionInfo.RegionHandle;
                packet.RegionData.TimeDilation = timeDilation;
                packet.ObjectData = new ImprovedTerseObjectUpdatePacket.ObjectDataBlock[blocks.Count];

                for (int i = 0; i < blocks.Count; i++)
                    packet.ObjectData[i] = blocks[i];

                OutPacket(packet, ThrottleOutPacketType.Unknown, true);
            }

            #endregion Packet Sending
        }

        public void FlushPrimUpdates()
        {
            m_log.Info("[CLIENT]: Flushing prim updates for " + m_firstName + " " + m_lastName);

            while (m_entityUpdates.Count > 0)
                ProcessEntityUpdates(-1);
        }

        #endregion Primitive Packet/Data Sending Methods

        void HandleQueueEmpty(ThrottleOutPacketTypeFlags categories)
        {
            if ((categories & ThrottleOutPacketTypeFlags.Task) != 0)
            {
                ProcessEntityUpdates(m_udpServer.PrimUpdatesPerCallback);
            }

            if ((categories & ThrottleOutPacketTypeFlags.Texture) != 0)
            {
                ProcessTextureRequests();
            }
        }

        void ProcessTextureRequests()
        {
            if (m_imageManager != null)
                m_imageManager.ProcessImageQueue(m_udpServer.TextureSendLimit);
        }

        public void SendAssetUploadCompleteMessage(sbyte AssetType, bool Success, UUID AssetFullID)
        {
            AssetUploadCompletePacket newPack = new AssetUploadCompletePacket();
            newPack.AssetBlock.Type = AssetType;
            newPack.AssetBlock.Success = Success;
            newPack.AssetBlock.UUID = AssetFullID;
            newPack.Header.Zerocoded = true;
            OutPacket(newPack, ThrottleOutPacketType.Asset);
        }

        public void SendXferRequest(ulong XferID, short AssetType, UUID vFileID, byte FilePath, byte[] FileName)
        {
            RequestXferPacket newPack = new RequestXferPacket();
            newPack.XferID.ID = XferID;
            newPack.XferID.VFileType = AssetType;
            newPack.XferID.VFileID = vFileID;
            newPack.XferID.FilePath = FilePath;
            newPack.XferID.Filename = FileName;
            newPack.Header.Zerocoded = true;
            OutPacket(newPack, ThrottleOutPacketType.Asset);
        }

        public void SendConfirmXfer(ulong xferID, uint PacketID)
        {
            ConfirmXferPacketPacket newPack = new ConfirmXferPacketPacket();
            newPack.XferID.ID = xferID;
            newPack.XferID.Packet = PacketID;
            newPack.Header.Zerocoded = true;
            OutPacket(newPack, ThrottleOutPacketType.Asset);
        }

        public void SendInitiateDownload(string simFileName, string clientFileName)
        {
            InitiateDownloadPacket newPack = new InitiateDownloadPacket();
            newPack.AgentData.AgentID = AgentId;
            newPack.FileData.SimFilename = Utils.StringToBytes(simFileName);
            newPack.FileData.ViewerFilename = Utils.StringToBytes(clientFileName);
            OutPacket(newPack, ThrottleOutPacketType.Asset);
        }

        public void SendImageFirstPart(
            ushort numParts, UUID ImageUUID, uint ImageSize, byte[] ImageData, byte imageCodec)
        {
            ImageDataPacket im = new ImageDataPacket();
            im.Header.Reliable = false;
            im.ImageID.Packets = numParts;
            im.ImageID.ID = ImageUUID;

            if (ImageSize > 0)
                im.ImageID.Size = ImageSize;

            im.ImageData.Data = ImageData;
            im.ImageID.Codec = imageCodec;
            im.Header.Zerocoded = true;
            OutPacket(im, ThrottleOutPacketType.Texture);
        }

        public void SendImageNextPart(ushort partNumber, UUID imageUuid, byte[] imageData)
        {
            ImagePacketPacket im = new ImagePacketPacket();
            im.Header.Reliable = false;
            im.ImageID.Packet = partNumber;
            im.ImageID.ID = imageUuid;
            im.ImageData.Data = imageData;

            OutPacket(im, ThrottleOutPacketType.Texture);
        }

        public void SendImageNotFound(UUID imageid)
        {
            ImageNotInDatabasePacket notFoundPacket
            = (ImageNotInDatabasePacket)PacketPool.Instance.GetPacket(PacketType.ImageNotInDatabase);

            notFoundPacket.ImageID.ID = imageid;

            OutPacket(notFoundPacket, ThrottleOutPacketType.Texture);
        }

        public void SendShutdownConnectionNotice()
        {
            OutPacket(PacketPool.Instance.GetPacket(PacketType.DisableSimulator), ThrottleOutPacketType.Unknown);
        }

        public void SendSimStats(SimStats stats)
        {
            SimStatsPacket pack = new SimStatsPacket();
            pack.Region = stats.RegionBlock;
            pack.Stat = stats.StatsBlock;

            pack.Header.Reliable = false;

            OutPacket(pack, ThrottleOutPacketType.Task);
        }

        public void SendObjectPropertiesFamilyData(uint RequestFlags, UUID ObjectUUID, UUID OwnerID, UUID GroupID,
                                                    uint BaseMask, uint OwnerMask, uint GroupMask, uint EveryoneMask,
                                                    uint NextOwnerMask, int OwnershipCost, byte SaleType, int SalePrice, uint Category,
                                                    UUID LastOwnerID, string ObjectName, string Description)
        {
            ObjectPropertiesFamilyPacket objPropFamilyPack = (ObjectPropertiesFamilyPacket)PacketPool.Instance.GetPacket(PacketType.ObjectPropertiesFamily);
            // TODO: don't create new blocks if recycling an old packet

            ObjectPropertiesFamilyPacket.ObjectDataBlock objPropDB = new ObjectPropertiesFamilyPacket.ObjectDataBlock();
            objPropDB.RequestFlags = RequestFlags;
            objPropDB.ObjectID = ObjectUUID;
            if (OwnerID == GroupID)
                objPropDB.OwnerID = UUID.Zero;
            else
                objPropDB.OwnerID = OwnerID;
            objPropDB.GroupID = GroupID;
            objPropDB.BaseMask = BaseMask;
            objPropDB.OwnerMask = OwnerMask;
            objPropDB.GroupMask = GroupMask;
            objPropDB.EveryoneMask = EveryoneMask;
            objPropDB.NextOwnerMask = NextOwnerMask;

            objPropDB.OwnershipCost = OwnershipCost;
            objPropDB.SaleType = SaleType;
            objPropDB.SalePrice = SalePrice;
            objPropDB.Category = Category;
            objPropDB.LastOwnerID = LastOwnerID;
            objPropDB.Name = Util.StringToBytes256(ObjectName);
            objPropDB.Description = Util.StringToBytes256(Description);
            objPropFamilyPack.ObjectData = objPropDB;
            objPropFamilyPack.Header.Zerocoded = true;
            objPropFamilyPack.HasVariableBlocks = false;
            OutPacket(objPropFamilyPack, ThrottleOutPacketType.Task);
        }

        public void SendObjectPropertiesReply(List<ISceneEntity> parts)
        {
            //ObjectPropertiesPacket proper = (ObjectPropertiesPacket)PacketPool.Instance.GetPacket(PacketType.ObjectProperties);
            // TODO: don't create new blocks if recycling an old packet

            List<ObjectPropertiesPacket.ObjectDataBlock> blocks = new List<ObjectPropertiesPacket.ObjectDataBlock>();
            foreach (ISceneEntity entity in parts)
            {
                if (!(entity is SceneObjectPart))
                    return;
                SceneObjectPart part = entity as SceneObjectPart;

                ObjectPropertiesPacket.ObjectDataBlock block =
                        new ObjectPropertiesPacket.ObjectDataBlock();

                block.ItemID = part.FromUserInventoryItemID;
                block.CreationDate = (ulong)part.CreationDate * 1000000;
                block.CreatorID = part.CreatorID;
                block.FolderID = UUID.Zero;
                block.FromTaskID = UUID.Zero;
                block.GroupID = part.GroupID;
                block.InventorySerial = (short)part.InventorySerial;

                block.LastOwnerID = part.LastOwnerID;

                block.ObjectID = part.UUID;

                if (part.OwnerID == part.GroupID)
                    block.OwnerID = UUID.Zero;
                else
                    block.OwnerID = part.OwnerID;
                block.TouchName = Util.StringToBytes256(part.ParentGroup.RootPart.TouchName);
                block.TextureID = new byte[0];
                block.SitName = Util.StringToBytes256(part.ParentGroup.RootPart.SitName);
                block.Name = Util.StringToBytes256(part.Name);
                block.Description = Util.StringToBytes256(part.Description);
                block.OwnerMask = part.ParentGroup.RootPart.OwnerMask;
                block.NextOwnerMask = part.ParentGroup.RootPart.NextOwnerMask;
                block.GroupMask = part.ParentGroup.RootPart.GroupMask;
                block.EveryoneMask = part.ParentGroup.RootPart.EveryoneMask;
                block.BaseMask = part.ParentGroup.RootPart.BaseMask;
                //            proper.ObjectData[0].AggregatePerms = 53;
                //            proper.ObjectData[0].AggregatePermTextures = 0;
                //            proper.ObjectData[0].AggregatePermTexturesOwner = 0;
                block.SaleType = part.ParentGroup.RootPart.ObjectSaleType;
                block.SalePrice = part.ParentGroup.RootPart.SalePrice;
                if (!blocks.Contains(block))
                    blocks.Add(block);
            }
            //ProcessObjectPropertiesPacket(null, null);

            ObjectPropertiesPacket proper = (ObjectPropertiesPacket)PacketPool.Instance.GetPacket(PacketType.ObjectProperties);
            
            proper.ObjectData = blocks.ToArray();

            proper.Header.Zerocoded = true;
            OutPacket(proper, ThrottleOutPacketType.Unknown);
        }

        private void ProcessObjectPropertiesPacket(Object sender, ElapsedEventArgs e)
        {
            ObjectPropertiesPacket proper = (ObjectPropertiesPacket)PacketPool.Instance.GetPacket(PacketType.ObjectProperties);

            lock (m_propertiesPacketTimer)
            {
                if (m_propertiesBlocks.Count > 100)
                {
                    for (int i = 0; i < (m_propertiesBlocks.Count / 100); i++)
                    {
                        proper = (ObjectPropertiesPacket)PacketPool.Instance.GetPacket(PacketType.ObjectProperties);
                        proper.ObjectData = m_propertiesBlocks.GetRange(0, 100).ToArray();
                        m_propertiesBlocks.RemoveRange(0, 100);

                        proper.Header.Zerocoded = true;
                        OutPacket(proper, ThrottleOutPacketType.State);
                    }
                    return;
                }
                else
                {
                    m_propertiesPacketTimer.Stop();

                    proper.ObjectData = m_propertiesBlocks.GetRange(0, m_propertiesBlocks.Count).ToArray();
                    m_propertiesBlocks.Clear();

                    proper.Header.Zerocoded = true;
                    OutPacket(proper, ThrottleOutPacketType.State);
                }
            }
        }

        #region Estate Data Sending Methods

        private static bool convertParamStringToBool(byte[] field)
        {
            string s = Utils.BytesToString(field);
            if (s == "1" || s.ToLower() == "y" || s.ToLower() == "yes" || s.ToLower() == "t" || s.ToLower() == "true")
            {
                return true;
            }
            return false;
        }

        public void SendEstateList(UUID invoice, int code, UUID[] Data, uint estateID)

        {
            EstateOwnerMessagePacket packet = new EstateOwnerMessagePacket();
            packet.AgentData.TransactionID = UUID.Random();
            packet.AgentData.AgentID = AgentId;
            packet.AgentData.SessionID = SessionId;
            packet.MethodData.Invoice = invoice;
            packet.MethodData.Method = Utils.StringToBytes("setaccess");

            EstateOwnerMessagePacket.ParamListBlock[] returnblock = new EstateOwnerMessagePacket.ParamListBlock[6 + Data.Length];

            for (int i = 0; i < (6 + Data.Length); i++)
            {
                returnblock[i] = new EstateOwnerMessagePacket.ParamListBlock();
            }
            int j = 0;

            returnblock[j].Parameter = Utils.StringToBytes(estateID.ToString()); j++;
            returnblock[j].Parameter = Utils.StringToBytes(code.ToString()); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;

            j = 2; // Agents
            if ((code & 2) != 0)
                j = 3; // Groups
            if ((code & 8) != 0)
                j = 5; // Managers

            returnblock[j].Parameter = Utils.StringToBytes(Data.Length.ToString());
            j = 6;

            for (int i = 0; i < Data.Length; i++)
            {
                returnblock[j].Parameter = Data[i].GetBytes(); j++;
            }
            packet.ParamList = returnblock;
            packet.Header.Reliable = true;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendBannedUserList(UUID invoice, EstateBan[] bl, uint estateID)
        {
            List<UUID> BannedUsers = new List<UUID>();

            for (int i = 0; i < bl.Length; i++)
            {
                if (bl[i] == null)
                    continue;
                if (bl[i].BannedUserID == UUID.Zero)
                    continue;
                BannedUsers.Add(bl[i].BannedUserID);
            }

            EstateOwnerMessagePacket packet = new EstateOwnerMessagePacket();
            packet.AgentData.TransactionID = UUID.Random();
            packet.AgentData.AgentID = AgentId;
            packet.AgentData.SessionID = SessionId;
            packet.MethodData.Invoice = invoice;
            packet.MethodData.Method = Utils.StringToBytes("setaccess");

            EstateOwnerMessagePacket.ParamListBlock[] returnblock = new EstateOwnerMessagePacket.ParamListBlock[6 + BannedUsers.Count];

            for (int i = 0; i < (6 + BannedUsers.Count); i++)
            {
                returnblock[i] = new EstateOwnerMessagePacket.ParamListBlock();
            }
            int j = 0;

            returnblock[j].Parameter = Utils.StringToBytes(estateID.ToString()); j++;
            returnblock[j].Parameter = Utils.StringToBytes(((int)Constants.EstateAccessCodex.EstateBans).ToString()); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;
            returnblock[j].Parameter = Utils.StringToBytes(BannedUsers.Count.ToString()); j++;
            returnblock[j].Parameter = Utils.StringToBytes("0"); j++;

            foreach (UUID banned in BannedUsers)
            {
                returnblock[j].Parameter = banned.GetBytes(); j++;
            }
            packet.ParamList = returnblock;
            packet.Header.Reliable = false;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendRegionInfoToEstateMenu(RegionInfoForEstateMenuArgs args)
        {
            RegionInfoPacket rinfopack = new RegionInfoPacket();
            RegionInfoPacket.RegionInfoBlock rinfoblk = new RegionInfoPacket.RegionInfoBlock();
            rinfopack.AgentData.AgentID = AgentId;
            rinfopack.AgentData.SessionID = SessionId;
            rinfoblk.BillableFactor = args.billableFactor;
            rinfoblk.EstateID = args.estateID;
            rinfoblk.MaxAgents = args.maxAgents;
            rinfoblk.ObjectBonusFactor = args.objectBonusFactor;
            rinfoblk.ParentEstateID = args.parentEstateID;
            rinfoblk.PricePerMeter = args.pricePerMeter;
            rinfoblk.RedirectGridX = args.redirectGridX;
            rinfoblk.RedirectGridY = args.redirectGridY;
            rinfoblk.RegionFlags = args.regionFlags;
            rinfoblk.SimAccess = args.simAccess;
            rinfoblk.SunHour = args.sunHour;
            rinfoblk.TerrainLowerLimit = args.terrainLowerLimit;
            rinfoblk.TerrainRaiseLimit = args.terrainRaiseLimit;
            rinfoblk.UseEstateSun = args.useEstateSun;
            rinfoblk.WaterHeight = args.waterHeight;
            rinfoblk.SimName = Utils.StringToBytes(args.simName);

            rinfopack.RegionInfo2 = new RegionInfoPacket.RegionInfo2Block();
            rinfopack.RegionInfo2.HardMaxAgents = uint.MaxValue;
            rinfopack.RegionInfo2.HardMaxObjects = uint.MaxValue;
            rinfopack.RegionInfo2.MaxAgents32 = (uint)args.maxAgents;
            rinfopack.RegionInfo2.ProductName = Utils.StringToBytes(args.regionType);
            rinfopack.RegionInfo2.ProductSKU = Utils.EmptyBytes;

            rinfopack.HasVariableBlocks = true;
            rinfopack.RegionInfo = rinfoblk;
            rinfopack.AgentData = new RegionInfoPacket.AgentDataBlock();
            rinfopack.AgentData.AgentID = AgentId;
            rinfopack.AgentData.SessionID = SessionId;


            OutPacket(rinfopack, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendEstateCovenantInformation(UUID covenant, int covenantLastUpdated)
        {
            EstateCovenantReplyPacket einfopack = new EstateCovenantReplyPacket();
            EstateCovenantReplyPacket.DataBlock edata = new EstateCovenantReplyPacket.DataBlock();
            edata.CovenantID = covenant;
            edata.CovenantTimestamp = (uint)covenantLastUpdated;
            edata.EstateOwnerID = m_scene.RegionInfo.EstateSettings.EstateOwner;
            edata.EstateName = Utils.StringToBytes(m_scene.RegionInfo.EstateSettings.EstateName);
            einfopack.Data = edata;
            OutPacket(einfopack, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendDetailedEstateData(UUID invoice, string estateName, uint estateID, uint parentEstate, uint estateFlags, uint sunPosition, UUID covenant, int CovenantLastUpdated, string abuseEmail, UUID estateOwner)
        {
            EstateOwnerMessagePacket packet = new EstateOwnerMessagePacket();
            packet.MethodData.Invoice = invoice;
            packet.AgentData.TransactionID = UUID.Random();
            packet.MethodData.Method = Utils.StringToBytes("estateupdateinfo");
            EstateOwnerMessagePacket.ParamListBlock[] returnblock = new EstateOwnerMessagePacket.ParamListBlock[10];

            for (int i = 0; i < 10; i++)
            {
                returnblock[i] = new EstateOwnerMessagePacket.ParamListBlock();
            }

            //Sending Estate Settings
            returnblock[0].Parameter = Utils.StringToBytes(estateName);
            returnblock[1].Parameter = Utils.StringToBytes(estateOwner.ToString());
            returnblock[2].Parameter = Utils.StringToBytes(estateID.ToString());

            returnblock[3].Parameter = Utils.StringToBytes(estateFlags.ToString());
            returnblock[4].Parameter = Utils.StringToBytes(sunPosition.ToString());
            returnblock[5].Parameter = Utils.StringToBytes(parentEstate.ToString());
            returnblock[6].Parameter = Utils.StringToBytes(covenant.ToString());
            returnblock[7].Parameter = Utils.StringToBytes(CovenantLastUpdated.ToString());
            returnblock[8].Parameter = Utils.StringToBytes("1"); // Send to this agent only
            returnblock[9].Parameter = Utils.StringToBytes(abuseEmail);

            packet.ParamList = returnblock;
            packet.Header.Reliable = false;
            //m_log.Debug("[ESTATE]: SIM--->" + packet.ToString());
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        #endregion

        #region Land Data Sending Methods

        public void SendLandParcelOverlay(byte[] data, int sequence_id)
        {
            ParcelOverlayPacket packet = (ParcelOverlayPacket)PacketPool.Instance.GetPacket(PacketType.ParcelOverlay);
            packet.ParcelData.Data = data;
            packet.ParcelData.SequenceID = sequence_id;
            packet.Header.Zerocoded = true;
            OutPacket(packet, ThrottleOutPacketType.Land);
        }

        public void SendLandProperties(int sequence_id, bool snap_selection, int request_result, LandData landData, float simObjectBonusFactor, int parcelObjectCapacity, int simObjectCapacity, uint regionFlags)
        {
            ParcelPropertiesMessage updateMessage = new ParcelPropertiesMessage();

            updateMessage.AABBMax = landData.AABBMax;
            updateMessage.AABBMin = landData.AABBMin;
            updateMessage.Area = landData.Area;
            updateMessage.AuctionID = landData.AuctionID;
            updateMessage.AuthBuyerID = landData.AuthBuyerID;

            updateMessage.Bitmap = landData.Bitmap;

            updateMessage.Desc = landData.Description;
            updateMessage.Category = landData.Category;
            updateMessage.ClaimDate = Util.ToDateTime(landData.ClaimDate);
            updateMessage.ClaimPrice = landData.ClaimPrice;
            updateMessage.GroupID = landData.GroupID;
            updateMessage.GroupPrims = landData.GroupPrims;
            updateMessage.IsGroupOwned = landData.IsGroupOwned;
            updateMessage.LandingType = (LandingType) landData.LandingType;
            updateMessage.LocalID = landData.LocalID;

            updateMessage.MaxPrims = parcelObjectCapacity;

            updateMessage.MediaAutoScale = Convert.ToBoolean(landData.MediaAutoScale);
            updateMessage.MediaID = landData.MediaID;
            updateMessage.MediaURL = landData.MediaURL;
            updateMessage.MusicURL = landData.MusicURL;
            updateMessage.Name = landData.Name;
            updateMessage.OtherCleanTime = landData.OtherCleanTime;
            updateMessage.OtherCount = 0; //TODO: Unimplemented
            updateMessage.OtherPrims = landData.OtherPrims;
            updateMessage.OwnerID = landData.OwnerID;
            updateMessage.OwnerPrims = landData.OwnerPrims;
            updateMessage.ParcelFlags = (ParcelFlags) landData.Flags;
            updateMessage.ParcelPrimBonus = simObjectBonusFactor;
            updateMessage.PassHours = landData.PassHours;
            updateMessage.PassPrice = landData.PassPrice;
            updateMessage.PublicCount = 0; //TODO: Unimplemented

            updateMessage.RegionPushOverride = (regionFlags & (uint)RegionFlags.RestrictPushObject) > 0;
            updateMessage.RegionDenyAnonymous = (regionFlags & (uint)RegionFlags.DenyAnonymous) > 0;

            //updateMessage.RegionDenyIdentified = (regionFlags & (uint)RegionFlags.DenyIdentified) > 0;
            //updateMessage.RegionDenyTransacted = (regionFlags & (uint)RegionFlags.DenyTransacted) > 0;

            updateMessage.RentPrice = 0;
            updateMessage.RequestResult = (ParcelResult) request_result;
            updateMessage.SalePrice = landData.SalePrice;
            updateMessage.SelectedPrims = landData.SelectedPrims;
            updateMessage.SelfCount = 0; //TODO: Unimplemented
            updateMessage.SequenceID = sequence_id;
            updateMessage.SimWideMaxPrims = simObjectCapacity;
            updateMessage.SimWideTotalPrims = landData.SimwidePrims;
            updateMessage.SnapSelection = snap_selection;
            updateMessage.SnapshotID = landData.SnapshotID;
            updateMessage.Status = (ParcelStatus) landData.Status;
            updateMessage.TotalPrims = landData.OwnerPrims + landData.GroupPrims + landData.OtherPrims +
                                                 landData.SelectedPrims;
            updateMessage.UserLocation = landData.UserLocation;
            updateMessage.UserLookAt = landData.UserLookAt;

            updateMessage.MediaType = landData.MediaType;
            updateMessage.MediaDesc = landData.MediaDescription;
            updateMessage.MediaWidth = landData.MediaWidth;
            updateMessage.MediaHeight = landData.MediaHeight;
            updateMessage.MediaLoop = landData.MediaLoop;
            updateMessage.ObscureMusic = landData.ObscureMusic;
            updateMessage.ObscureMedia = landData.ObscureMedia;

            try
            {
                IEventQueueService eq = Scene.RequestModuleInterface<IEventQueueService>();
                if (eq != null)
                {
                    eq.ParcelProperties(updateMessage, this.AgentId, this.Scene.RegionInfo.RegionHandle);
                } else {
                    m_log.Warn("No EQ Interface when sending parcel data.");
                }
            }
            catch (Exception ex)
            {
                m_log.Error("Unable to send parcel data via eventqueue - exception: " + ex.ToString());
            }
        }

        public void SendLandAccessListData(List<UUID> avatars, uint accessFlag, int localLandID)
        {
            ParcelAccessListReplyPacket replyPacket = (ParcelAccessListReplyPacket)PacketPool.Instance.GetPacket(PacketType.ParcelAccessListReply);
            replyPacket.Data.AgentID = AgentId;
            replyPacket.Data.Flags = accessFlag;
            replyPacket.Data.LocalID = localLandID;
            replyPacket.Data.SequenceID = 0;

            List<ParcelAccessListReplyPacket.ListBlock> list = new List<ParcelAccessListReplyPacket.ListBlock>();
            foreach (UUID avatar in avatars)
            {
                ParcelAccessListReplyPacket.ListBlock block = new ParcelAccessListReplyPacket.ListBlock();
                block.Flags = accessFlag;
                block.ID = avatar;
                block.Time = 0;
                list.Add(block);
            }

            replyPacket.List = list.ToArray();
            replyPacket.Header.Zerocoded = true;
            OutPacket(replyPacket, ThrottleOutPacketType.Land);
        }

        public void SendForceClientSelectObjects(List<uint> ObjectIDs)
        {
            bool firstCall = true;
            const int MAX_OBJECTS_PER_PACKET = 251;
            ForceObjectSelectPacket pack = (ForceObjectSelectPacket)PacketPool.Instance.GetPacket(PacketType.ForceObjectSelect);
            ForceObjectSelectPacket.DataBlock[] data;
            while (ObjectIDs.Count > 0)
            {
                if (firstCall)
                {
                    pack._Header.ResetList = true;
                    firstCall = false;
                }
                else
                {
                    pack._Header.ResetList = false;
                }

                if (ObjectIDs.Count > MAX_OBJECTS_PER_PACKET)
                {
                    data = new ForceObjectSelectPacket.DataBlock[MAX_OBJECTS_PER_PACKET];
                }
                else
                {
                    data = new ForceObjectSelectPacket.DataBlock[ObjectIDs.Count];
                }

                int i;
                for (i = 0; i < MAX_OBJECTS_PER_PACKET && ObjectIDs.Count > 0; i++)
                {
                    data[i] = new ForceObjectSelectPacket.DataBlock();
                    data[i].LocalID = Convert.ToUInt32(ObjectIDs[0]);
                    ObjectIDs.RemoveAt(0);
                }
                pack.Data = data;
                pack.Header.Zerocoded = true;
                OutPacket(pack, ThrottleOutPacketType.State);
            }
        }

        public void SendCameraConstraint(Vector4 ConstraintPlane)
        {
            CameraConstraintPacket cpack = (CameraConstraintPacket)PacketPool.Instance.GetPacket(PacketType.CameraConstraint);
            cpack.CameraCollidePlane = new CameraConstraintPacket.CameraCollidePlaneBlock();
            cpack.CameraCollidePlane.Plane = ConstraintPlane;
            //m_log.DebugFormat("[CLIENTVIEW]: Constraint {0}", ConstraintPlane);
            OutPacket(cpack, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendLandObjectOwners(LandData land, List<UUID> groups, Dictionary<UUID, int> ownersAndCount)
        {
            int notifyCount = ownersAndCount.Count;
            ParcelObjectOwnersReplyPacket pack = (ParcelObjectOwnersReplyPacket)PacketPool.Instance.GetPacket(PacketType.ParcelObjectOwnersReply);

            if (notifyCount > 0)
            {
                if (notifyCount > 32)
                {
                    m_log.InfoFormat(
                        "[LAND]: Mor e than {0} avatars own prims on this parcel.  Only sending back details of first {0}"
                        + " - a developer might want to investigate whether this is a hard limit", 32);

                    notifyCount = 32;
                }

                ParcelObjectOwnersReplyPacket.DataBlock[] dataBlock
                    = new ParcelObjectOwnersReplyPacket.DataBlock[notifyCount];

                int num = 0;
                foreach (UUID owner in ownersAndCount.Keys)
                {
                    dataBlock[num] = new ParcelObjectOwnersReplyPacket.DataBlock();
                    dataBlock[num].Count = ownersAndCount[owner];

                    if (land.GroupID == owner || groups.Contains(owner))
                        dataBlock[num].IsGroupOwned = true;

                    dataBlock[num].OnlineStatus = true;
                    dataBlock[num].OwnerID = owner;

                    num++;

                    if (num >= notifyCount)
                        break;
                }

                pack.Data = dataBlock;
            }
            else
            {
                pack.Data = new ParcelObjectOwnersReplyPacket.DataBlock[]{new ParcelObjectOwnersReplyPacket.DataBlock()};
            }
            pack.Header.Zerocoded = true;
            OutPacket(pack, ThrottleOutPacketType.Land);
        }

        #endregion

        #region Helper Methods

        protected ImprovedTerseObjectUpdatePacket.ObjectDataBlock CreateImprovedTerseBlock(ISceneEntity entity, bool sendTexture)
        {
            #region ScenePresence/SOP Handling

            bool avatar = (entity is ScenePresence);
            uint localID = entity.LocalId;
            int attachPoint;
            Vector4 collisionPlane;
            Vector3 position, velocity, acceleration, angularVelocity;
            Quaternion rotation;
            byte[] textureEntry;

            if (entity is ScenePresence)
            {
                ScenePresence presence = (ScenePresence)entity;

                attachPoint = 0;
                collisionPlane = presence.CollisionPlane;
                position = presence.OffsetPosition;
                velocity = presence.Velocity;
                acceleration = Vector3.Zero;
                angularVelocity = Vector3.Zero;
                rotation = presence.Rotation;
                if (sendTexture)
                    textureEntry = presence.Appearance.Texture.GetBytes();
                else
                    textureEntry = null;
            }
            else
            {
                SceneObjectPart part = (SceneObjectPart)entity;

                attachPoint = part.AttachmentPoint;
                collisionPlane = Vector4.Zero;
                position = part.RelativePosition;
                velocity = part.Velocity;
                acceleration = part.Acceleration;
                angularVelocity = part.AngularVelocity;
                rotation = part.RotationOffset;

                if (sendTexture)
                    textureEntry = part.Shape.TextureEntry;
                else
                    textureEntry = null;
            }

            #endregion ScenePresence/SOP Handling

            int pos = 0;
            byte[] data = new byte[(avatar ? 60 : 44)];

            // LocalID
            Utils.UIntToBytes(localID, data, pos);
            pos += 4;

            // Avatar/CollisionPlane
            data[pos++] = (byte)((attachPoint % 16) * 16 + (attachPoint / 16)); ;
            if (avatar)
            {
                data[pos++] = 1;

                if (collisionPlane == Vector4.Zero)
                    collisionPlane = Vector4.UnitW;
                //m_log.DebugFormat("CollisionPlane: {0}",collisionPlane);
                collisionPlane.ToBytes(data, pos);
                pos += 16;
            }
            else
            {
                ++pos;
            }

            // Position
            position.ToBytes(data, pos);
            pos += 12;

            // Velocity
            //m_log.DebugFormat("Velocity: {0}", velocity);
            Utils.UInt16ToBytes(Utils.FloatToUInt16(velocity.X, -128.0f, 128.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(velocity.Y, -128.0f, 128.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(velocity.Z, -128.0f, 128.0f), data, pos); pos += 2;

            // Acceleration
            Utils.UInt16ToBytes(Utils.FloatToUInt16(acceleration.X, -64.0f, 64.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(acceleration.Y, -64.0f, 64.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(acceleration.Z, -64.0f, 64.0f), data, pos); pos += 2;

            // Rotation
            Utils.UInt16ToBytes(Utils.FloatToUInt16(rotation.X, -1.0f, 1.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(rotation.Y, -1.0f, 1.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(rotation.Z, -1.0f, 1.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(rotation.W, -1.0f, 1.0f), data, pos); pos += 2;

            // Angular Velocity
            Utils.UInt16ToBytes(Utils.FloatToUInt16(angularVelocity.X, -64.0f, 64.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(angularVelocity.Y, -64.0f, 64.0f), data, pos); pos += 2;
            Utils.UInt16ToBytes(Utils.FloatToUInt16(angularVelocity.Z, -64.0f, 64.0f), data, pos); pos += 2;

            ImprovedTerseObjectUpdatePacket.ObjectDataBlock block = new ImprovedTerseObjectUpdatePacket.ObjectDataBlock();
            block.Data = data;

            if (textureEntry != null && textureEntry.Length > 0)
            {
                byte[] teBytesFinal = new byte[textureEntry.Length + 4];

                // Texture Length
                Utils.IntToBytes(textureEntry.Length, textureEntry, 0);
                // Texture
                Buffer.BlockCopy(textureEntry, 0, teBytesFinal, 4, textureEntry.Length);

                block.TextureEntry = teBytesFinal;
            }
            else
            {
                block.TextureEntry = Utils.EmptyBytes;
            }
            return block;
        }

        protected ObjectUpdatePacket.ObjectDataBlock CreateAvatarUpdateBlock(ScenePresence data)
        {
            byte[] objectData = new byte[76];

            if (data.CollisionPlane == Vector4.Zero)
                data.CollisionPlane = Vector4.UnitW;
            //m_log.DebugFormat("CollisionPlane: {0}", data.CollisionPlane);
            data.CollisionPlane.ToBytes(objectData, 0);
            data.OffsetPosition.ToBytes(objectData, 16);
            data.Velocity.ToBytes(objectData, 28);
            //data.Acceleration.ToBytes(objectData, 40);
            data.Rotation.ToBytes(objectData, 52);
            //data.AngularVelocity.ToBytes(objectData, 64);

            ObjectUpdatePacket.ObjectDataBlock update = new ObjectUpdatePacket.ObjectDataBlock();

            update.Data = Utils.EmptyBytes;
            update.ExtraParams = new byte[1];
            update.FullID = data.UUID;
            update.ID = data.LocalId;
            update.Material = (byte)Material.Flesh;
            update.MediaURL = Utils.EmptyBytes;
            update.NameValue = Utils.StringToBytes("FirstName STRING RW SV " + data.Firstname + "\nLastName STRING RW SV " +
                data.Lastname + "\nTitle STRING RW SV " + (m_GroupsModule == null ? "" : m_GroupsModule.GetGroupTitle(data.UUID)));
            update.ObjectData = objectData;
            if (data.ParentID == UUID.Zero)
                update.ParentID = 0;
            else
            {
                SceneObjectPart part = ((Scene)Scene).GetSceneObjectPart(data.ParentID);
                update.ParentID = part.LocalId;
            }
            update.PathCurve = 16;
            update.PathScaleX = 100;
            update.PathScaleY = 100;
            update.PCode = (byte)PCode.Avatar;
            update.ProfileCurve = 1;
            update.PSBlock = Utils.EmptyBytes;
            update.Scale = new Vector3(0.45f, 0.6f, 1.9f);
            update.Text = Utils.EmptyBytes;
            update.TextColor = new byte[4];
            update.TextureAnim = Utils.EmptyBytes;
            update.TextureEntry = (data.Appearance.Texture != null) ? data.Appearance.Texture.GetBytes() : Utils.EmptyBytes;
            update.UpdateFlags = (uint)(
                PrimFlags.Physics | PrimFlags.ObjectModify | PrimFlags.ObjectCopy | PrimFlags.ObjectAnyOwner |
                PrimFlags.ObjectYouOwner | PrimFlags.ObjectMove | PrimFlags.InventoryEmpty | PrimFlags.ObjectTransfer |
                PrimFlags.ObjectOwnerModify);

            return update;
        }

        protected ObjectUpdateCachedPacket.ObjectDataBlock CreatePrimCachedUpdateBlock(SceneObjectPart data, UUID recipientID)
        {
            ObjectUpdateCachedPacket.ObjectDataBlock odb = new ObjectUpdateCachedPacket.ObjectDataBlock();
            odb.CRC = data.CRC;
            odb.ID = data.LocalId;

            #region PrimFlags

            PrimFlags flags = (PrimFlags)m_scene.Permissions.GenerateClientFlags(recipientID, data);
            
            // Don't send the CreateSelected flag to everyone
            flags &= ~PrimFlags.CreateSelected;

            if (recipientID == data.OwnerID)
            {
                if (data.CreateSelected)
                {
                    // Only send this flag once, then unset it
                    flags |= PrimFlags.CreateSelected;
                    data.CreateSelected = false;
                }
            }

            #endregion PrimFlags

            odb.UpdateFlags = (uint)flags;
            return odb;
        }

        protected ObjectUpdatePacket.ObjectDataBlock CreatePrimUpdateBlock(SceneObjectPart data, UUID recipientID)
        {
            byte[] objectData = new byte[60];
            data.RelativePosition.ToBytes(objectData, 0);
            data.Velocity.ToBytes(objectData, 12);
            data.Acceleration.ToBytes(objectData, 24);
            data.RotationOffset.ToBytes(objectData, 36);
            data.AngularVelocity.ToBytes(objectData, 48);

            ObjectUpdatePacket.ObjectDataBlock update = new ObjectUpdatePacket.ObjectDataBlock();
            update.ClickAction = (byte)data.ClickAction;
            update.CRC = data.CRC;
            update.ExtraParams = data.Shape.ExtraParams ?? Utils.EmptyBytes;
            update.FullID = data.UUID;
            update.ID = data.LocalId;
            //update.JointAxisOrAnchor = Vector3.Zero; // These are deprecated
            //update.JointPivot = Vector3.Zero;
            //update.JointType = 0;
            update.Material = data.Material;
            update.MediaURL = Utils.StringToBytes(data.CurrentMediaVersion);
            if (data.IsAttachment)
            {
                update.NameValue = Util.StringToBytes256("AttachItemID STRING RW SV " + data.FromItemID);
                update.State = (byte)((data.AttachmentPoint % 16) * 16 + (data.AttachmentPoint / 16));
            }
            else
            {
                update.NameValue = Utils.EmptyBytes;
                update.State = data.Shape.State;
            }

            update.ObjectData = objectData;
            update.ParentID = data.ParentID;
            update.PathBegin = data.Shape.PathBegin;
            update.PathCurve = data.Shape.PathCurve;
            update.PathEnd = data.Shape.PathEnd;
            update.PathRadiusOffset = data.Shape.PathRadiusOffset;
            update.PathRevolutions = data.Shape.PathRevolutions;
            update.PathScaleX = data.Shape.PathScaleX;
            update.PathScaleY = data.Shape.PathScaleY;
            update.PathShearX = data.Shape.PathShearX;
            update.PathShearY = data.Shape.PathShearY;
            update.PathSkew = data.Shape.PathSkew;
            update.PathTaperX = data.Shape.PathTaperX;
            update.PathTaperY = data.Shape.PathTaperY;
            update.PathTwist = data.Shape.PathTwist;
            update.PathTwistBegin = data.Shape.PathTwistBegin;
            update.PCode = data.Shape.PCode;
            update.ProfileBegin = data.Shape.ProfileBegin;
            update.ProfileCurve = data.Shape.ProfileCurve;
            update.ProfileEnd = data.Shape.ProfileEnd;
            update.ProfileHollow = data.Shape.ProfileHollow;
            update.PSBlock = data.ParticleSystem ?? Utils.EmptyBytes;
            update.TextColor = data.GetTextColor().GetBytes(false);
            update.TextureAnim = data.TextureAnimation ?? Utils.EmptyBytes;
            update.TextureEntry = data.Shape.TextureEntry ?? Utils.EmptyBytes;
            update.Scale = data.Shape.Scale;
            update.Text = Util.StringToBytes256(data.Text);
            update.MediaURL = Util.StringToBytes256(data.MediaUrl);

            #region PrimFlags

            PrimFlags flags = (PrimFlags)m_scene.Permissions.GenerateClientFlags(recipientID, data);
            
            // Don't send the CreateSelected flag to everyone
            flags &= ~PrimFlags.CreateSelected;

            if (recipientID == data.OwnerID)
            {
                if (data.CreateSelected)
                {
                    // Only send this flag once, then unset it
                    flags |= PrimFlags.CreateSelected;
                    data.CreateSelected = false;
                }
            }

//            m_log.DebugFormat(
//                "[LLCLIENTVIEW]: Constructing client update for part {0} {1} with flags {2}, localId {3}",
//                data.Name, update.FullID, flags, update.ID);

            update.UpdateFlags = (uint)flags;

            #endregion PrimFlags

            if (data.Sound != UUID.Zero)
            {
                update.Sound = data.Sound;
                update.OwnerID = data.OwnerID;
                update.Gain = (float)data.SoundGain;
                update.Radius = (float)data.SoundRadius;
                update.Flags = data.SoundFlags;
            }

            switch ((PCode)data.Shape.PCode)
            {
                case PCode.Grass:
                case PCode.Tree:
                case PCode.NewTree:
                    update.Data = new byte[] { data.Shape.State };
                    break;
                default:
                    update.Data = Utils.EmptyBytes;
                    break;
            }

            return update;
        }

        protected ObjectUpdateCompressedPacket.ObjectDataBlock CreateCompressedUpdateBlock(SceneObjectPart part, CompressedFlags updateFlags, PrimUpdateFlags flags)
        {
            ObjectUpdateCompressedPacket.ObjectDataBlock block = new ObjectUpdateCompressedPacket.ObjectDataBlock();
            byte[] objectData = new byte[500];
            int i = 0;
            part.UUID.ToBytes(objectData, 0);
            i += 16;
            Utils.UIntToBytes(part.LocalId, objectData, i);
            i += 4;
            objectData[i] = (byte)part.Shape.PCode; //Type of prim
            i += 1;

            if (part.Shape.PCode == (byte)PCode.Tree || part.Shape.PCode == (byte)PCode.NewTree)
                updateFlags |= CompressedFlags.Tree;

            //Attachment point
            objectData[i] = (byte)part.AttachmentPoint;
            i += 1;
            //CRC
            Utils.UIntToBytes(part.CRC, objectData, i);
            i += 4;
            objectData[i] = part.Material;
            i++;
            objectData[i] = part.ClickAction;
            i++;
            part.Shape.Scale.ToBytes(objectData, i);
            i += 12;
            part.RelativePosition.ToBytes(objectData, i);
            i += 12;
            part.RotationOffset.ToBytes(objectData, i);
            i += 12;
            Utils.UIntToBytes((uint)updateFlags, objectData, i);
            i += 4;
            part.OwnerID.ToBytes(objectData, i);
            i += 16;

            if ((updateFlags & CompressedFlags.HasAngularVelocity) != 0)
            {
                part.AngularVelocity.ToBytes(objectData, i);
                i += 12;
            }
            if ((updateFlags & CompressedFlags.HasParent) != 0)
            {
                Utils.UIntToBytes(part.ParentID, objectData, i);
                i += 4;
            }
            if ((updateFlags & CompressedFlags.Tree) != 0)
            {
                objectData[i] = part.Shape.State; //Tree type
                i++;
            }
            else if ((updateFlags & CompressedFlags.ScratchPad) != 0)
            {
                //Remove the flag, we have no clue what to do with this
                updateFlags &= ~(CompressedFlags.ScratchPad);
            }
            if ((updateFlags & CompressedFlags.HasText) != 0)
            {
                foreach(char c in part.Text)
                {
                    objectData[i] = (byte)c;
                    i++;
                }
                i++;
                
                byte[] textcolor = part.GetTextColor().GetBytes(false);
                Buffer.BlockCopy(textcolor, 0, objectData, i, textcolor.Length);
                i += 4;
            }
            if ((updateFlags & CompressedFlags.MediaURL) != 0)
            {
                byte[] text = Util.StringToBytes256(part.CurrentMediaVersion);
                Buffer.BlockCopy(text, 0, objectData, i, text.Length);
                i += text.Length;
            }

            if ((updateFlags & CompressedFlags.HasParticles) != 0)
            {
                if (part.ParticleSystem.Length == 0)
                {
                    Primitive.ParticleSystem Sys = new Primitive.ParticleSystem();
                    byte[] pdata = Sys.GetBytes();
                    Buffer.BlockCopy(pdata, 0, objectData, i, pdata.Length);
                    i += pdata.Length; //86
                    //updateFlags = updateFlags & ~CompressedFlags.HasParticles;
                }
                else
                {
                    Buffer.BlockCopy(part.ParticleSystem, 0, objectData, i, part.ParticleSystem.Length);
                    i += part.ParticleSystem.Length; //86
                }
            }

            byte[] ExtraData = part.Shape.ExtraParamsToBytes();
            Buffer.BlockCopy(ExtraData, 0, objectData, i, ExtraData.Length);
            i += ExtraData.Length;

            if ((updateFlags & CompressedFlags.HasSound) != 0)
            {
                part.Sound.ToBytes(objectData, i);
                i += 16;
                Utils.FloatToBytes((float)part.SoundGain, objectData, i);
                i += 4;
                objectData[i] = part.SoundFlags;
                i++;
                Utils.FloatToBytes((float)part.SoundRadius, objectData, i);
                i += 4;
            }
            if ((updateFlags & CompressedFlags.HasNameValues) != 0)
            {
                if (part.IsAttachment)
                {
                    byte[] NV = Util.StringToBytes256("AttachItemID STRING RW SV " + part.FromItemID);
                    Buffer.BlockCopy(NV, 0, objectData, i, NV.Length);
                    i += NV.Length;
                }
            }

            objectData[i] = part.Shape.PathCurve;
            i++;
            Utils.UInt16ToBytes(part.Shape.PathBegin, objectData, i);
            i += 2;
            Utils.UInt16ToBytes(part.Shape.PathEnd, objectData, i);
            i += 2;
            objectData[i] = part.Shape.PathScaleX;
            i++;
            objectData[i] = part.Shape.PathScaleY;
            i++;
            objectData[i] = part.Shape.PathShearX;
            i++;
            objectData[i] = part.Shape.PathShearY;
            i++;
            objectData[i] = (byte)part.Shape.PathTwist;
            i++;
            objectData[i] = (byte)part.Shape.PathTwistBegin;
            i++;
            objectData[i] = (byte)part.Shape.PathRadiusOffset;
            i++;
            objectData[i] = (byte)part.Shape.PathTaperX;
            i++;
            objectData[i] = (byte)part.Shape.PathTaperY;
            i++;
            objectData[i] = part.Shape.PathRevolutions;
            i++;
            objectData[i] = (byte)part.Shape.PathSkew;
            i++;
            objectData[i] = part.Shape.ProfileCurve;
            i++;

            Utils.UInt16ToBytes(part.Shape.ProfileBegin, objectData, i);
            i += 2;
            Utils.UInt16ToBytes(part.Shape.ProfileEnd, objectData, i);
            i += 2;
            Utils.UInt16ToBytes(part.Shape.ProfileHollow, objectData, i);
            i += 2;

            if (part.Shape.TextureEntry != null && part.Shape.TextureEntry.Length > 0)
            {
                // Texture Length
                Utils.IntToBytes(part.Shape.TextureEntry.Length, objectData, i);
                i += 4;
                // Texture
                Buffer.BlockCopy(part.Shape.TextureEntry, 0, objectData, i, part.Shape.TextureEntry.Length);
                i += part.Shape.TextureEntry.Length;
            }
            else
            {
                Utils.IntToBytes(0, objectData, i);
                i += 4;
            }

            if ((updateFlags & CompressedFlags.TextureAnimation) != 0)
            {
                Utils.UInt64ToBytes((ulong)part.TextureAnimation.Length, objectData, i);
                i += 4;
                Buffer.BlockCopy(part.TextureAnimation, 0, objectData, i, part.TextureAnimation.Length);
                i += part.TextureAnimation.Length;
            }

            ObjectUpdateCompressedPacket.ObjectDataBlock update = new ObjectUpdateCompressedPacket.ObjectDataBlock();
            
            #region PrimFlags

            PrimFlags primflags = (PrimFlags)m_scene.Permissions.GenerateClientFlags(AgentId, part);

            // Don't send the CreateSelected flag to everyone
            primflags &= ~PrimFlags.CreateSelected;

            if (AgentId == part.OwnerID)
            {
                if (part.CreateSelected)
                {
                    // Only send this flag once, then unset it
                    primflags |= PrimFlags.CreateSelected;
                    part.CreateSelected = false;
                }
            }

            update.UpdateFlags = (uint)primflags;

            #endregion PrimFlags

            byte[] PacketObjectData = new byte[i]; //Makes the packet smaller so we can send more!
            Buffer.BlockCopy(objectData, 0, PacketObjectData, 0, i);
            update.Data = PacketObjectData;

            return update;
        }

        public void SendNameReply(UUID profileId, string firstname, string lastname)
        {
            UUIDNameReplyPacket packet = (UUIDNameReplyPacket)PacketPool.Instance.GetPacket(PacketType.UUIDNameReply);
            // TODO: don't create new blocks if recycling an old packet
            packet.UUIDNameBlock = new UUIDNameReplyPacket.UUIDNameBlockBlock[1];
            packet.UUIDNameBlock[0] = new UUIDNameReplyPacket.UUIDNameBlockBlock();
            packet.UUIDNameBlock[0].ID = profileId;
            packet.UUIDNameBlock[0].FirstName = Util.StringToBytes256(firstname);
            packet.UUIDNameBlock[0].LastName = Util.StringToBytes256(lastname);

            OutPacket(packet, ThrottleOutPacketType.Asset);
        }

        /// <summary>
        /// This is a utility method used by single states to not duplicate kicks and blue card of death messages.
        /// </summary>
        public bool ChildAgentStatus()
        {
            ScenePresence Sp = m_scene.GetScenePresence(AgentId);
            if (Sp == null || (Sp.IsChildAgent))
                return true;
            return false;
        }

        #endregion

        /// <summary>
        /// This is a different way of processing packets then ProcessInPacket
        /// </summary>
        protected virtual void RegisterLocalPacketHandlers()
        {
            AddLocalPacketHandler(PacketType.LogoutRequest, HandleLogout);
            AddLocalPacketHandler(PacketType.AgentUpdate, HandleAgentUpdate, false);
            AddLocalPacketHandler(PacketType.ViewerEffect, HandleViewerEffect, true);
            AddLocalPacketHandler(PacketType.AgentCachedTexture, HandleAgentTextureCached, false);
            AddLocalPacketHandler(PacketType.MultipleObjectUpdate, HandleMultipleObjUpdate, false);
            AddLocalPacketHandler(PacketType.MoneyTransferRequest, HandleMoneyTransferRequest, false);
            AddLocalPacketHandler(PacketType.ParcelBuy, HandleParcelBuyRequest, false);
            AddLocalPacketHandler(PacketType.UUIDGroupNameRequest, HandleUUIDGroupNameRequest, false);
            AddLocalPacketHandler(PacketType.ObjectGroup, HandleObjectGroupRequest, false);
            AddLocalPacketHandler(PacketType.GenericMessage, HandleGenericMessage);
            AddLocalPacketHandler(PacketType.AvatarPropertiesRequest, HandleAvatarPropertiesRequest);
            AddLocalPacketHandler(PacketType.ChatFromViewer, HandleChatFromViewer);
            AddLocalPacketHandler(PacketType.AvatarPropertiesUpdate, HandlerAvatarPropertiesUpdate);
            AddLocalPacketHandler(PacketType.ScriptDialogReply, HandlerScriptDialogReply);
            AddLocalPacketHandler(PacketType.ImprovedInstantMessage, HandlerImprovedInstantMessage, false);
            AddLocalPacketHandler(PacketType.AcceptFriendship, HandlerAcceptFriendship);
            AddLocalPacketHandler(PacketType.DeclineFriendship, HandlerDeclineFriendship);
            AddLocalPacketHandler(PacketType.TerminateFriendship, HandlerTerminateFrendship);
            AddLocalPacketHandler(PacketType.RezObject, HandlerRezObject);
            AddLocalPacketHandler(PacketType.RezObjectFromNotecard, HandlerRezObjectFromNotecard);
            AddLocalPacketHandler(PacketType.DeRezObject, HandlerDeRezObject);
            AddLocalPacketHandler(PacketType.ModifyLand, HandlerModifyLand);
            AddLocalPacketHandler(PacketType.RegionHandshakeReply, HandlerRegionHandshakeReply);
            AddLocalPacketHandler(PacketType.AgentWearablesRequest, HandlerAgentWearablesRequest);
            AddLocalPacketHandler(PacketType.AgentSetAppearance, HandlerAgentSetAppearance);
            AddLocalPacketHandler(PacketType.AgentIsNowWearing, HandlerAgentIsNowWearing);
            AddLocalPacketHandler(PacketType.RezSingleAttachmentFromInv, HandlerRezSingleAttachmentFromInv);
            AddLocalPacketHandler(PacketType.RezRestoreToWorld, HandlerRezRestoreToWorld);
            AddLocalPacketHandler(PacketType.RezMultipleAttachmentsFromInv, HandleRezMultipleAttachmentsFromInv);
            AddLocalPacketHandler(PacketType.DetachAttachmentIntoInv, HandleDetachAttachmentIntoInv);
            AddLocalPacketHandler(PacketType.ObjectAttach, HandleObjectAttach);
            AddLocalPacketHandler(PacketType.ObjectDetach, HandleObjectDetach);
            AddLocalPacketHandler(PacketType.ObjectDrop, HandleObjectDrop);
            AddLocalPacketHandler(PacketType.SetAlwaysRun, HandleSetAlwaysRun, false);
            AddLocalPacketHandler(PacketType.CompleteAgentMovement, HandleCompleteAgentMovement);
            AddLocalPacketHandler(PacketType.AgentAnimation, HandleAgentAnimation, false);
            AddLocalPacketHandler(PacketType.AgentRequestSit, HandleAgentRequestSit);
            AddLocalPacketHandler(PacketType.AgentSit, HandleAgentSit);
            AddLocalPacketHandler(PacketType.SoundTrigger, HandleSoundTrigger);
            AddLocalPacketHandler(PacketType.AvatarPickerRequest, HandleAvatarPickerRequest);
            AddLocalPacketHandler(PacketType.AgentDataUpdateRequest, HandleAgentDataUpdateRequest);
            AddLocalPacketHandler(PacketType.UserInfoRequest, HandleUserInfoRequest);
            AddLocalPacketHandler(PacketType.UpdateUserInfo, HandleUpdateUserInfo);
            AddLocalPacketHandler(PacketType.SetStartLocationRequest, HandleSetStartLocationRequest);
            AddLocalPacketHandler(PacketType.AgentThrottle, HandleAgentThrottle, false);
            AddLocalPacketHandler(PacketType.AgentPause, HandleAgentPause, false);
            AddLocalPacketHandler(PacketType.AgentResume, HandleAgentResume, false);
            AddLocalPacketHandler(PacketType.ForceScriptControlRelease, HandleForceScriptControlRelease);
            AddLocalPacketHandler(PacketType.ObjectLink, HandleObjectLink);
            AddLocalPacketHandler(PacketType.ObjectDelink, HandleObjectDelink);
            AddLocalPacketHandler(PacketType.ObjectAdd, HandleObjectAdd);
            AddLocalPacketHandler(PacketType.ObjectShape, HandleObjectShape);
            AddLocalPacketHandler(PacketType.ObjectExtraParams, HandleObjectExtraParams);
            AddLocalPacketHandler(PacketType.ObjectDuplicate, HandleObjectDuplicate);
            AddLocalPacketHandler(PacketType.RequestMultipleObjects, HandleRequestMultipleObjects);
            AddLocalPacketHandler(PacketType.ObjectSelect, HandleObjectSelect);
            AddLocalPacketHandler(PacketType.ObjectDeselect, HandleObjectDeselect);
            AddLocalPacketHandler(PacketType.ObjectPosition, HandleObjectPosition);
            AddLocalPacketHandler(PacketType.ObjectScale, HandleObjectScale);
            AddLocalPacketHandler(PacketType.ObjectRotation, HandleObjectRotation);
            AddLocalPacketHandler(PacketType.ObjectFlagUpdate, HandleObjectFlagUpdate);

            // Handle ObjectImage (TextureEntry) updates synchronously, since when updating multiple prim faces at once,
            // some clients will send out a separate ObjectImage packet for each face
            AddLocalPacketHandler(PacketType.ObjectImage, HandleObjectImage, false);

            AddLocalPacketHandler(PacketType.ObjectGrab, HandleObjectGrab, false);
            AddLocalPacketHandler(PacketType.ObjectGrabUpdate, HandleObjectGrabUpdate, false);
            AddLocalPacketHandler(PacketType.ObjectDeGrab, HandleObjectDeGrab);
            AddLocalPacketHandler(PacketType.ObjectSpinStart, HandleObjectSpinStart, false);
            AddLocalPacketHandler(PacketType.ObjectSpinUpdate, HandleObjectSpinUpdate, false);
            AddLocalPacketHandler(PacketType.ObjectSpinStop, HandleObjectSpinStop, false);
            AddLocalPacketHandler(PacketType.ObjectDescription, HandleObjectDescription, false);
            AddLocalPacketHandler(PacketType.ObjectName, HandleObjectName, false);
            AddLocalPacketHandler(PacketType.ObjectPermissions, HandleObjectPermissions, false);
            AddLocalPacketHandler(PacketType.Undo, HandleUndo, false);
            AddLocalPacketHandler(PacketType.UndoLand, HandleLandUndo, false);
            AddLocalPacketHandler(PacketType.Redo, HandleRedo, false);
            AddLocalPacketHandler(PacketType.ObjectDuplicateOnRay, HandleObjectDuplicateOnRay);
            AddLocalPacketHandler(PacketType.RequestObjectPropertiesFamily, HandleRequestObjectPropertiesFamily, false);
            AddLocalPacketHandler(PacketType.ObjectIncludeInSearch, HandleObjectIncludeInSearch);
            AddLocalPacketHandler(PacketType.ScriptAnswerYes, HandleScriptAnswerYes, false);
            AddLocalPacketHandler(PacketType.ObjectClickAction, HandleObjectClickAction, false);
            AddLocalPacketHandler(PacketType.ObjectMaterial, HandleObjectMaterial, false);
            AddLocalPacketHandler(PacketType.RequestImage, HandleRequestImage);
            AddLocalPacketHandler(PacketType.TransferRequest, HandleTransferRequest);
            AddLocalPacketHandler(PacketType.AssetUploadRequest, HandleAssetUploadRequest);
            AddLocalPacketHandler(PacketType.RequestXfer, HandleRequestXfer);
            AddLocalPacketHandler(PacketType.SendXferPacket, HandleSendXferPacket);
            AddLocalPacketHandler(PacketType.ConfirmXferPacket, HandleConfirmXferPacket);
            AddLocalPacketHandler(PacketType.AbortXfer, HandleAbortXfer);
            AddLocalPacketHandler(PacketType.CreateInventoryFolder, HandleCreateInventoryFolder);
            AddLocalPacketHandler(PacketType.UpdateInventoryFolder, HandleUpdateInventoryFolder);
            AddLocalPacketHandler(PacketType.MoveInventoryFolder, HandleMoveInventoryFolder);
            AddLocalPacketHandler(PacketType.CreateInventoryItem, HandleCreateInventoryItem);
            AddLocalPacketHandler(PacketType.LinkInventoryItem, HandleLinkInventoryItem);
            AddLocalPacketHandler(PacketType.FetchInventory, HandleFetchInventory);
            AddLocalPacketHandler(PacketType.FetchInventoryDescendents, HandleFetchInventoryDescendents);
            AddLocalPacketHandler(PacketType.PurgeInventoryDescendents, HandlePurgeInventoryDescendents);
            AddLocalPacketHandler(PacketType.UpdateInventoryItem, HandleUpdateInventoryItem);
            AddLocalPacketHandler(PacketType.CopyInventoryItem, HandleCopyInventoryItem);
            AddLocalPacketHandler(PacketType.MoveInventoryItem, HandleMoveInventoryItem);
            AddLocalPacketHandler(PacketType.ChangeInventoryItemFlags, HandleChangeInventoryItemFlags);
            AddLocalPacketHandler(PacketType.RemoveInventoryItem, HandleRemoveInventoryItem);
            AddLocalPacketHandler(PacketType.RemoveInventoryFolder, HandleRemoveInventoryFolder);
            AddLocalPacketHandler(PacketType.RemoveInventoryObjects, HandleRemoveInventoryObjects);
            AddLocalPacketHandler(PacketType.RequestTaskInventory, HandleRequestTaskInventory);
            AddLocalPacketHandler(PacketType.UpdateTaskInventory, HandleUpdateTaskInventory);
            AddLocalPacketHandler(PacketType.RemoveTaskInventory, HandleRemoveTaskInventory);
            AddLocalPacketHandler(PacketType.MoveTaskInventory, HandleMoveTaskInventory);
            AddLocalPacketHandler(PacketType.RezScript, HandleRezScript);
            AddLocalPacketHandler(PacketType.MapLayerRequest, HandleMapLayerRequest, false);
            AddLocalPacketHandler(PacketType.MapBlockRequest, HandleMapBlockRequest, false);
            AddLocalPacketHandler(PacketType.MapNameRequest, HandleMapNameRequest, false);
            AddLocalPacketHandler(PacketType.TeleportLandmarkRequest, HandleTeleportLandmarkRequest);
            AddLocalPacketHandler(PacketType.TeleportLocationRequest, HandleTeleportLocationRequest);
            AddLocalPacketHandler(PacketType.UUIDNameRequest, HandleUUIDNameRequest, false);
            AddLocalPacketHandler(PacketType.RegionHandleRequest, HandleRegionHandleRequest);
            AddLocalPacketHandler(PacketType.ParcelInfoRequest, HandleParcelInfoRequest, false);
            AddLocalPacketHandler(PacketType.ParcelAccessListRequest, HandleParcelAccessListRequest, false);
            AddLocalPacketHandler(PacketType.ParcelAccessListUpdate, HandleParcelAccessListUpdate, false);
            AddLocalPacketHandler(PacketType.ParcelPropertiesRequest, HandleParcelPropertiesRequest, false);
            AddLocalPacketHandler(PacketType.ParcelDivide, HandleParcelDivide);
            AddLocalPacketHandler(PacketType.ParcelJoin, HandleParcelJoin);
            AddLocalPacketHandler(PacketType.ParcelPropertiesUpdate, HandleParcelPropertiesUpdate);
            AddLocalPacketHandler(PacketType.ParcelSelectObjects, HandleParcelSelectObjects);
            AddLocalPacketHandler(PacketType.ParcelObjectOwnersRequest, HandleParcelObjectOwnersRequest);
            AddLocalPacketHandler(PacketType.ParcelGodForceOwner, HandleParcelGodForceOwner);
            AddLocalPacketHandler(PacketType.ParcelRelease, HandleParcelRelease);
            AddLocalPacketHandler(PacketType.ParcelReclaim, HandleParcelReclaim);
            AddLocalPacketHandler(PacketType.ParcelReturnObjects, HandleParcelReturnObjects);
            AddLocalPacketHandler(PacketType.ParcelSetOtherCleanTime, HandleParcelSetOtherCleanTime);
            AddLocalPacketHandler(PacketType.LandStatRequest, HandleLandStatRequest);
            AddLocalPacketHandler(PacketType.ParcelDwellRequest, HandleParcelDwellRequest);
            AddLocalPacketHandler(PacketType.EstateOwnerMessage, HandleEstateOwnerMessage);
            AddLocalPacketHandler(PacketType.RequestRegionInfo, HandleRequestRegionInfo, false);
            AddLocalPacketHandler(PacketType.EstateCovenantRequest, HandleEstateCovenantRequest);
            AddLocalPacketHandler(PacketType.RequestGodlikePowers, HandleRequestGodlikePowers);
            AddLocalPacketHandler(PacketType.GodKickUser, HandleGodKickUser);
            AddLocalPacketHandler(PacketType.MoneyBalanceRequest, HandleMoneyBalanceRequest);
            AddLocalPacketHandler(PacketType.EconomyDataRequest, HandleEconomyDataRequest);
            AddLocalPacketHandler(PacketType.RequestPayPrice, HandleRequestPayPrice);
            AddLocalPacketHandler(PacketType.ObjectSaleInfo, HandleObjectSaleInfo);
            AddLocalPacketHandler(PacketType.ObjectBuy, HandleObjectBuy);
            AddLocalPacketHandler(PacketType.GetScriptRunning, HandleGetScriptRunning);
            AddLocalPacketHandler(PacketType.SetScriptRunning, HandleSetScriptRunning);
            AddLocalPacketHandler(PacketType.ScriptReset, HandleScriptReset);
            AddLocalPacketHandler(PacketType.ActivateGestures, HandleActivateGestures);
            AddLocalPacketHandler(PacketType.DeactivateGestures, HandleDeactivateGestures);
            AddLocalPacketHandler(PacketType.ObjectOwner, HandleObjectOwner);
            AddLocalPacketHandler(PacketType.AgentFOV, HandleAgentFOV, false);
            AddLocalPacketHandler(PacketType.ViewerStats, HandleViewerStats);
            AddLocalPacketHandler(PacketType.MapItemRequest, HandleMapItemRequest, false);
            AddLocalPacketHandler(PacketType.TransferAbort, HandleTransferAbort, false);
            AddLocalPacketHandler(PacketType.MuteListRequest, HandleMuteListRequest, true);
            AddLocalPacketHandler(PacketType.UseCircuitCode, HandleUseCircuitCode);
            AddLocalPacketHandler(PacketType.AgentHeightWidth, HandleAgentHeightWidth, false);
            AddLocalPacketHandler(PacketType.InventoryDescendents, HandleInventoryDescendents);
            AddLocalPacketHandler(PacketType.DirPlacesQuery, HandleDirPlacesQuery);
            AddLocalPacketHandler(PacketType.DirFindQuery, HandleDirFindQuery);
            AddLocalPacketHandler(PacketType.DirLandQuery, HandleDirLandQuery);
            AddLocalPacketHandler(PacketType.DirPopularQuery, HandleDirPopularQuery);
            AddLocalPacketHandler(PacketType.DirClassifiedQuery, HandleDirClassifiedQuery);
            AddLocalPacketHandler(PacketType.EventInfoRequest, HandleEventInfoRequest);
            AddLocalPacketHandler(PacketType.OfferCallingCard, HandleOfferCallingCard);
            AddLocalPacketHandler(PacketType.AcceptCallingCard, HandleAcceptCallingCard);
            AddLocalPacketHandler(PacketType.DeclineCallingCard, HandleDeclineCallingCard);
            AddLocalPacketHandler(PacketType.ActivateGroup, HandleActivateGroup);
            AddLocalPacketHandler(PacketType.GroupTitlesRequest, HandleGroupTitlesRequest);
            AddLocalPacketHandler(PacketType.GroupProfileRequest, HandleGroupProfileRequest);
            AddLocalPacketHandler(PacketType.GroupMembersRequest, HandleGroupMembersRequest);
            AddLocalPacketHandler(PacketType.GroupRoleDataRequest, HandleGroupRoleDataRequest);
            AddLocalPacketHandler(PacketType.GroupRoleMembersRequest, HandleGroupRoleMembersRequest);
            AddLocalPacketHandler(PacketType.CreateGroupRequest, HandleCreateGroupRequest);
            AddLocalPacketHandler(PacketType.UpdateGroupInfo, HandleUpdateGroupInfo);
            AddLocalPacketHandler(PacketType.SetGroupAcceptNotices, HandleSetGroupAcceptNotices);
            AddLocalPacketHandler(PacketType.GroupTitleUpdate, HandleGroupTitleUpdate);
            AddLocalPacketHandler(PacketType.ParcelDeedToGroup, HandleParcelDeedToGroup);
            AddLocalPacketHandler(PacketType.GroupNoticesListRequest, HandleGroupNoticesListRequest);
            AddLocalPacketHandler(PacketType.GroupNoticeRequest, HandleGroupNoticeRequest);
            AddLocalPacketHandler(PacketType.GroupRoleUpdate, HandleGroupRoleUpdate);
            AddLocalPacketHandler(PacketType.GroupRoleChanges, HandleGroupRoleChanges);
            AddLocalPacketHandler(PacketType.JoinGroupRequest, HandleJoinGroupRequest);
            AddLocalPacketHandler(PacketType.LeaveGroupRequest, HandleLeaveGroupRequest);
            AddLocalPacketHandler(PacketType.EjectGroupMemberRequest, HandleEjectGroupMemberRequest);
            AddLocalPacketHandler(PacketType.InviteGroupRequest, HandleInviteGroupRequest);
            AddLocalPacketHandler(PacketType.StartLure, HandleStartLure);
            AddLocalPacketHandler(PacketType.TeleportLureRequest, HandleTeleportLureRequest);
            AddLocalPacketHandler(PacketType.ClassifiedInfoRequest, HandleClassifiedInfoRequest);
            AddLocalPacketHandler(PacketType.ClassifiedInfoUpdate, HandleClassifiedInfoUpdate);
            AddLocalPacketHandler(PacketType.ClassifiedDelete, HandleClassifiedDelete);
            AddLocalPacketHandler(PacketType.ClassifiedGodDelete, HandleClassifiedGodDelete);
            AddLocalPacketHandler(PacketType.EventGodDelete, HandleEventGodDelete);
            AddLocalPacketHandler(PacketType.EventNotificationAddRequest, HandleEventNotificationAddRequest);
            AddLocalPacketHandler(PacketType.EventNotificationRemoveRequest, HandleEventNotificationRemoveRequest);
            AddLocalPacketHandler(PacketType.RetrieveInstantMessages, HandleRetrieveInstantMessages);
            AddLocalPacketHandler(PacketType.PickDelete, HandlePickDelete);
            AddLocalPacketHandler(PacketType.PickGodDelete, HandlePickGodDelete);
            AddLocalPacketHandler(PacketType.PickInfoUpdate, HandlePickInfoUpdate);
            AddLocalPacketHandler(PacketType.AvatarNotesUpdate, HandleAvatarNotesUpdate);
            AddLocalPacketHandler(PacketType.AvatarInterestsUpdate, HandleAvatarInterestsUpdate);
            AddLocalPacketHandler(PacketType.GrantUserRights, HandleGrantUserRights);
            AddLocalPacketHandler(PacketType.PlacesQuery, HandlePlacesQuery);
            AddLocalPacketHandler(PacketType.UpdateMuteListEntry, HandleUpdateMuteListEntry);
            AddLocalPacketHandler(PacketType.RemoveMuteListEntry, HandleRemoveMuteListEntry);
            AddLocalPacketHandler(PacketType.UserReport, HandleUserReport);
            AddLocalPacketHandler(PacketType.FindAgent, HandleFindAgent);
            AddLocalPacketHandler(PacketType.TrackAgent, HandleTrackAgent);
            AddLocalPacketHandler(PacketType.GodUpdateRegionInfo, HandleGodUpdateRegionInfoUpdate);
            AddLocalPacketHandler(PacketType.GodlikeMessage, HandleGodlikeMessage);
            AddLocalPacketHandler(PacketType.StateSave, HandleSaveStatePacket);
            AddLocalPacketHandler(PacketType.GroupAccountDetailsRequest, HandleGroupAccountDetailsRequest);
            AddLocalPacketHandler(PacketType.GroupAccountSummaryRequest, HandleGroupAccountSummaryRequest);
            AddLocalPacketHandler(PacketType.GroupAccountTransactionsRequest, HandleGroupTransactionsDetailsRequest);
            AddLocalPacketHandler(PacketType.FreezeUser, HandleFreezeUser);
            AddLocalPacketHandler(PacketType.EjectUser, HandleEjectUser);
            AddLocalPacketHandler(PacketType.ParcelBuyPass, HandleParcelBuyPass);
            AddLocalPacketHandler(PacketType.ParcelGodMarkAsContent, HandleParcelGodMarkAsContent);
            AddLocalPacketHandler(PacketType.GroupActiveProposalsRequest, HandleGroupActiveProposalsRequest);
            AddLocalPacketHandler(PacketType.GroupVoteHistoryRequest, HandleGroupVoteHistoryRequest);
            AddLocalPacketHandler(PacketType.GroupProposalBallot, HandleGroupProposalBallot);
            AddLocalPacketHandler(PacketType.SimWideDeletes, HandleSimWideDeletes);
            AddLocalPacketHandler(PacketType.SendPostcard, HandleSendPostcard);
            AddLocalPacketHandler(PacketType.TeleportCancel, HandleTeleportCancel);
            AddLocalPacketHandler(PacketType.ViewerStartAuction, HandleViewerStartAuction);
            AddLocalPacketHandler(PacketType.ParcelDisableObjects, HandleParcelDisableObjects);
        }

        #region Packet Handlers

        #region Scene/Avatar

        private bool HandleAgentUpdate(IClientAPI sener, Packet Pack)
        {
            if (OnAgentUpdate != null)
            {
                bool update = false;
                AgentUpdatePacket agenUpdate = (AgentUpdatePacket)Pack;

                #region Packet Session and User Check
                if (agenUpdate.AgentData.SessionID != SessionId || agenUpdate.AgentData.AgentID != AgentId)
                    return false;
                #endregion

                AgentUpdatePacket.AgentDataBlock x = agenUpdate.AgentData;

                // We can only check when we have something to check
                // against.

                if (lastarg != null)
                {
                    update =
                       (
                        (x.BodyRotation != lastarg.BodyRotation) ||
                        (x.CameraAtAxis != lastarg.CameraAtAxis) ||
                        (x.CameraCenter != lastarg.CameraCenter) ||
                        (x.CameraLeftAxis != lastarg.CameraLeftAxis) ||
                        (x.CameraUpAxis != lastarg.CameraUpAxis) ||
                        (x.ControlFlags != lastarg.ControlFlags) ||
                        (x.Far != lastarg.Far) ||
                        (x.Flags != lastarg.Flags) ||
                        (x.State != lastarg.State) ||
                        (x.HeadRotation != lastarg.HeadRotation) ||
                        (x.SessionID != lastarg.SessionID) ||
                        (x.AgentID != lastarg.AgentID)
                       );
                }
                else
                    update = true;

                // These should be ordered from most-likely to
                // least likely to change. I've made an initial
                // guess at that.

                if (update)
                {
                    AgentUpdateArgs arg = new AgentUpdateArgs();
                    arg.AgentID = x.AgentID;
                    arg.BodyRotation = x.BodyRotation;
                    arg.CameraAtAxis = x.CameraAtAxis;
                    arg.CameraCenter = x.CameraCenter;
                    arg.CameraLeftAxis = x.CameraLeftAxis;
                    arg.CameraUpAxis = x.CameraUpAxis;
                    arg.ControlFlags = x.ControlFlags;
                    arg.Far = x.Far;
                    arg.Flags = x.Flags;
                    arg.HeadRotation = x.HeadRotation;
                    arg.SessionID = x.SessionID;
                    arg.State = x.State;
                    UpdateAgent handlerAgentUpdate = OnAgentUpdate;
                    lastarg = arg; // save this set of arguments for nexttime
                    if (handlerAgentUpdate != null)
                        OnAgentUpdate(this, arg);

                    handlerAgentUpdate = null;
                }
            }

            return true;
        }

        private bool HandleMoneyTransferRequest(IClientAPI sender, Packet Pack)
        {
            MoneyTransferRequestPacket money = (MoneyTransferRequestPacket)Pack;
            // validate the agent owns the agentID and sessionID
            if (money.MoneyData.SourceID == sender.AgentId && money.AgentData.AgentID == sender.AgentId &&
                money.AgentData.SessionID == sender.SessionId)
            {
                MoneyTransferRequest handlerMoneyTransferRequest = OnMoneyTransferRequest;
                if (handlerMoneyTransferRequest != null)
                {
                    handlerMoneyTransferRequest(money.MoneyData.SourceID, money.MoneyData.DestID,
                                                money.MoneyData.Amount, money.MoneyData.TransactionType,
                                                Util.FieldToString(money.MoneyData.Description));
                }

                return true;
            }

            return false;
        }

        private bool HandleParcelGodMarkAsContent(IClientAPI client, Packet Packet)
        {
            ParcelGodMarkAsContentPacket ParcelGodMarkAsContent =
                (ParcelGodMarkAsContentPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (ParcelGodMarkAsContent.AgentData.SessionID != SessionId ||
                    ParcelGodMarkAsContent.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelGodMark ParcelGodMarkAsContentHandler = OnParcelGodMark;
            if (ParcelGodMarkAsContentHandler != null)
            {
                ParcelGodMarkAsContentHandler(this,
                                 ParcelGodMarkAsContent.AgentData.AgentID,
                                 ParcelGodMarkAsContent.ParcelData.LocalID);
                return true;
            }
            return false;
        }

        private bool HandleFreezeUser(IClientAPI client, Packet Packet)
        {
            FreezeUserPacket FreezeUser = (FreezeUserPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (FreezeUser.AgentData.SessionID != SessionId ||
                    FreezeUser.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            FreezeUserUpdate FreezeUserHandler = OnParcelFreezeUser;
            if (FreezeUserHandler != null)
            {
                FreezeUserHandler(this,
                                  FreezeUser.AgentData.AgentID,
                                  FreezeUser.Data.Flags,
                                  FreezeUser.Data.TargetID);
                return true;
            }
            return false;
        }

        private bool HandleEjectUser(IClientAPI client, Packet Packet)
        {
            EjectUserPacket EjectUser =
                (EjectUserPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (EjectUser.AgentData.SessionID != SessionId ||
                    EjectUser.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            EjectUserUpdate EjectUserHandler = OnParcelEjectUser;
            if (EjectUserHandler != null)
            {
                EjectUserHandler(this,
                                 EjectUser.AgentData.AgentID,
                                 EjectUser.Data.Flags,
                                 EjectUser.Data.TargetID);
                return true;
            }
            return false;
        }

        private bool HandleParcelBuyPass(IClientAPI client, Packet Packet)
        {
            ParcelBuyPassPacket ParcelBuyPass =
                (ParcelBuyPassPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (ParcelBuyPass.AgentData.SessionID != SessionId ||
                    ParcelBuyPass.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            
            ParcelBuyPass ParcelBuyPassHandler = OnParcelBuyPass;
            if (ParcelBuyPassHandler != null)
            {
                ParcelBuyPassHandler(this,
                                 ParcelBuyPass.AgentData.AgentID,
                                 ParcelBuyPass.ParcelData.LocalID);
                return true;
            }
            return false;
        }

        private bool HandleParcelBuyRequest(IClientAPI sender, Packet Pack)
        {
            ParcelBuyPacket parcel = (ParcelBuyPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (parcel.AgentData.SessionID != SessionId ||
                    parcel.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            if (parcel.AgentData.AgentID == AgentId && parcel.AgentData.SessionID == SessionId)
            {
                ParcelBuy handlerParcelBuy = OnParcelBuy;
                if (handlerParcelBuy != null)
                {
                    handlerParcelBuy(parcel.AgentData.AgentID, parcel.Data.GroupID, parcel.Data.Final,
                                     parcel.Data.IsGroupOwned,
                                     parcel.Data.RemoveContribution, parcel.Data.LocalID, parcel.ParcelData.Area,
                                     parcel.ParcelData.Price,
                                     false);
                }
                return true;
            }
            return false;
        }

        private bool HandleUUIDGroupNameRequest(IClientAPI sender, Packet Pack)
        {
            UUIDGroupNameRequestPacket upack = (UUIDGroupNameRequestPacket)Pack;

            for (int i = 0; i < upack.UUIDNameBlock.Length; i++)
            {
                UUIDNameRequest handlerUUIDGroupNameRequest = OnUUIDGroupNameRequest;
                if (handlerUUIDGroupNameRequest != null)
                {
                    handlerUUIDGroupNameRequest(upack.UUIDNameBlock[i].ID, this);
                }
            }

            return true;
        }

        public bool HandleGenericMessage(IClientAPI sender, Packet pack)
        {
            GenericMessagePacket gmpack = (GenericMessagePacket)pack;
            if (m_genericPacketHandlers.Count == 0) return false;
            if (gmpack.AgentData.SessionID != SessionId) return false;

            GenericMessage handlerGenericMessage = null;

            string method = Util.FieldToString(gmpack.MethodData.Method).ToLower().Trim();

            if (m_genericPacketHandlers.TryGetValue(method, out handlerGenericMessage))
            {
                List<string> msg = new List<string>();
                List<byte[]> msgBytes = new List<byte[]>();

                if (handlerGenericMessage != null)
                {
                    foreach (GenericMessagePacket.ParamListBlock block in gmpack.ParamList)
                    {
                        msg.Add(Util.FieldToString(block.Parameter));
                        msgBytes.Add(block.Parameter);
                    }
                    try
                    {
                        if (OnBinaryGenericMessage != null)
                        {
                            OnBinaryGenericMessage(this, method, msgBytes.ToArray());
                        }
                        handlerGenericMessage(sender, method, msg);
                        return true;
                    }
                    catch (Exception e)
                    {
                        m_log.ErrorFormat(
                            "[LLCLIENTVIEW]: Exeception when handling generic message {0}{1}", e.Message, e.StackTrace);
                    }
                }
            }
            
            //m_log.Debug("[LLCLIENTVIEW]: Not handling GenericMessage with method-type of: " + method);
            return false;
        }

        public bool HandleObjectGroupRequest(IClientAPI sender, Packet Pack)
        {
            ObjectGroupPacket ogpack = (ObjectGroupPacket)Pack;
            if (ogpack.AgentData.SessionID != SessionId) return false;

            RequestObjectPropertiesFamily handlerObjectGroupRequest = OnObjectGroupRequest;
            if (handlerObjectGroupRequest != null)
            {
                for (int i = 0; i < ogpack.ObjectData.Length; i++)
                {
                    handlerObjectGroupRequest(this, ogpack.AgentData.GroupID, ogpack.ObjectData[i].ObjectLocalID, UUID.Zero);
                }
            }
            return true;
        }

        private bool HandleViewerEffect(IClientAPI sender, Packet Pack)
        {
            ViewerEffectPacket viewer = (ViewerEffectPacket)Pack;
            if (viewer.AgentData.SessionID != SessionId) return false;
            ViewerEffectEventHandler handlerViewerEffect = OnViewerEffect;
            if (handlerViewerEffect != null)
            {
                int length = viewer.Effect.Length;
                List<ViewerEffectEventHandlerArg> args = new List<ViewerEffectEventHandlerArg>(length);
                for (int i = 0; i < length; i++)
                {
                    //copy the effects block arguments into the event handler arg.
                    ViewerEffectEventHandlerArg argument = new ViewerEffectEventHandlerArg();
                    argument.AgentID = viewer.Effect[i].AgentID;
                    argument.Color = viewer.Effect[i].Color;
                    argument.Duration = viewer.Effect[i].Duration;
                    argument.ID = viewer.Effect[i].ID;
                    argument.Type = viewer.Effect[i].Type;
                    argument.TypeData = viewer.Effect[i].TypeData;
                    args.Add(argument);
                }

                handlerViewerEffect(sender, args);
            }

            return true;
        }

        private bool HandleAvatarPropertiesRequest(IClientAPI sender, Packet Pack)
        {
            AvatarPropertiesRequestPacket avatarProperties = (AvatarPropertiesRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avatarProperties.AgentData.SessionID != SessionId ||
                    avatarProperties.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RequestAvatarProperties handlerRequestAvatarProperties = OnRequestAvatarProperties;
            if (handlerRequestAvatarProperties != null)
            {
                handlerRequestAvatarProperties(this, avatarProperties.AgentData.AvatarID);
            }
            return true;
        }

        private bool HandleChatFromViewer(IClientAPI sender, Packet Pack)
        {
            ChatFromViewerPacket inchatpack = (ChatFromViewerPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (inchatpack.AgentData.SessionID != SessionId ||
                    inchatpack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            string fromName = String.Empty; //ClientAvatar.firstname + " " + ClientAvatar.lastname;
            byte[] message = inchatpack.ChatData.Message;
            byte type = inchatpack.ChatData.Type;
            Vector3 fromPos = new Vector3(); // ClientAvatar.Pos;
            // UUID fromAgentID = AgentId;

            int channel = inchatpack.ChatData.Channel;

            if (OnChatFromClient != null)
            {
                OSChatMessage args = new OSChatMessage();
                args.Channel = channel;
                args.From = fromName;
                args.Message = Utils.BytesToString(message);
                args.Type = (ChatTypeEnum)type;
                args.Position = fromPos;

                args.Scene = Scene;
                args.Sender = this;
                args.SenderUUID = this.AgentId;

                ChatMessage handlerChatFromClient = OnChatFromClient;
                if (handlerChatFromClient != null)
                    handlerChatFromClient(this, args);
            }
            return true;
        }

        private bool HandlerAvatarPropertiesUpdate(IClientAPI sender, Packet Pack)
        {
            AvatarPropertiesUpdatePacket avatarProps = (AvatarPropertiesUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avatarProps.AgentData.SessionID != SessionId ||
                    avatarProps.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            UpdateAvatarProperties handlerUpdateAvatarProperties = OnUpdateAvatarProperties;
            if (handlerUpdateAvatarProperties != null)
            {
                AvatarPropertiesUpdatePacket.PropertiesDataBlock Properties = avatarProps.PropertiesData;
                
                handlerUpdateAvatarProperties(this,
                    Utils.BytesToString(Properties.AboutText),
                    Utils.BytesToString(Properties.FLAboutText),
                    Properties.FLImageID,
                    Properties.ImageID,
                    Utils.BytesToString(Properties.ProfileURL),
                    Properties.AllowPublish,
                    Properties.MaturePublish);
            }
            return true;
        }

        private bool HandlerScriptDialogReply(IClientAPI sender, Packet Pack)
        {
            ScriptDialogReplyPacket rdialog = (ScriptDialogReplyPacket)Pack;

            //m_log.DebugFormat("[CLIENT]: Received ScriptDialogReply from {0}", rdialog.Data.ObjectID);

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rdialog.AgentData.SessionID != SessionId ||
                    rdialog.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            int ch = rdialog.Data.ChatChannel;
            byte[] msg = rdialog.Data.ButtonLabel;
            if (OnChatFromClient != null)
            {
                OSChatMessage args = new OSChatMessage();
                args.Channel = ch;
                args.From = String.Empty;
                args.Message = Utils.BytesToString(msg);
                args.Type = ChatTypeEnum.Shout;
                args.Position = new Vector3();
                args.Scene = Scene;
                args.Sender = this;
                ChatMessage handlerChatFromClient2 = OnChatFromClient;
                if (handlerChatFromClient2 != null)
                    handlerChatFromClient2(this, args);
            }

            return true;
        }

        private bool HandlerImprovedInstantMessage(IClientAPI sender, Packet Pack)
        {
            ImprovedInstantMessagePacket msgpack = (ImprovedInstantMessagePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (msgpack.AgentData.SessionID != SessionId ||
                    msgpack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            string IMfromName = Util.FieldToString(msgpack.MessageBlock.FromAgentName);
            string IMmessage = Utils.BytesToString(msgpack.MessageBlock.Message);
            ImprovedInstantMessage handlerInstantMessage = OnInstantMessage;

            if (handlerInstantMessage != null)
            {
                GridInstantMessage im = new GridInstantMessage(Scene,
                        msgpack.AgentData.AgentID,
                        IMfromName,
                        msgpack.MessageBlock.ToAgentID,
                        msgpack.MessageBlock.Dialog,
                        msgpack.MessageBlock.FromGroup,
                        IMmessage,
                        msgpack.MessageBlock.ID,
                        msgpack.MessageBlock.Offline != 0 ? true : false,
                        msgpack.MessageBlock.Position,
                        msgpack.MessageBlock.BinaryBucket);

                handlerInstantMessage(this, im);
            }
            return true;

        }

        private bool HandlerAcceptFriendship(IClientAPI sender, Packet Pack)
        {
            AcceptFriendshipPacket afriendpack = (AcceptFriendshipPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (afriendpack.AgentData.SessionID != SessionId ||
                    afriendpack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            // My guess is this is the folder to stick the calling card into
            List<UUID> callingCardFolders = new List<UUID>();

            UUID agentID = afriendpack.AgentData.AgentID;
            UUID transactionID = afriendpack.TransactionBlock.TransactionID;

            for (int fi = 0; fi < afriendpack.FolderData.Length; fi++)
            {
                callingCardFolders.Add(afriendpack.FolderData[fi].FolderID);
            }

            FriendActionDelegate handlerApproveFriendRequest = OnApproveFriendRequest;
            if (handlerApproveFriendRequest != null)
            {
                handlerApproveFriendRequest(this, agentID, transactionID, callingCardFolders);
            }
            return true;

        }

        private bool HandlerDeclineFriendship(IClientAPI sender, Packet Pack)
        {
            DeclineFriendshipPacket dfriendpack = (DeclineFriendshipPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dfriendpack.AgentData.SessionID != SessionId ||
                    dfriendpack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (OnDenyFriendRequest != null)
            {
                OnDenyFriendRequest(this,
                                    dfriendpack.AgentData.AgentID,
                                    dfriendpack.TransactionBlock.TransactionID,
                                    null);
            }
            return true;
        }

        private bool HandlerTerminateFrendship(IClientAPI sender, Packet Pack)
        {
            TerminateFriendshipPacket tfriendpack = (TerminateFriendshipPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (tfriendpack.AgentData.SessionID != SessionId ||
                    tfriendpack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            UUID listOwnerAgentID = tfriendpack.AgentData.AgentID;
            UUID exFriendID = tfriendpack.ExBlock.OtherID;

            FriendshipTermination handlerTerminateFriendship = OnTerminateFriendship;
            if (handlerTerminateFriendship != null)
            {
                handlerTerminateFriendship(this, listOwnerAgentID, exFriendID);
            }
            return true;
        }

        private bool HandleFindAgent(IClientAPI client, Packet Packet)
        {
            FindAgentPacket FindAgent =
                (FindAgentPacket)Packet;

            FindAgentUpdate FindAgentHandler = OnFindAgent;
            if (FindAgentHandler != null)
            {
                FindAgentHandler(this,FindAgent.AgentBlock.Hunter,FindAgent.AgentBlock.Prey);
                return true;
            }
            return false;
        }

        private bool HandleTrackAgent(IClientAPI client, Packet Packet)
        {
            TrackAgentPacket TrackAgent =
                (TrackAgentPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (TrackAgent.AgentData.SessionID != SessionId ||
                    TrackAgent.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            
            TrackAgentUpdate TrackAgentHandler = OnTrackAgent;
            if (TrackAgentHandler != null)
            {
                TrackAgentHandler(this,
                                  TrackAgent.AgentData.AgentID,
                                  TrackAgent.TargetData.PreyID);
                return true;
            }
            return false;
        }

        private bool HandlerRezObject(IClientAPI sender, Packet Pack)
        {
            RezObjectPacket rezPacket = (RezObjectPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rezPacket.AgentData.SessionID != SessionId ||
                    rezPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RezObject handlerRezObject = OnRezObject;
            if (handlerRezObject != null)
            {
                handlerRezObject(this, rezPacket.InventoryData.ItemID, rezPacket.RezData.RayEnd,
                                 rezPacket.RezData.RayStart, rezPacket.RezData.RayTargetID,
                                 rezPacket.RezData.BypassRaycast, rezPacket.RezData.RayEndIsIntersection,
                                 rezPacket.RezData.RezSelected, rezPacket.RezData.RemoveItem,
                                 rezPacket.RezData.FromTaskID);
            }
            return true;
        }

        private bool HandlerRezObjectFromNotecard(IClientAPI sender, Packet Pack)
        {
            RezObjectFromNotecardPacket rezPacket = (RezObjectFromNotecardPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rezPacket.AgentData.SessionID != SessionId ||
                    rezPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RezObject handlerRezObject = OnRezObject;
            if (handlerRezObject != null)
            {
                handlerRezObject(this, rezPacket.InventoryData[0].ItemID, rezPacket.RezData.RayEnd,
                                 rezPacket.RezData.RayStart, rezPacket.RezData.RayTargetID,
                                 rezPacket.RezData.BypassRaycast, rezPacket.RezData.RayEndIsIntersection,
                                 rezPacket.RezData.RezSelected, rezPacket.RezData.RemoveItem,
                                 rezPacket.RezData.FromTaskID);
            }
            return true;
        }

        private bool HandlerDeRezObject(IClientAPI sender, Packet Pack)
        {
            DeRezObjectPacket DeRezPacket = (DeRezObjectPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (DeRezPacket.AgentData.SessionID != SessionId ||
                    DeRezPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DeRezObject handlerDeRezObject = OnDeRezObject;
            if (handlerDeRezObject != null)
            {
                List<uint> deRezIDs = new List<uint>();

                foreach (DeRezObjectPacket.ObjectDataBlock data in
                    DeRezPacket.ObjectData)
                {
                    deRezIDs.Add(data.ObjectLocalID);
                }
                // It just so happens that the values on the DeRezAction enumerator match the Destination
                // values given by a Second Life client
                handlerDeRezObject(this, deRezIDs,
                                   DeRezPacket.AgentBlock.GroupID,
                                   (DeRezAction)DeRezPacket.AgentBlock.Destination,
                                   DeRezPacket.AgentBlock.DestinationID);

            }
            return true;
        }

        private bool HandlerModifyLand(IClientAPI sender, Packet Pack)
        {
            ModifyLandPacket modify = (ModifyLandPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (modify.AgentData.SessionID != SessionId ||
                    modify.AgentData.AgentID != AgentId)
                    return true;
            }

            #endregion
            //m_log.Info("[LAND]: LAND:" + modify.ToString());
            if (modify.ParcelData.Length > 0)
            {
                if (OnModifyTerrain != null)
                {
                    for (int i = 0; i < modify.ParcelData.Length; i++)
                    {
                        ModifyTerrain handlerModifyTerrain = OnModifyTerrain;
                        if (handlerModifyTerrain != null)
                        {
                            handlerModifyTerrain(AgentId, modify.ModifyBlock.Height, modify.ModifyBlock.Seconds,
                                                 modify.ModifyBlock.BrushSize,
                                                 modify.ModifyBlock.Action, modify.ParcelData[i].North,
                                                 modify.ParcelData[i].West, modify.ParcelData[i].South,
                                                 modify.ParcelData[i].East, AgentId,
                                                 modify.ModifyBlockExtended[i].BrushSize);
                        }
                    }
                }
            }

            return true;
        }

        private bool HandlerRegionHandshakeReply(IClientAPI sender, Packet Pack)
        {
            Action<IClientAPI> handlerRegionHandShakeReply = OnRegionHandShakeReply;
            if (handlerRegionHandShakeReply != null)
            {
                handlerRegionHandShakeReply(this);
            }

            return true;
        }

        private bool HandlerAgentWearablesRequest(IClientAPI sender, Packet Pack)
        {
            GenericCall1 handlerRequestWearables = OnRequestWearables;

            if (handlerRequestWearables != null)
            {
                handlerRequestWearables(this);
            }

            Action<IClientAPI> handlerRequestAvatarsData = OnRequestAvatarsData;

            if (handlerRequestAvatarsData != null)
            {
                handlerRequestAvatarsData(this);
            }

            return true;
        }

        private bool HandlerAgentSetAppearance(IClientAPI sender, Packet Pack)
        {
            AgentSetAppearancePacket appear = (AgentSetAppearancePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (appear.AgentData.SessionID != SessionId ||
                    appear.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SetAppearance handlerSetAppearance = OnSetAppearance;
            if (handlerSetAppearance != null)
            {
                // Temporarily protect ourselves from the mantis #951 failure.
                // However, we could do this for several other handlers where a failure isn't terminal
                // for the client session anyway, in order to protect ourselves against bad code in plugins
                try
                {

                    byte[] visualparams = new byte[appear.VisualParam.Length];
                    for (int i = 0; i < appear.VisualParam.Length; i++)
                        visualparams[i] = appear.VisualParam[i].ParamValue;

                    Primitive.TextureEntry te = null;
                    if (appear.ObjectData.TextureEntry.Length > 1)
                        te = new Primitive.TextureEntry(appear.ObjectData.TextureEntry, 0, appear.ObjectData.TextureEntry.Length);

                    WearableCache[] items = new WearableCache[appear.WearableData.Length];
                    for(int i = 0; i < appear.WearableData.Length; i++)
                    {
                        WearableCache cache = new WearableCache();
                        cache.CacheID = appear.WearableData[i].CacheID;
                        cache.TextureIndex = appear.WearableData[i].TextureIndex;
                        items[i] = cache;
                    }
                    handlerSetAppearance(this, te, visualparams, items);
                }
                catch (Exception e)
                {
                    m_log.ErrorFormat(
                        "[CLIENT VIEW]: AgentSetApperance packet handler threw an exception, {0}",
                        e);
                }
            }

            return true;
        }

        /// <summary>
        /// Send a response back to a client when it asks the asset server (via the region server) if it has
        /// its appearance texture cached.
        ///
        /// At the moment, we always reply that there is no cached texture.
        /// </summary>
        /// <param name="simclient"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        protected bool HandleAgentTextureCached(IClientAPI simclient, Packet packet)
        {
            //m_log.Debug("texture cached: " + packet.ToString());
            AgentCachedTexturePacket cachedtex = (AgentCachedTexturePacket)packet;

            if (cachedtex.AgentData.SessionID != SessionId) return false;

            List<CachedAgentArgs> args = new List<CachedAgentArgs>();

            for (int i = 0; i < cachedtex.WearableData.Length; i++)
            {
                args.Add(new CachedAgentArgs() { ID = cachedtex.WearableData[i].ID, TextureIndex = cachedtex.WearableData[i].TextureIndex });
            }

            AgentCachedTextureRequest actr = OnAgentCachedTextureRequest;
            if (actr != null)
                actr(this, args);

            return true;
        }

        public void SendAgentCachedTexture(List<CachedAgentArgs> args)
        {
            AgentCachedTextureResponsePacket cachedresp = (AgentCachedTextureResponsePacket)PacketPool.Instance.GetPacket(PacketType.AgentCachedTextureResponse);
            cachedresp.AgentData.AgentID = AgentId;
            cachedresp.AgentData.SessionID = m_sessionId;
            cachedresp.AgentData.SerialNum = m_cachedTextureSerial;
            m_cachedTextureSerial++;
            cachedresp.WearableData =
                new AgentCachedTextureResponsePacket.WearableDataBlock[args.Count];
            for (int i = 0; i < args.Count; i++)
            {
                cachedresp.WearableData[i] = new AgentCachedTextureResponsePacket.WearableDataBlock();
                cachedresp.WearableData[i].TextureIndex = args[i].TextureIndex;
                cachedresp.WearableData[i].TextureID = args[i].ID;
                cachedresp.WearableData[i].HostName = new byte[0];
            }

            cachedresp.Header.Zerocoded = true;
            OutPacket(cachedresp, ThrottleOutPacketType.Texture);
        }

        private bool HandlerAgentIsNowWearing(IClientAPI sender, Packet Pack)
        {
            if (OnAvatarNowWearing != null)
            {
                AgentIsNowWearingPacket nowWearing = (AgentIsNowWearingPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (nowWearing.AgentData.SessionID != SessionId ||
                        nowWearing.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                AvatarWearingArgs wearingArgs = new AvatarWearingArgs();
                for (int i = 0; i < nowWearing.WearableData.Length; i++)
                {
                    AvatarWearingArgs.Wearable wearable =
                        new AvatarWearingArgs.Wearable(nowWearing.WearableData[i].ItemID,
                                                       nowWearing.WearableData[i].WearableType);
                    wearingArgs.NowWearing.Add(wearable);
                }

                AvatarNowWearing handlerAvatarNowWearing = OnAvatarNowWearing;
                if (handlerAvatarNowWearing != null)
                {
                    handlerAvatarNowWearing(this, wearingArgs);
                }
            }
            return true;
        }

        private bool HandlerRezSingleAttachmentFromInv(IClientAPI sender, Packet Pack)
        {
            RezSingleAttachmentFromInv handlerRezSingleAttachment = OnRezSingleAttachmentFromInv;
            if (handlerRezSingleAttachment != null)
            {
                RezSingleAttachmentFromInvPacket rez = (RezSingleAttachmentFromInvPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (rez.AgentData.SessionID != SessionId ||
                        rez.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                handlerRezSingleAttachment(this, rez.ObjectData.ItemID,
                                           rez.ObjectData.AttachmentPt);
            }

            return true;
        }

        private bool HandlerRezRestoreToWorld(IClientAPI sender, Packet Pack)
        {
            RezSingleAttachmentFromInv handlerRezSingleAttachment = OnRezSingleAttachmentFromInv;
            if (handlerRezSingleAttachment != null)
            {
                RezRestoreToWorldPacket rez = (RezRestoreToWorldPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (rez.AgentData.SessionID != SessionId ||
                        rez.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                handlerRezSingleAttachment(this, rez.InventoryData.ItemID,
                                           0);
            }

            return true;
        }

        private bool HandleRezMultipleAttachmentsFromInv(IClientAPI sender, Packet Pack)
        {
            RezMultipleAttachmentsFromInv handlerRezMultipleAttachments = OnRezMultipleAttachmentsFromInv;

            if (handlerRezMultipleAttachments != null)
            {
                RezMultipleAttachmentsFromInvPacket rez = (RezMultipleAttachmentsFromInvPacket)Pack;
                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (rez.AgentData.SessionID != SessionId ||
                        rez.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion
                handlerRezMultipleAttachments(this, rez.HeaderData,
                                              rez.ObjectData);
            }

            return true;
        }

        private bool HandleDetachAttachmentIntoInv(IClientAPI sender, Packet Pack)
        {
            UUIDNameRequest handlerDetachAttachmentIntoInv = OnDetachAttachmentIntoInv;
            if (handlerDetachAttachmentIntoInv != null)
            {
                DetachAttachmentIntoInvPacket detachtoInv = (DetachAttachmentIntoInvPacket)Pack;

                #region Packet Session and User Check
                // UNSUPPORTED ON THIS PACKET
                #endregion

                UUID itemID = detachtoInv.ObjectData.ItemID;
                // UUID ATTACH_agentID = detachtoInv.ObjectData.AgentID;

                handlerDetachAttachmentIntoInv(itemID, this);
            }
            return true;
        }

        private bool HandleObjectAttach(IClientAPI sender, Packet Pack)
        {
            if (OnObjectAttach != null)
            {
                ObjectAttachPacket att = (ObjectAttachPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (att.AgentData.SessionID != SessionId ||
                        att.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                ObjectAttach handlerObjectAttach = OnObjectAttach;

                if (handlerObjectAttach != null)
                {
                    if (att.ObjectData.Length > 0)
                    {
                        handlerObjectAttach(this, att.ObjectData[0].ObjectLocalID, att.AgentData.AttachmentPoint, false);
                    }
                }
            }
            return true;
        }

        private bool HandleObjectDetach(IClientAPI sender, Packet Pack)
        {
            ObjectDetachPacket dett = (ObjectDetachPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dett.AgentData.SessionID != SessionId ||
                    dett.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            for (int j = 0; j < dett.ObjectData.Length; j++)
            {
                uint obj = dett.ObjectData[j].ObjectLocalID;
                ObjectDeselect handlerObjectDetach = OnObjectDetach;
                if (handlerObjectDetach != null)
                {
                    handlerObjectDetach(obj, this);
                }

            }
            return true;
        }

        private bool HandleObjectDrop(IClientAPI sender, Packet Pack)
        {
            ObjectDropPacket dropp = (ObjectDropPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dropp.AgentData.SessionID != SessionId ||
                    dropp.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            for (int j = 0; j < dropp.ObjectData.Length; j++)
            {
                uint obj = dropp.ObjectData[j].ObjectLocalID;
                ObjectDrop handlerObjectDrop = OnObjectDrop;
                if (handlerObjectDrop != null)
                {
                    handlerObjectDrop(obj, this);
                }
            }
            return true;
        }

        private bool HandleSetAlwaysRun(IClientAPI sender, Packet Pack)
        {
            SetAlwaysRunPacket run = (SetAlwaysRunPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (run.AgentData.SessionID != SessionId ||
                    run.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SetAlwaysRun handlerSetAlwaysRun = OnSetAlwaysRun;
            if (handlerSetAlwaysRun != null)
                handlerSetAlwaysRun(this, run.AgentData.AlwaysRun);

            return true;
        }

        private bool HandleCompleteAgentMovement(IClientAPI sender, Packet Pack)
        {
            GenericCall1 handlerCompleteMovementToRegion = OnCompleteMovementToRegion;
            if (handlerCompleteMovementToRegion != null)
            {
                handlerCompleteMovementToRegion(sender);
            }
            handlerCompleteMovementToRegion = null;

            return true;
        }

        private bool HandleAgentAnimation(IClientAPI sender, Packet Pack)
        {
            AgentAnimationPacket AgentAni = (AgentAnimationPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (AgentAni.AgentData.SessionID != SessionId ||
                    AgentAni.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            StartAnim handlerStartAnim = null;
            StopAnim handlerStopAnim = null;

            for (int i = 0; i < AgentAni.AnimationList.Length; i++)
            {
                if (AgentAni.AnimationList[i].StartAnim)
                {
                    handlerStartAnim = OnStartAnim;
                    if (handlerStartAnim != null)
                    {
                        handlerStartAnim(this, AgentAni.AnimationList[i].AnimID);
                    }
                }
                else
                {
                    handlerStopAnim = OnStopAnim;
                    if (handlerStopAnim != null)
                    {
                        handlerStopAnim(this, AgentAni.AnimationList[i].AnimID);
                    }
                }
            }
            return true;
        }

        private bool HandleAgentRequestSit(IClientAPI sender, Packet Pack)
        {
            if (OnAgentRequestSit != null)
            {
                AgentRequestSitPacket agentRequestSit = (AgentRequestSitPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (agentRequestSit.AgentData.SessionID != SessionId ||
                        agentRequestSit.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                AgentRequestSit handlerAgentRequestSit = OnAgentRequestSit;
                if (handlerAgentRequestSit != null)
                    handlerAgentRequestSit(this, agentRequestSit.AgentData.AgentID,
                                           agentRequestSit.TargetObject.TargetID, agentRequestSit.TargetObject.Offset);
            }
            return true;
        }

        private bool HandleAgentSit(IClientAPI sender, Packet Pack)
        {
            if (OnAgentSit != null)
            {
                AgentSitPacket agentSit = (AgentSitPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (agentSit.AgentData.SessionID != SessionId ||
                        agentSit.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                AgentSit handlerAgentSit = OnAgentSit;
                if (handlerAgentSit != null)
                {
                    OnAgentSit(this, agentSit.AgentData.AgentID);
                }
            }
            return true;
        }

        private bool HandleSoundTrigger(IClientAPI sender, Packet Pack)
        {
            SoundTriggerPacket soundTriggerPacket = (SoundTriggerPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                // UNSUPPORTED ON THIS PACKET
            }
            #endregion

            SoundTrigger handlerSoundTrigger = OnSoundTrigger;
            if (handlerSoundTrigger != null)
            {
                // UUIDS are sent as zeroes by the client, substitute agent's id
                handlerSoundTrigger(soundTriggerPacket.SoundData.SoundID, AgentId,
                    AgentId, AgentId,
                    soundTriggerPacket.SoundData.Gain, soundTriggerPacket.SoundData.Position,
                    soundTriggerPacket.SoundData.Handle, 0);

            }
            return true;
        }

        private bool HandleAvatarPickerRequest(IClientAPI sender, Packet Pack)
        {
            AvatarPickerRequestPacket avRequestQuery = (AvatarPickerRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avRequestQuery.AgentData.SessionID != SessionId ||
                    avRequestQuery.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            AvatarPickerRequestPacket.AgentDataBlock Requestdata = avRequestQuery.AgentData;
            AvatarPickerRequestPacket.DataBlock querydata = avRequestQuery.Data;
            //m_log.Debug("Agent Sends:" + Utils.BytesToString(querydata.Name));

            AvatarPickerRequest handlerAvatarPickerRequest = OnAvatarPickerRequest;
            if (handlerAvatarPickerRequest != null)
            {
                handlerAvatarPickerRequest(this, Requestdata.AgentID, Requestdata.QueryID,
                                           Utils.BytesToString(querydata.Name));
            }
            return true;
        }

        private bool HandleAgentDataUpdateRequest(IClientAPI sender, Packet Pack)
        {
            AgentDataUpdateRequestPacket avRequestDataUpdatePacket = (AgentDataUpdateRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avRequestDataUpdatePacket.AgentData.SessionID != SessionId ||
                    avRequestDataUpdatePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            FetchInventory handlerAgentDataUpdateRequest = OnAgentDataUpdateRequest;

            if (handlerAgentDataUpdateRequest != null)
            {
                handlerAgentDataUpdateRequest(this, avRequestDataUpdatePacket.AgentData.AgentID, avRequestDataUpdatePacket.AgentData.SessionID);
            }

            return true;
        }

        private bool HandleUserInfoRequest(IClientAPI sender, Packet Pack)
        {
            UserInfoRequest handlerUserInfoRequest = OnUserInfoRequest;
            if (handlerUserInfoRequest != null)
            {
                handlerUserInfoRequest(this);
            }
            else
            {
                SendUserInfoReply(false, true, "");
            }
            return true;

        }

        private bool HandleUpdateUserInfo(IClientAPI sender, Packet Pack)
        {
            UpdateUserInfoPacket updateUserInfo = (UpdateUserInfoPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (updateUserInfo.AgentData.SessionID != SessionId ||
                    updateUserInfo.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            UpdateUserInfo handlerUpdateUserInfo = OnUpdateUserInfo;
            if (handlerUpdateUserInfo != null)
            {
                bool visible = true;
                string DirectoryVisibility =
                        Utils.BytesToString(updateUserInfo.UserData.DirectoryVisibility);
                if (DirectoryVisibility == "hidden")
                    visible = false;

                handlerUpdateUserInfo(
                        updateUserInfo.UserData.IMViaEMail,
                        visible, this);
            }
            return true;
        }

        private bool HandleSetStartLocationRequest(IClientAPI sender, Packet Pack)
        {
            SetStartLocationRequestPacket avSetStartLocationRequestPacket = (SetStartLocationRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avSetStartLocationRequestPacket.AgentData.SessionID != SessionId ||
                    avSetStartLocationRequestPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (avSetStartLocationRequestPacket.AgentData.AgentID == AgentId && avSetStartLocationRequestPacket.AgentData.SessionID == SessionId)
            {
                // Linden Client limitation..
                if (avSetStartLocationRequestPacket.StartLocationData.LocationPos.X == 255.5f
                    || avSetStartLocationRequestPacket.StartLocationData.LocationPos.Y == 255.5f)
                {
                    ScenePresence avatar = null;
                    if (((Scene)m_scene).TryGetScenePresence(AgentId, out avatar))
                    {
                        if (avSetStartLocationRequestPacket.StartLocationData.LocationPos.X == 255.5f)
                        {
                            avSetStartLocationRequestPacket.StartLocationData.LocationPos.X = avatar.AbsolutePosition.X;
                        }
                        if (avSetStartLocationRequestPacket.StartLocationData.LocationPos.Y == 255.5f)
                        {
                            avSetStartLocationRequestPacket.StartLocationData.LocationPos.Y = avatar.AbsolutePosition.Y;
                        }
                    }

                }
                TeleportLocationRequest handlerSetStartLocationRequest = OnSetStartLocationRequest;
                if (handlerSetStartLocationRequest != null)
                {
                    handlerSetStartLocationRequest(this, 0, avSetStartLocationRequestPacket.StartLocationData.LocationPos,
                                                   avSetStartLocationRequestPacket.StartLocationData.LocationLookAt,
                                                   avSetStartLocationRequestPacket.StartLocationData.LocationID);
                }
            }
            return true;
        }

        private bool HandleAgentThrottle(IClientAPI sender, Packet Pack)
        {
            AgentThrottlePacket atpack = (AgentThrottlePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (atpack.AgentData.SessionID != SessionId ||
                    atpack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            m_udpClient.SetThrottles(atpack.Throttle.Throttles);
            return true;
        }

        private bool HandleAgentPause(IClientAPI sender, Packet Pack)
        {
            #region Packet Session and User Check
            if (m_checkPackets)
            {

                if ((Pack as AgentPausePacket).AgentData.SessionID != SessionId ||
                    (Pack as AgentPausePacket).AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            m_udpClient.IsPaused = true;
            return true;
        }

        private bool HandleAgentResume(IClientAPI sender, Packet Pack)
        {
            #region Packet Session and User Check
            if (m_checkPackets)
            {

                if ((Pack as AgentResumePacket).AgentData.SessionID != SessionId ||
                    (Pack as AgentResumePacket).AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            m_udpClient.IsPaused = false;
            SendStartPingCheck(m_udpClient.CurrentPingSequence++);
            return true;
        }

        private bool HandleForceScriptControlRelease(IClientAPI sender, Packet Pack)
        {
            ForceReleaseControls handlerForceReleaseControls = OnForceReleaseControls;
            if (handlerForceReleaseControls != null)
            {
                handlerForceReleaseControls(this, AgentId);
            }
            return true;
        }

        #endregion Scene/Avatar

        #region Objects/m_sceneObjects

        private bool HandleObjectLink(IClientAPI sender, Packet Pack)
        {
            ObjectLinkPacket link = (ObjectLinkPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (link.AgentData.SessionID != SessionId ||
                    link.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(LinkObjects, Pack);
            return true;
        }

        private void LinkObjects(object Pack)
        {
            ObjectLinkPacket link = (ObjectLinkPacket)Pack;
            uint parentprimid = 0;
            List<uint> childrenprims = new List<uint>();
            if (link.ObjectData.Length > 1)
            {
                parentprimid = link.ObjectData[0].ObjectLocalID;

                for (int i = 1; i < link.ObjectData.Length; i++)
                {
                    childrenprims.Add(link.ObjectData[i].ObjectLocalID);
                }
            }
            LinkObjects handlerLinkObjects = OnLinkObjects;
            if (handlerLinkObjects != null)
            {
                handlerLinkObjects(this, parentprimid, childrenprims);
            }
        }

        private bool HandleObjectDelink(IClientAPI sender, Packet Pack)
        {
            ObjectDelinkPacket delink = (ObjectDelinkPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (delink.AgentData.SessionID != SessionId ||
                    delink.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(DelinkObjects, Pack);
            return true;
        }

        private void DelinkObjects(object Pack)
        {
            ObjectDelinkPacket delink = (ObjectDelinkPacket)Pack;

            // It appears the prim at index 0 is not always the root prim (for
            // instance, when one prim of a link set has been edited independently
            // of the others).  Therefore, we'll pass all the ids onto the delink
            // method for it to decide which is the root.
            List<uint> prims = new List<uint>();
            for (int i = 0; i < delink.ObjectData.Length; i++)
            {
                prims.Add(delink.ObjectData[i].ObjectLocalID);
            }
            DelinkObjects handlerDelinkObjects = OnDelinkObjects;
            if (handlerDelinkObjects != null)
            {
                handlerDelinkObjects(prims, this);
            }
        }

        private bool HandleObjectAdd(IClientAPI sender, Packet Pack)
        {
            if (OnAddPrim != null)
            {
                ObjectAddPacket addPacket = (ObjectAddPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (addPacket.AgentData.SessionID != SessionId ||
                        addPacket.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                PrimitiveBaseShape shape = GetShapeFromAddPacket(addPacket);
                // m_log.Info("[REZData]: " + addPacket.ToString());
                //BypassRaycast: 1
                //RayStart: <69.79469, 158.2652, 98.40343>
                //RayEnd: <61.97724, 141.995, 92.58341>
                //RayTargetID: 00000000-0000-0000-0000-000000000000

                //Check to see if adding the prim is allowed; useful for any module wanting to restrict the
                //object from rezing initially

                AddNewPrim handlerAddPrim = OnAddPrim;
                if (handlerAddPrim != null)
                    handlerAddPrim(AgentId, ActiveGroupId, addPacket.ObjectData.RayEnd, addPacket.ObjectData.Rotation, shape, addPacket.ObjectData.BypassRaycast, addPacket.ObjectData.RayStart, addPacket.ObjectData.RayTargetID, addPacket.ObjectData.RayEndIsIntersection);
            }
            return true;
        }

        private bool HandleObjectShape(IClientAPI sender, Packet Pack)
        {
            ObjectShapePacket shapePacket = (ObjectShapePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (shapePacket.AgentData.SessionID != SessionId ||
                    shapePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            UpdateShape handlerUpdatePrimShape = null;
            for (int i = 0; i < shapePacket.ObjectData.Length; i++)
            {
                handlerUpdatePrimShape = OnUpdatePrimShape;
                if (handlerUpdatePrimShape != null)
                {
                    UpdateShapeArgs shapeData = new UpdateShapeArgs();
                    shapeData.ObjectLocalID = shapePacket.ObjectData[i].ObjectLocalID;
                    shapeData.PathBegin = shapePacket.ObjectData[i].PathBegin;
                    shapeData.PathCurve = shapePacket.ObjectData[i].PathCurve;
                    shapeData.PathEnd = shapePacket.ObjectData[i].PathEnd;
                    shapeData.PathRadiusOffset = shapePacket.ObjectData[i].PathRadiusOffset;
                    shapeData.PathRevolutions = shapePacket.ObjectData[i].PathRevolutions;
                    shapeData.PathScaleX = shapePacket.ObjectData[i].PathScaleX;
                    shapeData.PathScaleY = shapePacket.ObjectData[i].PathScaleY;
                    shapeData.PathShearX = shapePacket.ObjectData[i].PathShearX;
                    shapeData.PathShearY = shapePacket.ObjectData[i].PathShearY;
                    shapeData.PathSkew = shapePacket.ObjectData[i].PathSkew;
                    shapeData.PathTaperX = shapePacket.ObjectData[i].PathTaperX;
                    shapeData.PathTaperY = shapePacket.ObjectData[i].PathTaperY;
                    shapeData.PathTwist = shapePacket.ObjectData[i].PathTwist;
                    shapeData.PathTwistBegin = shapePacket.ObjectData[i].PathTwistBegin;
                    shapeData.ProfileBegin = shapePacket.ObjectData[i].ProfileBegin;
                    shapeData.ProfileCurve = shapePacket.ObjectData[i].ProfileCurve;
                    shapeData.ProfileEnd = shapePacket.ObjectData[i].ProfileEnd;
                    shapeData.ProfileHollow = shapePacket.ObjectData[i].ProfileHollow;

                    handlerUpdatePrimShape(m_agentId, shapePacket.ObjectData[i].ObjectLocalID,
                                           shapeData);
                }
            }
            return true;
        }

        private bool HandleObjectExtraParams(IClientAPI sender, Packet Pack)
        {
            ObjectExtraParamsPacket extraPar = (ObjectExtraParamsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (extraPar.AgentData.SessionID != SessionId ||
                    extraPar.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectExtraParams handlerUpdateExtraParams = OnUpdateExtraParams;
            if (handlerUpdateExtraParams != null)
            {
                for (int i = 0; i < extraPar.ObjectData.Length; i++)
                {
                    handlerUpdateExtraParams(m_agentId, extraPar.ObjectData[i].ObjectLocalID,
                                             extraPar.ObjectData[i].ParamType,
                                             extraPar.ObjectData[i].ParamInUse, extraPar.ObjectData[i].ParamData);
                }
            }
            return true;
        }

        private bool HandleObjectDuplicate(IClientAPI sender, Packet Pack)
        {
            ObjectDuplicatePacket dupe = (ObjectDuplicatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dupe.AgentData.SessionID != SessionId ||
                    dupe.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

//            ObjectDuplicatePacket.AgentDataBlock AgentandGroupData = dupe.AgentData;

            ObjectDuplicate handlerObjectDuplicate = null;

            for (int i = 0; i < dupe.ObjectData.Length; i++)
            {
                handlerObjectDuplicate = OnObjectDuplicate;
                if (handlerObjectDuplicate != null)
                {
                    handlerObjectDuplicate(dupe.ObjectData[i].ObjectLocalID, dupe.SharedData.Offset,
                                           dupe.SharedData.DuplicateFlags, AgentId,
                                           m_activeGroupID, Quaternion.Identity);
                }
            }

            return true;
        }

        private bool HandleRequestMultipleObjects(IClientAPI sender, Packet Pack)
        {
            RequestMultipleObjectsPacket incomingRequest = (RequestMultipleObjectsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (incomingRequest.AgentData.SessionID != SessionId ||
                    incomingRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectRequest handlerObjectRequest = null;

            for (int i = 0; i < incomingRequest.ObjectData.Length; i++)
            {
                handlerObjectRequest = OnObjectRequest;
                if (handlerObjectRequest != null)
                {
                    handlerObjectRequest(incomingRequest.ObjectData[i].ID, incomingRequest.ObjectData[i].CacheMissType, this);
                }
            }
            return true;
        }

        private bool HandleObjectSelect(IClientAPI sender, Packet Pack)
        {
            ObjectSelectPacket incomingselect = (ObjectSelectPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (incomingselect.AgentData.SessionID != SessionId ||
                    incomingselect.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectSelect handlerObjectSelect = null;
            List<uint> LocalIDs = new List<uint>();
            for (int i = 0; i < incomingselect.ObjectData.Length; i++)
            {
                LocalIDs.Add(incomingselect.ObjectData[i].ObjectLocalID);
            }
            handlerObjectSelect = OnObjectSelect;
            if (handlerObjectSelect != null)
            {
                handlerObjectSelect(LocalIDs, this);
            }
            return true;
        }

        private bool HandleObjectDeselect(IClientAPI sender, Packet Pack)
        {
            ObjectDeselectPacket incomingdeselect = (ObjectDeselectPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (incomingdeselect.AgentData.SessionID != SessionId ||
                    incomingdeselect.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectDeselect handlerObjectDeselect = null;

            for (int i = 0; i < incomingdeselect.ObjectData.Length; i++)
            {
                handlerObjectDeselect = OnObjectDeselect;
                if (handlerObjectDeselect != null)
                {
                    OnObjectDeselect(incomingdeselect.ObjectData[i].ObjectLocalID, this);
                }
            }
            return true;
        }

        private bool HandleObjectPosition(IClientAPI sender, Packet Pack)
        {
            // DEPRECATED: but till libsecondlife removes it, people will use it
            ObjectPositionPacket position = (ObjectPositionPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (position.AgentData.SessionID != SessionId ||
                    position.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion


            for (int i = 0; i < position.ObjectData.Length; i++)
            {
                UpdateVectorWithUpdate handlerUpdateVector = OnUpdatePrimGroupPosition;
                if (handlerUpdateVector != null)
                    handlerUpdateVector(position.ObjectData[i].ObjectLocalID, position.ObjectData[i].Position, this, true);
            }

            return true;
        }

        private bool HandleObjectScale(IClientAPI sender, Packet Pack)
        {
            // DEPRECATED: but till libsecondlife removes it, people will use it
            ObjectScalePacket scale = (ObjectScalePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (scale.AgentData.SessionID != SessionId ||
                    scale.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            for (int i = 0; i < scale.ObjectData.Length; i++)
            {
                UpdateVector handlerUpdatePrimGroupScale = OnUpdatePrimGroupScale;
                if (handlerUpdatePrimGroupScale != null)
                    handlerUpdatePrimGroupScale(scale.ObjectData[i].ObjectLocalID, scale.ObjectData[i].Scale, this);
            }

            return true;
        }

        private bool HandleObjectRotation(IClientAPI sender, Packet Pack)
        {
            // DEPRECATED: but till libsecondlife removes it, people will use it
            ObjectRotationPacket rotation = (ObjectRotationPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rotation.AgentData.SessionID != SessionId ||
                    rotation.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            for (int i = 0; i < rotation.ObjectData.Length; i++)
            {
                UpdatePrimRotation handlerUpdatePrimRotation = OnUpdatePrimGroupRotation;
                if (handlerUpdatePrimRotation != null)
                    handlerUpdatePrimRotation(rotation.ObjectData[i].ObjectLocalID, rotation.ObjectData[i].Rotation, this);
            }

            return true;
        }

        private bool HandleObjectFlagUpdate(IClientAPI sender, Packet Pack)
        {
            ObjectFlagUpdatePacket flags = (ObjectFlagUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (flags.AgentData.SessionID != SessionId ||
                    flags.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            UpdatePrimFlags handlerUpdatePrimFlags = OnUpdatePrimFlags;

            if (handlerUpdatePrimFlags != null)
            {
                handlerUpdatePrimFlags(flags.AgentData.ObjectLocalID, flags.AgentData.UsePhysics, flags.AgentData.IsTemporary, flags.AgentData.IsPhantom, this);
            }
            return true;
        }

        private bool HandleObjectImage(IClientAPI sender, Packet Pack)
        {
            ObjectImagePacket imagePack = (ObjectImagePacket)Pack;

            UpdatePrimTexture handlerUpdatePrimTexture = null;
            for (int i = 0; i < imagePack.ObjectData.Length; i++)
            {
                handlerUpdatePrimTexture = OnUpdatePrimTexture;
                if (handlerUpdatePrimTexture != null)
                {
                    handlerUpdatePrimTexture(imagePack.ObjectData[i].ObjectLocalID,
                                             imagePack.ObjectData[i].TextureEntry, this);
                }
            }
            return true;
        }

        private bool HandleObjectGrab(IClientAPI sender, Packet Pack)
        {
            ObjectGrabPacket grab = (ObjectGrabPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (grab.AgentData.SessionID != SessionId ||
                    grab.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GrabObject handlerGrabObject = OnGrabObject;

            if (handlerGrabObject != null)
            {
                List<SurfaceTouchEventArgs> touchArgs = new List<SurfaceTouchEventArgs>();
                if ((grab.SurfaceInfo != null) && (grab.SurfaceInfo.Length > 0))
                {
                    foreach (ObjectGrabPacket.SurfaceInfoBlock surfaceInfo in grab.SurfaceInfo)
                    {
                        SurfaceTouchEventArgs arg = new SurfaceTouchEventArgs();
                        arg.Binormal = surfaceInfo.Binormal;
                        arg.FaceIndex = surfaceInfo.FaceIndex;
                        arg.Normal = surfaceInfo.Normal;
                        arg.Position = surfaceInfo.Position;
                        arg.STCoord = surfaceInfo.STCoord;
                        arg.UVCoord = surfaceInfo.UVCoord;
                        touchArgs.Add(arg);
                    }
                }
                handlerGrabObject(grab.ObjectData.LocalID, grab.ObjectData.GrabOffset, this, touchArgs);
            }
            return true;
        }

        private bool HandleObjectGrabUpdate(IClientAPI sender, Packet Pack)
        {
            ObjectGrabUpdatePacket grabUpdate = (ObjectGrabUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (grabUpdate.AgentData.SessionID != SessionId ||
                    grabUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            MoveObject handlerGrabUpdate = OnGrabUpdate;

            if (handlerGrabUpdate != null)
            {
                List<SurfaceTouchEventArgs> touchArgs = new List<SurfaceTouchEventArgs>();
                if ((grabUpdate.SurfaceInfo != null) && (grabUpdate.SurfaceInfo.Length > 0))
                {
                    foreach (ObjectGrabUpdatePacket.SurfaceInfoBlock surfaceInfo in grabUpdate.SurfaceInfo)
                    {
                        SurfaceTouchEventArgs arg = new SurfaceTouchEventArgs();
                        arg.Binormal = surfaceInfo.Binormal;
                        arg.FaceIndex = surfaceInfo.FaceIndex;
                        arg.Normal = surfaceInfo.Normal;
                        arg.Position = surfaceInfo.Position;
                        arg.STCoord = surfaceInfo.STCoord;
                        arg.UVCoord = surfaceInfo.UVCoord;
                        touchArgs.Add(arg);
                    }
                }
                handlerGrabUpdate(grabUpdate.ObjectData.ObjectID, grabUpdate.ObjectData.GrabOffsetInitial,
                                  grabUpdate.ObjectData.GrabPosition, this, touchArgs);
            }
            return true;
        }

        private bool HandleObjectDeGrab(IClientAPI sender, Packet Pack)
        {
            ObjectDeGrabPacket deGrab = (ObjectDeGrabPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (deGrab.AgentData.SessionID != SessionId ||
                    deGrab.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DeGrabObject handlerDeGrabObject = OnDeGrabObject;
            if (handlerDeGrabObject != null)
            {
                List<SurfaceTouchEventArgs> touchArgs = new List<SurfaceTouchEventArgs>();
                if ((deGrab.SurfaceInfo != null) && (deGrab.SurfaceInfo.Length > 0))
                {
                    foreach (ObjectDeGrabPacket.SurfaceInfoBlock surfaceInfo in deGrab.SurfaceInfo)
                    {
                        SurfaceTouchEventArgs arg = new SurfaceTouchEventArgs();
                        arg.Binormal = surfaceInfo.Binormal;
                        arg.FaceIndex = surfaceInfo.FaceIndex;
                        arg.Normal = surfaceInfo.Normal;
                        arg.Position = surfaceInfo.Position;
                        arg.STCoord = surfaceInfo.STCoord;
                        arg.UVCoord = surfaceInfo.UVCoord;
                        touchArgs.Add(arg);
                    }
                }
                handlerDeGrabObject(deGrab.ObjectData.LocalID, this, touchArgs);
            }
            return true;
        }

        private bool HandleObjectSpinStart(IClientAPI sender, Packet Pack)
        {
            //m_log.Warn("[CLIENT]: unhandled ObjectSpinStart packet");
            ObjectSpinStartPacket spinStart = (ObjectSpinStartPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (spinStart.AgentData.SessionID != SessionId ||
                    spinStart.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SpinStart handlerSpinStart = OnSpinStart;
            if (handlerSpinStart != null)
            {
                handlerSpinStart(spinStart.ObjectData.ObjectID, this);
            }
            return true;
        }

        private bool HandleObjectSpinUpdate(IClientAPI sender, Packet Pack)
        {
            //m_log.Warn("[CLIENT]: unhandled ObjectSpinUpdate packet");
            ObjectSpinUpdatePacket spinUpdate = (ObjectSpinUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (spinUpdate.AgentData.SessionID != SessionId ||
                    spinUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Vector3 axis;
            float angle;
            spinUpdate.ObjectData.Rotation.GetAxisAngle(out axis, out angle);
            //m_log.Warn("[CLIENT]: ObjectSpinUpdate packet rot axis:" + axis + " angle:" + angle);

            SpinObject handlerSpinUpdate = OnSpinUpdate;
            if (handlerSpinUpdate != null)
            {
                handlerSpinUpdate(spinUpdate.ObjectData.ObjectID, spinUpdate.ObjectData.Rotation, this);
            }
            return true;
        }

        private bool HandleObjectSpinStop(IClientAPI sender, Packet Pack)
        {
            //m_log.Warn("[CLIENT]: unhandled ObjectSpinStop packet");
            ObjectSpinStopPacket spinStop = (ObjectSpinStopPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (spinStop.AgentData.SessionID != SessionId ||
                    spinStop.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SpinStop handlerSpinStop = OnSpinStop;
            if (handlerSpinStop != null)
            {
                handlerSpinStop(spinStop.ObjectData.ObjectID, this);
            }
            return true;
        }

        private bool HandleObjectDescription(IClientAPI sender, Packet Pack)
        {
            ObjectDescriptionPacket objDes = (ObjectDescriptionPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (objDes.AgentData.SessionID != SessionId ||
                    objDes.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GenericCall7 handlerObjectDescription = null;

            for (int i = 0; i < objDes.ObjectData.Length; i++)
            {
                handlerObjectDescription = OnObjectDescription;
                if (handlerObjectDescription != null)
                {
                    handlerObjectDescription(this, objDes.ObjectData[i].LocalID,
                                             Util.FieldToString(objDes.ObjectData[i].Description));
                }
            }
            return true;
        }

        private bool HandleObjectName(IClientAPI sender, Packet Pack)
        {
            ObjectNamePacket objName = (ObjectNamePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (objName.AgentData.SessionID != SessionId ||
                    objName.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GenericCall7 handlerObjectName = null;
            for (int i = 0; i < objName.ObjectData.Length; i++)
            {
                handlerObjectName = OnObjectName;
                if (handlerObjectName != null)
                {
                    handlerObjectName(this, objName.ObjectData[i].LocalID,
                                      Util.FieldToString(objName.ObjectData[i].Name));
                }
            }
            return true;
        }

        private bool HandleObjectPermissions(IClientAPI sender, Packet Pack)
        {
            if (OnObjectPermissions != null)
            {
                ObjectPermissionsPacket newobjPerms = (ObjectPermissionsPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (newobjPerms.AgentData.SessionID != SessionId ||
                        newobjPerms.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                UUID AgentID = newobjPerms.AgentData.AgentID;
                UUID SessionID = newobjPerms.AgentData.SessionID;

                ObjectPermissions handlerObjectPermissions = null;

                for (int i = 0; i < newobjPerms.ObjectData.Length; i++)
                {
                    ObjectPermissionsPacket.ObjectDataBlock permChanges = newobjPerms.ObjectData[i];

                    byte field = permChanges.Field;
                    uint localID = permChanges.ObjectLocalID;
                    uint mask = permChanges.Mask;
                    byte set = permChanges.Set;

                    handlerObjectPermissions = OnObjectPermissions;

                    if (handlerObjectPermissions != null)
                        handlerObjectPermissions(this, AgentID, SessionID, field, localID, mask, set);
                }
            }

            // Here's our data,
            // PermField contains the field the info goes into
            // PermField determines which mask we're changing
            //
            // chmask is the mask of the change
            // setTF is whether we're adding it or taking it away
            //
            // objLocalID is the localID of the object.

            // Unfortunately, we have to pass the event the packet because objData is an array
            // That means multiple object perms may be updated in a single packet.

            return true;
        }

        private bool HandleUndo(IClientAPI sender, Packet Pack)
        {
            UndoPacket undoitem = (UndoPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (undoitem.AgentData.SessionID != SessionId ||
                    undoitem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (undoitem.ObjectData.Length > 0)
            {
                for (int i = 0; i < undoitem.ObjectData.Length; i++)
                {
                    UUID objiD = undoitem.ObjectData[i].ObjectID;
                    AgentSit handlerOnUndo = OnUndo;
                    if (handlerOnUndo != null)
                    {
                        handlerOnUndo(this, objiD);
                    }

                }
            }
            return true;
        }

        private bool HandleLandUndo(IClientAPI sender, Packet Pack)
        {
            UndoLandPacket undolanditem = (UndoLandPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (undolanditem.AgentData.SessionID != SessionId ||
                    undolanditem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            LandUndo handlerOnUndo = OnLandUndo;
            if (handlerOnUndo != null)
            {
                handlerOnUndo(this);
            }
            return true;
        }

        private bool HandleRedo(IClientAPI sender, Packet Pack)
        {
            RedoPacket redoitem = (RedoPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (redoitem.AgentData.SessionID != SessionId ||
                    redoitem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (redoitem.ObjectData.Length > 0)
            {
                for (int i = 0; i < redoitem.ObjectData.Length; i++)
                {
                    UUID objiD = redoitem.ObjectData[i].ObjectID;
                    AgentSit handlerOnRedo = OnRedo;
                    if (handlerOnRedo != null)
                    {
                        handlerOnRedo(this, objiD);
                    }

                }
            }
            return true;
        }

        private bool HandleObjectDuplicateOnRay(IClientAPI sender, Packet Pack)
        {
            ObjectDuplicateOnRayPacket dupeOnRay = (ObjectDuplicateOnRayPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dupeOnRay.AgentData.SessionID != SessionId ||
                    dupeOnRay.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectDuplicateOnRay handlerObjectDuplicateOnRay = null;

            for (int i = 0; i < dupeOnRay.ObjectData.Length; i++)
            {
                handlerObjectDuplicateOnRay = OnObjectDuplicateOnRay;
                if (handlerObjectDuplicateOnRay != null)
                {
                    handlerObjectDuplicateOnRay(dupeOnRay.ObjectData[i].ObjectLocalID, dupeOnRay.AgentData.DuplicateFlags,
                                                AgentId, m_activeGroupID, dupeOnRay.AgentData.RayTargetID, dupeOnRay.AgentData.RayEnd,
                                                dupeOnRay.AgentData.RayStart, dupeOnRay.AgentData.BypassRaycast, dupeOnRay.AgentData.RayEndIsIntersection,
                                                dupeOnRay.AgentData.CopyCenters, dupeOnRay.AgentData.CopyRotates);
                }
            }

            return true;
        }

        private bool HandleRequestObjectPropertiesFamily(IClientAPI sender, Packet Pack)
        {
            //This powers the little tooltip that appears when you move your mouse over an object
            RequestObjectPropertiesFamilyPacket packToolTip = (RequestObjectPropertiesFamilyPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (packToolTip.AgentData.SessionID != SessionId ||
                    packToolTip.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RequestObjectPropertiesFamilyPacket.ObjectDataBlock packObjBlock = packToolTip.ObjectData;

            RequestObjectPropertiesFamily handlerRequestObjectPropertiesFamily = OnRequestObjectPropertiesFamily;

            if (handlerRequestObjectPropertiesFamily != null)
            {
                handlerRequestObjectPropertiesFamily(this, m_agentId, packObjBlock.RequestFlags,
                                                     packObjBlock.ObjectID);
            }

            return true;
        }

        private bool HandleObjectIncludeInSearch(IClientAPI sender, Packet Pack)
        {
            //This lets us set objects to appear in search (stuff like DataSnapshot, etc)
            ObjectIncludeInSearchPacket packInSearch = (ObjectIncludeInSearchPacket)Pack;
            ObjectIncludeInSearch handlerObjectIncludeInSearch = null;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (packInSearch.AgentData.SessionID != SessionId ||
                    packInSearch.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            foreach (ObjectIncludeInSearchPacket.ObjectDataBlock objData in packInSearch.ObjectData)
            {
                bool inSearch = objData.IncludeInSearch;
                uint localID = objData.ObjectLocalID;

                handlerObjectIncludeInSearch = OnObjectIncludeInSearch;

                if (handlerObjectIncludeInSearch != null)
                {
                    handlerObjectIncludeInSearch(this, inSearch, localID);
                }
            }
            return true;
        }

        private bool HandleScriptAnswerYes(IClientAPI sender, Packet Pack)
        {
            ScriptAnswerYesPacket scriptAnswer = (ScriptAnswerYesPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (scriptAnswer.AgentData.SessionID != SessionId ||
                    scriptAnswer.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ScriptAnswer handlerScriptAnswer = OnScriptAnswer;
            if (handlerScriptAnswer != null)
            {
                handlerScriptAnswer(this, scriptAnswer.Data.TaskID, scriptAnswer.Data.ItemID, scriptAnswer.Data.Questions);
            }
            return true;
        }

        private bool HandleObjectClickAction(IClientAPI sender, Packet Pack)
        {
            ObjectClickActionPacket ocpacket = (ObjectClickActionPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (ocpacket.AgentData.SessionID != SessionId ||
                    ocpacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GenericCall7 handlerObjectClickAction = OnObjectClickAction;
            if (handlerObjectClickAction != null)
            {
                foreach (ObjectClickActionPacket.ObjectDataBlock odata in ocpacket.ObjectData)
                {
                    byte action = odata.ClickAction;
                    uint localID = odata.ObjectLocalID;
                    handlerObjectClickAction(this, localID, action.ToString());
                }
            }
            return true;
        }

        private bool HandleObjectMaterial(IClientAPI sender, Packet Pack)
        {
            ObjectMaterialPacket ompacket = (ObjectMaterialPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (ompacket.AgentData.SessionID != SessionId ||
                    ompacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GenericCall7 handlerObjectMaterial = OnObjectMaterial;
            if (handlerObjectMaterial != null)
            {
                foreach (ObjectMaterialPacket.ObjectDataBlock odata in ompacket.ObjectData)
                {
                    byte material = odata.Material;
                    uint localID = odata.ObjectLocalID;
                    handlerObjectMaterial(this, localID, material.ToString());
                }
            }
            return true;
        }

        #endregion Objects/m_sceneObjects

        #region Inventory/Asset/Other related packets

        private bool HandleRequestImage(IClientAPI sender, Packet Pack)
        {
            RequestImagePacket imageRequest = (RequestImagePacket)Pack;
            //m_log.Debug("image request: " + Pack.ToString());

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (imageRequest.AgentData.SessionID != SessionId ||
                    imageRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            //handlerTextureRequest = null;
            for (int i = 0; i < imageRequest.RequestImage.Length; i++)
            {
                TextureRequestArgs args = new TextureRequestArgs();

                RequestImagePacket.RequestImageBlock block = imageRequest.RequestImage[i];

                args.RequestedAssetID = block.Image;
                args.DiscardLevel = block.DiscardLevel;
                args.PacketNumber = block.Packet;
                args.Priority = block.DownloadPriority;
                args.requestSequence = imageRequest.Header.Sequence;

                // NOTE: This is not a built in part of the LLUDP protocol, but we double the
                // priority of avatar textures to get avatars rezzing in faster than the
                // surrounding scene
                if ((ImageType)block.Type == ImageType.Baked)
                    args.Priority *= 2.0f;

                // in the end, we null this, so we have to check if it's null
                if (m_imageManager != null)
                {
                    m_imageManager.EnqueueReq(args);
                }
            }
            return true;
        }

        /// <summary>
        /// This is the entry point for the UDP route by which the client can retrieve asset data.  If the request
        /// is successful then a TransferInfo packet will be sent back, followed by one or more TransferPackets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="Pack"></param>
        /// <returns>This parameter may be ignored since we appear to return true whatever happens</returns>
        private bool HandleTransferRequest(IClientAPI sender, Packet Pack)
        {
            //m_log.Debug("ClientView.ProcessPackets.cs:ProcessInPacket() - Got transfer request");

            TransferRequestPacket transfer = (TransferRequestPacket)Pack;
            //m_log.Debug("Transfer Request: " + transfer.ToString());
            // Validate inventory transfers
            // Has to be done here, because AssetCache can't do it
            //
            UUID taskID = UUID.Zero;
            if (transfer.TransferInfo.SourceType == (int)SourceType.SimInventoryItem)
            {
                taskID = new UUID(transfer.TransferInfo.Params, 48);
                UUID itemID = new UUID(transfer.TransferInfo.Params, 64);
                UUID requestID = new UUID(transfer.TransferInfo.Params, 80);

//                m_log.DebugFormat(
//                    "[CLIENT]: Got request for asset {0} from item {1} in prim {2} by {3}",
//                    requestID, itemID, taskID, Name);

                if (!(((Scene)m_scene).Permissions.BypassPermissions()))
                {
                    if (taskID != UUID.Zero) // Prim
                    {
                        SceneObjectPart part = ((Scene)m_scene).GetSceneObjectPart(taskID);

                        if (part == null)
                        {
                            m_log.WarnFormat(
                                "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but prim does not exist",
                                Name, requestID, itemID, taskID);
                            return true;
                        }

                        TaskInventoryItem tii = part.Inventory.GetInventoryItem(itemID);
                        if (tii == null)
                        {
                            m_log.WarnFormat(
                                "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but item does not exist",
                                Name, requestID, itemID, taskID);
                            return true;
                        }

                        if (tii.Type == (int)AssetType.LSLText)
                        {
                            if (!((Scene)m_scene).Permissions.CanEditScript(itemID, taskID, AgentId))
                                return true;
                        }
                        else if (tii.Type == (int)AssetType.Notecard)
                        {
                            if (!((Scene)m_scene).Permissions.CanEditNotecard(itemID, taskID, AgentId))
                                return true;
                        }
                        else
                        {
                            if (!((Scene)m_scene).Permissions.CanEditObjectInventory(part.UUID, AgentId))
                            {
                                m_log.Warn("[LLClientView]: Permissions check for CanEditObjectInventory fell through to standard code!");
                                
                                if (part.OwnerID != AgentId)
                                {
                                    m_log.WarnFormat(
                                        "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but the prim is owned by {4}",
                                        Name, requestID, itemID, taskID, part.OwnerID);
                                    return true;
                                }

                                if ((part.OwnerMask & (uint)PermissionMask.Modify) == 0)
                                {
                                    m_log.WarnFormat(
                                        "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but modify permissions are not set",
                                        Name, requestID, itemID, taskID);
                                    return true;
                                }

                                if (tii.OwnerID != AgentId)
                                {
                                    m_log.WarnFormat(
                                        "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but the item is owned by {4}",
                                        Name, requestID, itemID, taskID, tii.OwnerID);
                                    return true;
                                }

                                if ((
                                    tii.CurrentPermissions & ((uint)PermissionMask.Modify | (uint)PermissionMask.Copy | (uint)PermissionMask.Transfer))
                                        != ((uint)PermissionMask.Modify | (uint)PermissionMask.Copy | (uint)PermissionMask.Transfer))
                                {
                                    m_log.WarnFormat(
                                        "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but item permissions are not modify/copy/transfer",
                                        Name, requestID, itemID, taskID);
                                    return true;
                                }

                                if (tii.AssetID != requestID)
                                {
                                    m_log.WarnFormat(
                                        "[CLIENT]: {0} requested asset {1} from item {2} in prim {3} but this does not match item's asset {4}",
                                        Name, requestID, itemID, taskID, tii.AssetID);
                                    return true;
                                }
                            }
                        }
                    }
                    else // Agent
                    {
                        IInventoryAccessModule invAccess = m_scene.RequestModuleInterface<IInventoryAccessModule>();
                        if (invAccess != null)
                        {
                            if (!invAccess.GetAgentInventoryItem(this, itemID, requestID))
                                return false;

                        }
                        else
                            return false;

                    }
                }
            }

            MakeAssetRequest(transfer, taskID);

            return true;
        }

        private bool HandleAssetUploadRequest(IClientAPI sender, Packet Pack)
        {
            AssetUploadRequestPacket request = (AssetUploadRequestPacket)Pack;


            // m_log.Debug("upload request " + request.ToString());
            // m_log.Debug("upload request was for assetid: " + request.AssetBlock.TransactionID.Combine(this.SecureSessionId).ToString());
            UUID temp = UUID.Combine(request.AssetBlock.TransactionID, SecureSessionId);

            UDPAssetUploadRequest handlerAssetUploadRequest = OnAssetUploadRequest;

            if (handlerAssetUploadRequest != null)
            {
                handlerAssetUploadRequest(this, temp,
                                          request.AssetBlock.TransactionID, request.AssetBlock.Type,
                                          request.AssetBlock.AssetData, request.AssetBlock.StoreLocal,
                                          request.AssetBlock.Tempfile);
            }
            return true;
        }

        private bool HandleRequestXfer(IClientAPI sender, Packet Pack)
        {
            RequestXferPacket xferReq = (RequestXferPacket)Pack;

            RequestXfer handlerRequestXfer = OnRequestXfer;

            if (handlerRequestXfer != null)
            {
                handlerRequestXfer(this, xferReq.XferID.ID, Util.FieldToString(xferReq.XferID.Filename));
            }
            return true;
        }

        private bool HandleSendXferPacket(IClientAPI sender, Packet Pack)
        {
            SendXferPacketPacket xferRec = (SendXferPacketPacket)Pack;

            XferReceive handlerXferReceive = OnXferReceive;
            if (handlerXferReceive != null)
            {
                handlerXferReceive(this, xferRec.XferID.ID, xferRec.XferID.Packet, xferRec.DataPacket.Data);
            }
            return true;
        }

        private bool HandleConfirmXferPacket(IClientAPI sender, Packet Pack)
        {
            ConfirmXferPacketPacket confirmXfer = (ConfirmXferPacketPacket)Pack;

            ConfirmXfer handlerConfirmXfer = OnConfirmXfer;
            if (handlerConfirmXfer != null)
            {
                handlerConfirmXfer(this, confirmXfer.XferID.ID, confirmXfer.XferID.Packet);
            }
            return true;
        }

        private bool HandleAbortXfer(IClientAPI sender, Packet Pack)
        {
            AbortXferPacket abortXfer = (AbortXferPacket)Pack;
            AbortXfer handlerAbortXfer = OnAbortXfer;
            if (handlerAbortXfer != null)
            {
                handlerAbortXfer(this, abortXfer.XferID.ID);
            }

            return true;
        }

        public void SendAbortXferPacket(ulong xferID)
        {
            AbortXferPacket xferItem = (AbortXferPacket)PacketPool.Instance.GetPacket(PacketType.AbortXfer);
            xferItem.XferID.ID = xferID;
            OutPacket(xferItem, ThrottleOutPacketType.Asset);
        }

        private bool HandleCreateInventoryFolder(IClientAPI sender, Packet Pack)
        {
            CreateInventoryFolderPacket invFolder = (CreateInventoryFolderPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (invFolder.AgentData.SessionID != SessionId ||
                    invFolder.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            CreateInventoryFolder handlerCreateInventoryFolder = OnCreateNewInventoryFolder;
            if (handlerCreateInventoryFolder != null)
            {
                handlerCreateInventoryFolder(this, invFolder.FolderData.FolderID,
                                             (ushort)invFolder.FolderData.Type,
                                             Util.FieldToString(invFolder.FolderData.Name),
                                             invFolder.FolderData.ParentID);
            }

            return true;
        }

        private bool HandleUpdateInventoryFolder(IClientAPI sender, Packet Pack)
        {
            if (OnUpdateInventoryFolder != null)
            {
                UpdateInventoryFolderPacket invFolderx = (UpdateInventoryFolderPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (invFolderx.AgentData.SessionID != SessionId ||
                        invFolderx.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                UpdateInventoryFolder handlerUpdateInventoryFolder = null;

                for (int i = 0; i < invFolderx.FolderData.Length; i++)
                {
                    handlerUpdateInventoryFolder = OnUpdateInventoryFolder;
                    if (handlerUpdateInventoryFolder != null)
                    {
                        OnUpdateInventoryFolder(this, invFolderx.FolderData[i].FolderID,
                                                (ushort)invFolderx.FolderData[i].Type,
                                                Util.FieldToString(invFolderx.FolderData[i].Name),
                                                invFolderx.FolderData[i].ParentID);
                    }
                }
            }
            return true;
        }

        private bool HandleMoveInventoryFolder(IClientAPI sender, Packet Pack)
        {
            if (OnMoveInventoryFolder != null)
            {
                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    MoveInventoryFolderPacket invFoldery = (MoveInventoryFolderPacket)Pack;

                    if (invFoldery.AgentData.SessionID != SessionId ||
                        invFoldery.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                Util.FireAndForget(MoveInventoryFolder, Pack);
            }
            return true;
        }

        private void MoveInventoryFolder(object Pack)
        {
            MoveInventoryFolderPacket invFoldery = (MoveInventoryFolderPacket)Pack;

            MoveInventoryFolder handlerMoveInventoryFolder = null;

            for (int i = 0; i < invFoldery.InventoryData.Length; i++)
            {
                handlerMoveInventoryFolder = OnMoveInventoryFolder;
                if (handlerMoveInventoryFolder != null)
                {
                    OnMoveInventoryFolder(this, invFoldery.InventoryData[i].FolderID,
                                          invFoldery.InventoryData[i].ParentID);
                }
            }
        }

        private bool HandleCreateInventoryItem(IClientAPI sender, Packet Pack)
        {
            CreateInventoryItemPacket createItem = (CreateInventoryItemPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (createItem.AgentData.SessionID != SessionId ||
                    createItem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            CreateNewInventoryItem handlerCreateNewInventoryItem = OnCreateNewInventoryItem;
            if (handlerCreateNewInventoryItem != null)
            {
                handlerCreateNewInventoryItem(this, createItem.InventoryBlock.TransactionID,
                                              createItem.InventoryBlock.FolderID,
                                              createItem.InventoryBlock.CallbackID,
                                              Util.FieldToString(createItem.InventoryBlock.Description),
                                              Util.FieldToString(createItem.InventoryBlock.Name),
                                              createItem.InventoryBlock.InvType,
                                              createItem.InventoryBlock.Type,
                                              createItem.InventoryBlock.WearableType,
                                              createItem.InventoryBlock.NextOwnerMask,
                                              Util.UnixTimeSinceEpoch());
            }

            return true;
        }

        private bool HandleLinkInventoryItem(IClientAPI sender, Packet Pack)
        {
            LinkInventoryItemPacket createLink = (LinkInventoryItemPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (createLink.AgentData.SessionID != SessionId ||
                    createLink.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            LinkInventoryItem linkInventoryItem = OnLinkInventoryItem;

            if (linkInventoryItem != null)
            {
                linkInventoryItem(
                    this,
                    createLink.InventoryBlock.TransactionID,
                    createLink.InventoryBlock.FolderID,
                    createLink.InventoryBlock.CallbackID,
                    Util.FieldToString(createLink.InventoryBlock.Description),
                    Util.FieldToString(createLink.InventoryBlock.Name),
                    createLink.InventoryBlock.InvType,
                    createLink.InventoryBlock.Type,
                    createLink.InventoryBlock.OldItemID);
            }

            return true;
        }

        private bool HandleFetchInventory(IClientAPI sender, Packet Pack)
        {
            if (OnFetchInventory != null)
            {
                FetchInventoryPacket FetchInventoryx = (FetchInventoryPacket)Pack;

                #region Packet Session and User Check
                if (m_checkPackets)
                {
                    if (FetchInventoryx.AgentData.SessionID != SessionId ||
                        FetchInventoryx.AgentData.AgentID != AgentId)
                        return true;
                }
                #endregion

                FetchInventory handlerFetchInventory = null;
                for (int i = 0; i < FetchInventoryx.InventoryData.Length; i++)
                {
                    handlerFetchInventory = OnFetchInventory;

                    if (handlerFetchInventory != null)
                    {
                        OnFetchInventory(this, FetchInventoryx.InventoryData[i].ItemID,
                                         FetchInventoryx.InventoryData[i].OwnerID);
                    }
                }
            }
            return true;
        }

        private bool HandleFetchInventoryDescendents(IClientAPI sender, Packet Pack)
        {
            FetchInventoryDescendentsPacket Fetch = (FetchInventoryDescendentsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (Fetch.AgentData.SessionID != SessionId ||
                    Fetch.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            FetchInventoryDescendents handlerFetchInventoryDescendents = OnFetchInventoryDescendents;
            if (handlerFetchInventoryDescendents != null)
            {
                handlerFetchInventoryDescendents(this, Fetch.InventoryData.FolderID, Fetch.InventoryData.OwnerID,
                                                 Fetch.InventoryData.FetchFolders, Fetch.InventoryData.FetchItems,
                                                 Fetch.InventoryData.SortOrder);
            }
            return true;
        }

        private bool HandlePurgeInventoryDescendents(IClientAPI sender, Packet Pack)
        {
            PurgeInventoryDescendentsPacket Purge = (PurgeInventoryDescendentsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (Purge.AgentData.SessionID != SessionId ||
                    Purge.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(PurgeInventoryDescendents, Pack);
            
            return true;
        }

        private void PurgeInventoryDescendents(object Pack)
        {
            PurgeInventoryDescendentsPacket Purge = (PurgeInventoryDescendentsPacket)Pack;

            PurgeInventoryDescendents handlerPurgeInventoryDescendents = OnPurgeInventoryDescendents;
            if (handlerPurgeInventoryDescendents != null)
            {
                handlerPurgeInventoryDescendents(this, Purge.InventoryData.FolderID);
            }
        }

        private bool HandleUpdateInventoryItem(IClientAPI sender, Packet Pack)
        {
            UpdateInventoryItemPacket inventoryItemUpdate = (UpdateInventoryItemPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (inventoryItemUpdate.AgentData.SessionID != SessionId ||
                    inventoryItemUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(UpdateInventoryItem, Pack);

            return true;
        }

        private void UpdateInventoryItem(object Pack)
        {
            UpdateInventoryItemPacket inventoryItemUpdate = (UpdateInventoryItemPacket)Pack;

            if (OnUpdateInventoryItem != null)
            {
                UpdateInventoryItem handlerUpdateInventoryItem = null;
                for (int i = 0; i < inventoryItemUpdate.InventoryData.Length; i++)
                {
                    handlerUpdateInventoryItem = OnUpdateInventoryItem;

                    if (handlerUpdateInventoryItem != null)
                    {
                        InventoryItemBase itemUpd = new InventoryItemBase();
                        itemUpd.ID = inventoryItemUpdate.InventoryData[i].ItemID;
                        itemUpd.Name = Util.FieldToString(inventoryItemUpdate.InventoryData[i].Name);
                        itemUpd.Description = Util.FieldToString(inventoryItemUpdate.InventoryData[i].Description);
                        itemUpd.GroupID = inventoryItemUpdate.InventoryData[i].GroupID;
                        itemUpd.GroupOwned = inventoryItemUpdate.InventoryData[i].GroupOwned;
                        itemUpd.GroupPermissions = inventoryItemUpdate.InventoryData[i].GroupMask;
                        itemUpd.NextPermissions = inventoryItemUpdate.InventoryData[i].NextOwnerMask;
                        itemUpd.EveryOnePermissions = inventoryItemUpdate.InventoryData[i].EveryoneMask;
                        itemUpd.CreationDate = inventoryItemUpdate.InventoryData[i].CreationDate;
                        itemUpd.Folder = inventoryItemUpdate.InventoryData[i].FolderID;
                        itemUpd.InvType = inventoryItemUpdate.InventoryData[i].InvType;
                        itemUpd.SalePrice = inventoryItemUpdate.InventoryData[i].SalePrice;
                        itemUpd.SaleType = inventoryItemUpdate.InventoryData[i].SaleType;
                        itemUpd.Flags = inventoryItemUpdate.InventoryData[i].Flags;

                        OnUpdateInventoryItem(this, inventoryItemUpdate.InventoryData[i].TransactionID,
                                              inventoryItemUpdate.InventoryData[i].ItemID,
                                              itemUpd);
                    }
                }
            }
        }

        private bool HandleCopyInventoryItem(IClientAPI sender, Packet Pack)
        {
            CopyInventoryItemPacket copyitem = (CopyInventoryItemPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (copyitem.AgentData.SessionID != SessionId ||
                    copyitem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(CopyInventoryItem, Pack);
            
            return true;
        }

        private void CopyInventoryItem(object Pack)
        {
            CopyInventoryItemPacket copyitem = (CopyInventoryItemPacket)Pack;

            CopyInventoryItem handlerCopyInventoryItem = null;
            if (OnCopyInventoryItem != null)
            {
                foreach (CopyInventoryItemPacket.InventoryDataBlock datablock in copyitem.InventoryData)
                {
                    handlerCopyInventoryItem = OnCopyInventoryItem;
                    if (handlerCopyInventoryItem != null)
                    {
                        handlerCopyInventoryItem(this, datablock.CallbackID, datablock.OldAgentID,
                                                 datablock.OldItemID, datablock.NewFolderID,
                                                 Util.FieldToString(datablock.NewName));
                    }
                }
            }
        }

        private bool HandleMoveInventoryItem(IClientAPI sender, Packet Pack)
        {
            MoveInventoryItemPacket moveitem = (MoveInventoryItemPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (moveitem.AgentData.SessionID != SessionId ||
                    moveitem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(MoveInventoryItem, Pack);

            return true;
        }

        private bool HandleChangeInventoryItemFlags(IClientAPI sender, Packet Pack)
        {
            ChangeInventoryItemFlagsPacket inventoryItemUpdate = (ChangeInventoryItemFlagsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (inventoryItemUpdate.AgentData.SessionID != SessionId ||
                    inventoryItemUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (OnChangeInventoryItemFlags != null)
            {
                ChangeInventoryItemFlags handlerUpdateInventoryItem = null;
                for (int i = 0; i < inventoryItemUpdate.InventoryData.Length; i++)
                {
                    handlerUpdateInventoryItem = OnChangeInventoryItemFlags;

                    if (handlerUpdateInventoryItem != null)
                    {
                        OnChangeInventoryItemFlags(this,
                                              inventoryItemUpdate.InventoryData[i].ItemID,
                                              inventoryItemUpdate.InventoryData[i].Flags);
                    }
                }
            }

            return true;
        }

        private void MoveInventoryItem(object Pack)
        {
            MoveInventoryItemPacket moveitem = (MoveInventoryItemPacket)Pack;

            if (OnMoveInventoryItem != null)
            {
                MoveInventoryItem handlerMoveInventoryItem = null;
                InventoryItemBase itm = null;
                List<InventoryItemBase> items = new List<InventoryItemBase>();
                foreach (MoveInventoryItemPacket.InventoryDataBlock datablock in moveitem.InventoryData)
                {
                    itm = new InventoryItemBase(datablock.ItemID, AgentId);
                    itm.Folder = datablock.FolderID;
                    itm.Name = Util.FieldToString(datablock.NewName);
                    // weird, comes out as empty string
                    //m_log.DebugFormat("[XXX] new name: {0}", itm.Name);
                    items.Add(itm);
                }
                handlerMoveInventoryItem = OnMoveInventoryItem;
                if (handlerMoveInventoryItem != null)
                {
                    handlerMoveInventoryItem(this, items);
                }
            }
        }

        private bool HandleRemoveInventoryItem(IClientAPI sender, Packet Pack)
        {
            RemoveInventoryItemPacket removeItem = (RemoveInventoryItemPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (removeItem.AgentData.SessionID != SessionId ||
                    removeItem.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(RemoveInventoryItem, Pack);

            return true;
        }

        private void RemoveInventoryItem(object Pack)
        {
            RemoveInventoryItemPacket removeItem = (RemoveInventoryItemPacket)Pack;

            if (OnRemoveInventoryItem != null)
            {
                RemoveInventoryItem handlerRemoveInventoryItem = null;
                List<UUID> uuids = new List<UUID>();
                foreach (RemoveInventoryItemPacket.InventoryDataBlock datablock in removeItem.InventoryData)
                {
                    uuids.Add(datablock.ItemID);
                }
                handlerRemoveInventoryItem = OnRemoveInventoryItem;
                if (handlerRemoveInventoryItem != null)
                {
                    handlerRemoveInventoryItem(this, uuids);
                }

            }
        }

        private bool HandleRemoveInventoryFolder(IClientAPI sender, Packet Pack)
        {
            RemoveInventoryFolderPacket removeFolder = (RemoveInventoryFolderPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (removeFolder.AgentData.SessionID != SessionId ||
                    removeFolder.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(RemoveInventoryFolder, Pack);
            
            return true;
        }

        private void RemoveInventoryFolder(object Pack)
        {
            RemoveInventoryFolderPacket removeFolder = (RemoveInventoryFolderPacket)Pack;

            if (OnRemoveInventoryFolder != null)
            {
                RemoveInventoryFolder handlerRemoveInventoryFolder = null;
                List<UUID> uuids = new List<UUID>();
                foreach (RemoveInventoryFolderPacket.FolderDataBlock datablock in removeFolder.FolderData)
                {
                    uuids.Add(datablock.FolderID);
                }
                handlerRemoveInventoryFolder = OnRemoveInventoryFolder;
                if (handlerRemoveInventoryFolder != null)
                {
                    handlerRemoveInventoryFolder(this, uuids);
                }
            }
        }

        private bool HandleRemoveInventoryObjects(IClientAPI sender, Packet Pack)
        {
            RemoveInventoryObjectsPacket removeObject = (RemoveInventoryObjectsPacket)Pack;
            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (removeObject.AgentData.SessionID != SessionId ||
                    removeObject.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            Util.FireAndForget(RemoveInventoryObjects, Pack);
            
            return true;
        }

        private void RemoveInventoryObjects(object Pack)
        {
            RemoveInventoryObjectsPacket removeObject = (RemoveInventoryObjectsPacket)Pack;
            
            if (OnRemoveInventoryFolder != null)
            {
                RemoveInventoryFolder handlerRemoveInventoryFolder = null;
                List<UUID> uuids = new List<UUID>();
                foreach (RemoveInventoryObjectsPacket.FolderDataBlock datablock in removeObject.FolderData)
                {
                    uuids.Add(datablock.FolderID);
                }
                handlerRemoveInventoryFolder = OnRemoveInventoryFolder;
                if (handlerRemoveInventoryFolder != null)
                {
                    handlerRemoveInventoryFolder(this, uuids);
                }
            }

            if (OnRemoveInventoryItem != null)
            {
                RemoveInventoryItem handlerRemoveInventoryItem = null;
                List<UUID> uuids = new List<UUID>();
                foreach (RemoveInventoryObjectsPacket.ItemDataBlock datablock in removeObject.ItemData)
                {
                    uuids.Add(datablock.ItemID);
                }
                handlerRemoveInventoryItem = OnRemoveInventoryItem;
                if (handlerRemoveInventoryItem != null)
                {
                    handlerRemoveInventoryItem(this, uuids);
                }
            }
        }

        private bool HandleRequestTaskInventory(IClientAPI sender, Packet Pack)
        {
            RequestTaskInventoryPacket requesttask = (RequestTaskInventoryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (requesttask.AgentData.SessionID != SessionId ||
                    requesttask.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RequestTaskInventory handlerRequestTaskInventory = OnRequestTaskInventory;
            if (handlerRequestTaskInventory != null)
            {
                handlerRequestTaskInventory(this, requesttask.InventoryData.LocalID);
            }
            return true;
        }

        private bool HandleUpdateTaskInventory(IClientAPI sender, Packet Pack)
        {
            UpdateTaskInventoryPacket updatetask = (UpdateTaskInventoryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (updatetask.AgentData.SessionID != SessionId ||
                    updatetask.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (OnUpdateTaskInventory != null)
            {
                if (updatetask.UpdateData.Key == 0)
                {
                    UpdateTaskInventory handlerUpdateTaskInventory = OnUpdateTaskInventory;
                    if (handlerUpdateTaskInventory != null)
                    {
                        TaskInventoryItem newTaskItem = new TaskInventoryItem();
                        newTaskItem.ItemID = updatetask.InventoryData.ItemID;
                        newTaskItem.ParentID = updatetask.InventoryData.FolderID;
                        newTaskItem.CreatorID = updatetask.InventoryData.CreatorID;
                        newTaskItem.OwnerID = updatetask.InventoryData.OwnerID;
                        newTaskItem.GroupID = updatetask.InventoryData.GroupID;
                        newTaskItem.BasePermissions = updatetask.InventoryData.BaseMask;
                        newTaskItem.CurrentPermissions = updatetask.InventoryData.OwnerMask;
                        newTaskItem.GroupPermissions = updatetask.InventoryData.GroupMask;
                        newTaskItem.EveryonePermissions = updatetask.InventoryData.EveryoneMask;
                        newTaskItem.NextPermissions = updatetask.InventoryData.NextOwnerMask;

                        // Unused?  Clicking share with group sets GroupPermissions instead, so perhaps this is something
                        // different
                        //newTaskItem.GroupOwned=updatetask.InventoryData.GroupOwned;
                        newTaskItem.Type = updatetask.InventoryData.Type;
                        newTaskItem.InvType = updatetask.InventoryData.InvType;
                        newTaskItem.Flags = updatetask.InventoryData.Flags;
                        newTaskItem.SaleType = updatetask.InventoryData.SaleType;
                        newTaskItem.SalePrice = updatetask.InventoryData.SalePrice;
                        newTaskItem.Name = Util.FieldToString(updatetask.InventoryData.Name);
                        newTaskItem.Description = Util.FieldToString(updatetask.InventoryData.Description);
                        newTaskItem.CreationDate = (uint)updatetask.InventoryData.CreationDate;
                        handlerUpdateTaskInventory(this, updatetask.InventoryData.TransactionID,
                                                   newTaskItem, updatetask.UpdateData.LocalID);
                    }
                }
            }

            return true;
        }

        private bool HandleRemoveTaskInventory(IClientAPI sender, Packet Pack)
        {
            RemoveTaskInventoryPacket removeTask = (RemoveTaskInventoryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (removeTask.AgentData.SessionID != SessionId ||
                    removeTask.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RemoveTaskInventory handlerRemoveTaskItem = OnRemoveTaskItem;

            if (handlerRemoveTaskItem != null)
            {
                handlerRemoveTaskItem(this, removeTask.InventoryData.ItemID, removeTask.InventoryData.LocalID);
            }

            return true;
        }

        private bool HandleMoveTaskInventory(IClientAPI sender, Packet Pack)
        {
            MoveTaskInventoryPacket moveTaskInventoryPacket = (MoveTaskInventoryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (moveTaskInventoryPacket.AgentData.SessionID != SessionId ||
                    moveTaskInventoryPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            MoveTaskInventory handlerMoveTaskItem = OnMoveTaskItem;

            if (handlerMoveTaskItem != null)
            {
                handlerMoveTaskItem(
                    this, moveTaskInventoryPacket.AgentData.FolderID,
                    moveTaskInventoryPacket.InventoryData.LocalID,
                    moveTaskInventoryPacket.InventoryData.ItemID);
            }

            return true;
        }

        private bool HandleRezScript(IClientAPI sender, Packet Pack)
        {
            //m_log.Debug(Pack.ToString());
            RezScriptPacket rezScriptx = (RezScriptPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rezScriptx.AgentData.SessionID != SessionId ||
                    rezScriptx.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RezScript handlerRezScript = OnRezScript;
            InventoryItemBase item = new InventoryItemBase();
            item.ID = rezScriptx.InventoryBlock.ItemID;
            item.Folder = rezScriptx.InventoryBlock.FolderID;
            item.CreatorId = rezScriptx.InventoryBlock.CreatorID.ToString();
            item.Owner = rezScriptx.InventoryBlock.OwnerID;
            item.BasePermissions = rezScriptx.InventoryBlock.BaseMask;
            item.CurrentPermissions = rezScriptx.InventoryBlock.OwnerMask;
            item.EveryOnePermissions = rezScriptx.InventoryBlock.EveryoneMask;
            item.NextPermissions = rezScriptx.InventoryBlock.NextOwnerMask;
            item.GroupPermissions = rezScriptx.InventoryBlock.GroupMask;
            item.GroupOwned = rezScriptx.InventoryBlock.GroupOwned;
            item.GroupID = rezScriptx.InventoryBlock.GroupID;
            item.AssetType = rezScriptx.InventoryBlock.Type;
            item.InvType = rezScriptx.InventoryBlock.InvType;
            item.Flags = rezScriptx.InventoryBlock.Flags;
            item.SaleType = rezScriptx.InventoryBlock.SaleType;
            item.SalePrice = rezScriptx.InventoryBlock.SalePrice;
            item.Name = Util.FieldToString(rezScriptx.InventoryBlock.Name);
            item.Description = Util.FieldToString(rezScriptx.InventoryBlock.Description);
            item.CreationDate = rezScriptx.InventoryBlock.CreationDate;

            if (handlerRezScript != null)
            {
                handlerRezScript(this, item, rezScriptx.InventoryBlock.TransactionID, rezScriptx.UpdateBlock.ObjectLocalID);
            }
            return true;
        }

        private bool HandleMapLayerRequest(IClientAPI sender, Packet Pack)
        {
            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if ((Pack as MapLayerRequestPacket).AgentData.SessionID != SessionId ||
                    (Pack as MapLayerRequestPacket).AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            return true;
        }

        private bool HandleMapBlockRequest(IClientAPI sender, Packet Pack)
        {
            MapBlockRequestPacket MapRequest = (MapBlockRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (MapRequest.AgentData.SessionID != SessionId ||
                    MapRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RequestMapBlocks handlerRequestMapBlocks = OnRequestMapBlocks;
            if (handlerRequestMapBlocks != null)
            {
                handlerRequestMapBlocks(this, MapRequest.PositionData.MinX, MapRequest.PositionData.MinY,
                                        MapRequest.PositionData.MaxX, MapRequest.PositionData.MaxY, MapRequest.AgentData.Flags);
            }
            return true;
        }

        private bool HandleMapNameRequest(IClientAPI sender, Packet Pack)
        {
            MapNameRequestPacket map = (MapNameRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (map.AgentData.SessionID != SessionId ||
                    map.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            string mapName = Util.UTF8.GetString(map.NameData.Name, 0,
                                                     map.NameData.Name.Length - 1);
            RequestMapName handlerMapNameRequest = OnMapNameRequest;
            if (handlerMapNameRequest != null)
            {
                handlerMapNameRequest(this, mapName);
            }
            return true;
        }

        private bool HandleTeleportLandmarkRequest(IClientAPI sender, Packet Pack)
        {
            TeleportLandmarkRequestPacket tpReq = (TeleportLandmarkRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (tpReq.Info.SessionID != SessionId ||
                    tpReq.Info.AgentID != AgentId)
                    return true;
            }
            #endregion

            UUID lmid = tpReq.Info.LandmarkID;
            AssetLandmark lm;
            if (lmid != UUID.Zero)
            {
                //AssetBase lma = m_assetCache.GetAsset(lmid, false);
                AssetBase lma = m_assetService.Get(lmid.ToString());

                if (lma == null)
                {
                    // Failed to find landmark
                    TeleportCancelPacket tpCancel = (TeleportCancelPacket)PacketPool.Instance.GetPacket(PacketType.TeleportCancel);
                    tpCancel.Info.SessionID = tpReq.Info.SessionID;
                    tpCancel.Info.AgentID = tpReq.Info.AgentID;
                    OutPacket(tpCancel, ThrottleOutPacketType.Asset);
                }

                try
                {
                    lm = new AssetLandmark(lma);
                }
                catch (NullReferenceException)
                {
                    // asset not found generates null ref inside the assetlandmark constructor.
                    TeleportCancelPacket tpCancel = (TeleportCancelPacket)PacketPool.Instance.GetPacket(PacketType.TeleportCancel);
                    tpCancel.Info.SessionID = tpReq.Info.SessionID;
                    tpCancel.Info.AgentID = tpReq.Info.AgentID;
                    OutPacket(tpCancel, ThrottleOutPacketType.Asset);
                    return true;
                }
            }
            else
            {
                // Teleport home request
                UUIDNameRequest handlerTeleportHomeRequest = OnTeleportHomeRequest;
                if (handlerTeleportHomeRequest != null)
                {
                    handlerTeleportHomeRequest(AgentId, this);
                }
                return true;
            }

            TeleportLandmarkRequest handlerTeleportLandmarkRequest = OnTeleportLandmarkRequest;
            if (handlerTeleportLandmarkRequest != null)
            {
                handlerTeleportLandmarkRequest(this, lm.RegionID, lm.Position);
            }
            else
            {
                //no event handler so cancel request


                TeleportCancelPacket tpCancel = (TeleportCancelPacket)PacketPool.Instance.GetPacket(PacketType.TeleportCancel);
                tpCancel.Info.AgentID = tpReq.Info.AgentID;
                tpCancel.Info.SessionID = tpReq.Info.SessionID;
                OutPacket(tpCancel, ThrottleOutPacketType.Asset);

            }
            return true;
        }

        private bool HandleTeleportLocationRequest(IClientAPI sender, Packet Pack)
        {
            TeleportLocationRequestPacket tpLocReq = (TeleportLocationRequestPacket)Pack;
            // m_log.Debug(tpLocReq.ToString());

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (tpLocReq.AgentData.SessionID != SessionId ||
                    tpLocReq.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            TeleportLocationRequest handlerTeleportLocationRequest = OnTeleportLocationRequest;
            if (handlerTeleportLocationRequest != null)
            {
                handlerTeleportLocationRequest(this, tpLocReq.Info.RegionHandle, tpLocReq.Info.Position,
                                               tpLocReq.Info.LookAt, 16);
            }
            else
            {
                //no event handler so cancel request
                TeleportCancelPacket tpCancel = (TeleportCancelPacket)PacketPool.Instance.GetPacket(PacketType.TeleportCancel);
                tpCancel.Info.SessionID = tpLocReq.AgentData.SessionID;
                tpCancel.Info.AgentID = tpLocReq.AgentData.AgentID;
                OutPacket(tpCancel, ThrottleOutPacketType.Asset);
            }
            return true;
        }

        #endregion Inventory/Asset/Other related packets

        private bool HandleUUIDNameRequest(IClientAPI sender, Packet Pack)
        {
            UUIDNameRequestPacket incoming = (UUIDNameRequestPacket)Pack;

            foreach (UUIDNameRequestPacket.UUIDNameBlockBlock UUIDBlock in incoming.UUIDNameBlock)
            {
                UUIDNameRequest handlerNameRequest = OnNameFromUUIDRequest;
                if (handlerNameRequest != null)
                {
                    handlerNameRequest(UUIDBlock.ID, this);
                }
            }
            return true;
        }

        #region Parcel related packets

        private bool HandleRegionHandleRequest(IClientAPI sender, Packet Pack)
        {
            RegionHandleRequestPacket rhrPack = (RegionHandleRequestPacket)Pack;

            RegionHandleRequest handlerRegionHandleRequest = OnRegionHandleRequest;
            if (handlerRegionHandleRequest != null)
            {
                handlerRegionHandleRequest(this, rhrPack.RequestBlock.RegionID);
            }
            return true;
        }

        private bool HandleParcelInfoRequest(IClientAPI sender, Packet Pack)
        {
            ParcelInfoRequestPacket pirPack = (ParcelInfoRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (pirPack.AgentData.SessionID != SessionId ||
                    pirPack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelInfoRequest handlerParcelInfoRequest = OnParcelInfoRequest;
            if (handlerParcelInfoRequest != null)
            {
                handlerParcelInfoRequest(this, pirPack.Data.ParcelID);
            }
            return true;
        }

        private bool HandleParcelAccessListRequest(IClientAPI sender, Packet Pack)
        {
            ParcelAccessListRequestPacket requestPacket = (ParcelAccessListRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (requestPacket.AgentData.SessionID != SessionId ||
                    requestPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelAccessListRequest handlerParcelAccessListRequest = OnParcelAccessListRequest;

            if (handlerParcelAccessListRequest != null)
            {
                handlerParcelAccessListRequest(requestPacket.AgentData.AgentID, requestPacket.AgentData.SessionID,
                                               requestPacket.Data.Flags, requestPacket.Data.SequenceID,
                                               requestPacket.Data.LocalID, this);
            }
            return true;
        }

        private bool HandleParcelAccessListUpdate(IClientAPI sender, Packet Pack)
        {
            ParcelAccessListUpdatePacket updatePacket = (ParcelAccessListUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (updatePacket.AgentData.SessionID != SessionId ||
                    updatePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            List<ParcelManager.ParcelAccessEntry> entries = new List<ParcelManager.ParcelAccessEntry>();
            foreach (ParcelAccessListUpdatePacket.ListBlock block in updatePacket.List)
            {
                ParcelManager.ParcelAccessEntry entry = new ParcelManager.ParcelAccessEntry();
                entry.AgentID = block.ID;
                entry.Flags = (AccessList)block.Flags;
                entry.Time = new DateTime();
                entries.Add(entry);
            }

            ParcelAccessListUpdateRequest handlerParcelAccessListUpdateRequest = OnParcelAccessListUpdateRequest;
            if (handlerParcelAccessListUpdateRequest != null)
            {
                handlerParcelAccessListUpdateRequest(updatePacket.AgentData.AgentID,
                                                     updatePacket.AgentData.SessionID, updatePacket.Data.Flags,
                                                     updatePacket.Data.LocalID, entries, this);
            }
            return true;
        }

        private bool HandleParcelPropertiesRequest(IClientAPI sender, Packet Pack)
        {
            ParcelPropertiesRequestPacket propertiesRequest = (ParcelPropertiesRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (propertiesRequest.AgentData.SessionID != SessionId ||
                    propertiesRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelPropertiesRequest handlerParcelPropertiesRequest = OnParcelPropertiesRequest;
            if (handlerParcelPropertiesRequest != null)
            {
                handlerParcelPropertiesRequest((int)Math.Round(propertiesRequest.ParcelData.West),
                                               (int)Math.Round(propertiesRequest.ParcelData.South),
                                               (int)Math.Round(propertiesRequest.ParcelData.East),
                                               (int)Math.Round(propertiesRequest.ParcelData.North),
                                               propertiesRequest.ParcelData.SequenceID,
                                               propertiesRequest.ParcelData.SnapSelection, this);
            }
            return true;
        }

        private bool HandleParcelDivide(IClientAPI sender, Packet Pack)
        {
            ParcelDividePacket landDivide = (ParcelDividePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (landDivide.AgentData.SessionID != SessionId ||
                    landDivide.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelDivideRequest handlerParcelDivideRequest = OnParcelDivideRequest;
            if (handlerParcelDivideRequest != null)
            {
                handlerParcelDivideRequest((int)Math.Round(landDivide.ParcelData.West),
                                           (int)Math.Round(landDivide.ParcelData.South),
                                           (int)Math.Round(landDivide.ParcelData.East),
                                           (int)Math.Round(landDivide.ParcelData.North), this);
            }
            return true;
        }

        private bool HandleParcelJoin(IClientAPI sender, Packet Pack)
        {
            ParcelJoinPacket landJoin = (ParcelJoinPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (landJoin.AgentData.SessionID != SessionId ||
                    landJoin.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelJoinRequest handlerParcelJoinRequest = OnParcelJoinRequest;

            if (handlerParcelJoinRequest != null)
            {
                handlerParcelJoinRequest((int)Math.Round(landJoin.ParcelData.West),
                                         (int)Math.Round(landJoin.ParcelData.South),
                                         (int)Math.Round(landJoin.ParcelData.East),
                                         (int)Math.Round(landJoin.ParcelData.North), this);
            }
            return true;
        }

        private bool HandleParcelPropertiesUpdate(IClientAPI sender, Packet Pack)
        {
            ParcelPropertiesUpdatePacket parcelPropertiesPacket = (ParcelPropertiesUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (parcelPropertiesPacket.AgentData.SessionID != SessionId ||
                    parcelPropertiesPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelPropertiesUpdateRequest handlerParcelPropertiesUpdateRequest = OnParcelPropertiesUpdateRequest;

            if (handlerParcelPropertiesUpdateRequest != null)
            {
                LandUpdateArgs args = new LandUpdateArgs();

                args.AuthBuyerID = parcelPropertiesPacket.ParcelData.AuthBuyerID;
                args.Category = (ParcelCategory)parcelPropertiesPacket.ParcelData.Category;
                args.Desc = Utils.BytesToString(parcelPropertiesPacket.ParcelData.Desc);
                args.GroupID = parcelPropertiesPacket.ParcelData.GroupID;
                args.LandingType = parcelPropertiesPacket.ParcelData.LandingType;
                args.MediaAutoScale = parcelPropertiesPacket.ParcelData.MediaAutoScale;
                args.MediaID = parcelPropertiesPacket.ParcelData.MediaID;
                args.MediaURL = Utils.BytesToString(parcelPropertiesPacket.ParcelData.MediaURL);
                args.MusicURL = Utils.BytesToString(parcelPropertiesPacket.ParcelData.MusicURL);
                args.Name = Utils.BytesToString(parcelPropertiesPacket.ParcelData.Name);
                args.ParcelFlags = parcelPropertiesPacket.ParcelData.ParcelFlags;
                args.PassHours = parcelPropertiesPacket.ParcelData.PassHours;
                args.PassPrice = parcelPropertiesPacket.ParcelData.PassPrice;
                args.SalePrice = parcelPropertiesPacket.ParcelData.SalePrice;
                args.SnapshotID = parcelPropertiesPacket.ParcelData.SnapshotID;
                args.UserLocation = parcelPropertiesPacket.ParcelData.UserLocation;
                args.UserLookAt = parcelPropertiesPacket.ParcelData.UserLookAt;
                handlerParcelPropertiesUpdateRequest(args, parcelPropertiesPacket.ParcelData.LocalID, this);
            }
            return true;
        }

        public void FireUpdateParcel(LandUpdateArgs args, int LocalID)
        {
            ParcelPropertiesUpdateRequest handlerParcelPropertiesUpdateRequest = OnParcelPropertiesUpdateRequest;

            if (handlerParcelPropertiesUpdateRequest != null)
            {
                handlerParcelPropertiesUpdateRequest(args, LocalID, this);
            }
        }

        private bool HandleParcelSelectObjects(IClientAPI sender, Packet Pack)
        {
            ParcelSelectObjectsPacket selectPacket = (ParcelSelectObjectsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (selectPacket.AgentData.SessionID != SessionId ||
                    selectPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            List<UUID> returnIDs = new List<UUID>();

            foreach (ParcelSelectObjectsPacket.ReturnIDsBlock rb in
                     selectPacket.ReturnIDs)
            {
                returnIDs.Add(rb.ReturnID);
            }

            ParcelSelectObjects handlerParcelSelectObjects = OnParcelSelectObjects;

            if (handlerParcelSelectObjects != null)
            {
                handlerParcelSelectObjects(selectPacket.ParcelData.LocalID,
                                           Convert.ToInt32(selectPacket.ParcelData.ReturnType), returnIDs, this);
            }
            return true;
        }

        private bool HandleParcelObjectOwnersRequest(IClientAPI sender, Packet Pack)
        {
            ParcelObjectOwnersRequestPacket reqPacket = (ParcelObjectOwnersRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (reqPacket.AgentData.SessionID != SessionId ||
                    reqPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelObjectOwnerRequest handlerParcelObjectOwnerRequest = OnParcelObjectOwnerRequest;

            if (handlerParcelObjectOwnerRequest != null)
            {
                handlerParcelObjectOwnerRequest(reqPacket.ParcelData.LocalID, this);
            }
            return true;

        }

        private bool HandleParcelGodForceOwner(IClientAPI sender, Packet Pack)
        {
            ParcelGodForceOwnerPacket godForceOwnerPacket = (ParcelGodForceOwnerPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (godForceOwnerPacket.AgentData.SessionID != SessionId ||
                    godForceOwnerPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelGodForceOwner handlerParcelGodForceOwner = OnParcelGodForceOwner;
            if (handlerParcelGodForceOwner != null)
            {
                handlerParcelGodForceOwner(godForceOwnerPacket.Data.LocalID, godForceOwnerPacket.Data.OwnerID, this);
            }
            return true;
        }

        private bool HandleParcelRelease(IClientAPI sender, Packet Pack)
        {
            ParcelReleasePacket releasePacket = (ParcelReleasePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (releasePacket.AgentData.SessionID != SessionId ||
                    releasePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelAbandonRequest handlerParcelAbandonRequest = OnParcelAbandonRequest;
            if (handlerParcelAbandonRequest != null)
            {
                handlerParcelAbandonRequest(releasePacket.Data.LocalID, this);
            }
            return true;
        }

        private bool HandleParcelReclaim(IClientAPI sender, Packet Pack)
        {
            ParcelReclaimPacket reclaimPacket = (ParcelReclaimPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (reclaimPacket.AgentData.SessionID != SessionId ||
                    reclaimPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelReclaim handlerParcelReclaim = OnParcelReclaim;
            if (handlerParcelReclaim != null)
            {
                handlerParcelReclaim(reclaimPacket.Data.LocalID, this);
            }
            return true;
        }

        private bool HandleParcelReturnObjects(IClientAPI sender, Packet Pack)
        {
            ParcelReturnObjectsPacket parcelReturnObjects = (ParcelReturnObjectsPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (parcelReturnObjects.AgentData.SessionID != SessionId ||
                    parcelReturnObjects.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            UUID[] puserselectedOwnerIDs = new UUID[parcelReturnObjects.OwnerIDs.Length];
            for (int parceliterator = 0; parceliterator < parcelReturnObjects.OwnerIDs.Length; parceliterator++)
                puserselectedOwnerIDs[parceliterator] = parcelReturnObjects.OwnerIDs[parceliterator].OwnerID;

            UUID[] puserselectedTaskIDs = new UUID[parcelReturnObjects.TaskIDs.Length];

            for (int parceliterator = 0; parceliterator < parcelReturnObjects.TaskIDs.Length; parceliterator++)
                puserselectedTaskIDs[parceliterator] = parcelReturnObjects.TaskIDs[parceliterator].TaskID;

            ParcelReturnObjectsRequest handlerParcelReturnObjectsRequest = OnParcelReturnObjectsRequest;
            if (handlerParcelReturnObjectsRequest != null)
            {
                handlerParcelReturnObjectsRequest(parcelReturnObjects.ParcelData.LocalID, parcelReturnObjects.ParcelData.ReturnType, puserselectedOwnerIDs, puserselectedTaskIDs, this);

            }
            return true;
        }

        private bool HandleParcelSetOtherCleanTime(IClientAPI sender, Packet Pack)
        {
            ParcelSetOtherCleanTimePacket parcelSetOtherCleanTimePacket = (ParcelSetOtherCleanTimePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (parcelSetOtherCleanTimePacket.AgentData.SessionID != SessionId ||
                    parcelSetOtherCleanTimePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelSetOtherCleanTime handlerParcelSetOtherCleanTime = OnParcelSetOtherCleanTime;
            if (handlerParcelSetOtherCleanTime != null)
            {
                handlerParcelSetOtherCleanTime(this,
                                               parcelSetOtherCleanTimePacket.ParcelData.LocalID,
                                               parcelSetOtherCleanTimePacket.ParcelData.OtherCleanTime);
            }
            return true;
        }

        private bool HandleLandStatRequest(IClientAPI sender, Packet Pack)
        {
            LandStatRequestPacket lsrp = (LandStatRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (lsrp.AgentData.SessionID != SessionId ||
                    lsrp.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GodLandStatRequest handlerLandStatRequest = OnLandStatRequest;
            if (handlerLandStatRequest != null)
            {
                handlerLandStatRequest(lsrp.RequestData.ParcelLocalID, lsrp.RequestData.ReportType, lsrp.RequestData.RequestFlags, Utils.BytesToString(lsrp.RequestData.Filter), this);
            }
            return true;
        }

        private bool HandleParcelDwellRequest(IClientAPI sender, Packet Pack)
        {
            ParcelDwellRequestPacket dwellrq =
                            (ParcelDwellRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dwellrq.AgentData.SessionID != SessionId ||
                    dwellrq.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ParcelDwellRequest handlerParcelDwellRequest = OnParcelDwellRequest;
            if (handlerParcelDwellRequest != null)
            {
                handlerParcelDwellRequest(dwellrq.Data.LocalID, this);
            }
            return true;
        }

        #endregion Parcel related packets

        #region Estate Packets

        private bool HandleEstateOwnerMessage(IClientAPI sender, Packet Pack)
        {
            EstateOwnerMessagePacket messagePacket = (EstateOwnerMessagePacket)Pack;
            //m_log.Debug(messagePacket.ToString());
            GodLandStatRequest handlerLandStatRequest;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (messagePacket.AgentData.SessionID != SessionId ||
                    messagePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            switch (Utils.BytesToString(messagePacket.MethodData.Method))
            {
                case "getinfo":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        OnDetailedEstateDataRequest(this, messagePacket.MethodData.Invoice);
                    }
                    return true;
                case "setregioninfo":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        OnSetEstateFlagsRequest(this, convertParamStringToBool(messagePacket.ParamList[0].Parameter), convertParamStringToBool(messagePacket.ParamList[1].Parameter),
                                                convertParamStringToBool(messagePacket.ParamList[2].Parameter), convertParamStringToBool(messagePacket.ParamList[3].Parameter),
                                                Convert.ToInt16(Convert.ToDecimal(Utils.BytesToString(messagePacket.ParamList[4].Parameter), Culture.NumberFormatInfo)),
                                                (float)Convert.ToDecimal(Utils.BytesToString(messagePacket.ParamList[5].Parameter), Culture.NumberFormatInfo),
                                                Convert.ToInt16(Utils.BytesToString(messagePacket.ParamList[6].Parameter)),
                                                convertParamStringToBool(messagePacket.ParamList[7].Parameter), convertParamStringToBool(messagePacket.ParamList[8].Parameter));
                    }
                    return true;
                //                            case "texturebase":
                //                                if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                //                                {
                //                                    foreach (EstateOwnerMessagePacket.ParamListBlock block in messagePacket.ParamList)
                //                                    {
                //                                        string s = Utils.BytesToString(block.Parameter);
                //                                        string[] splitField = s.Split(' ');
                //                                        if (splitField.Length == 2)
                //                                        {
                //                                            UUID tempUUID = new UUID(splitField[1]);
                //                                            OnSetEstateTerrainBaseTexture(this, Convert.ToInt16(splitField[0]), tempUUID);
                //                                        }
                //                                    }
                //                                }
                //                                break;
                case "texturedetail":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        foreach (EstateOwnerMessagePacket.ParamListBlock block in messagePacket.ParamList)
                        {
                            string s = Utils.BytesToString(block.Parameter);
                            string[] splitField = s.Split(' ');
                            if (splitField.Length == 2)
                            {
                                Int16 corner = Convert.ToInt16(splitField[0]);
                                UUID textureUUID = new UUID(splitField[1]);

                                OnSetEstateTerrainDetailTexture(this, corner, textureUUID);
                            }
                        }
                    }

                    return true;
                case "textureheights":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        foreach (EstateOwnerMessagePacket.ParamListBlock block in messagePacket.ParamList)
                        {
                            string s = Utils.BytesToString(block.Parameter);
                            string[] splitField = s.Split(' ');
                            if (splitField.Length == 3)
                            {
                                Int16 corner = Convert.ToInt16(splitField[0]);
                                float lowValue = (float)Convert.ToDecimal(splitField[1], Culture.NumberFormatInfo);
                                float highValue = (float)Convert.ToDecimal(splitField[2], Culture.NumberFormatInfo);

                                OnSetEstateTerrainTextureHeights(this, corner, lowValue, highValue);
                            }
                        }
                    }
                    return true;
                case "texturecommit":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        OnCommitEstateTerrainTextureRequest(this);
                    }
                    return true;
                case "setregionterrain":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        if (messagePacket.ParamList.Length != 9)
                        {
                            m_log.Error("EstateOwnerMessage: SetRegionTerrain method has a ParamList of invalid length");
                        }
                        else
                        {
                            try
                            {
                                string tmp = Utils.BytesToString(messagePacket.ParamList[0].Parameter);
                                if (!tmp.Contains(".")) tmp += ".00";
                                float WaterHeight = (float)Convert.ToDecimal(tmp, Culture.NumberFormatInfo);
                                tmp = Utils.BytesToString(messagePacket.ParamList[1].Parameter);
                                if (!tmp.Contains(".")) tmp += ".00";
                                float TerrainRaiseLimit = (float)Convert.ToDecimal(tmp, Culture.NumberFormatInfo);
                                tmp = Utils.BytesToString(messagePacket.ParamList[2].Parameter);
                                if (!tmp.Contains(".")) tmp += ".00";
                                float TerrainLowerLimit = (float)Convert.ToDecimal(tmp, Culture.NumberFormatInfo);
                                bool UseEstateSun = convertParamStringToBool(messagePacket.ParamList[3].Parameter);
                                bool UseFixedSun = convertParamStringToBool(messagePacket.ParamList[4].Parameter);
                                float SunHour = (float)Convert.ToDecimal(Utils.BytesToString(messagePacket.ParamList[5].Parameter), Culture.NumberFormatInfo);
                                bool UseGlobal = convertParamStringToBool(messagePacket.ParamList[6].Parameter);
                                bool EstateFixedSun = convertParamStringToBool(messagePacket.ParamList[7].Parameter);
                                float EstateSunHour = (float)Convert.ToDecimal(Utils.BytesToString(messagePacket.ParamList[8].Parameter), Culture.NumberFormatInfo);

                                OnSetRegionTerrainSettings(WaterHeight, TerrainRaiseLimit, TerrainLowerLimit, UseEstateSun, UseFixedSun, SunHour, UseGlobal, EstateFixedSun, EstateSunHour);

                            }
                            catch (Exception ex)
                            {
                                m_log.Error("EstateOwnerMessage: Exception while setting terrain settings: \n" + messagePacket + "\n" + ex);
                            }
                        }
                    }

                    return true;
                case "restart":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        // There's only 1 block in the estateResetSim..   and that's the number of seconds till restart.
                        foreach (EstateOwnerMessagePacket.ParamListBlock block in messagePacket.ParamList)
                        {
                            float timeSeconds;
                            Utils.TryParseSingle(Utils.BytesToString(block.Parameter), out timeSeconds);
                            timeSeconds = (int)timeSeconds;
                            OnEstateRestartSimRequest(this, (int)timeSeconds);

                        }
                    }
                    return true;
                case "estatechangecovenantid":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        foreach (EstateOwnerMessagePacket.ParamListBlock block in messagePacket.ParamList)
                        {
                            UUID newCovenantID = new UUID(Utils.BytesToString(block.Parameter));
                            OnEstateChangeCovenantRequest(this, newCovenantID);
                        }
                    }
                    return true;
                case "estateaccessdelta": // Estate access delta manages the banlist and allow list too.
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        int estateAccessType = Convert.ToInt16(Utils.BytesToString(messagePacket.ParamList[1].Parameter));
                        OnUpdateEstateAccessDeltaRequest(this, messagePacket.MethodData.Invoice, estateAccessType, new UUID(Utils.BytesToString(messagePacket.ParamList[2].Parameter)));

                    }
                    return true;
                case "simulatormessage":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID invoice = messagePacket.MethodData.Invoice;
                        UUID SenderID = new UUID(Utils.BytesToString(messagePacket.ParamList[2].Parameter));
                        string SenderName = Utils.BytesToString(messagePacket.ParamList[3].Parameter);
                        string Message = Utils.BytesToString(messagePacket.ParamList[4].Parameter);
                        UUID sessionID = messagePacket.AgentData.SessionID;
                        OnSimulatorBlueBoxMessageRequest(this, invoice, SenderID, sessionID, SenderName, Message);
                    }
                    return true;
                case "instantmessage":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID invoice = messagePacket.MethodData.Invoice; UUID sessionID = messagePacket.AgentData.SessionID;
                        string Message = "";
                        string SenderName = "";
                        UUID SenderID = UUID.Zero;
                        if (messagePacket.ParamList.Length < 5)
                        {
                            SenderName = Utils.BytesToString(messagePacket.ParamList[0].Parameter);
                            Message = Utils.BytesToString(messagePacket.ParamList[1].Parameter);
                            SenderID = AgentId;
                        }
                        else
                        {
                            SenderID = new UUID(Utils.BytesToString(messagePacket.ParamList[2].Parameter));
                            SenderName = Utils.BytesToString(messagePacket.ParamList[3].Parameter);
                            Message = Utils.BytesToString(messagePacket.ParamList[4].Parameter);
                        }
                        OnEstateBlueBoxMessageRequest(this, invoice, SenderID, sessionID, SenderName, Message);
                    }
                    return true;
                case "setregiondebug":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID invoice = messagePacket.MethodData.Invoice;
                        UUID SenderID = messagePacket.AgentData.AgentID;
                        bool scripted = convertParamStringToBool(messagePacket.ParamList[0].Parameter);
                        bool collisionEvents = convertParamStringToBool(messagePacket.ParamList[1].Parameter);
                        bool physics = convertParamStringToBool(messagePacket.ParamList[2].Parameter);

                        OnEstateDebugRegionRequest(this, invoice, SenderID, scripted, collisionEvents, physics);
                    }
                    return true;
                case "teleporthomeuser":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID invoice = messagePacket.MethodData.Invoice;
                        UUID SenderID = messagePacket.AgentData.AgentID;
                        UUID Prey;

                        UUID.TryParse(Utils.BytesToString(messagePacket.ParamList[1].Parameter), out Prey);

                        OnEstateTeleportOneUserHomeRequest(this, invoice, SenderID, Prey);
                    }
                    return true;
                case "teleporthomeallusers":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID invoice = messagePacket.MethodData.Invoice;
                        UUID SenderID = messagePacket.AgentData.AgentID;
                        OnEstateTeleportAllUsersHomeRequest(this, invoice, SenderID);
                    }
                    return true;
                case "colliders":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        handlerLandStatRequest = OnLandStatRequest;
                        if (handlerLandStatRequest != null)
                        {
                            handlerLandStatRequest(0, 1, 0, "", this);
                        }
                    }
                    return true;
                case "scripts":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        handlerLandStatRequest = OnLandStatRequest;
                        if (handlerLandStatRequest != null)
                        {
                            handlerLandStatRequest(0, 0, 0, "", this);
                        }
                    }
                    return true;
                case "terrain":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        if (messagePacket.ParamList.Length > 0)
                        {
                            if (Utils.BytesToString(messagePacket.ParamList[0].Parameter) == "bake")
                            {
                                BakeTerrain handlerBakeTerrain = OnBakeTerrain;
                                if (handlerBakeTerrain != null)
                                {
                                    handlerBakeTerrain(this);
                                }
                            }
                            if (Utils.BytesToString(messagePacket.ParamList[0].Parameter) == "download filename")
                            {
                                if (messagePacket.ParamList.Length > 1)
                                {
                                    RequestTerrain handlerRequestTerrain = OnRequestTerrain;
                                    if (handlerRequestTerrain != null)
                                    {
                                        handlerRequestTerrain(this, Utils.BytesToString(messagePacket.ParamList[1].Parameter));
                                    }
                                }
                            }
                            if (Utils.BytesToString(messagePacket.ParamList[0].Parameter) == "upload filename")
                            {
                                if (messagePacket.ParamList.Length > 1)
                                {
                                    RequestTerrain handlerUploadTerrain = OnUploadTerrain;
                                    if (handlerUploadTerrain != null)
                                    {
                                        handlerUploadTerrain(this, Utils.BytesToString(messagePacket.ParamList[1].Parameter));
                                    }
                                }
                            }

                        }


                    }
                    return true;

                case "estatechangeinfo":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID invoice = messagePacket.MethodData.Invoice;
                        UUID SenderID = messagePacket.AgentData.AgentID;
                        UInt32 param1 = Convert.ToUInt32(Utils.BytesToString(messagePacket.ParamList[1].Parameter));
                        UInt32 param2 = Convert.ToUInt32(Utils.BytesToString(messagePacket.ParamList[2].Parameter));
                        
                        EstateChangeInfo handlerEstateChangeInfo = OnEstateChangeInfo;
                        if (handlerEstateChangeInfo != null)
                        {
                            handlerEstateChangeInfo(this, invoice, SenderID, param1, param2);
                        }
                    }
                    return true;

                case "refreshmapvisibility":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        IWorldMapModule mapModule;
                        mapModule = Scene.RequestModuleInterface<IWorldMapModule>();
                        mapModule.CreateTerrainTexture();
                    }
                    return true;

                case "kickestate":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        UUID Prey;

                        UUID.TryParse(Utils.BytesToString(messagePacket.ParamList[0].Parameter), out Prey);
                        IClientAPI client;
                        m_scene.TryGetClient(Prey, out client);
                        if (client == null)
                            return true;
                        client.Kick("The Aurora Manager has kicked you");
                        m_scene.IncomingCloseAgent(Prey);
                    }
                    return true;
                case "telehub":
                    if (((Scene)m_scene).Permissions.CanIssueEstateCommand(AgentId, false))
                    {
                        List<string> Parameters = new List<string>();
                        foreach (EstateOwnerMessagePacket.ParamListBlock block in messagePacket.ParamList)
                        {
                            Parameters.Add(OpenMetaverse.Utils.BytesToString(block.Parameter));
                        }
                        GodlikeMessage handlerEstateTelehubRequest = OnEstateTelehubRequest;
                        if (handlerEstateTelehubRequest != null)
                        {
                            handlerEstateTelehubRequest(this,
                                                  messagePacket.MethodData.Invoice,
                                                  OpenMetaverse.Utils.BytesToString(messagePacket.MethodData.Method),
                                                  Parameters);
                        }
                    }
                    return true;
                case "estateobjectreturn":
                    SimWideDeletesDelegate handlerSimWideDeletesRequest = OnSimWideDeletes;
                    if (handlerSimWideDeletesRequest != null)
                    {
                        UUID Prey;
                        UUID.TryParse(Utils.BytesToString(messagePacket.ParamList[1].Parameter), out Prey);
                        int flags = int.Parse(Utils.BytesToString(messagePacket.ParamList[0].Parameter));
                        handlerSimWideDeletesRequest(this, flags, Prey);
                        return true;
                    }
                    return true;
                default:
                    m_log.Error("EstateOwnerMessage: Unknown method requested " + Utils.BytesToString(messagePacket.MethodData.Method));
                    return true;
            }
        }

        private bool HandleRequestRegionInfo(IClientAPI sender, Packet Pack)
        {
            RequestRegionInfoPacket.AgentDataBlock mPacket = ((RequestRegionInfoPacket)Pack).AgentData;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (mPacket.SessionID != SessionId ||
                    mPacket.AgentID != AgentId)
                    return true;
            }
            #endregion

            RegionInfoRequest handlerRegionInfoRequest = OnRegionInfoRequest;
            if (handlerRegionInfoRequest != null)
            {
                handlerRegionInfoRequest(this);
            }
            return true;
        }

        private bool HandleEstateCovenantRequest(IClientAPI sender, Packet Pack)
        {

            //EstateCovenantRequestPacket.AgentDataBlock epack =
            //     ((EstateCovenantRequestPacket)Pack).AgentData;

            EstateCovenantRequest handlerEstateCovenantRequest = OnEstateCovenantRequest;
            if (handlerEstateCovenantRequest != null)
            {
                handlerEstateCovenantRequest(this);
            }
            return true;

        }

        #endregion Estate Packets

        #region GodPackets

        private bool HandleRequestGodlikePowers(IClientAPI sender, Packet Pack)
        {
            RequestGodlikePowersPacket rglpPack = (RequestGodlikePowersPacket)Pack;
            RequestGodlikePowersPacket.RequestBlockBlock rblock = rglpPack.RequestBlock;
            UUID token = rblock.Token;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rglpPack.AgentData.SessionID != SessionId ||
                    rglpPack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RequestGodlikePowersPacket.AgentDataBlock ablock = rglpPack.AgentData;

            RequestGodlikePowers handlerReqGodlikePowers = OnRequestGodlikePowers;

            if (handlerReqGodlikePowers != null)
            {
                handlerReqGodlikePowers(ablock.AgentID, ablock.SessionID, token, rblock.Godlike, this);
            }

            return true;
        }

        private bool HandleGodUpdateRegionInfoUpdate(IClientAPI client, Packet Packet)
        {
            GodUpdateRegionInfoPacket GodUpdateRegionInfo =
                (GodUpdateRegionInfoPacket)Packet;

            GodUpdateRegionInfoUpdate handlerGodUpdateRegionInfo = OnGodUpdateRegionInfoUpdate;
            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (GodUpdateRegionInfo.AgentData.SessionID != SessionId ||
                    GodUpdateRegionInfo.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (handlerGodUpdateRegionInfo != null)
            {
                handlerGodUpdateRegionInfo(this,
                                           GodUpdateRegionInfo.RegionInfo.BillableFactor,
                                           GodUpdateRegionInfo.RegionInfo.PricePerMeter,
                                           GodUpdateRegionInfo.RegionInfo.EstateID,
                                           GodUpdateRegionInfo.RegionInfo.RegionFlags,
                                           GodUpdateRegionInfo.RegionInfo.SimName,
                                           GodUpdateRegionInfo.RegionInfo.RedirectGridX,
                                           GodUpdateRegionInfo.RegionInfo.RedirectGridY);
                return true;
            }
            return false;
        }

        private bool HandleSimWideDeletes(IClientAPI client, Packet Packet)
        {
            SimWideDeletesPacket SimWideDeletesRequest =
                (SimWideDeletesPacket)Packet;
            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (SimWideDeletesRequest.AgentData.SessionID != SessionId ||
                    SimWideDeletesRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SimWideDeletesDelegate handlerSimWideDeletesRequest = OnSimWideDeletes;
            if (handlerSimWideDeletesRequest != null)
            {
                handlerSimWideDeletesRequest(this,(int)SimWideDeletesRequest.DataBlock.Flags,SimWideDeletesRequest.DataBlock.TargetID);
                return true;
            }
            return false;
        }

        private bool HandleGodlikeMessage(IClientAPI client, Packet Packet)
        {
            GodlikeMessagePacket GodlikeMessage =
                (GodlikeMessagePacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (GodlikeMessage.AgentData.SessionID != SessionId ||
                    GodlikeMessage.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            GodlikeMessage handlerGodlikeMessage = OnGodlikeMessage;
            List<string> Parameters = new List<string>();
            foreach (GodlikeMessagePacket.ParamListBlock block in GodlikeMessage.ParamList)
            {
                Parameters.Add(OpenMetaverse.Utils.BytesToString(block.Parameter));
            }
            if (handlerGodlikeMessage != null)
            {
                handlerGodlikeMessage(this,
                                      GodlikeMessage.MethodData.Invoice,
                                      OpenMetaverse.Utils.BytesToString(GodlikeMessage.MethodData.Method),
                                      Parameters);
                return true;
            }
            return false;
        }

        private bool HandleSaveStatePacket(IClientAPI client, Packet Packet)
        {
            StateSavePacket SaveStateMessage =
                (StateSavePacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (SaveStateMessage.AgentData.SessionID != SessionId ||
                    SaveStateMessage.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            SaveStateHandler handlerSaveStatePacket = OnSaveState;
            if (handlerSaveStatePacket != null)
            {
                handlerSaveStatePacket(this,SaveStateMessage.AgentData.AgentID);
                return true;
            }
            return false;
        }

        private bool HandleGodKickUser(IClientAPI sender, Packet Pack)
        {
            GodKickUserPacket gkupack = (GodKickUserPacket)Pack;

            if (gkupack.UserInfo.GodSessionID == SessionId && AgentId == gkupack.UserInfo.GodID)
            {
                GodKickUser handlerGodKickUser = OnGodKickUser;
                if (handlerGodKickUser != null)
                {
                    handlerGodKickUser(gkupack.UserInfo.GodID, gkupack.UserInfo.GodSessionID,
                                       gkupack.UserInfo.AgentID, gkupack.UserInfo.KickFlags, gkupack.UserInfo.Reason);
                }
            }
            else
            {
                SendAgentAlertMessage("Kick request denied", false);
            }
            return true;
        }
        #endregion GodPackets

        #region Economy/Transaction Packets

        private bool HandleMoneyBalanceRequest(IClientAPI sender, Packet Pack)
        {
            MoneyBalanceRequestPacket moneybalancerequestpacket = (MoneyBalanceRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (moneybalancerequestpacket.AgentData.SessionID != SessionId ||
                    moneybalancerequestpacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            MoneyBalanceRequest handlerMoneyBalanceRequest = OnMoneyBalanceRequest;

            if (handlerMoneyBalanceRequest != null)
            {
                handlerMoneyBalanceRequest(this, moneybalancerequestpacket.AgentData.AgentID, moneybalancerequestpacket.AgentData.SessionID, moneybalancerequestpacket.MoneyData.TransactionID);
            }

            return true;
        }
        private bool HandleEconomyDataRequest(IClientAPI sender, Packet Pack)
        {
            EconomyDataRequest handlerEconomoyDataRequest = OnEconomyDataRequest;
            if (handlerEconomoyDataRequest != null)
            {
                handlerEconomoyDataRequest(AgentId);
            }
            return true;
        }
        private bool HandleRequestPayPrice(IClientAPI sender, Packet Pack)
        {
            RequestPayPricePacket requestPayPricePacket = (RequestPayPricePacket)Pack;

            RequestPayPrice handlerRequestPayPrice = OnRequestPayPrice;
            if (handlerRequestPayPrice != null)
            {
                handlerRequestPayPrice(this, requestPayPricePacket.ObjectData.ObjectID);
            }
            return true;
        }
        private bool HandleObjectSaleInfo(IClientAPI sender, Packet Pack)
        {
            ObjectSaleInfoPacket objectSaleInfoPacket = (ObjectSaleInfoPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (objectSaleInfoPacket.AgentData.SessionID != SessionId ||
                    objectSaleInfoPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectSaleInfo handlerObjectSaleInfo = OnObjectSaleInfo;
            if (handlerObjectSaleInfo != null)
            {
                foreach (ObjectSaleInfoPacket.ObjectDataBlock d
                    in objectSaleInfoPacket.ObjectData)
                {
                    handlerObjectSaleInfo(this,
                                          objectSaleInfoPacket.AgentData.AgentID,
                                          objectSaleInfoPacket.AgentData.SessionID,
                                          d.LocalID,
                                          d.SaleType,
                                          d.SalePrice);
                }
            }
            return true;
        }
        private bool HandleObjectBuy(IClientAPI sender, Packet Pack)
        {
            ObjectBuyPacket objectBuyPacket = (ObjectBuyPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (objectBuyPacket.AgentData.SessionID != SessionId ||
                    objectBuyPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ObjectBuy handlerObjectBuy = OnObjectBuy;

            if (handlerObjectBuy != null)
            {
                foreach (ObjectBuyPacket.ObjectDataBlock d
                    in objectBuyPacket.ObjectData)
                {
                    handlerObjectBuy(this,
                                     objectBuyPacket.AgentData.AgentID,
                                     objectBuyPacket.AgentData.SessionID,
                                     objectBuyPacket.AgentData.GroupID,
                                     objectBuyPacket.AgentData.CategoryID,
                                     d.ObjectLocalID,
                                     d.SaleType,
                                     d.SalePrice);
                }
            }
            return true;
        }

        #endregion Economy/Transaction Packets

        #region Script Packets
        private bool HandleGetScriptRunning(IClientAPI sender, Packet Pack)
        {
            GetScriptRunningPacket scriptRunning = (GetScriptRunningPacket)Pack;

            GetScriptRunning handlerGetScriptRunning = OnGetScriptRunning;
            if (handlerGetScriptRunning != null)
            {
                handlerGetScriptRunning(this, scriptRunning.Script.ObjectID, scriptRunning.Script.ItemID);
            }
            return true;
        }
        private bool HandleSetScriptRunning(IClientAPI sender, Packet Pack)
        {
            SetScriptRunningPacket setScriptRunning = (SetScriptRunningPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (setScriptRunning.AgentData.SessionID != SessionId ||
                    setScriptRunning.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SetScriptRunning handlerSetScriptRunning = OnSetScriptRunning;
            if (handlerSetScriptRunning != null)
            {
                handlerSetScriptRunning(this, setScriptRunning.Script.ObjectID, setScriptRunning.Script.ItemID, setScriptRunning.Script.Running);
            }
            return true;
        }

        private bool HandleScriptReset(IClientAPI sender, Packet Pack)
        {
            ScriptResetPacket scriptResetPacket = (ScriptResetPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (scriptResetPacket.AgentData.SessionID != SessionId ||
                    scriptResetPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ScriptReset handlerScriptReset = OnScriptReset;
            if (handlerScriptReset != null)
            {
                handlerScriptReset(this, scriptResetPacket.Script.ObjectID, scriptResetPacket.Script.ItemID);
            }
            return true;
        }

        #endregion Script Packets

        #region Gesture Managment

        private bool HandleActivateGestures(IClientAPI sender, Packet Pack)
        {
            ActivateGesturesPacket activateGesturePacket = (ActivateGesturesPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (activateGesturePacket.AgentData.SessionID != SessionId ||
                    activateGesturePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ActivateGesture handlerActivateGesture = OnActivateGesture;
            if (handlerActivateGesture != null)
            {
                handlerActivateGesture(this,
                                       activateGesturePacket.Data[0].AssetID,
                                       activateGesturePacket.Data[0].ItemID);
            }
            else m_log.Error("Null pointer for activateGesture");

            return true;
        }
        private bool HandleDeactivateGestures(IClientAPI sender, Packet Pack)
        {
            DeactivateGesturesPacket deactivateGesturePacket = (DeactivateGesturesPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (deactivateGesturePacket.AgentData.SessionID != SessionId ||
                    deactivateGesturePacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DeactivateGesture handlerDeactivateGesture = OnDeactivateGesture;
            if (handlerDeactivateGesture != null)
            {
                handlerDeactivateGesture(this, deactivateGesturePacket.Data[0].ItemID);
            }
            return true;
        }
        private bool HandleObjectOwner(IClientAPI sender, Packet Pack)
        {
            ObjectOwnerPacket objectOwnerPacket = (ObjectOwnerPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (objectOwnerPacket.AgentData.SessionID != SessionId ||
                    objectOwnerPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            List<uint> localIDs = new List<uint>();

            foreach (ObjectOwnerPacket.ObjectDataBlock d in objectOwnerPacket.ObjectData)
                localIDs.Add(d.ObjectLocalID);

            ObjectOwner handlerObjectOwner = OnObjectOwner;
            if (handlerObjectOwner != null)
            {
                handlerObjectOwner(this, objectOwnerPacket.HeaderData.OwnerID, objectOwnerPacket.HeaderData.GroupID, localIDs);
            }
            return true;
        }

        #endregion Gesture Managment

        private bool HandleAgentFOV(IClientAPI sender, Packet Pack)
        {
            AgentFOVPacket fovPacket = (AgentFOVPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (fovPacket.AgentData.SessionID != SessionId ||
                    fovPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (fovPacket.FOVBlock.GenCounter > m_agentFOVCounter)
            {
                m_agentFOVCounter = fovPacket.FOVBlock.GenCounter;
                AgentFOV handlerAgentFOV = OnAgentFOV;
                if (handlerAgentFOV != null)
                {
                    handlerAgentFOV(this, fovPacket.FOVBlock.VerticalAngle);
                }
            }
            return true;
        }

        #region unimplemented handlers

        private bool HandleViewerStats(IClientAPI sender, Packet Pack)
        {
            //m_log.Warn("[CLIENT]: unhandled ViewerStats packet");
            return true;
        }

        private bool HandleTransferAbort(IClientAPI sender, Packet Pack)
        {
            return true;
        }

        private bool HandleUseCircuitCode(IClientAPI sender, Packet Pack)
        {
            return true;
        }

        private bool HandleAgentHeightWidth(IClientAPI sender, Packet Pack)
        {
            return true;
        }

        private bool HandleInventoryDescendents(IClientAPI sender, Packet Pack)
        {
            return true;
        }

        #endregion unimplemented handlers

        private bool HandleMapItemRequest(IClientAPI sender, Packet Pack)
        {
            MapItemRequestPacket mirpk = (MapItemRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (mirpk.AgentData.SessionID != SessionId ||
                    mirpk.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            //m_log.Debug(mirpk.ToString());
            MapItemRequest handlerMapItemRequest = OnMapItemRequest;
            if (handlerMapItemRequest != null)
            {
                handlerMapItemRequest(this, mirpk.AgentData.Flags, mirpk.AgentData.EstateID,
                                      mirpk.AgentData.Godlike, mirpk.RequestData.ItemType,
                                      mirpk.RequestData.RegionHandle);

            }
            return true;
        }

        private bool HandleMuteListRequest(IClientAPI sender, Packet Pack)
        {
            MuteListRequestPacket muteListRequest =
                            (MuteListRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (muteListRequest.AgentData.SessionID != SessionId ||
                    muteListRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            MuteListRequest handlerMuteListRequest = OnMuteListRequest;
            if (handlerMuteListRequest != null)
            {
                handlerMuteListRequest(this, muteListRequest.MuteData.MuteCRC);
            }
            else
            {
                SendUseCachedMuteList();
            }
            return true;
        }

        private bool HandleUpdateMuteListEntry(IClientAPI client, Packet Packet)
        {
            UpdateMuteListEntryPacket UpdateMuteListEntry =
                (UpdateMuteListEntryPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (UpdateMuteListEntry.AgentData.SessionID != SessionId ||
                    UpdateMuteListEntry.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            MuteListEntryUpdate handlerUpdateMuteListEntry = OnUpdateMuteListEntry;
            if (handlerUpdateMuteListEntry != null)
            {
                handlerUpdateMuteListEntry(this, UpdateMuteListEntry.MuteData.MuteID,
                                           Utils.BytesToString(UpdateMuteListEntry.MuteData.MuteName),
                                           UpdateMuteListEntry.MuteData.MuteType,
                                           UpdateMuteListEntry.AgentData.AgentID);
                return true;
            }
            return false;
        }

        private bool HandleRemoveMuteListEntry(IClientAPI client, Packet Packet)
        {
            RemoveMuteListEntryPacket RemoveMuteListEntry =
                (RemoveMuteListEntryPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (RemoveMuteListEntry.AgentData.SessionID != SessionId ||
                    RemoveMuteListEntry.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            MuteListEntryRemove handlerRemoveMuteListEntry = OnRemoveMuteListEntry;
            if (handlerRemoveMuteListEntry != null)
            {
                handlerRemoveMuteListEntry(this,
                                           RemoveMuteListEntry.MuteData.MuteID,
                                           Utils.BytesToString(RemoveMuteListEntry.MuteData.MuteName),
                                           RemoveMuteListEntry.AgentData.AgentID);
                return true;
            }
            return false;
        }

        private bool HandleUserReport(IClientAPI client, Packet Packet)
        {
            UserReportPacket UserReport =
                (UserReportPacket)Packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (UserReport.AgentData.SessionID != SessionId ||
                    UserReport.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            NewUserReport handlerUserReport = OnUserReport;
            if (handlerUserReport != null)
            {
                handlerUserReport(this,
                    Utils.BytesToString(UserReport.ReportData.AbuseRegionName),
                    UserReport.ReportData.AbuserID,
                    UserReport.ReportData.Category,
                    UserReport.ReportData.CheckFlags,
                    Utils.BytesToString(UserReport.ReportData.Details),
                    UserReport.ReportData.ObjectID,
                    UserReport.ReportData.Position,
                    UserReport.ReportData.ReportType,
                    UserReport.ReportData.ScreenshotID,
                    Utils.BytesToString(UserReport.ReportData.Summary),
                    UserReport.AgentData.AgentID);
                return true;
            }
            return false;
        }

        private bool HandleSendPostcard(IClientAPI client, Packet packet)
        {
            SendPostcardPacket SendPostcard =
                (SendPostcardPacket)packet;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (SendPostcard.AgentData.SessionID != SessionId ||
                    SendPostcard.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            SendPostcard handlerSendPostcard = OnSendPostcard;
            if (handlerSendPostcard != null)
            {
                handlerSendPostcard(this);
                return true;
            }
            return false;
        }

        private bool HandleViewerStartAuction(IClientAPI client, Packet packet)
        {
            ViewerStartAuctionPacket aPacket = (ViewerStartAuctionPacket)packet;
            ViewerStartAuction handlerStartAuction = OnViewerStartAuction;

            if (handlerStartAuction != null)
            {
                handlerStartAuction(this, aPacket.ParcelData.LocalID, aPacket.ParcelData.SnapshotID);
                return true;
            }
            return false;
        }

        private bool HandleParcelDisableObjects(IClientAPI client, Packet packet)
        {
            ParcelDisableObjectsPacket aPacket = (ParcelDisableObjectsPacket)packet;
            ParcelReturnObjectsRequest handlerParcelDisableObjectsRequest = OnParcelDisableObjectsRequest;

            List<UUID> OwnerIDs = new List<UUID>();
            foreach (ParcelDisableObjectsPacket.OwnerIDsBlock block in aPacket.OwnerIDs)
            {
                OwnerIDs.Add(block.OwnerID);
            }
            List<UUID> TaskIDs = new List<UUID>();
            foreach (ParcelDisableObjectsPacket.TaskIDsBlock block in aPacket.TaskIDs)
            {
                TaskIDs.Add(block.TaskID);
            }

            if (handlerParcelDisableObjectsRequest != null)
            {
                handlerParcelDisableObjectsRequest(aPacket.ParcelData.LocalID, aPacket.ParcelData.ReturnType, OwnerIDs.ToArray(), TaskIDs.ToArray(), this);
                return true;
            }
            return false;
        }

        private bool HandleTeleportCancel(IClientAPI client, Packet packet)
        {
            TeleportCancel handlerTeleportCancel = OnTeleportCancel;

            if (handlerTeleportCancel != null)
            {
                handlerTeleportCancel(this);
                return true;
            }
            return false;
        }

        #region Dir handlers

        private bool HandleDirPlacesQuery(IClientAPI sender, Packet Pack)
        {
            DirPlacesQueryPacket dirPlacesQueryPacket = (DirPlacesQueryPacket)Pack;
            //m_log.Debug(dirPlacesQueryPacket.ToString());

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dirPlacesQueryPacket.AgentData.SessionID != SessionId ||
                    dirPlacesQueryPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DirPlacesQuery handlerDirPlacesQuery = OnDirPlacesQuery;
            if (handlerDirPlacesQuery != null)
            {
                handlerDirPlacesQuery(this,
                                      dirPlacesQueryPacket.QueryData.QueryID,
                                      Utils.BytesToString(
                                          dirPlacesQueryPacket.QueryData.QueryText),
                                      (int)dirPlacesQueryPacket.QueryData.QueryFlags,
                                      (int)dirPlacesQueryPacket.QueryData.Category,
                                      Utils.BytesToString(
                                          dirPlacesQueryPacket.QueryData.SimName),
                                      dirPlacesQueryPacket.QueryData.QueryStart);
            }
            return true;
        }

        private bool HandleDirFindQuery(IClientAPI sender, Packet Pack)
        {
            DirFindQueryPacket dirFindQueryPacket = (DirFindQueryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dirFindQueryPacket.AgentData.SessionID != SessionId ||
                    dirFindQueryPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DirFindQuery handlerDirFindQuery = OnDirFindQuery;
            if (handlerDirFindQuery != null)
            {
                handlerDirFindQuery(this,
                                    dirFindQueryPacket.QueryData.QueryID,
                                    Utils.BytesToString(
                                        dirFindQueryPacket.QueryData.QueryText),
                                    dirFindQueryPacket.QueryData.QueryFlags,
                                    dirFindQueryPacket.QueryData.QueryStart);
            }
            return true;
        }

        private bool HandleDirLandQuery(IClientAPI sender, Packet Pack)
        {
            DirLandQueryPacket dirLandQueryPacket = (DirLandQueryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dirLandQueryPacket.AgentData.SessionID != SessionId ||
                    dirLandQueryPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DirLandQuery handlerDirLandQuery = OnDirLandQuery;
            if (handlerDirLandQuery != null)
            {
                handlerDirLandQuery(this,
                                    dirLandQueryPacket.QueryData.QueryID,
                                    dirLandQueryPacket.QueryData.QueryFlags,
                                    dirLandQueryPacket.QueryData.SearchType,
                                    dirLandQueryPacket.QueryData.Price,
                                    dirLandQueryPacket.QueryData.Area,
                                    dirLandQueryPacket.QueryData.QueryStart);
            }
            return true;
        }

        private bool HandleDirPopularQuery(IClientAPI sender, Packet Pack)
        {
            DirPopularQueryPacket dirPopularQueryPacket = (DirPopularQueryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dirPopularQueryPacket.AgentData.SessionID != SessionId ||
                    dirPopularQueryPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DirPopularQuery handlerDirPopularQuery = OnDirPopularQuery;
            if (handlerDirPopularQuery != null)
            {
                handlerDirPopularQuery(this,
                                       dirPopularQueryPacket.QueryData.QueryID,
                                       dirPopularQueryPacket.QueryData.QueryFlags);
            }
            return true;
        }

        private bool HandleDirClassifiedQuery(IClientAPI sender, Packet Pack)
        {
            DirClassifiedQueryPacket dirClassifiedQueryPacket = (DirClassifiedQueryPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (dirClassifiedQueryPacket.AgentData.SessionID != SessionId ||
                    dirClassifiedQueryPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            DirClassifiedQuery handlerDirClassifiedQuery = OnDirClassifiedQuery;
            if (handlerDirClassifiedQuery != null)
            {
                handlerDirClassifiedQuery(this,
                                          dirClassifiedQueryPacket.QueryData.QueryID,
                                          Utils.BytesToString(
                                              dirClassifiedQueryPacket.QueryData.QueryText),
                                          dirClassifiedQueryPacket.QueryData.QueryFlags,
                                          dirClassifiedQueryPacket.QueryData.Category,
                                          dirClassifiedQueryPacket.QueryData.QueryStart);
            }
            return true;
        }

        private bool HandleEventInfoRequest(IClientAPI sender, Packet Pack)
        {
            EventInfoRequestPacket eventInfoRequestPacket = (EventInfoRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (eventInfoRequestPacket.AgentData.SessionID != SessionId ||
                    eventInfoRequestPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (OnEventInfoRequest != null)
            {
                OnEventInfoRequest(this, eventInfoRequestPacket.EventData.EventID);
            }
            return true;
        }

        #endregion

        #region Calling Card

        private bool HandleOfferCallingCard(IClientAPI sender, Packet Pack)
        {
            OfferCallingCardPacket offerCallingCardPacket = (OfferCallingCardPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (offerCallingCardPacket.AgentData.SessionID != SessionId ||
                    offerCallingCardPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (OnOfferCallingCard != null)
            {
                OnOfferCallingCard(this,
                                   offerCallingCardPacket.AgentBlock.DestID,
                                   offerCallingCardPacket.AgentBlock.TransactionID);
            }
            return true;
        }

        private bool HandleAcceptCallingCard(IClientAPI sender, Packet Pack)
        {
            AcceptCallingCardPacket acceptCallingCardPacket = (AcceptCallingCardPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (acceptCallingCardPacket.AgentData.SessionID != SessionId ||
                    acceptCallingCardPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            // according to http://wiki.secondlife.com/wiki/AcceptCallingCard FolderData should
            // contain exactly one entry
            if (OnAcceptCallingCard != null && acceptCallingCardPacket.FolderData.Length > 0)
            {
                OnAcceptCallingCard(this,
                                    acceptCallingCardPacket.TransactionBlock.TransactionID,
                                    acceptCallingCardPacket.FolderData[0].FolderID);
            }
            return true;
        }

        private bool HandleDeclineCallingCard(IClientAPI sender, Packet Pack)
        {
            DeclineCallingCardPacket declineCallingCardPacket = (DeclineCallingCardPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (declineCallingCardPacket.AgentData.SessionID != SessionId ||
                    declineCallingCardPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (OnDeclineCallingCard != null)
            {
                OnDeclineCallingCard(this,
                                     declineCallingCardPacket.TransactionBlock.TransactionID);
            }
            return true;
        }

        #endregion Calling Card

        #region Groups

        private bool HandleActivateGroup(IClientAPI sender, Packet Pack)
        {
            ActivateGroupPacket activateGroupPacket = (ActivateGroupPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (activateGroupPacket.AgentData.SessionID != SessionId ||
                    activateGroupPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.ActivateGroup(this, activateGroupPacket.AgentData.GroupID);
                m_GroupsModule.SendAgentGroupDataUpdate(this);
            }
            return true;

        }

        private bool HandleGroupVoteHistoryRequest(IClientAPI client, Packet Packet)
        {
            GroupVoteHistoryRequestPacket GroupVoteHistoryRequest =
                (GroupVoteHistoryRequestPacket)Packet;
            GroupVoteHistoryRequest handlerGroupVoteHistoryRequest = OnGroupVoteHistoryRequest;
            if (handlerGroupVoteHistoryRequest != null)
            {
                handlerGroupVoteHistoryRequest(this, GroupVoteHistoryRequest.AgentData.AgentID,GroupVoteHistoryRequest.AgentData.SessionID,GroupVoteHistoryRequest.GroupData.GroupID,GroupVoteHistoryRequest.TransactionData.TransactionID);
                return true;
            }
            return false;
        }

        private bool HandleGroupProposalBallot(IClientAPI client, Packet Packet)
        {
            GroupProposalBallotPacket GroupProposalBallotRequest =
                (GroupProposalBallotPacket)Packet;
            GroupProposalBallotRequest handlerGroupActiveProposalsRequest = OnGroupProposalBallotRequest;
            if (handlerGroupActiveProposalsRequest != null)
            {
                handlerGroupActiveProposalsRequest(this, GroupProposalBallotRequest.AgentData.AgentID, GroupProposalBallotRequest.AgentData.SessionID, GroupProposalBallotRequest.ProposalData.GroupID, GroupProposalBallotRequest.ProposalData.ProposalID, Utils.BytesToString(GroupProposalBallotRequest.ProposalData.VoteCast));
                return true;
            }
            return false;
        }

        private bool HandleGroupActiveProposalsRequest(IClientAPI client, Packet Packet)
        {
            GroupActiveProposalsRequestPacket GroupActiveProposalsRequest =
                (GroupActiveProposalsRequestPacket)Packet;
            GroupActiveProposalsRequest handlerGroupActiveProposalsRequest = OnGroupActiveProposalsRequest;
            if (handlerGroupActiveProposalsRequest != null)
            {
                handlerGroupActiveProposalsRequest(this, GroupActiveProposalsRequest.AgentData.AgentID,GroupActiveProposalsRequest.AgentData.SessionID,GroupActiveProposalsRequest.GroupData.GroupID,GroupActiveProposalsRequest.TransactionData.TransactionID);
                return true;
            }
            return false;
        }

        private bool HandleGroupAccountDetailsRequest(IClientAPI client, Packet Packet)
        {
            GroupAccountDetailsRequestPacket GroupAccountDetailsRequest =
                (GroupAccountDetailsRequestPacket)Packet;
            GroupAccountDetailsRequest handlerGroupAccountDetailsRequest = OnGroupAccountDetailsRequest;
            if (handlerGroupAccountDetailsRequest != null)
            {
                handlerGroupAccountDetailsRequest(this, GroupAccountDetailsRequest.AgentData.AgentID,GroupAccountDetailsRequest.AgentData.GroupID,GroupAccountDetailsRequest.MoneyData.RequestID,GroupAccountDetailsRequest.AgentData.SessionID);
                return true;
            }
            return false;
        }

        private bool HandleGroupAccountSummaryRequest(IClientAPI client, Packet Packet)
        {
            GroupAccountSummaryRequestPacket GroupAccountSummaryRequest =
                (GroupAccountSummaryRequestPacket)Packet;
            GroupAccountSummaryRequest handlerGroupAccountSummaryRequest = OnGroupAccountSummaryRequest;
            if (handlerGroupAccountSummaryRequest != null)
            {
                handlerGroupAccountSummaryRequest(this, GroupAccountSummaryRequest.AgentData.AgentID,GroupAccountSummaryRequest.AgentData.GroupID);
                return true;
            }
            return false;
        }

        private bool HandleGroupTransactionsDetailsRequest(IClientAPI client, Packet Packet)
        {
            GroupAccountTransactionsRequestPacket GroupAccountTransactionsRequest =
                (GroupAccountTransactionsRequestPacket)Packet;
            GroupAccountTransactionsRequest handlerGroupAccountTransactionsRequest = OnGroupAccountTransactionsRequest;
            if (handlerGroupAccountTransactionsRequest != null)
            {
                handlerGroupAccountTransactionsRequest(this, GroupAccountTransactionsRequest.AgentData.AgentID,GroupAccountTransactionsRequest.AgentData.GroupID,GroupAccountTransactionsRequest.MoneyData.RequestID,GroupAccountTransactionsRequest.AgentData.SessionID);
                return true;
            }
            return false;
        }

        private bool HandleGroupTitlesRequest(IClientAPI sender, Packet Pack)
        {
            GroupTitlesRequestPacket groupTitlesRequest =
                            (GroupTitlesRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupTitlesRequest.AgentData.SessionID != SessionId ||
                    groupTitlesRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                GroupTitlesReplyPacket groupTitlesReply = (GroupTitlesReplyPacket)PacketPool.Instance.GetPacket(PacketType.GroupTitlesReply);

                groupTitlesReply.AgentData =
                    new GroupTitlesReplyPacket.AgentDataBlock();

                groupTitlesReply.AgentData.AgentID = AgentId;
                groupTitlesReply.AgentData.GroupID =
                    groupTitlesRequest.AgentData.GroupID;

                groupTitlesReply.AgentData.RequestID =
                    groupTitlesRequest.AgentData.RequestID;

                List<GroupTitlesData> titles =
                    m_GroupsModule.GroupTitlesRequest(this,
                                                      groupTitlesRequest.AgentData.GroupID);

                groupTitlesReply.GroupData =
                    new GroupTitlesReplyPacket.GroupDataBlock[titles.Count];

                int i = 0;
                foreach (GroupTitlesData d in titles)
                {
                    groupTitlesReply.GroupData[i] =
                        new GroupTitlesReplyPacket.GroupDataBlock();

                    groupTitlesReply.GroupData[i].Title =
                        Util.StringToBytes256(d.Name);
                    groupTitlesReply.GroupData[i].RoleID =
                        d.UUID;
                    groupTitlesReply.GroupData[i].Selected =
                        d.Selected;
                    i++;
                }

                OutPacket(groupTitlesReply, ThrottleOutPacketType.Asset);
            }
            return true;
        }
        private bool HandleGroupProfileRequest(IClientAPI sender, Packet Pack)
        {
            GroupProfileRequestPacket groupProfileRequest =
                       (GroupProfileRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupProfileRequest.AgentData.SessionID != SessionId ||
                    groupProfileRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                GroupProfileReplyPacket groupProfileReply = (GroupProfileReplyPacket)PacketPool.Instance.GetPacket(PacketType.GroupProfileReply);

                groupProfileReply.AgentData = new GroupProfileReplyPacket.AgentDataBlock();
                groupProfileReply.GroupData = new GroupProfileReplyPacket.GroupDataBlock();
                groupProfileReply.AgentData.AgentID = AgentId;

                GroupProfileData d = m_GroupsModule.GroupProfileRequest(this,
                                                                        groupProfileRequest.GroupData.GroupID);

                groupProfileReply.GroupData.GroupID = d.GroupID;
                groupProfileReply.GroupData.Name = Util.StringToBytes256(d.Name);
                groupProfileReply.GroupData.Charter = Util.StringToBytes1024(d.Charter);
                groupProfileReply.GroupData.ShowInList = d.ShowInList;
                groupProfileReply.GroupData.MemberTitle = Util.StringToBytes256(d.MemberTitle);
                groupProfileReply.GroupData.PowersMask = d.PowersMask;
                groupProfileReply.GroupData.InsigniaID = d.InsigniaID;
                groupProfileReply.GroupData.FounderID = d.FounderID;
                groupProfileReply.GroupData.MembershipFee = d.MembershipFee;
                groupProfileReply.GroupData.OpenEnrollment = d.OpenEnrollment;
                groupProfileReply.GroupData.Money = d.Money;
                groupProfileReply.GroupData.GroupMembershipCount = d.GroupMembershipCount;
                groupProfileReply.GroupData.GroupRolesCount = d.GroupRolesCount;
                groupProfileReply.GroupData.AllowPublish = d.AllowPublish;
                groupProfileReply.GroupData.MaturePublish = d.MaturePublish;
                groupProfileReply.GroupData.OwnerRole = d.OwnerRole;

                OutPacket(groupProfileReply, ThrottleOutPacketType.Asset);
            }
            return true;
        }
        private bool HandleGroupMembersRequest(IClientAPI sender, Packet Pack)
        {
            GroupMembersRequestPacket groupMembersRequestPacket =
                        (GroupMembersRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupMembersRequestPacket.AgentData.SessionID != SessionId ||
                    groupMembersRequestPacket.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                List<GroupMembersData> members =
                    m_GroupsModule.GroupMembersRequest(this, groupMembersRequestPacket.GroupData.GroupID);

                int memberCount = members.Count;

                while (true)
                {
                    int blockCount = members.Count;
                    if (blockCount > 40)
                        blockCount = 40;

                    GroupMembersReplyPacket groupMembersReply = (GroupMembersReplyPacket)PacketPool.Instance.GetPacket(PacketType.GroupMembersReply);

                    groupMembersReply.AgentData =
                        new GroupMembersReplyPacket.AgentDataBlock();
                    groupMembersReply.GroupData =
                        new GroupMembersReplyPacket.GroupDataBlock();
                    groupMembersReply.MemberData =
                        new GroupMembersReplyPacket.MemberDataBlock[
                            blockCount];

                    groupMembersReply.AgentData.AgentID = AgentId;
                    groupMembersReply.GroupData.GroupID =
                        groupMembersRequestPacket.GroupData.GroupID;
                    groupMembersReply.GroupData.RequestID =
                        groupMembersRequestPacket.GroupData.RequestID;
                    groupMembersReply.GroupData.MemberCount = memberCount;

                    for (int i = 0; i < blockCount; i++)
                    {
                        GroupMembersData m = members[0];
                        members.RemoveAt(0);

                        groupMembersReply.MemberData[i] =
                            new GroupMembersReplyPacket.MemberDataBlock();
                        groupMembersReply.MemberData[i].AgentID =
                            m.AgentID;
                        groupMembersReply.MemberData[i].Contribution =
                            m.Contribution;
                        groupMembersReply.MemberData[i].OnlineStatus =
                            Util.StringToBytes256(m.OnlineStatus);
                        groupMembersReply.MemberData[i].AgentPowers =
                            m.AgentPowers;
                        groupMembersReply.MemberData[i].Title =
                            Util.StringToBytes256(m.Title);
                        groupMembersReply.MemberData[i].IsOwner =
                            m.IsOwner;
                    }
                    OutPacket(groupMembersReply, ThrottleOutPacketType.Asset);
                    if (members.Count == 0)
                        return true;
                }
            }
            return true;
        }
        private bool HandleGroupRoleDataRequest(IClientAPI sender, Packet Pack)
        {
            GroupRoleDataRequestPacket groupRolesRequest =
                        (GroupRoleDataRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupRolesRequest.AgentData.SessionID != SessionId ||
                    groupRolesRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                GroupRoleDataReplyPacket groupRolesReply = (GroupRoleDataReplyPacket)PacketPool.Instance.GetPacket(PacketType.GroupRoleDataReply);

                groupRolesReply.AgentData =
                    new GroupRoleDataReplyPacket.AgentDataBlock();

                groupRolesReply.AgentData.AgentID = AgentId;

                groupRolesReply.GroupData =
                    new GroupRoleDataReplyPacket.GroupDataBlock();

                groupRolesReply.GroupData.GroupID =
                    groupRolesRequest.GroupData.GroupID;

                groupRolesReply.GroupData.RequestID =
                    groupRolesRequest.GroupData.RequestID;

                List<GroupRolesData> titles =
                    m_GroupsModule.GroupRoleDataRequest(this,
                                                        groupRolesRequest.GroupData.GroupID);

                groupRolesReply.GroupData.RoleCount =
                    titles.Count;

                groupRolesReply.RoleData =
                    new GroupRoleDataReplyPacket.RoleDataBlock[titles.Count];

                int i = 0;
                foreach (GroupRolesData d in titles)
                {
                    groupRolesReply.RoleData[i] =
                        new GroupRoleDataReplyPacket.RoleDataBlock();

                    groupRolesReply.RoleData[i].RoleID =
                        d.RoleID;
                    groupRolesReply.RoleData[i].Name =
                        Util.StringToBytes256(d.Name);
                    groupRolesReply.RoleData[i].Title =
                        Util.StringToBytes256(d.Title);
                    groupRolesReply.RoleData[i].Description =
                        Util.StringToBytes1024(d.Description);
                    groupRolesReply.RoleData[i].Powers =
                        d.Powers;
                    groupRolesReply.RoleData[i].Members =
                        (uint)d.Members;

                    i++;
                }

                OutPacket(groupRolesReply, ThrottleOutPacketType.Asset);
            }
            return true;
        }
        private bool HandleGroupRoleMembersRequest(IClientAPI sender, Packet Pack)
        {
            GroupRoleMembersRequestPacket groupRoleMembersRequest =
                       (GroupRoleMembersRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupRoleMembersRequest.AgentData.SessionID != SessionId ||
                    groupRoleMembersRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                List<GroupRoleMembersData> mappings =
                        m_GroupsModule.GroupRoleMembersRequest(this,
                        groupRoleMembersRequest.GroupData.GroupID);

                int mappingsCount = mappings.Count;

                while (mappings.Count > 0)
                {
                    int pairs = mappings.Count;
                    if (pairs > 32)
                        pairs = 32;

                    GroupRoleMembersReplyPacket groupRoleMembersReply = (GroupRoleMembersReplyPacket)PacketPool.Instance.GetPacket(PacketType.GroupRoleMembersReply);
                    groupRoleMembersReply.AgentData =
                            new GroupRoleMembersReplyPacket.AgentDataBlock();
                    groupRoleMembersReply.AgentData.AgentID =
                            AgentId;
                    groupRoleMembersReply.AgentData.GroupID =
                            groupRoleMembersRequest.GroupData.GroupID;
                    groupRoleMembersReply.AgentData.RequestID =
                            groupRoleMembersRequest.GroupData.RequestID;

                    groupRoleMembersReply.AgentData.TotalPairs =
                            (uint)mappingsCount;

                    groupRoleMembersReply.MemberData =
                            new GroupRoleMembersReplyPacket.MemberDataBlock[pairs];

                    for (int i = 0; i < pairs; i++)
                    {
                        GroupRoleMembersData d = mappings[0];
                        mappings.RemoveAt(0);

                        groupRoleMembersReply.MemberData[i] =
                            new GroupRoleMembersReplyPacket.MemberDataBlock();

                        groupRoleMembersReply.MemberData[i].RoleID =
                                d.RoleID;
                        groupRoleMembersReply.MemberData[i].MemberID =
                                d.MemberID;
                    }

                    OutPacket(groupRoleMembersReply, ThrottleOutPacketType.Asset);
                }
            }
            return true;
        }
        private bool HandleCreateGroupRequest(IClientAPI sender, Packet Pack)
        {
            CreateGroupRequestPacket createGroupRequest =
                       (CreateGroupRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (createGroupRequest.AgentData.SessionID != SessionId ||
                    createGroupRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.CreateGroup(this,
                                           Utils.BytesToString(createGroupRequest.GroupData.Name),
                                           Utils.BytesToString(createGroupRequest.GroupData.Charter),
                                           createGroupRequest.GroupData.ShowInList,
                                           createGroupRequest.GroupData.InsigniaID,
                                           createGroupRequest.GroupData.MembershipFee,
                                           createGroupRequest.GroupData.OpenEnrollment,
                                           createGroupRequest.GroupData.AllowPublish,
                                           createGroupRequest.GroupData.MaturePublish);
            }
            return true;
        }
        private bool HandleUpdateGroupInfo(IClientAPI sender, Packet Pack)
        {
            UpdateGroupInfoPacket updateGroupInfo =
                        (UpdateGroupInfoPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (updateGroupInfo.AgentData.SessionID != SessionId ||
                    updateGroupInfo.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.UpdateGroupInfo(this,
                                               updateGroupInfo.GroupData.GroupID,
                                               Utils.BytesToString(updateGroupInfo.GroupData.Charter),
                                               updateGroupInfo.GroupData.ShowInList,
                                               updateGroupInfo.GroupData.InsigniaID,
                                               updateGroupInfo.GroupData.MembershipFee,
                                               updateGroupInfo.GroupData.OpenEnrollment,
                                               updateGroupInfo.GroupData.AllowPublish,
                                               updateGroupInfo.GroupData.MaturePublish);
            }

            return true;
        }
        private bool HandleSetGroupAcceptNotices(IClientAPI sender, Packet Pack)
        {
            SetGroupAcceptNoticesPacket setGroupAcceptNotices =
                        (SetGroupAcceptNoticesPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (setGroupAcceptNotices.AgentData.SessionID != SessionId ||
                    setGroupAcceptNotices.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.SetGroupAcceptNotices(this,
                                                     setGroupAcceptNotices.Data.GroupID,
                                                     setGroupAcceptNotices.Data.AcceptNotices,
                                                     setGroupAcceptNotices.NewData.ListInProfile);
            }

            return true;
        }
        private bool HandleGroupTitleUpdate(IClientAPI sender, Packet Pack)
        {
            GroupTitleUpdatePacket groupTitleUpdate =
                        (GroupTitleUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupTitleUpdate.AgentData.SessionID != SessionId ||
                    groupTitleUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.GroupTitleUpdate(this,
                                                groupTitleUpdate.AgentData.GroupID,
                                                groupTitleUpdate.AgentData.TitleRoleID);
            }

            return true;
        }
        private bool HandleParcelDeedToGroup(IClientAPI sender, Packet Pack)
        {
            ParcelDeedToGroupPacket parcelDeedToGroup = (ParcelDeedToGroupPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (parcelDeedToGroup.AgentData.SessionID != SessionId ||
                    parcelDeedToGroup.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                ParcelDeedToGroup handlerParcelDeedToGroup = OnParcelDeedToGroup;
                if (handlerParcelDeedToGroup != null)
                {
                    handlerParcelDeedToGroup(parcelDeedToGroup.Data.LocalID, parcelDeedToGroup.Data.GroupID, this);

                }
            }

            return true;
        }
        private bool HandleGroupNoticesListRequest(IClientAPI sender, Packet Pack)
        {
            GroupNoticesListRequestPacket groupNoticesListRequest =
                        (GroupNoticesListRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupNoticesListRequest.AgentData.SessionID != SessionId ||
                    groupNoticesListRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                GroupNoticeData[] gn =
                    m_GroupsModule.GroupNoticesListRequest(this,
                                                           groupNoticesListRequest.Data.GroupID);

                GroupNoticesListReplyPacket groupNoticesListReply = (GroupNoticesListReplyPacket)PacketPool.Instance.GetPacket(PacketType.GroupNoticesListReply);
                groupNoticesListReply.AgentData =
                    new GroupNoticesListReplyPacket.AgentDataBlock();
                groupNoticesListReply.AgentData.AgentID = AgentId;
                groupNoticesListReply.AgentData.GroupID = groupNoticesListRequest.Data.GroupID;

                groupNoticesListReply.Data = new GroupNoticesListReplyPacket.DataBlock[gn.Length];

                int i = 0;
                foreach (GroupNoticeData g in gn)
                {
                    groupNoticesListReply.Data[i] = new GroupNoticesListReplyPacket.DataBlock();
                    groupNoticesListReply.Data[i].NoticeID =
                        g.NoticeID;
                    groupNoticesListReply.Data[i].Timestamp =
                        g.Timestamp;
                    groupNoticesListReply.Data[i].FromName =
                        Util.StringToBytes256(g.FromName);
                    groupNoticesListReply.Data[i].Subject =
                        Util.StringToBytes256(g.Subject);
                    groupNoticesListReply.Data[i].HasAttachment =
                        g.HasAttachment;
                    groupNoticesListReply.Data[i].AssetType =
                        g.AssetType;
                    i++;
                }

                OutPacket(groupNoticesListReply, ThrottleOutPacketType.Asset);
            }

            return true;
        }
        private bool HandleGroupNoticeRequest(IClientAPI sender, Packet Pack)
        {
            GroupNoticeRequestPacket groupNoticeRequest =
                        (GroupNoticeRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupNoticeRequest.AgentData.SessionID != SessionId ||
                    groupNoticeRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.GroupNoticeRequest(this,
                                                  groupNoticeRequest.Data.GroupNoticeID);
            }
            return true;
        }
        private bool HandleGroupRoleUpdate(IClientAPI sender, Packet Pack)
        {
            GroupRoleUpdatePacket groupRoleUpdate =
                        (GroupRoleUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupRoleUpdate.AgentData.SessionID != SessionId ||
                    groupRoleUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                foreach (GroupRoleUpdatePacket.RoleDataBlock d in
                    groupRoleUpdate.RoleData)
                {
                    m_GroupsModule.GroupRoleUpdate(this,
                                                   groupRoleUpdate.AgentData.GroupID,
                                                   d.RoleID,
                                                   Utils.BytesToString(d.Name),
                                                   Utils.BytesToString(d.Description),
                                                   Utils.BytesToString(d.Title),
                                                   d.Powers,
                                                   d.UpdateType);
                }
                m_GroupsModule.NotifyChange(groupRoleUpdate.AgentData.GroupID);
            }
            return true;
        }
        private bool HandleGroupRoleChanges(IClientAPI sender, Packet Pack)
        {
            GroupRoleChangesPacket groupRoleChanges =
                        (GroupRoleChangesPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (groupRoleChanges.AgentData.SessionID != SessionId ||
                    groupRoleChanges.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                foreach (GroupRoleChangesPacket.RoleChangeBlock d in
                    groupRoleChanges.RoleChange)
                {
                    m_GroupsModule.GroupRoleChanges(this,
                                                    groupRoleChanges.AgentData.GroupID,
                                                    d.RoleID,
                                                    d.MemberID,
                                                    d.Change);
                }
                m_GroupsModule.NotifyChange(groupRoleChanges.AgentData.GroupID);
            }
            return true;
        }
        private bool HandleJoinGroupRequest(IClientAPI sender, Packet Pack)
        {
            JoinGroupRequestPacket joinGroupRequest =
                        (JoinGroupRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (joinGroupRequest.AgentData.SessionID != SessionId ||
                    joinGroupRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.JoinGroupRequest(this,
                        joinGroupRequest.GroupData.GroupID);
            }
            return true;
        }
        private bool HandleLeaveGroupRequest(IClientAPI sender, Packet Pack)
        {
            LeaveGroupRequestPacket leaveGroupRequest =
                        (LeaveGroupRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (leaveGroupRequest.AgentData.SessionID != SessionId ||
                    leaveGroupRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                m_GroupsModule.LeaveGroupRequest(this,
                        leaveGroupRequest.GroupData.GroupID);
            }
            return true;
        }
        private bool HandleEjectGroupMemberRequest(IClientAPI sender, Packet Pack)
        {
            EjectGroupMemberRequestPacket ejectGroupMemberRequest =
                       (EjectGroupMemberRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (ejectGroupMemberRequest.AgentData.SessionID != SessionId ||
                    ejectGroupMemberRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                foreach (EjectGroupMemberRequestPacket.EjectDataBlock e
                        in ejectGroupMemberRequest.EjectData)
                {
                    m_GroupsModule.EjectGroupMemberRequest(this,
                            ejectGroupMemberRequest.GroupData.GroupID,
                            e.EjecteeID);
                }
            }
            return true;
        }
        private bool HandleInviteGroupRequest(IClientAPI sender, Packet Pack)
        {
            InviteGroupRequestPacket inviteGroupRequest =
                        (InviteGroupRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (inviteGroupRequest.AgentData.SessionID != SessionId ||
                    inviteGroupRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            if (m_GroupsModule != null)
            {
                foreach (InviteGroupRequestPacket.InviteDataBlock b in
                        inviteGroupRequest.InviteData)
                {
                    m_GroupsModule.InviteGroupRequest(this,
                            inviteGroupRequest.GroupData.GroupID,
                            b.InviteeID,
                            b.RoleID);
                }
            }
            return true;
        }

        #endregion Groups

        private bool HandleStartLure(IClientAPI sender, Packet Pack)
        {
            StartLurePacket startLureRequest = (StartLurePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (startLureRequest.AgentData.SessionID != SessionId ||
                    startLureRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            StartLure handlerStartLure = OnStartLure;
            if (handlerStartLure != null)
                handlerStartLure(startLureRequest.Info.LureType,
                                 Utils.BytesToString(
                                    startLureRequest.Info.Message),
                                 startLureRequest.TargetData[0].TargetID,
                                 this);
            return true;
        }
        private bool HandleTeleportLureRequest(IClientAPI sender, Packet Pack)
        {
            TeleportLureRequestPacket teleportLureRequest =
                            (TeleportLureRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (teleportLureRequest.Info.SessionID != SessionId ||
                    teleportLureRequest.Info.AgentID != AgentId)
                    return true;
            }
            #endregion

            TeleportLureRequest handlerTeleportLureRequest = OnTeleportLureRequest;
            if (handlerTeleportLureRequest != null)
                handlerTeleportLureRequest(
                         teleportLureRequest.Info.LureID,
                         teleportLureRequest.Info.TeleportFlags,
                         this);
            return true;
        }
        private bool HandleClassifiedInfoRequest(IClientAPI sender, Packet Pack)
        {
            ClassifiedInfoRequestPacket classifiedInfoRequest =
                            (ClassifiedInfoRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (classifiedInfoRequest.AgentData.SessionID != SessionId ||
                    classifiedInfoRequest.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ClassifiedInfoRequest handlerClassifiedInfoRequest = OnClassifiedInfoRequest;
            if (handlerClassifiedInfoRequest != null)
                handlerClassifiedInfoRequest(
                         classifiedInfoRequest.Data.ClassifiedID,
                         this);
            return true;
        }
        private bool HandleClassifiedInfoUpdate(IClientAPI sender, Packet Pack)
        {
            ClassifiedInfoUpdatePacket classifiedInfoUpdate =
                            (ClassifiedInfoUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (classifiedInfoUpdate.AgentData.SessionID != SessionId ||
                    classifiedInfoUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ClassifiedInfoUpdate handlerClassifiedInfoUpdate = OnClassifiedInfoUpdate;
            if (handlerClassifiedInfoUpdate != null)
                handlerClassifiedInfoUpdate(
                        classifiedInfoUpdate.Data.ClassifiedID,
                        classifiedInfoUpdate.Data.Category,
                        Utils.BytesToString(
                                classifiedInfoUpdate.Data.Name),
                        Utils.BytesToString(
                                classifiedInfoUpdate.Data.Desc),
                        classifiedInfoUpdate.Data.ParcelID,
                        classifiedInfoUpdate.Data.ParentEstate,
                        classifiedInfoUpdate.Data.SnapshotID,
                        new Vector3(
                            classifiedInfoUpdate.Data.PosGlobal),
                        classifiedInfoUpdate.Data.ClassifiedFlags,
                        classifiedInfoUpdate.Data.PriceForListing,
                        this);
            return true;
        }
        private bool HandleClassifiedDelete(IClientAPI sender, Packet Pack)
        {
            ClassifiedDeletePacket classifiedDelete =
                           (ClassifiedDeletePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (classifiedDelete.AgentData.SessionID != SessionId ||
                    classifiedDelete.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ClassifiedDelete handlerClassifiedDelete = OnClassifiedDelete;
            if (handlerClassifiedDelete != null)
                handlerClassifiedDelete(
                         classifiedDelete.Data.ClassifiedID,
                         this);
            return true;
        }
        private bool HandleClassifiedGodDelete(IClientAPI sender, Packet Pack)
        {
            ClassifiedGodDeletePacket classifiedGodDelete =
                            (ClassifiedGodDeletePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (classifiedGodDelete.AgentData.SessionID != SessionId ||
                    classifiedGodDelete.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            ClassifiedDelete handlerClassifiedGodDelete = OnClassifiedGodDelete;
            if (handlerClassifiedGodDelete != null)
                handlerClassifiedGodDelete(
                         classifiedGodDelete.Data.ClassifiedID,
                         this);
            return true;
        }
        private bool HandleEventGodDelete(IClientAPI sender, Packet Pack)
        {
            EventGodDeletePacket eventGodDelete =
                               (EventGodDeletePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (eventGodDelete.AgentData.SessionID != SessionId ||
                    eventGodDelete.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            EventGodDelete handlerEventGodDelete = OnEventGodDelete;
            if (handlerEventGodDelete != null)
                handlerEventGodDelete(
                        eventGodDelete.EventData.EventID,
                        eventGodDelete.QueryData.QueryID,
                        Utils.BytesToString(
                                eventGodDelete.QueryData.QueryText),
                        eventGodDelete.QueryData.QueryFlags,
                        eventGodDelete.QueryData.QueryStart,
                        this);
            return true;
        }
        private bool HandleEventNotificationAddRequest(IClientAPI sender, Packet Pack)
        {
            EventNotificationAddRequestPacket eventNotificationAdd =
                            (EventNotificationAddRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (eventNotificationAdd.AgentData.SessionID != SessionId ||
                    eventNotificationAdd.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            EventNotificationAddRequest handlerEventNotificationAddRequest = OnEventNotificationAddRequest;
            if (handlerEventNotificationAddRequest != null)
                handlerEventNotificationAddRequest(
                        eventNotificationAdd.EventData.EventID, this);
            return true;
        }
        private bool HandleEventNotificationRemoveRequest(IClientAPI sender, Packet Pack)
        {
            EventNotificationRemoveRequestPacket eventNotificationRemove =
                            (EventNotificationRemoveRequestPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (eventNotificationRemove.AgentData.SessionID != SessionId ||
                    eventNotificationRemove.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            EventNotificationRemoveRequest handlerEventNotificationRemoveRequest = OnEventNotificationRemoveRequest;
            if (handlerEventNotificationRemoveRequest != null)
                handlerEventNotificationRemoveRequest(
                        eventNotificationRemove.EventData.EventID, this);
            return true;
        }
        private bool HandleRetrieveInstantMessages(IClientAPI sender, Packet Pack)
        {
            RetrieveInstantMessagesPacket rimpInstantMessagePack = (RetrieveInstantMessagesPacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (rimpInstantMessagePack.AgentData.SessionID != SessionId ||
                    rimpInstantMessagePack.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            RetrieveInstantMessages handlerRetrieveInstantMessages = OnRetrieveInstantMessages;
            if (handlerRetrieveInstantMessages != null)
                handlerRetrieveInstantMessages(this);
            return true;
        }
        private bool HandlePickDelete(IClientAPI sender, Packet Pack)
        {
            PickDeletePacket pickDelete =
                            (PickDeletePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (pickDelete.AgentData.SessionID != SessionId ||
                    pickDelete.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            PickDelete handlerPickDelete = OnPickDelete;
            if (handlerPickDelete != null)
                handlerPickDelete(this, pickDelete.Data.PickID);
            return true;
        }
        private bool HandlePickGodDelete(IClientAPI sender, Packet Pack)
        {
            PickGodDeletePacket pickGodDelete =
                           (PickGodDeletePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (pickGodDelete.AgentData.SessionID != SessionId ||
                    pickGodDelete.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            PickGodDelete handlerPickGodDelete = OnPickGodDelete;
            if (handlerPickGodDelete != null)
                handlerPickGodDelete(this,
                        pickGodDelete.AgentData.AgentID,
                        pickGodDelete.Data.PickID,
                        pickGodDelete.Data.QueryID);
            return true;
        }
        private bool HandlePickInfoUpdate(IClientAPI sender, Packet Pack)
        {
            PickInfoUpdatePacket pickInfoUpdate =
                            (PickInfoUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (pickInfoUpdate.AgentData.SessionID != SessionId ||
                    pickInfoUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            PickInfoUpdate handlerPickInfoUpdate = OnPickInfoUpdate;
            if (handlerPickInfoUpdate != null)
                handlerPickInfoUpdate(this,
                        pickInfoUpdate.Data.PickID,
                        pickInfoUpdate.Data.CreatorID,
                        pickInfoUpdate.Data.TopPick,
                        Utils.BytesToString(pickInfoUpdate.Data.Name),
                        Utils.BytesToString(pickInfoUpdate.Data.Desc),
                        pickInfoUpdate.Data.SnapshotID,
                        pickInfoUpdate.Data.SortOrder,
                        pickInfoUpdate.Data.Enabled,
                        pickInfoUpdate.Data.PosGlobal);
            return true;
        }
        private bool HandleAvatarNotesUpdate(IClientAPI sender, Packet Pack)
        {
            AvatarNotesUpdatePacket avatarNotesUpdate =
                            (AvatarNotesUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avatarNotesUpdate.AgentData.SessionID != SessionId ||
                    avatarNotesUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            AvatarNotesUpdate handlerAvatarNotesUpdate = OnAvatarNotesUpdate;
            if (handlerAvatarNotesUpdate != null)
                handlerAvatarNotesUpdate(this,
                        avatarNotesUpdate.Data.TargetID,
                        Utils.BytesToString(avatarNotesUpdate.Data.Notes));
            return true;
        }
        private bool HandleAvatarInterestsUpdate(IClientAPI sender, Packet Pack)
        {
            AvatarInterestsUpdatePacket avatarInterestUpdate =
                            (AvatarInterestsUpdatePacket)Pack;

            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (avatarInterestUpdate.AgentData.SessionID != SessionId ||
                    avatarInterestUpdate.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion

            AvatarInterestUpdate handlerAvatarInterestUpdate = OnAvatarInterestUpdate;
            if (handlerAvatarInterestUpdate != null)
                handlerAvatarInterestUpdate(this,
                    avatarInterestUpdate.PropertiesData.WantToMask,
                    Utils.BytesToString(avatarInterestUpdate.PropertiesData.WantToText),
                    avatarInterestUpdate.PropertiesData.SkillsMask,
                    Utils.BytesToString(avatarInterestUpdate.PropertiesData.SkillsText),
                    Utils.BytesToString(avatarInterestUpdate.PropertiesData.LanguagesText));
            return true;
        }
        private bool HandleGrantUserRights(IClientAPI sender, Packet Pack)
        {
            GrantUserRightsPacket GrantUserRights =
                            (GrantUserRightsPacket)Pack;
            #region Packet Session and User Check
            if (m_checkPackets)
            {
                if (GrantUserRights.AgentData.SessionID != SessionId ||
                    GrantUserRights.AgentData.AgentID != AgentId)
                    return true;
            }
            #endregion
            GrantUserFriendRights GrantUserRightsHandler = OnGrantUserRights;
            if (GrantUserRightsHandler != null)
                GrantUserRightsHandler(this,
                    GrantUserRights.AgentData.AgentID,
                    GrantUserRights.Rights[0].AgentRelated,
                    GrantUserRights.Rights[0].RelatedRights);
            return true;
        }
        private bool HandlePlacesQuery(IClientAPI sender, Packet Pack)
        {
            PlacesQueryPacket placesQueryPacket =
                            (PlacesQueryPacket)Pack;

            PlacesQuery handlerPlacesQuery = OnPlacesQuery;

            if (handlerPlacesQuery != null)
                handlerPlacesQuery(placesQueryPacket.AgentData.QueryID,
                        placesQueryPacket.TransactionData.TransactionID,
                        Utils.BytesToString(
                                placesQueryPacket.QueryData.QueryText),
                        placesQueryPacket.QueryData.QueryFlags,
                        (byte)placesQueryPacket.QueryData.Category,
                        Utils.BytesToString(
                                placesQueryPacket.QueryData.SimName),
                        this);
            return true;
        }

        #endregion Packet Handlers

        public void SendScriptQuestion(UUID taskID, string taskName, string ownerName, UUID itemID, int question)
        {
            ScriptQuestionPacket scriptQuestion = (ScriptQuestionPacket)PacketPool.Instance.GetPacket(PacketType.ScriptQuestion);
            scriptQuestion.Data = new ScriptQuestionPacket.DataBlock();
            // TODO: don't create new blocks if recycling an old packet
            scriptQuestion.Data.TaskID = taskID;
            scriptQuestion.Data.ItemID = itemID;
            scriptQuestion.Data.Questions = question;
            scriptQuestion.Data.ObjectName = Util.StringToBytes256(taskName);
            scriptQuestion.Data.ObjectOwner = Util.StringToBytes256(ownerName);

            OutPacket(scriptQuestion, ThrottleOutPacketType.AvatarInfo);
        }

        private void InitDefaultAnimations()
        {
            using (XmlTextReader reader = new XmlTextReader("data/avataranimations.xml"))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                if (doc.DocumentElement != null)
                    foreach (XmlNode nod in doc.DocumentElement.ChildNodes)
                    {
                        if (nod.Attributes["name"] != null)
                        {
                            string name = nod.Attributes["name"].Value.ToLower();
                            string id = nod.InnerText;
                            m_defaultAnimations.Add(name, (UUID)id);
                        }
                    }
            }
        }

        public UUID GetDefaultAnimation(string name)
        {
            if (m_defaultAnimations.ContainsKey(name))
                return m_defaultAnimations[name];
            return UUID.Zero;
        }

        /// <summary>
        /// Handler called when we receive a logout packet.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        /// <returns></returns>
        protected virtual bool HandleLogout(IClientAPI client, Packet packet)
        {
            if (packet.Type == PacketType.LogoutRequest)
            {
                if (((LogoutRequestPacket)packet).AgentData.SessionID != SessionId) return false;
            }

            return Logout(client);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual bool Logout(IClientAPI client)
        {
            //m_log.InfoFormat("[CLIENT]: Got a logout request for {0} in {1}", Name, Scene.RegionInfo.RegionName);

            Action<IClientAPI> handlerLogout = OnLogout;

            if (handlerLogout != null)
            {
                handlerLogout(client);
            }

            return true;
        }

        protected bool HandleMultipleObjUpdate(IClientAPI simClient, Packet packet)
        {
            MultipleObjectUpdatePacket multipleupdate = (MultipleObjectUpdatePacket)packet;
            if (multipleupdate.AgentData.SessionID != SessionId) return false;
            // m_log.Debug("new multi update packet " + multipleupdate.ToString());
            Scene tScene = (Scene)m_scene;

            for (int i = 0; i < multipleupdate.ObjectData.Length; i++)
            {
                MultipleObjectUpdatePacket.ObjectDataBlock block = multipleupdate.ObjectData[i];

                // Can't act on Null Data
                if (block.Data != null)
                {
                    uint localId = block.ObjectLocalID;
                    SceneObjectPart part = tScene.GetSceneObjectPart(localId);

                    if (part == null)
                    {
                        // It's a ghost! tell the client to delete it from view.
                        simClient.SendKillObject(Scene.RegionInfo.RegionHandle,
                                                 new ISceneEntity[]{part});
                    }
                    else
                    {
                        // UUID partId = part.UUID;
                        UpdatePrimGroupRotation handlerUpdatePrimGroupRotation;

                        switch (block.Type)
                        {
                            case 1:
                                Vector3 pos1 = new Vector3(block.Data, 0);

                                UpdateVectorWithUpdate handlerUpdatePrimSinglePosition = OnUpdatePrimSinglePosition;
                                if (handlerUpdatePrimSinglePosition != null)
                                {
                                    // m_log.Debug("new movement position is " + pos.X + " , " + pos.Y + " , " + pos.Z);
                                    handlerUpdatePrimSinglePosition(localId, pos1, this, true);
                                }
                                break;
                            case 2:
                                Quaternion rot1 = new Quaternion(block.Data, 0, true);

                                UpdatePrimSingleRotation handlerUpdatePrimSingleRotation = OnUpdatePrimSingleRotation;
                                if (handlerUpdatePrimSingleRotation != null)
                                {
                                    // m_log.Info("new tab rotation is " + rot1.X + " , " + rot1.Y + " , " + rot1.Z + " , " + rot1.W);
                                    handlerUpdatePrimSingleRotation(localId, rot1, this);
                                }
                                break;
                            case 3:
                                Vector3 rotPos = new Vector3(block.Data, 0);
                                Quaternion rot2 = new Quaternion(block.Data, 12, true);

                                UpdatePrimSingleRotationPosition handlerUpdatePrimSingleRotationPosition = OnUpdatePrimSingleRotationPosition;
                                if (handlerUpdatePrimSingleRotationPosition != null)
                                {
                                    // m_log.Debug("new mouse rotation position is " + rotPos.X + " , " + rotPos.Y + " , " + rotPos.Z);
                                    // m_log.Info("new mouse rotation is " + rot2.X + " , " + rot2.Y + " , " + rot2.Z + " , " + rot2.W);
                                    handlerUpdatePrimSingleRotationPosition(localId, rot2, rotPos, this);
                                }
                                break;
                            case 4:
                            case 20:
                                Vector3 scale4 = new Vector3(block.Data, 0);

                                UpdateVector handlerUpdatePrimScale = OnUpdatePrimScale;
                                if (handlerUpdatePrimScale != null)
                                {
                                    //                                     m_log.Debug("new scale is " + scale4.X + " , " + scale4.Y + " , " + scale4.Z);
                                    handlerUpdatePrimScale(localId, scale4, this);
                                }
                                break;
                            case 5:

                                Vector3 scale1 = new Vector3(block.Data, 12);
                                Vector3 pos11 = new Vector3(block.Data, 0);

                                handlerUpdatePrimScale = OnUpdatePrimScale;
                                if (handlerUpdatePrimScale != null)
                                {
                                    // m_log.Debug("new scale is " + scale.X + " , " + scale.Y + " , " + scale.Z);
                                    handlerUpdatePrimScale(localId, scale1, this);

                                    handlerUpdatePrimSinglePosition = OnUpdatePrimSinglePosition;
                                    if (handlerUpdatePrimSinglePosition != null)
                                    {
                                        handlerUpdatePrimSinglePosition(localId, pos11, this, false);
                                    }
                                }
                                break;
                            case 9:
                                Vector3 pos2 = new Vector3(block.Data, 0);

                                UpdateVectorWithUpdate handlerUpdateVector = OnUpdatePrimGroupPosition;

                                if (handlerUpdateVector != null)
                                {

                                    handlerUpdateVector(localId, pos2, this, true);
                                }
                                break;
                            case 10:
                                Quaternion rot3 = new Quaternion(block.Data, 0, true);

                                UpdatePrimRotation handlerUpdatePrimRotation = OnUpdatePrimGroupRotation;
                                if (handlerUpdatePrimRotation != null)
                                {
                                    //  Console.WriteLine("new rotation is " + rot3.X + " , " + rot3.Y + " , " + rot3.Z + " , " + rot3.W);
                                    handlerUpdatePrimRotation(localId, rot3, this);
                                }
                                break;
                            case 11:
                                Vector3 pos3 = new Vector3(block.Data, 0);
                                Quaternion rot4 = new Quaternion(block.Data, 12, true);

                                handlerUpdatePrimGroupRotation = OnUpdatePrimGroupMouseRotation;
                                if (handlerUpdatePrimGroupRotation != null)
                                {
                                    //  m_log.Debug("new rotation position is " + pos.X + " , " + pos.Y + " , " + pos.Z);
                                    // m_log.Debug("new group mouse rotation is " + rot4.X + " , " + rot4.Y + " , " + rot4.Z + " , " + rot4.W);
                                    handlerUpdatePrimGroupRotation(localId, pos3, rot4, this);
                                }
                                break;
                            case 12:
                            case 28:
                                Vector3 scale7 = new Vector3(block.Data, 0);

                                UpdateVector handlerUpdatePrimGroupScale = OnUpdatePrimGroupScale;
                                if (handlerUpdatePrimGroupScale != null)
                                {
                                    //                                     m_log.Debug("new scale is " + scale7.X + " , " + scale7.Y + " , " + scale7.Z);
                                    handlerUpdatePrimGroupScale(localId, scale7, this);
                                }
                                break;
                            case 13:
                                Vector3 scale2 = new Vector3(block.Data, 12);
                                Vector3 pos4 = new Vector3(block.Data, 0);

                                handlerUpdatePrimScale = OnUpdatePrimScale;
                                if (handlerUpdatePrimScale != null)
                                {
                                    //m_log.Debug("new scale is " + scale.X + " , " + scale.Y + " , " + scale.Z);
                                    handlerUpdatePrimScale(localId, scale2, this);

                                    // Change the position based on scale (for bug number 246)
                                    handlerUpdatePrimSinglePosition = OnUpdatePrimSinglePosition;
                                    // m_log.Debug("new movement position is " + pos.X + " , " + pos.Y + " , " + pos.Z);
                                    if (handlerUpdatePrimSinglePosition != null)
                                    {
                                        handlerUpdatePrimSinglePosition(localId, pos4, this, false);
                                    }
                                }
                                break;
                            case 29:
                                Vector3 scale5 = new Vector3(block.Data, 12);
                                Vector3 pos5 = new Vector3(block.Data, 0);

                                handlerUpdatePrimGroupScale = OnUpdatePrimGroupScale;
                                if (handlerUpdatePrimGroupScale != null)
                                {
                                    // m_log.Debug("new scale is " + scale.X + " , " + scale.Y + " , " + scale.Z);
                                    handlerUpdatePrimGroupScale(localId, scale5, this);
                                    handlerUpdateVector = OnUpdatePrimGroupPosition;

                                    if (handlerUpdateVector != null)
                                    {
                                        handlerUpdateVector(localId, pos5, this, false);
                                    }
                                }
                                break;
                            case 21:
                                Vector3 scale6 = new Vector3(block.Data, 12);
                                Vector3 pos6 = new Vector3(block.Data, 0);

                                handlerUpdatePrimScale = OnUpdatePrimScale;
                                if (handlerUpdatePrimScale != null)
                                {
                                    // m_log.Debug("new scale is " + scale.X + " , " + scale.Y + " , " + scale.Z);
                                    handlerUpdatePrimScale(localId, scale6, this);
                                    handlerUpdatePrimSinglePosition = OnUpdatePrimSinglePosition;
                                    if (handlerUpdatePrimSinglePosition != null)
                                    {
                                        handlerUpdatePrimSinglePosition(localId, pos6, this, false);
                                    }
                                }
                                break;
                            default:
                                m_log.Debug("[CLIENT] MultipleObjUpdate recieved an unknown packet type: " + (block.Type));
                                break;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the throttles from values supplied by the client
        /// </summary>
        /// <param name="throttles"></param>
        public void SetChildAgentThrottle(byte[] throttles)
        {
            m_udpClient.SetThrottles(throttles);
        }

        /// <summary>
        /// Get the current throttles for this client as a packed byte array
        /// </summary>
        /// <param name="multiplier">Unused</param>
        /// <returns></returns>
        public byte[] GetThrottlesPacked(float multiplier)
        {
            return m_udpClient.GetThrottlesPacked();
        }

        /// <summary>
        /// This is the starting point for sending a simulator packet out to the client
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="throttlePacketType">Throttling category for the packet</param>
        protected void OutPacket(Packet packet, ThrottleOutPacketType throttlePacketType)
        {
            #region BinaryStats
            LLUDPServer.LogPacketHeader(false, m_circuitCode, 0, packet.Type, (ushort)packet.Length);
            #endregion BinaryStats

            OutPacket(packet, throttlePacketType, true);
        }

        /// <summary>
        /// This is the starting point for sending a simulator packet out to the client
        /// </summary>
        /// <param name="packet">Packet to send</param>
        /// <param name="throttlePacketType">Throttling category for the packet</param>
        /// <param name="doAutomaticSplitting">True to automatically split oversized
        /// packets (the default), or false to disable splitting if the calling code
        /// handles splitting manually</param>
        protected void OutPacket(Packet packet, ThrottleOutPacketType throttlePacketType, bool doAutomaticSplitting)
        {
            if (m_debugPacketLevel > 0)
            {
                bool outputPacket = true;

                if (m_debugPacketLevel <= 255
                    && (packet.Type == PacketType.SimStats || packet.Type == PacketType.SimulatorViewerTimeMessage))
                    outputPacket = false;

                if (m_debugPacketLevel <= 200
                    && (packet.Type == PacketType.ImagePacket
                        || packet.Type == PacketType.ImageData
                        || packet.Type == PacketType.LayerData
                        || packet.Type == PacketType.CoarseLocationUpdate))
                    outputPacket = false;

                if (m_debugPacketLevel <= 100 && (packet.Type == PacketType.AvatarAnimation || packet.Type == PacketType.ViewerEffect))
                    outputPacket = false;

                if (outputPacket)
                    m_log.DebugFormat("[CLIENT]: Packet OUT {0}", packet.Type);
            }

            m_udpServer.SendPacket(m_udpClient, packet, throttlePacketType, doAutomaticSplitting);
        }

        /// <summary>
        /// Breaks down the genericMessagePacket into specific events
        /// </summary>
        /// <param name="gmMethod"></param>
        /// <param name="gmInvoice"></param>
        /// <param name="gmParams"></param>
        public void DecipherGenericMessage(string gmMethod, UUID gmInvoice, GenericMessagePacket.ParamListBlock[] gmParams)
        {
            switch (gmMethod)
            {
                case "autopilot":
                    float locx;
                    float locy;
                    float locz;

                    try
                    {
                        uint regionX;
                        uint regionY;
                        Utils.LongToUInts(Scene.RegionInfo.RegionHandle, out regionX, out regionY);
                        locx = Convert.ToSingle(Utils.BytesToString(gmParams[0].Parameter)) - regionX;
                        locy = Convert.ToSingle(Utils.BytesToString(gmParams[1].Parameter)) - regionY;
                        locz = Convert.ToSingle(Utils.BytesToString(gmParams[2].Parameter));
                    }
                    catch (InvalidCastException)
                    {
                        m_log.Error("[CLIENT]: Invalid autopilot request");
                        return;
                    }

                    UpdateVector handlerAutoPilotGo = OnAutoPilotGo;
                    if (handlerAutoPilotGo != null)
                    {
                        handlerAutoPilotGo(0, new Vector3(locx, locy, locz), this);
                    }
                    m_log.InfoFormat("[CLIENT]: Client Requests autopilot to position <{0},{1},{2}>", locx, locy, locz);


                    break;
                default:
                    m_log.Debug("[CLIENT]: Unknown Generic Message, Method: " + gmMethod + ". Invoice: " + gmInvoice + ".  Dumping Params:");
                    for (int hi = 0; hi < gmParams.Length; hi++)
                    {
                        Console.WriteLine(gmParams[hi].ToString());
                    }
                    //gmpack.MethodData.
                    break;

            }
        }

        /// <summary>
        /// Entryway from the client to the simulator.  All UDP packets from the client will end up here
        /// </summary>
        /// <param name="Pack">OpenMetaverse.packet</param>
        public void ProcessInPacket(Packet packet)
        {
            if (m_debugPacketLevel > 0)
            {
                bool outputPacket = true;

                if (m_debugPacketLevel <= 255 && packet.Type == PacketType.AgentUpdate)
                    outputPacket = false;

                if (m_debugPacketLevel <= 200 && packet.Type == PacketType.RequestImage)
                    outputPacket = false;

                if (m_debugPacketLevel <= 100 && (packet.Type == PacketType.ViewerEffect || packet.Type == PacketType.AgentAnimation))
                    outputPacket = false;

                if (outputPacket)
                    m_log.DebugFormat("[CLIENT]: Packet IN {0}", packet.Type);
            }

            if (!ProcessPacketMethod(packet))
                m_log.Warn("[CLIENT]: unhandled packet " + packet.Type);

            PacketPool.Instance.ReturnPacket(packet);
        }

        private static PrimitiveBaseShape GetShapeFromAddPacket(ObjectAddPacket addPacket)
        {
            PrimitiveBaseShape shape = new PrimitiveBaseShape();

            shape.PCode = addPacket.ObjectData.PCode;
            shape.State = addPacket.ObjectData.State;
            shape.PathBegin = addPacket.ObjectData.PathBegin;
            shape.PathEnd = addPacket.ObjectData.PathEnd;
            shape.PathScaleX = addPacket.ObjectData.PathScaleX;
            shape.PathScaleY = addPacket.ObjectData.PathScaleY;
            shape.PathShearX = addPacket.ObjectData.PathShearX;
            shape.PathShearY = addPacket.ObjectData.PathShearY;
            shape.PathSkew = addPacket.ObjectData.PathSkew;
            shape.ProfileBegin = addPacket.ObjectData.ProfileBegin;
            shape.ProfileEnd = addPacket.ObjectData.ProfileEnd;
            shape.Scale = addPacket.ObjectData.Scale;
            shape.PathCurve = addPacket.ObjectData.PathCurve;
            shape.ProfileCurve = addPacket.ObjectData.ProfileCurve;
            shape.ProfileHollow = addPacket.ObjectData.ProfileHollow;
            shape.PathRadiusOffset = addPacket.ObjectData.PathRadiusOffset;
            shape.PathRevolutions = addPacket.ObjectData.PathRevolutions;
            shape.PathTaperX = addPacket.ObjectData.PathTaperX;
            shape.PathTaperY = addPacket.ObjectData.PathTaperY;
            shape.PathTwist = addPacket.ObjectData.PathTwist;
            shape.PathTwistBegin = addPacket.ObjectData.PathTwistBegin;
            Primitive.TextureEntry ntex = new Primitive.TextureEntry(new UUID("89556747-24cb-43ed-920b-47caed15465f"));
            shape.TextureEntry = ntex.GetBytes();
            //shape.Textures = ntex;
            return shape;
        }

        public EndPoint GetClientEP()
        {
            return m_userEndPoint;
        }

        #region Media Parcel Members

        public void SendParcelMediaCommand(uint flags, ParcelMediaCommandEnum command, float time)
        {
            ParcelMediaCommandMessagePacket commandMessagePacket = new ParcelMediaCommandMessagePacket();
            commandMessagePacket.CommandBlock.Flags = flags;
            commandMessagePacket.CommandBlock.Command = (uint)command;
            commandMessagePacket.CommandBlock.Time = time;

            OutPacket(commandMessagePacket, ThrottleOutPacketType.Land);
        }

        public void SendParcelMediaUpdate(string mediaUrl, UUID mediaTextureID,
                                   byte autoScale, string mediaType, string mediaDesc, int mediaWidth, int mediaHeight,
                                   byte mediaLoop)
        {
            ParcelMediaUpdatePacket updatePacket = new ParcelMediaUpdatePacket();
            updatePacket.DataBlock.MediaURL = Util.StringToBytes256(mediaUrl);
            updatePacket.DataBlock.MediaID = mediaTextureID;
            updatePacket.DataBlock.MediaAutoScale = autoScale;

            updatePacket.DataBlockExtended.MediaType = Util.StringToBytes256(mediaType);
            updatePacket.DataBlockExtended.MediaDesc = Util.StringToBytes256(mediaDesc);
            updatePacket.DataBlockExtended.MediaWidth = mediaWidth;
            updatePacket.DataBlockExtended.MediaHeight = mediaHeight;
            updatePacket.DataBlockExtended.MediaLoop = mediaLoop;

            OutPacket(updatePacket, ThrottleOutPacketType.Land);
        }

        #endregion

        #region Camera

        public void SendSetFollowCamProperties(UUID objectID, SortedDictionary<int, float> parameters)
        {
            SetFollowCamPropertiesPacket packet = (SetFollowCamPropertiesPacket)PacketPool.Instance.GetPacket(PacketType.SetFollowCamProperties);
            packet.ObjectData.ObjectID = objectID;
            SetFollowCamPropertiesPacket.CameraPropertyBlock[] camPropBlock = new SetFollowCamPropertiesPacket.CameraPropertyBlock[parameters.Count];
            uint idx = 0;
            foreach (KeyValuePair<int, float> pair in parameters)
            {
                SetFollowCamPropertiesPacket.CameraPropertyBlock block = new SetFollowCamPropertiesPacket.CameraPropertyBlock();
                block.Type = pair.Key;
                block.Value = pair.Value;

                camPropBlock[idx++] = block;
            }
            packet.CameraProperty = camPropBlock;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendClearFollowCamProperties(UUID objectID)
        {
            ClearFollowCamPropertiesPacket packet = (ClearFollowCamPropertiesPacket)PacketPool.Instance.GetPacket(PacketType.ClearFollowCamProperties);
            packet.ObjectData.ObjectID = objectID;
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        #endregion

        #region IClientCore

        private readonly Dictionary<Type, object> m_clientInterfaces = new Dictionary<Type, object>();

        /// <summary>
        /// Register an interface on this client, should only be called in the constructor.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iface"></param>
        protected void RegisterInterface<T>(T iface)
        {
            lock (m_clientInterfaces)
            {
                if (!m_clientInterfaces.ContainsKey(typeof(T)))
                {
                    m_clientInterfaces.Add(typeof(T), iface);
                }
            }
        }

        public bool TryGet<T>(out T iface)
        {
            if (m_clientInterfaces.ContainsKey(typeof(T)))
            {
                iface = (T)m_clientInterfaces[typeof(T)];
                return true;
            }
            iface = default(T);
            return false;
        }

        public T Get<T>()
        {
            return (T)m_clientInterfaces[typeof(T)];
        }

        public void Disconnect(string reason)
        {
            Kick(reason);
            Thread.Sleep(1000);
            Disconnect();
        }

        public void Disconnect()
        {
            Close();
        }

        #endregion

        public string Report()
        {
            return m_udpClient.GetStats();
        }

        /// <summary>
        /// Make an asset request to the asset service in response to a client request.
        /// </summary>
        /// <param name="transferRequest"></param>
        /// <param name="taskID"></param>
        protected void MakeAssetRequest(TransferRequestPacket transferRequest, UUID taskID)
        {
            UUID requestID = UUID.Zero;
            if (transferRequest.TransferInfo.SourceType == (int)SourceType.Asset)
            {
                requestID = new UUID(transferRequest.TransferInfo.Params, 0);
            }
            else if (transferRequest.TransferInfo.SourceType == (int)SourceType.SimInventoryItem)
            {
                requestID = new UUID(transferRequest.TransferInfo.Params, 80);
            }

            //m_log.InfoFormat("[CLIENT]: {0} requesting asset {1}", Name, requestID);

            m_assetService.Get(requestID.ToString(), transferRequest, AssetReceived);
        }

        /// <summary>
        /// When we get a reply back from the asset service in response to a client request, send back the data.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sender"></param>
        /// <param name="asset"></param>
        protected void AssetReceived(string id, Object sender, AssetBase asset)
        {
            //m_log.InfoFormat("[CLIENT]: {0} found requested asset", Name);

            TransferRequestPacket transferRequest = (TransferRequestPacket)sender;

            UUID requestID = UUID.Zero;
            byte source = (byte)SourceType.Asset;

            if (transferRequest.TransferInfo.SourceType == (int)SourceType.Asset)
            {
                requestID = new UUID(transferRequest.TransferInfo.Params, 0);
            }
            else if (transferRequest.TransferInfo.SourceType == (int)SourceType.SimInventoryItem)
            {
                requestID = new UUID(transferRequest.TransferInfo.Params, 80);
                source = (byte)SourceType.SimInventoryItem;
            }

            // The asset is known to exist and is in our cache, so add it to the AssetRequests list
            AssetRequestToClient req = new AssetRequestToClient();
            req.AssetInf = asset;
            req.AssetRequestSource = source;
            req.IsTextureRequest = false;
            req.NumPackets = asset == null ? 0 : CalculateNumPackets(asset.Data);
            req.Params = transferRequest.TransferInfo.Params;
            req.RequestAssetID = requestID;
            req.TransferRequestID = transferRequest.TransferInfo.TransferID;

            if (asset == null)
            {
                SendFailedAsset(req, TransferPacketStatus.AssetUnknownSource);
                return;
            }
            // Scripts cannot be retrieved by direct request
            else if (transferRequest.TransferInfo.SourceType == (int)SourceType.Asset && asset.Type == 10)
            {
                SendFailedAsset(req, TransferPacketStatus.InsufficientPermissions);
                return;
            }

            SendAsset(req);
        }

        /// <summary>
        /// Calculate the number of packets required to send the asset to the client.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static int CalculateNumPackets(byte[] data)
        {
            const uint m_maxPacketSize = 1024;

            int numPackets = 1;

            if (data == null)
                return 0;

            if (data.LongLength > m_maxPacketSize)
            {
                // over max number of bytes so split up file
                long restData = data.LongLength - m_maxPacketSize;
                int restPackets = (int)((restData + m_maxPacketSize - 1) / m_maxPacketSize);
                numPackets += restPackets;
            }

            return numPackets;
        }

        #region IClientIPEndpoint Members

        public IPAddress EndPoint
        {
            get
            {
                if (m_userEndPoint is IPEndPoint)
                {
                    IPEndPoint ep = (IPEndPoint)m_userEndPoint;

                    return ep.Address;
                }
                return null;
            }
        }

        #endregion

        public void SendRebakeAvatarTextures(UUID textureID)
        {
            RebakeAvatarTexturesPacket pack =
                (RebakeAvatarTexturesPacket)PacketPool.Instance.GetPacket(PacketType.RebakeAvatarTextures);

            pack.TextureData = new RebakeAvatarTexturesPacket.TextureDataBlock();
            pack.TextureData.TextureID = textureID;
            OutPacket(pack, ThrottleOutPacketType.Texture);
        }

        #region PriorityQueue
        
        public struct PacketProcessor
        {
            public PacketMethod method;
            public bool Async;
        }

        public class AsyncPacketProcess
        {
            public bool result = false;
            public readonly LLClientView ClientView = null;
            public readonly Packet Pack = null;
            public readonly PacketMethod Method = null;
            public AsyncPacketProcess(LLClientView pClientview, PacketMethod pMethod, Packet pPack)
            {
                ClientView = pClientview;
                Method = pMethod;
                Pack = pPack;
            }
        }

        #endregion

        public static OSD BuildEvent(string eventName, OSD eventBody)
        {
            OSDMap osdEvent = new OSDMap(2);
            osdEvent.Add("message", new OSDString(eventName));
            osdEvent.Add("body", eventBody);

            return osdEvent;
        }

        public void SendAvatarInterestsReply(UUID avatarID, uint wantMask, string wantText, uint skillsMask, string skillsText, string languages)
        {
            AvatarInterestsReplyPacket packet = (AvatarInterestsReplyPacket)PacketPool.Instance.GetPacket(PacketType.AvatarInterestsReply);

            packet.AgentData = new AvatarInterestsReplyPacket.AgentDataBlock();
            packet.AgentData.AgentID = AgentId;
            packet.AgentData.AvatarID = avatarID;

            packet.PropertiesData = new AvatarInterestsReplyPacket.PropertiesDataBlock();
            packet.PropertiesData.WantToMask = wantMask;
            packet.PropertiesData.WantToText = Utils.StringToBytes(wantText);
            packet.PropertiesData.SkillsMask = skillsMask;
            packet.PropertiesData.SkillsText = Utils.StringToBytes(skillsText);
            packet.PropertiesData.LanguagesText = Utils.StringToBytes(languages);
            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendChangeUserRights(UUID agentID, UUID friendID, int rights)
        {
            ChangeUserRightsPacket packet = (ChangeUserRightsPacket)PacketPool.Instance.GetPacket(PacketType.ChangeUserRights);

            packet.AgentData = new ChangeUserRightsPacket.AgentDataBlock();
            packet.AgentData.AgentID = agentID;

            packet.Rights = new ChangeUserRightsPacket.RightsBlock[1];
            packet.Rights[0] = new ChangeUserRightsPacket.RightsBlock();
            packet.Rights[0].AgentRelated = friendID;
            packet.Rights[0].RelatedRights = rights;

            OutPacket(packet, ThrottleOutPacketType.AvatarInfo);
        }

        public void SendTextBoxRequest(string message, int chatChannel, string objectname, string ownerFirstName, string ownerLastName, UUID objectId)
        {
            ScriptDialogPacket dialog = (ScriptDialogPacket)PacketPool.Instance.GetPacket(PacketType.ScriptDialog);
            dialog.Data.ObjectID = objectId;
            dialog.Data.ChatChannel = chatChannel;
            dialog.Data.ImageID = UUID.Zero;
            dialog.Data.ObjectName = Util.StringToBytes256(objectname);
            // this is the username of the *owner*
            dialog.Data.FirstName = Util.StringToBytes256(ownerFirstName);
            dialog.Data.LastName = Util.StringToBytes256(ownerLastName);
            dialog.Data.Message =  Util.StringToBytes256(message);

            ScriptDialogPacket.ButtonsBlock[] buttons = new ScriptDialogPacket.ButtonsBlock[1];
            buttons[0] = new ScriptDialogPacket.ButtonsBlock();
            buttons[0].ButtonLabel = Util.StringToBytes256("!!llTextBox!!");
            dialog.Buttons = buttons;
            OutPacket(dialog, ThrottleOutPacketType.AvatarInfo);
        }

        public void StopFlying(ISceneEntity p)
        {
            if (p is ScenePresence)
            {
                ScenePresence presence = p as ScenePresence;
                // It turns out to get the agent to stop flying, you have to feed it stop flying velocities
                // There's no explicit message to send the client to tell it to stop flying..   it relies on the
                // velocity, collision plane and avatar height

                // Add 1/6 the avatar's height to it's position so it doesn't shoot into the air
                // when the avatar stands up

                Vector3 pos = presence.AbsolutePosition;

                pos += new Vector3(0f, 0f, (presence.Appearance.AvatarHeight/6f));

                presence.AbsolutePosition = pos;

                // attach a suitable collision plane regardless of the actual situation to force the LLClient to land.
                // Collision plane below the avatar's position a 6th of the avatar's height is suitable.
                // Mind you, that this method doesn't get called if the avatar's velocity magnitude is greater then a
                // certain amount..   because the LLClient wouldn't land in that situation anyway.

                // why are we still testing for this really old height value default???
                presence.CollisionPlane = new Vector4(0, 0, 0, pos.Z - presence.Appearance.AvatarHeight/6f);


                ImprovedTerseObjectUpdatePacket.ObjectDataBlock block =
                    CreateImprovedTerseBlock(p, false);

                float TIME_DILATION = m_scene.TimeDilation;
                ushort timeDilation = Utils.FloatToUInt16(TIME_DILATION, 0.0f, 1.0f);


                ImprovedTerseObjectUpdatePacket packet = new ImprovedTerseObjectUpdatePacket();
                packet.RegionData.RegionHandle = m_scene.RegionInfo.RegionHandle;
                packet.RegionData.TimeDilation = timeDilation;
                packet.ObjectData = new ImprovedTerseObjectUpdatePacket.ObjectDataBlock[1];

                packet.ObjectData[0] = block;

                OutPacket(packet, ThrottleOutPacketType.Task, true);
            }

            //ControllingClient.SendAvatarTerseUpdate(new SendAvatarTerseData(m_rootRegionHandle, (ushort)(m_scene.TimeDilation * ushort.MaxValue), LocalId,
            //        AbsolutePosition, Velocity, Vector3.Zero, m_bodyRot, new Vector4(0,0,1,AbsolutePosition.Z - 0.5f), m_uuid, null, GetUpdatePriority(ControllingClient)));
        }
    }
}
