using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FixedPoint;
using NaughtyAttributes;
using Sirenix.OdinInspector;

namespace MythrenFighter
{
    [System.Serializable]
    public class InputManager : SerializedMonoBehaviour
    {
        // Variables
        [SerializeField]private Dictionary<int, Dictionary<int, FighterInputs>> inputs = new Dictionary<int, Dictionary<int, FighterInputs>>();
        public bool inputsEnabled = true;
        // Dependencies
        [SerializeField]
        private List<Player> players = new List<Player>();

        private void Awake()
        {
            players = ReInput.players.GetPlayers().ToList();

            if(LobbyManager.Instance != null)
            {
                LobbyManager.Instance.receivedOnlineInputsEvent += ReceiveOnlineInputs;
            }
        }

        private void OnDestroy()
        {
            LobbyManager.Instance.receivedOnlineInputsEvent -= ReceiveOnlineInputs;
        }

        public FighterInputs GetInputs(int playerSlot)
        {
            int inputFrame = RollbackManager.currentFrame - RollbackManager.FRAME_BUFFER;
            if (inputs.ContainsKey(playerSlot) && inputs[playerSlot].ContainsKey(inputFrame) && inputsEnabled)
            {
                return inputs[playerSlot][inputFrame];
            }
            else
            {
                //Debug.Log("Requested inputs for frame " + inputFrame + " and didn't find them O_O");
                // predict inputs here based on previous frame (if exists)
                return new FighterInputs();
            }
        }

        public void ReceiveOnlineInputs(OnlineInputData onlineInputData)
        {
            Debug.Log("Received inputs for " + onlineInputData.playerSlot + " " + onlineInputData.frame + " ");
            if (!inputs.ContainsKey(onlineInputData.playerSlot))
            {
                inputs.Add(onlineInputData.playerSlot, new Dictionary<int, FighterInputs>());
            }
            if (!inputs[onlineInputData.playerSlot].ContainsKey(onlineInputData.frame))
            {
                inputs[onlineInputData.playerSlot].Add(onlineInputData.frame, new FighterInputs());
            }
            inputs[onlineInputData.playerSlot][onlineInputData.frame] = onlineInputData.fighterInputs;

        }

        private void Update()
        {
            List<PlayerSlotData> playerSlots = GameDataManager.Instance.battleLoadData.playerSlotDatas;
            List<PlayerSlotData> localPlayerSlots = playerSlots.Where(x => x.isLocal).ToList();
            for (int i = 0; i < localPlayerSlots.Count; i++)
            {
                int slot = localPlayerSlots[i].playerSlot;
                if (i < players.Count)
                {
                    if (!inputs.ContainsKey(slot))
                    {
                        inputs.Add(slot, new Dictionary<int, FighterInputs>());
                    }
                    if (!inputs[slot].ContainsKey(RollbackManager.currentFrame))
                    {
                        inputs[slot].Add(RollbackManager.currentFrame, new FighterInputs());
                    }
                    inputs[slot][RollbackManager.currentFrame].jumpInput = inputs[slot][RollbackManager.currentFrame].jumpInput || players[i].GetButtonDown("JUMP");
                    inputs[slot][RollbackManager.currentFrame].jabInput = inputs[slot][RollbackManager.currentFrame].jabInput || players[i].GetButtonDown("JAB");
                    inputs[slot][RollbackManager.currentFrame].heavyInput = inputs[slot][RollbackManager.currentFrame].heavyInput || players[i].GetButtonDown("HEAVY");
                    inputs[slot][RollbackManager.currentFrame].specialInput = inputs[slot][RollbackManager.currentFrame].specialInput || players[i].GetButtonDown("SPECIAL");
                    inputs[slot][RollbackManager.currentFrame].dashInput = inputs[slot][RollbackManager.currentFrame].dashInput || players[i].GetButtonDown("DASH");
                    inputs[slot][RollbackManager.currentFrame].skill1Input = inputs[slot][RollbackManager.currentFrame].skill1Input || players[i].GetButtonDown("SKILL1");
                    inputs[slot][RollbackManager.currentFrame].skill2Input = inputs[slot][RollbackManager.currentFrame].skill2Input || players[i].GetButtonDown("SKILL2");
                    inputs[slot][RollbackManager.currentFrame].skill3Input = inputs[slot][RollbackManager.currentFrame].skill3Input || players[i].GetButtonDown("SKILL3");
                    inputs[slot][RollbackManager.currentFrame].skill4Input = inputs[slot][RollbackManager.currentFrame].skill4Input || players[i].GetButtonDown("SKILL4");
                    inputs[slot][RollbackManager.currentFrame].shieldInput = inputs[slot][RollbackManager.currentFrame].shieldInput || players[i].GetButton("SHIELD");
                    inputs[slot][RollbackManager.currentFrame].shieldReleaseInput = inputs[slot][RollbackManager.currentFrame].shieldReleaseInput || players[i].GetButtonUp("SHIELD");
                    inputs[slot][RollbackManager.currentFrame].moveInput = new fp3(players[i].GetAxis("Horizontal").ToFixedPoint(), fp._0, players[i].GetAxis("Vertical").ToFixedPoint());
                }
                else
                {
                    Debug.Log("Not enough Rewired Players for the number of active player slots!!!!");
                }

                if (inputs.ContainsKey(slot) && inputs[slot].ContainsKey(RollbackManager.currentFrame - 1))
                {
                    LobbyManager.Instance.SendPacketToLobby(Extensions.SerializeData<OnlineInputData>(new OnlineInputData { playerSlot = slot, frame = RollbackManager.currentFrame - 1, fighterInputs = inputs[slot][RollbackManager.currentFrame - 1] }), LobbyManager.INPUT_CHANNEL);
                }
            }
        }
    }
}