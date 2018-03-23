﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    // Use this for initialization
    delegate void TestDelegate();
    void Start()
    {
        TestDelegate d1 = () => { Debug.Log("1"); };
        TestDelegate d2 = () => { d1(); };
        d1 += () => { Debug.Log("2"); };

        d2();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
