using MSCLoader;
using UnityEngine;
using System.Collections;
using HutongGames.PlayMaker;


namespace KeySystemExtended
{
    public class KSE : Mod
    {
        public override string ID => "KeySystemExtended";
        public override string Name => "KeySystemExtended";
        public override string Author => "qniman";
        public override string Version => "1.0a";
        public override string Description => "";

        // Binds
        public Keybind engineKey;
        public Keybind lockKey;

        // Modules
        KSECarController carController = new KSECarController();

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_OnUpdate);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }

        void Mod_Settings()
        {
            lockKey = Keybind.Add(this, "LockKey", "Lock/Unlock Car", KeyCode.F7);
            engineKey = Keybind.Add(this, "EngineKey", "Engine Start/Stop", KeyCode.F8);
        }

        void Mod_OnLoad()
        {
            carController.OnStart();
        }

        public void Mod_OnUpdate()
        {
            carController.OnUpdate();
            HandleInput();
        }

        void HandleInput()
        {
            if (engineKey.GetKeybindDown())
            {
                carController.CarEngineToggle();
            }
            if (lockKey.GetKeybindDown())
            {
                carController.CarLockToggle();
            }
        }
    }
}
