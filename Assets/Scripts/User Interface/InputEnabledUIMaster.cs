﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEnabledUIMaster : InputEnabledUI
{
    protected override void Update()
    {
        base.Update();
    }

    protected override void ProcessInput()
    {
        Shared();

        if (dragging)
        {
            dragVelocity = (currentInputPosition - lastFrameInputPosition) / Time.deltaTime;
        }

        lastFrameInputPosition = currentInputPosition;
    }

    // void PCInput()
    // {
    //     currentInputPosition.screen = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

    //     if (beginPress)
    //     {
    //         beginPress = false;
    //     }
    //     else
    //     {
    //         beginPress = Input.GetMouseButtonDown(0);
    //     }

    //     if (endPress)
    //     {
    //         endPress = false;
    //     }
    //     else
    //     {
    //         endPress = Input.GetMouseButtonUp(0);
    //     }

    //     pressed = Input.GetMouseButton(0);

    //     inputPoints = Input.GetKey(KeyCode.Space) ? 2 : 1;
    // }


    //bool lastState;
    // void MobileInput()
    // {
    //     inputPoints = Input.touchCount;
    //     Debug.Log(inputPoints);
    //     if (inputPoints > 0)
    //     {
    //         pressed = true;
    //         Touch touch = Input.GetTouch(0);
    //         currentInputPosition.screen = new Vector3(touch.position.x, touch.position.y);
    //     }
    //     else
    //     {
    //         pressed = false;
    //     }


    //     beginPress = !lastState && pressed;
    //     endPress = lastState && !pressed;

    //     lastState = pressed;
    // }

    bool lastState;
    void Shared()
    {
        pressed = Input.GetMouseButton(0);
        currentInputPosition.screen = Input.mousePosition;


        beginPress = !lastState && pressed;
        endPress = lastState && !pressed;
        tap = endPress && !dragging;

        if (beginPress)
        {
            initialInputPosition.screen = currentInputPosition.screen;
        }

        if (endPress)
        {
            dragging = false;
        }




        lastState = pressed;
    }
}