using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;
using Epic.OnlineServices.UserInfo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MythrenFighter
{
    [System.Serializable]
    public class PlayerInfo
    {
        public ProductUserId productUserId;
        public string userName;
        public int playerSlot;
        public bool isLobbyOwner;
        public bool isReady;
        public bool isLoaded;
        public string selectedFighterId;
    }

    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }
        public static readonly byte STRING_CHANNEL = 0x20;
        public static readonly byte INPUT_CHANNEL = 0x21;
        public static readonly int MAX_LOBBY_MEMBER_COUNT = 2;
        public static readonly AttributeData defaultLobbyAttribute = new AttributeData { Key = "default", Value = "default" };
        public static readonly List<AttributeData> defaultLobbyData = new List<AttributeData> { 
            defaultLobbyAttribute,
            new AttributeData { Key = "inGame", Value = false }
        };

        // Variables
        public bool inLobby = false;
        public string lobbyId = "";
        public bool isLobbyOwner = false;
        public bool inGame = false;
        public System.Diagnostics.Stopwatch pingStopwatch = new System.Diagnostics.Stopwatch();
        public List<float> lastFivePings = new List<float>();
        public float averagePing = 0;
        public LobbyDetails connectedLobby;
        public List<LobbyDetails> currentLobbySearch = new List<LobbyDetails>();
        public List<PlayerInfo> playerInfos = new List<PlayerInfo>();

        // References to notificatons
        ulong notifyLobbyUpdateReceived;
        ulong notifyLobbyMemberUpdateReceived;
        ulong notifyLobbyMemberStatusReceived;

        // Epic Online Callbacks
        private OnCreateLobbyCallback lobbyCreatedCallback;
        private OnUpdateLobbyCallback lobbyUpdatedCallback;
        private OnJoinLobbyCallback lobbyJoinedCallback;
        private OnLeaveLobbyCallback lobbyLeftCallback;
        private OnLobbyUpdateReceivedCallback lobbyUpdateReceivedCallback;
        private OnLobbyMemberUpdateReceivedCallback lobbyMemberUpdateReceivedCallback;
        private OnLobbyMemberStatusReceivedCallback lobbyMemberStatusReceivedCallback;
        private OnDestroyLobbyCallback lobbyDestroyedCallback;

        // My Lobby Callbacks
        public delegate void LobbySearchCompleteDelegate();
        public LobbySearchCompleteDelegate lobbySearchCompleteEvent;
        public delegate void LobbyCreatedDelegate();
        public LobbyCreatedDelegate lobbyCreatedEvent;
        public delegate void LobbyUpdatedDelegate();
        public LobbyCreatedDelegate lobbyUpdatedEvent;
        public delegate void LobbyJoinedDelegate();
        public LobbyJoinedDelegate lobbyJoinedEvent;
        public delegate void LobbyLeftDelegate();
        public LobbyLeftDelegate lobbyLeftEvent;
        public delegate void StartBattleDelegate();
        public StartBattleDelegate startBattleEvent;

        public delegate void ReceivedOnlineInputsDelegate(OnlineInputData onlineInputData);
        public ReceivedOnlineInputsDelegate receivedOnlineInputsEvent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                lobbyCreatedCallback += LobbyCreatedHandler;
                lobbyUpdatedCallback += LobbyUpdatedHandler;
                lobbyJoinedCallback += LobbyJoinedHandler;
                lobbyLeftCallback += LobbyLeftHandler;
                lobbyUpdateReceivedCallback += LobbyUpdateReceivedHandler;
                lobbyMemberUpdateReceivedCallback += LobbyMemberUpdateReceivedHandler;
                lobbyMemberStatusReceivedCallback += LobbyMemberStatusReceivedHandler;
                lobbyDestroyedCallback += LobbyDestroyedHandler;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                lobbyCreatedCallback -= LobbyCreatedHandler;
                lobbyUpdatedCallback -= LobbyUpdatedHandler;
                lobbyJoinedCallback -= LobbyJoinedHandler;
                lobbyLeftCallback -= LobbyLeftHandler;
                lobbyUpdateReceivedCallback -= LobbyUpdateReceivedHandler;
                lobbyMemberUpdateReceivedCallback -= LobbyMemberUpdateReceivedHandler;
                lobbyMemberStatusReceivedCallback -= LobbyMemberStatusReceivedHandler;
                lobbyDestroyedCallback -= LobbyDestroyedHandler;
            }
        }

        private void Update()
        {
            ReceivePacketsFromLobby();
        }

        public void CreateLobby()
        {
            Debug.Log("Trying to create lobby");
            CreateLobbyOptions options = new CreateLobbyOptions 
            {
                BucketId = "Online:NA:BattlePlatform",
                LocalUserId = EOSSDKManager.LocalUserProductId,
                MaxLobbyMembers = (uint)MAX_LOBBY_MEMBER_COUNT
            };
            EOSSDKManager.GetLobbyInterface().CreateLobby(options, null, lobbyCreatedCallback);
        }

        private void LobbyCreatedHandler(CreateLobbyCallbackInfo info)
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.Log("Failed to create lobby, error code " + info.ResultCode);
            }
            else
            {
                Debug.Log("Lobby successfully created with id " + info.LobbyId);
                inLobby = true;
                isLobbyOwner = true;
                lobbyId = info.LobbyId;
                EOSSDKManager.GetLobbyInterface().CopyLobbyDetailsHandle(new CopyLobbyDetailsHandleOptions { LobbyId = info.LobbyId, LocalUserId = EOSSDKManager.LocalUserProductId }, out connectedLobby);
                SetupLobbyNotifications();
                UpdatePlayerData();
                UpdateLobby(defaultLobbyData, GetInitialUserData());
                lobbyCreatedEvent?.Invoke();
            }
        }

        public void UpdateLobby(List<AttributeData> lobbyAttributeDatas, List<AttributeData> memberAttributeData)
        {
            Debug.Log("Trying to update lobby with id " + lobbyId);
            LobbyModification modHandle = new LobbyModification();
            UpdateLobbyModificationOptions updateLobbyModificationOptions = new UpdateLobbyModificationOptions
            {
                LobbyId = lobbyId,
                LocalUserId = EOSSDKManager.LocalUserProductId
            };
            EOSSDKManager.GetLobbyInterface().UpdateLobbyModification(updateLobbyModificationOptions, out modHandle);
            foreach (AttributeData attributeData in lobbyAttributeDatas)
            {
                modHandle.AddAttribute(new LobbyModificationAddAttributeOptions 
                { 
                    Attribute = attributeData, 
                    Visibility = LobbyAttributeVisibility.Public 
                });
            }
            foreach (AttributeData attributeData in memberAttributeData)
            {
                modHandle.AddMemberAttribute(new LobbyModificationAddMemberAttributeOptions
                {
                    Attribute = attributeData,
                    Visibility = LobbyAttributeVisibility.Public
                });
            }
            UpdateLobbyOptions updateLobbyOptions = new UpdateLobbyOptions
            {
                LobbyModificationHandle = modHandle
            };
            EOSSDKManager.GetLobbyInterface().UpdateLobby(updateLobbyOptions, null, lobbyUpdatedCallback);
        }

        private void LobbyUpdatedHandler(UpdateLobbyCallbackInfo info)
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.Log("Failed to update lobby, error code " + info.ResultCode);
            }
            else
            {
                Debug.Log("Lobby successfully update with id " + info.LobbyId);
                lobbyUpdatedEvent?.Invoke();
            }
        }

        public void SearchLobbies()
        {
            Debug.Log("Trying to search lobbies");
            LobbySearch lobbySearch;
            CreateLobbySearchOptions createSearchOptions = new CreateLobbySearchOptions
            {
                MaxResults = 10
            };
            EOSSDKManager.GetLobbyInterface().CreateLobbySearch(createSearchOptions, out lobbySearch);
            LobbySearchFindOptions searchFindOptions = new LobbySearchFindOptions
            {
                LocalUserId = EOSSDKManager.LocalUserProductId
            };
            LobbySearchSetParameterOptions lobbySearchParameterOption = new LobbySearchSetParameterOptions
            {
                ComparisonOp = ComparisonOp.Equal,
                Parameter = defaultLobbyAttribute
            };
            lobbySearch.SetParameter(lobbySearchParameterOption);
            lobbySearch.Find(searchFindOptions, null, (LobbySearchFindCallbackInfo info) => 
            {
                if (info.ResultCode != Result.Success)
                {
                    Debug.Log("Failed to search lobbies, error code " + info.ResultCode);
                }
                else
                {
                    Debug.Log("Lobbies searched successfully");
                    LobbySearchGetSearchResultCountOptions lobbySearchGetCountOptions = new LobbySearchGetSearchResultCountOptions();
                    currentLobbySearch = Enumerable.Range(0, (int)lobbySearch.GetSearchResultCount(lobbySearchGetCountOptions)).Select(i =>
                    {
                        LobbyDetails lobbyDetails;
                        LobbySearchCopySearchResultByIndexOptions lobbySearchCopyOptions = new LobbySearchCopySearchResultByIndexOptions();
                        lobbySearchCopyOptions.LobbyIndex = (uint)i;
                        lobbySearch.CopySearchResultByIndex(lobbySearchCopyOptions, out lobbyDetails);
                        return lobbyDetails;
                    }).ToList();
                    lobbySearchCompleteEvent?.Invoke();
                }
            });
        }

        public void JoinLobby(LobbyDetails lobbyDetails)
        {
            Debug.Log("Trying to join lobby with id " + lobbyId);
            JoinLobbyOptions options = new JoinLobbyOptions
            {
                LobbyDetailsHandle = lobbyDetails,
                LocalUserId = EOSSDKManager.LocalUserProductId, 
                PresenceEnabled = true 
            };
            EOSSDKManager.GetLobbyInterface().JoinLobby(options, null, lobbyJoinedCallback);
        }

        private void LobbyJoinedHandler(JoinLobbyCallbackInfo info)
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.Log("Failed to join lobby, error code " + info.ResultCode);
            }
            else
            {
                Debug.Log("Lobby joined with id " + info.LobbyId);
                inLobby = true;
                isLobbyOwner = false;
                lobbyId = info.LobbyId;
                EOSSDKManager.GetLobbyInterface().CopyLobbyDetailsHandle(new CopyLobbyDetailsHandleOptions { LobbyId = info.LobbyId, LocalUserId = EOSSDKManager.LocalUserProductId }, out connectedLobby);
                SetupLobbyNotifications();
                UpdatePlayerData();
                UpdateLobby(new List<AttributeData>(), GetInitialUserData());
                lobbyJoinedEvent?.Invoke();
            }
        }

        public void LeaveLobby()
        {
            Debug.Log("Trying to leave lobby with id " + lobbyId);
            LeaveLobbyOptions options = new LeaveLobbyOptions
            {
                LobbyId = lobbyId,
                LocalUserId = EOSSDKManager.LocalUserProductId
            };
            EOSSDKManager.GetLobbyInterface().LeaveLobby(options, null, lobbyLeftCallback);
        }

        private void LobbyLeftHandler(LeaveLobbyCallbackInfo info)
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.Log("Failed to leave lobby, error code " + info.ResultCode);
            }
            else
            {
                Debug.Log("Lobby left with id " + info.LobbyId);
                inLobby = false;
                isLobbyOwner = false;
                RemoveLobbyNotifications();
                lobbyLeftEvent?.Invoke();
            }
        }

        private void LobbyUpdateReceivedHandler(LobbyUpdateReceivedCallbackInfo info)
        {
            isLobbyOwner = connectedLobby.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions()) == EOSSDKManager.LocalUserProductId;

            Epic.OnlineServices.Lobby.Attribute inGameAttribute;
            connectedLobby.CopyAttributeByKey(new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = "inGame" }, out inGameAttribute);
            bool inGameNow = inGameAttribute?.Data?.Value?.AsBool != null ? (bool)inGameAttribute.Data.Value.AsBool : false;
            if (!inGame && inGameNow)
            {
                GameDataManager.Instance.battleLoadData = new BattleLoadData { playerSlotDatas = playerInfos.Select(x => new PlayerSlotData { playerSlot = x.playerSlot, fighterId = x.selectedFighterId, isLocal = x.productUserId == EOSSDKManager.LocalUserProductId }).ToList() };
                GameSceneManager.Instance.loadScene(GameSceneManager.BATTLE_SCENE_NAME);
                foreach (PlayerInfo playerInfo in playerInfos)
                {
                    if (playerInfo.productUserId != EOSSDKManager.LocalUserProductId)
                    {
                        EOSSDKManager.GetP2PInterface().AcceptConnection(new AcceptConnectionOptions
                        {
                            LocalUserId = EOSSDKManager.LocalUserProductId,
                            RemoteUserId = playerInfo.productUserId,
                            SocketId = new SocketId { SocketName = lobbyId }
                        });
                    }
                }
                if (isLobbyOwner)
                {
                    SendPing();
                }
            }
            inGame = inGameNow;

            lobbyUpdatedEvent?.Invoke();
        }

        private void LobbyMemberUpdateReceivedHandler(LobbyMemberUpdateReceivedCallbackInfo info)
        {
            isLobbyOwner = connectedLobby.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions()) == EOSSDKManager.LocalUserProductId;

            UpdatePlayerData();

            if (isLobbyOwner && playerInfos.All(x => x.isLoaded))
            {
                SendPacketToLobby(Encoding.UTF8.GetBytes("StartBattle"), STRING_CHANNEL);
                StartCoroutine(DelayBattleStart());
            }

            lobbyUpdatedEvent?.Invoke();
        }

        private void LobbyMemberStatusReceivedHandler(LobbyMemberStatusReceivedCallbackInfo info)
        {
            isLobbyOwner = connectedLobby.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions()) == EOSSDKManager.LocalUserProductId;
            lobbyUpdatedEvent?.Invoke();
        }

        public void DestroyLobby()
        {
            Debug.Log("Trying to destroy lobby with id " + lobbyId);
            DestroyLobbyOptions options = new DestroyLobbyOptions
            {
                LobbyId = lobbyId,
                LocalUserId = EOSSDKManager.LocalUserProductId
            };
            EOSSDKManager.GetLobbyInterface().DestroyLobby(options, null, lobbyDestroyedCallback);
        }

        private void LobbyDestroyedHandler(DestroyLobbyCallbackInfo info)
        {
            if (info.ResultCode != Result.Success)
            {
                Debug.Log("Failed to destroy lobby, error code " + info.ResultCode);
            }
            else
            {
                Debug.Log("Lobby destroyed with id " + info.LobbyId);
                inLobby = false;
                isLobbyOwner = false;
            }
        }

        public void SendPacketToLobby(byte[] data, byte channel)
        {
            foreach (PlayerInfo playerInfo in playerInfos)
            {
                if (playerInfo.productUserId != EOSSDKManager.LocalUserProductId)
                {
                    SendPacketOptions options = new SendPacketOptions
                    {
                        LocalUserId = EOSSDKManager.LocalUserProductId,
                        RemoteUserId = playerInfo.productUserId,
                        SocketId = new SocketId { SocketName = lobbyId },
                        Data = data,
                        Channel = channel,
                        AllowDelayedDelivery = true,
                        Reliability = PacketReliability.UnreliableUnordered
                    };
                    EOSSDKManager.GetP2PInterface().SendPacket(options);
                }
            }
        }

        public void ReceivePacketsFromLobby()
        {   
            // Get strings
            while (true)
            {
                uint nextPacketSize = 0;
                EOSSDKManager.GetP2PInterface().GetNextReceivedPacketSize(new GetNextReceivedPacketSizeOptions { LocalUserId = EOSSDKManager.LocalUserProductId, RequestedChannel = STRING_CHANNEL }, out nextPacketSize);
                ProductUserId peerId;
                SocketId socketId;
                byte channel;
                byte[] data;
                Result receivePacketResult = EOSSDKManager.GetP2PInterface().ReceivePacket(new ReceivePacketOptions { LocalUserId = EOSSDKManager.LocalUserProductId, MaxDataSizeBytes = nextPacketSize, RequestedChannel = STRING_CHANNEL }, out peerId, out socketId, out channel, out data);
                if (receivePacketResult == Result.Success)
                {
                    string received = Encoding.UTF8.GetString(data);
                    if (received == "ping")
                    {
                        SendPing();
                    }
                    else if (received == "StartBattle")
                    {
                        startBattleEvent?.Invoke();
                    }
                }
                else
                {
                    break;
                }
            }

            // Get inputs
            while (true)
            {
                uint nextPacketSize = 0;
                EOSSDKManager.GetP2PInterface().GetNextReceivedPacketSize(new GetNextReceivedPacketSizeOptions { LocalUserId = EOSSDKManager.LocalUserProductId, RequestedChannel = INPUT_CHANNEL }, out nextPacketSize);
                ProductUserId peerId;
                SocketId socketId;
                byte channel;
                byte[] data;
                Result receivePacketResult = EOSSDKManager.GetP2PInterface().ReceivePacket(new ReceivePacketOptions { LocalUserId = EOSSDKManager.LocalUserProductId, MaxDataSizeBytes = nextPacketSize, RequestedChannel = INPUT_CHANNEL }, out peerId, out socketId, out channel, out data);
                if (receivePacketResult == Result.Success)
                {
                    try
                    {
                        OnlineInputData onlineInputData = Extensions.DeserializeData<OnlineInputData>(data);
                        receivedOnlineInputsEvent?.Invoke(onlineInputData);
                    }
                    catch
                    {
                        Debug.Log("Failed to cast packet to OnlineInputData, this should never happen. Rogue packets?");
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void SetupLobbyNotifications()
        {
            notifyLobbyUpdateReceived = EOSSDKManager.GetLobbyInterface().AddNotifyLobbyUpdateReceived(new AddNotifyLobbyUpdateReceivedOptions(), null, lobbyUpdateReceivedCallback);
            notifyLobbyMemberUpdateReceived = EOSSDKManager.GetLobbyInterface().AddNotifyLobbyMemberUpdateReceived(new AddNotifyLobbyMemberUpdateReceivedOptions(), null, lobbyMemberUpdateReceivedCallback);
            notifyLobbyMemberStatusReceived = EOSSDKManager.GetLobbyInterface().AddNotifyLobbyMemberStatusReceived(new AddNotifyLobbyMemberStatusReceivedOptions(), null, lobbyMemberStatusReceivedCallback);
        }

        private void RemoveLobbyNotifications()
        {
            EOSSDKManager.GetLobbyInterface().RemoveNotifyLobbyUpdateReceived(notifyLobbyUpdateReceived);
            EOSSDKManager.GetLobbyInterface().RemoveNotifyLobbyMemberUpdateReceived(notifyLobbyMemberUpdateReceived);
            EOSSDKManager.GetLobbyInterface().RemoveNotifyLobbyMemberStatusReceived(notifyLobbyMemberStatusReceived);
        }

        private List<AttributeData> GetInitialUserData()
        {
            int playerSlot = Enumerable.Range(0, MAX_LOBBY_MEMBER_COUNT).Except(playerInfos.Select(x => x.playerSlot)).First();
            return new List<AttributeData> {
                new AttributeData { Key = "userName", Value = "User" },
                new AttributeData { Key = "playerSlot", Value = playerSlot },
                new AttributeData { Key = "isReady", Value = false },
                new AttributeData { Key = "selectedFighterId", Value = "" },
                new AttributeData { Key = "isLoaded", Value = false }
            };
        }

        private void UpdatePlayerData()
        {
            int memberCount = (int)connectedLobby.GetMemberCount(new LobbyDetailsGetMemberCountOptions());
            playerInfos.Clear();
            playerInfos = Enumerable.Range(0, memberCount).Select(x => {
                ProductUserId productUserId = connectedLobby.GetMemberByIndex(new LobbyDetailsGetMemberByIndexOptions { MemberIndex = (uint)x });
                if (productUserId == null)
                {
                    return null;
                }
                Epic.OnlineServices.Lobby.Attribute userNameAttribute;
                connectedLobby.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions { TargetUserId = productUserId, AttrKey = "userName" }, out userNameAttribute);
                Epic.OnlineServices.Lobby.Attribute playerSlotAttribute;
                connectedLobby.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions { TargetUserId = productUserId, AttrKey = "playerSlot" }, out playerSlotAttribute);
                Epic.OnlineServices.Lobby.Attribute isReadyAttribute;
                connectedLobby.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions { TargetUserId = productUserId, AttrKey = "isReady" }, out isReadyAttribute);
                Epic.OnlineServices.Lobby.Attribute selectedFighterIdAttribute;
                connectedLobby.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions { TargetUserId = productUserId, AttrKey = "selectedFighterId" }, out selectedFighterIdAttribute);
                Epic.OnlineServices.Lobby.Attribute isLoadedAttribute;
                connectedLobby.CopyMemberAttributeByKey(new LobbyDetailsCopyMemberAttributeByKeyOptions { TargetUserId = productUserId, AttrKey = "isLoaded" }, out isLoadedAttribute);
                if(userNameAttribute == null)
                {
                    return null;
                }
                return new PlayerInfo
                {
                    productUserId = productUserId,
                    userName = userNameAttribute != null ? userNameAttribute.Data.Value.AsUtf8 : "",
                    playerSlot = playerSlotAttribute != null ? (int)playerSlotAttribute.Data.Value.AsInt64 : 0,
                    isLobbyOwner = productUserId == connectedLobby.GetLobbyOwner(new LobbyDetailsGetLobbyOwnerOptions()),
                    isReady = isReadyAttribute?.Data?.Value?.AsBool != null ? (bool)isReadyAttribute.Data.Value.AsBool : false,
                    selectedFighterId = selectedFighterIdAttribute != null ? selectedFighterIdAttribute.Data.Value.AsUtf8 : "",
                    isLoaded = isLoadedAttribute?.Data?.Value?.AsBool != null ? (bool)isLoadedAttribute.Data.Value.AsBool : false
                };
            }).Where(x => x != null).ToList();
        }

        private void SendPing()
        {
            SendPacketToLobby(Encoding.UTF8.GetBytes("ping"), STRING_CHANNEL);
            if (!pingStopwatch.IsRunning)
            {
                pingStopwatch.Start();
            }
            else
            {
                lastFivePings.Add(pingStopwatch.ElapsedMilliseconds);
                if (lastFivePings.Count > 5)
                {
                    lastFivePings.RemoveAt(0);
                }
                averagePing = lastFivePings.Average();
                pingStopwatch.Restart();
            }
        }

        private IEnumerator DelayBattleStart()
        {
            yield return new WaitForSeconds(averagePing / 2f / 1000f);
            startBattleEvent?.Invoke();
        }
    }
}