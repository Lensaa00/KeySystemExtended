using MSCLoader;
using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;


namespace KeySystemExtended
{
    public class KeySystemExtended : Mod
    {
        public override string ID => "KeySystemExtended";
        public override string Name => "KeySystemExtended";
        public override string Author => "qniman";
        public override string Version => "1.0";
        public override string Description => "";


        // Бинды
        Keybind engineKey;
        Keybind lockKey;

        // Классы
        KSELightActions lightActions;
        KSESoundActions playerSoundActions;
        KSESoundActions carSoundActions;

        // Объекты
        GameObject Player;
        GameObject Satsuma;
        GameObject DoorLeft;
        GameObject DoorRight;
        GameObject Bootlid;
        GameObject Electrics;
        GameObject Starter;
        GameObject Key;

        // Состояния
        bool isCarLocked = false;
        bool isCarStarted = false;
        bool isCarStarting = false;

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }

        private void Mod_Settings()
        {
            lockKey = Keybind.Add(this, "LockKey", "Lock/Unlock Car", KeyCode.F7);
            engineKey = Keybind.Add(this, "EngineKey", "Engine Start/Stop", KeyCode.F8);
        }

        private void Mod_OnLoad()
        {
            // Поиск объекта Satsuma
            Satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            if (Satsuma == null) return;

            Player = GameObject.Find("PLAYER");
            if (Player == null) return;

            // Создаем KSE LightActions и добавляем его в Satsuma
            GameObject lightController = new GameObject("KSELightActions");
            lightController.transform.parent = Satsuma.transform; // Делаем дочерним элементом
            lightActions = lightController.AddComponent<KSELightActions>();
            ModConsole.Print("[KSE] KSELightActions successfully added to Satsuma.");

            playerSoundActions = Player.AddComponent<KSESoundActions>();
            ModConsole.Print("[KSE] KSESoundActions successfully added to Player.");

            carSoundActions = Satsuma.AddComponent<KSESoundActions>();
            ModConsole.Print("[KSE] KSESoundActions successfully added to Satsuma.");
        }

        private void Mod_Update()
        {
            HandleInput();
            HandleCarStates();
        }

        void HandleInput()
        {
            if (lockKey.GetKeybindDown())
            {
                CarLockToggle();
            }
            if (engineKey.GetKeybindDown())
            {
                CarEngineToggle();
            }
        }

        void HandleCarStates()
        {
            GetCarParts();
            foreach (FsmState state in Starter.GetComponent<PlayMakerFSM>().FsmStates)
            {
                if (state.Active)
                {
                    if (!Key.activeSelf)
                    {
                        if (state.Name == "Start engine" && !isCarStarting)
                        {
                            isCarStarting = true;
                        }
                        if (state.Name == "Running" && isCarStarting)
                        {
                            playerSoundActions.Play("player", "EngineStarted");
                            isCarStarting = false;
                        }
                    }
                }
            }
        }

        void GetCarParts()
        {
            Bootlid = GameObject.Find("SATSUMA(557kg, 248)/Body/pivot_bootlid/bootlid(Clone)/Handles");
            if (Bootlid == null) ModConsole.Error("[KSE] Bootlid not found!");

            DoorLeft = GameObject.Find("SATSUMA(557kg, 248)/Body/pivot_door_left/door left(Clone)");
            if (DoorLeft == null) ModConsole.Error("[KSE] Door Left not found!");

            DoorRight = GameObject.Find("SATSUMA(557kg, 248)/Body/pivot_door_right/door right(Clone)");
            if (DoorRight == null) ModConsole.Error("[KSE] Door Right not found!");

            Electrics = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
            if (Electrics == null) ModConsole.Error("[KSE] Electrics not found!");

            Starter = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Starter");
            if (Starter == null) ModConsole.Error("[KSE] Starter not found!");

            Key = GameObject.Find("SATSUMA(557kg, 248)/Dashboard/Steering/steering_column2/Ignition/Keys/Key");
            if (Key == null) ModConsole.Error("[KSE] Key not found!");
        }

        void CarEngineToggle()
        {
            GetCarParts();
            foreach (FsmState state in Starter.GetComponent<PlayMakerFSM>().FsmStates)
            {
                if(state.Active && !Key.activeSelf)
                {
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
        }

        void CarLockToggle()
        {
            bool bootlidState = false;
            bool leftDoorState = false;
            bool rightDoorState = false;

            GetCarParts();

            foreach (PlayMakerFSM component in Bootlid.GetComponents<PlayMakerFSM>())
            {
                if (component.FsmName == "Use")
                    bootlidState = component.FsmVariables.FindFsmBool("Open").Value;
            }
            foreach (PlayMakerFSM component in DoorLeft.GetComponents<PlayMakerFSM>())
            {
                if (component.FsmName == "Use")
                    leftDoorState = component.FsmVariables.FindFsmBool("Open").Value;
            }
            foreach (PlayMakerFSM component in DoorRight.GetComponents<PlayMakerFSM>())
            {
                if (component.FsmName == "Use")
                    rightDoorState = component.FsmVariables.FindFsmBool("Open").Value;
            }


            if (bootlidState == false &&
                leftDoorState == false &&
                rightDoorState == false)
            {
                isCarLocked = !isCarLocked;

                UpdateDoorLockState(Bootlid);
                UpdateDoorLockState(DoorLeft);
                UpdateDoorLockState(DoorRight);

                ModConsole.Print($"[KSE] Car lock state changed: {(isCarLocked ? "Locked" : "Unlocked")}");
            }
            else
            {
                playerSoundActions.Play("player", "Ping3");
            }
            
        }

        void UpdateDoorLockState(GameObject obj)
        {
            float distance = isCarLocked ? 0f : 1.5f;

            PlayMakerFSM[] fsm = obj.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM component in fsm)
            {
                if (component.FsmName == "Use")
                {
                    lightActions.LightBlinker(isCarLocked ? 1 : 2);
                    playerSoundActions.Play("player", isCarLocked ? "Ping" : "Ping2");
                    carSoundActions.Play("car", "LockUnlock");

                    ModConsole.Print($"[KSE] FSM of {obj.name} RayDistance: {component.FsmVariables.FindFsmFloat("RayDistance").Value}");
                    component.FsmVariables.FindFsmFloat("RayDistance").Value = distance;
                    ModConsole.Print($"[KSE] FSM of {obj.name} RayDistance: {component.FsmVariables.FindFsmFloat("RayDistance").Value}");
                }
            }
        }

        void CarStart()
        {
            PlayMakerFSM[] fsm = Electrics.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM component in fsm)
            {
                if (component.FsmName == "Electrics")
                {
                    component.FsmVariables.FindFsmBool("ElectricsOK").Value = true;
                }
            }

            fsm = Starter.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM component in fsm)
            {
                if (component.FsmName == "Starter")
                {
                    component.FsmVariables.FindFsmBool("ACC").Value = true;
                    component.FsmVariables.FindFsmBool("Starting").Value = true;
                }
            }
        }

        void CarStop()
        {
            PlayMakerFSM[] fsm = Starter.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM component in fsm)
            {
                if (component.FsmName == "Starter")
                {
                    component.FsmVariables.FindFsmBool("ACC").Value = false;
                    component.FsmVariables.FindFsmBool("Starting").Value = false;
                    component.FsmVariables.FindFsmBool("ShutOff").Value = true;
                }
            }

            fsm = Electrics.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM component in fsm)
            {
                if (component.FsmName == "Electrics")
                {
                    component.FsmVariables.FindFsmBool("ElectricsOK").Value = false;
                }
            }
        }
    }
}
