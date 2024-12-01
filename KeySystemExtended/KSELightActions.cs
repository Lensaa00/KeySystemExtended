using MSCLoader;
using System.Collections;
using UnityEngine;

namespace KeySystemExtended
{
    public class KSELightActions : MonoBehaviour
    {
        // GameObjects
        private GameObject leftMarkers;
        private GameObject rightMarkers;
        private GameObject electrics;

        private bool isBlinking = false;

        public void Start()
        {
            leftMarkers = FindAndLog("SATSUMA(557kg, 248)/Electricity/PowerON/Indicators/Left", "Left markers");
            rightMarkers = FindAndLog("SATSUMA(557kg, 248)/Electricity/PowerON/Indicators/Right", "Right markers");
            electrics = FindAndLog("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics", "Electrics");
        }

        private GameObject FindAndLog(string path, string name)
        {
            GameObject obj = GameObject.Find(path);
            if (obj == null)
                ModConsole.Error($"[KSE] {name} not found!");
            else
                ModConsole.Print($"[KSE] {name} found!");
            return obj;
        }

        private IEnumerator Blink(int times)
        {
            if (electrics == null || leftMarkers == null || rightMarkers == null)
            {
                ModConsole.Error("[KSE] One or more required GameObjects are missing!");
                yield break;
            }

            var electricsFSM = electrics.GetComponent<PlayMakerFSM>();
            var electricsOKVar = electricsFSM?.FsmVariables.FindFsmBool("ElectricsOK");
            if (electricsOKVar == null)
            {
                ModConsole.Error("[KSE] ElectricsOK variable not found in PlayMakerFSM!");
                yield break;
            }

            bool initialElectricsState = electricsOKVar.Value;
            electricsOKVar.Value = true;

            for (int i = 0; i < times; i++)
            {
                ToggleMarkers(true);
                yield return new WaitForSeconds(0.3f);
                ToggleMarkers(false);
                yield return new WaitForSeconds(0.3f);
            }

            electricsOKVar.Value = initialElectricsState;
        }

        public void LightBlinker(int times)
        {
            if (!isBlinking)
                StartCoroutine(Blink(times));
        }

        private void ToggleMarkers(bool state)
        {
            if (leftMarkers != null)
                leftMarkers.SetActive(state);
            if (rightMarkers != null)
                rightMarkers.SetActive(state);
        }
    }
}
