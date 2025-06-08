using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace IAManager
{
    public class CrowAlarmManager : MonoBehaviour
    {
        [Header("Alarm Settings")]
        [SerializeField] float InitialRadius = 100;

        [Header("World Settings")]
        GameObject[] wolfs;
        GameObject[] crows;
        List<GameObject> crowsFollowing = new List<GameObject>();

        private void Start()
        {
            wolfs = GameObject.FindGameObjectsWithTag("Wolf");
            crows = GameObject.FindGameObjectsWithTag("Crow");
        }

        private void Update()
        {
            foreach (GameObject crow in crows)
            {
                if (crow.GetComponent<CrowAI>().detected)
                {
                    if (!crowsFollowing.Contains(crow))
                        crowsFollowing.Add(crow);
                }
                else
                    if (crowsFollowing.Contains(crow))
                        crowsFollowing.Remove(crow);
            }

            if (crowsFollowing.Count > 0)
            {
                foreach (GameObject crow in crowsFollowing)
                    Alarm(crow.transform);
            } 
            else
                foreach (GameObject wolf in wolfs)
                {
                    wolf.GetComponent<WolfAI>().alarm = false;
                }
        }

        public void Alarm(Transform crow)
        {
            float radius = InitialRadius + (InitialRadius / 2 * (crowsFollowing.Count - 1));

            foreach (GameObject wolf in wolfs)
            {
                if ((wolf.transform.position - crow.position).magnitude <= radius)
                {
                    wolf.GetComponent<WolfAI>().detected = true;
                    wolf.GetComponent<WolfAI>().alarm = true;
                }
                else
                    wolf.GetComponent<WolfAI>().alarm = false;
            }
        }

        // Dibuja gizmos en la escena para visualizar el área de la alarma
        private void OnDrawGizmosSelected()
        {
            if (crowsFollowing == null || crowsFollowing.Count == 0) return;

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f); // rojo semi-transparente

            foreach (GameObject crow in crowsFollowing)
            {
                if (crow == null) continue;

                float radius = InitialRadius + (InitialRadius / 2 * (crowsFollowing.Count - 1));
                Gizmos.DrawWireSphere(crow.transform.position, radius);
            }
        }
    }
}

