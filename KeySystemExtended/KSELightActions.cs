using MSCLoader;
using System.Collections;
using UnityEngine;

namespace KeySystemExtended
{
    public class KSELightActions: MonoBehaviour
    {

        GameObject LeftMarkers;
        GameObject RightMarkers;
        GameObject Electrics;

        bool isBlinking = false;

        void Start ()
        {
            LeftMarkers = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON/Indicators/Left");
            if (LeftMarkers == null) ModConsole.Error("[KSELightActions] Left markers not found!");
            else ModConsole.Print("[KSELightActions] Left markers found!");

            RightMarkers = GameObject.Find("SATSUMA(557kg, 248)/Electricity/PowerON/Indicators/Right");
            if (LeftMarkers == null) ModConsole.Error("[KSELightActions] Right markers not found!");
            else ModConsole.Print("[KSELightActions] Right markers found!");

            Electrics = GameObject.Find("SATSUMA(557kg, 248)/CarSimulation/Car/Electrics");
            if (Electrics == null) ModConsole.Error("[KSELightActions] Electrics not found!");
            else ModConsole.Print("[KSELightActions] Electrics found!");
        }

        IEnumerator Blink(int times = 1)
        {
            bool electricsOn = Electrics.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("ElectricsOK").Value;
            Electrics.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("ElectricsOK").Value = true;
            if (!isBlinking)
            {
                for (int i = 0; i < times; i++)
                {
                    RightMarkers.SetActive(true);
                    LeftMarkers.SetActive(true);
                    yield return new WaitForSeconds(0.3f);
                    RightMarkers.SetActive(false);
                    LeftMarkers.SetActive(false);
                    yield return new WaitForSeconds(0.3f);
                }
            }
            else
            {
                RightMarkers.SetActive(true);
                LeftMarkers.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                RightMarkers.SetActive(false);
                LeftMarkers.SetActive(false);
                yield return new WaitForSeconds(3f);
                StartCoroutine(Blink());
            }
            if(!electricsOn) 
                Electrics.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmBool("ElectricsOK").Value = false;
            yield return null;
        }

        public void LightBlinker(int times)
        {
            StartCoroutine(Blink(times));
        }
    }
}
