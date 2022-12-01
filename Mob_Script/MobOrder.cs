using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MobOrder : MonoBehaviour
{
    [SerializeField] Renderer[] selectRenderers;
    [SerializeField] Renderer[] backRenderers;
    [SerializeField] Renderer[] middleRenderers;
    [SerializeField] Renderer[] frontRenderers;
    [SerializeField] string sortingLayerName;
    int originOrder;

    /*
    public void SetMostFrontOrder(bool isMostFront)
    {
        SetOrder(isMostFront ? 100 : originOrder);
    }
    */

    void Update()
    {
        SetOrder();
    }

    public void SetOrder()
    {
        int mulOrder = (int)System.Math.Abs(gameObject.transform.position.y * 10);
        foreach (var renderer in selectRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder - 1;
        }

        foreach (var renderer in backRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }

        foreach (var renderer in middleRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 1;
        }

        foreach (var renderer in frontRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 2;
        }
    }
}