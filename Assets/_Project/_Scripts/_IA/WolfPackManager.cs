using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WolfPackManager : MonoBehaviour
{
    [Header("Wolf Pack")]
    [SerializeField] float packArea = 5f;
    [SerializeField] float packAngle = 240f;
    private GameObject alpha;
    internal List<GameObject> pack = new List<GameObject>();

    [Header("World data")]
    GameObject player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (pack.Count >= 2)
        {
            LookForAlpha();
            SetPositions();
        }            
    }

    private void LookForAlpha()
    {
        float distance = (alpha.transform.position - player.transform.position).magnitude;
        GameObject newAlpha = alpha;

        foreach (GameObject wolf in pack)
            if ((wolf.transform.position - player.transform.position).magnitude < distance)
                newAlpha = wolf;

        if (newAlpha != alpha)
        {
            alpha.GetComponent<WolfAI>().imAlpha = false;

            List<GameObject> savePack = new List<GameObject>(pack);
            savePack.Remove(newAlpha);

            pack.Clear();
            AddWolftoPack(newAlpha);

            foreach (GameObject wolf in savePack)
                AddWolftoPack(wolf);
        }
    }

    private void SetPositions()
    {
        Vector3 direction = (alpha.transform.position - player.transform.position).normalized;

        int row = 1;
        int col = -1;

        for (int i = 1; i < pack.Count; i++)
        {
            if (col == row)
            {
                row ++;
                col = -1;
            }
            col++;

            GameObject wolf = pack[i];

            float rotation = -packAngle/2 + (packAngle / row) * col;
            Quaternion rotationForSon = Quaternion.AngleAxis(rotation, Vector3.up);
            Vector3 directionForSon = rotationForSon * direction;
            Vector3 positionForSon = alpha.transform.position + directionForSon * row * packArea;

            WolfAI wolfAI = wolf.GetComponent<WolfAI>();
            wolfAI.posInPack = positionForSon;
        }
    }

    public void AddWolftoPack(GameObject wolf)
    {
        if (pack.Count == 0)
        {
            alpha = wolf;
            pack.Add(alpha);
            alpha.GetComponent<WolfAI>().imAlpha = true;
        }
        else
        {
            pack.Add(wolf);
        }        
    }

    public void RemoveWolf(GameObject wolf)
    {
        pack.Remove(wolf);
    }

    public void ResetPack()
    {
        pack.Clear();
        alpha.GetComponent<WolfAI>().imAlpha = false;
        alpha = null;
    }
}
