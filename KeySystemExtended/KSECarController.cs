using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using MSCLoader;
using System.Collections;
using UnityEngine;
using static KeySystemExtended.KSESoundActions;

namespace KeySystemExtended
{
    public class KSECarController : MonoBehaviour
    {
        // Gameobjects
        public GameObject Player;
        public GameObject Satsuma;
        public GameObject DoorLeft;
        public GameObject DoorRight;
        public GameObject Bootlid;
        public GameObject Electrics;
        public GameObject Starter;
        public GameObject Key;
        public GameObject Speedometer;
        public GameObject RemoteKey;

        // Constants
        private const float SPEED_MULTIPLIER = -1.85f;

        // Modules
        private KSELightActions lightActions;
        private KSESoundActions soundActions;

        // States
        private bool isCarLocked = false;
        private bool isCarStarted = false;
        private bool isCarStarting = false;
        private bool isLockingBySpeed = false;
        private bool isLockTimerStarted = false;

        // Helper methods for GameObject finding and FSM interaction
        private GameObject FindGameObject(string path)
        {
            var obj = GameObject.Find(path);
            if (obj == null)
            {
                Debug.LogWarning($"GameObject '{path}' not found.");
            }
            return obj;
        }

        private T GetFSMVariable<T>(GameObject obj, string fsmName, string variableName)
        {
            foreach (PlayMakerFSM fsm in obj.GetComponents<PlayMakerFSM>())
            {
                if (fsm.FsmName == fsmName)
                {
                    if (typeof(T) == typeof(float))
                        return (T)(object)fsm.FsmVariables.FindFsmFloat(variableName).Value;
                    if (typeof(T) == typeof(bool))
                        return (T)(object)fsm.FsmVariables.FindFsmBool(variableName).Value;
                    if (typeof(T) == typeof(int))
                        return (T)(object)fsm.FsmVariables.FindFsmInt(variableName).Value;
                    if (typeof(T) == typeof(string))
                        return (T)(object)fsm.FsmVariables.FindFsmString(variableName).Value;

                    Debug.LogWarning($"Unsupported type {typeof(T)} for variable '{variableName}'.");
                }
            }
            return default;
        }

        private void SetFSMVariable(GameObject obj, string fsmName, string variableName, object value)
        {
            foreach (PlayMakerFSM fsm in obj.GetComponents<PlayMakerFSM>())
            {
                if (fsm.FsmName == fsmName)
                {
                    if (value is float floatValue)
                        fsm.FsmVariables.FindFsmFloat(variableName).Value = floatValue;
                    else if (value is bool boolValue)
                        fsm.FsmVariables.FindFsmBool(variableName).Value = boolValue;
                    else if (value is int intValue)
                        fsm.FsmVariables.FindFsmInt(variableName).Value = intValue;
                    else if (value is string stringValue)
                        fsm.FsmVariables.FindFsmString(variableName).Value = stringValue;
                    else
                        Debug.LogWarning($"Unsupported type {value.GetType()} for variable '{variableName}'.");
                }
            }
        }


        // Initialize GameObjects
        public void GetGameObjects()
        {
            Player = FindGameObject("PLAYER");
            Satsuma = FindGameObject("SATSUMA(557kg, 248)");
            Bootlid = FindGameObject("SATSUMA(557kg, 248)/Body/pivot_bootlid/bootlid(Clone)/Handles");
            DoorLeft = FindGameObject("SATSUMA(557kg, 248)/Body/pivot_door_left/door left(Clone)");
            DoorRight = FindGameObject("SATSUMA(557kg, 248)/Body/pivot_door_right/door right(Clone)");
            Electrics = FindGameObject("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
            Starter = FindGameObject("SATSUMA(557kg, 248)/CarSimulation/Car/Starter");
            Key = FindGameObject("SATSUMA(557kg, 248)/Dashboard/Steering/steering_column2/Ignition/Keys/Key");
            Speedometer = FindGameObject("SATSUMA(557kg, 248)/Dashboard/pivot_dashboard/dashboard(Clone)/pivot_meters/dashboard meters(Clone)/Gauges/Speedometer");
        }

        // Start method
        public void OnStart()
        {
            GetGameObjects();

            // Create and attach KSESoundActions
            GameObject soundController = new GameObject("KSE_SOUND_CONTROLLER");
            soundActions = soundController.AddComponent<KSESoundActions>();
            soundController.transform.position = Vector3.zero;
            soundActions.carAudioSource = Satsuma.AddComponent<AudioSource>();
            soundActions.playerAudioSource = Player.AddComponent<AudioSource>();

            // Create and attach KSELightActions
            GameObject lightController = new GameObject("KSELightActions");
            lightController.transform.parent = Satsuma.transform;
            lightActions = lightController.AddComponent<KSELightActions>();

            InvokeRepeating(nameof(UpdateGameObjects), 0f, 0.2f);
        }

        void UpdateGameObjects()
        {
            GetGameObjects();
        }

        // Update method
        public void OnUpdate()
        {
            HandleCarStates();
        }

        // Handle Satsuma states
        void HandleCarStates()
        {
            foreach (FsmState state in Starter.GetComponent<PlayMakerFSM>().FsmStates)
            {
                if (!state.Active || Key.activeSelf) continue;

                if (state.Name == "Start engine" && !isCarStarting)
                {
                    isCarStarting = true;
                }
                else if (state.Name == "Running" && isCarStarting)
                {
                    soundActions.Play(SoundCategory.Player, "EngineStarted");
                    isCarStarting = false;
                }
            }
        }

        // Toggle car engine state
        public void CarEngineToggle()
        {
            foreach (FsmState state in Starter.GetComponent<PlayMakerFSM>().FsmStates)
            {
                if (!state.Active || Key.activeSelf) continue;

                if (state.Name == "Wait for start")
                {
                    CarStart();
                }
                else if (state.Name == "Running")
                {
                    CarStop();
                }
            }
        }

        // Lock or unlock car
        public void CarLockToggle()
        {
            if (Key.activeSelf && isLockingBySpeed) return;

            bool areAllClosed = !GetFSMVariable<bool>(Bootlid, "Use", "Open") &&
                                !GetFSMVariable<bool>(DoorLeft, "Use", "Open") &&
                                !GetFSMVariable<bool>(DoorRight, "Use", "Open");

            if (!Key.activeSelf)
            {
                if (areAllClosed)
                {
                    isCarLocked = !isCarLocked;

                    UpdateDoorLockState(Bootlid);
                    UpdateDoorLockState(DoorLeft);
                    UpdateDoorLockState(DoorRight);

                    lightActions.LightBlinker(isCarLocked ? 1 : 2);
                    soundActions.Play(SoundCategory.Car, "LockUnlock");
                    if (!isLockingBySpeed)
                    {
                        soundActions.Play(SoundCategory.Player, isCarLocked ? "Ping" : "Ping2");
                    }
                }
                else
                {
                    soundActions.Play(SoundCategory.Player, "Ping3");
                }
            }
            
        }

        // Update door lock state
        void UpdateDoorLockState(GameObject obj)
        {
            SetFSMVariable(obj, "Use", "RayDistance", isCarLocked ? 0f : 1.5f);
        }

        // Start car
        public void CarStart()
        {
            SetFSMVariable(Electrics, "Electrics", "ElectricsOK", true);
            SetFSMVariable(Starter, "Starter", "ACC", true);
            SetFSMVariable(Starter, "Starter", "Starting", true);
        }

        // Stop car
        public void CarStop()
        {
            SetFSMVariable(Starter, "Starter", "ACC", false);
            SetFSMVariable(Starter, "Starter", "Starting", false);
            SetFSMVariable(Starter, "Starter", "ShutOff", true);
            SetFSMVariable(Electrics, "Electrics", "ElectricsOK", false);
        }
    }
}
