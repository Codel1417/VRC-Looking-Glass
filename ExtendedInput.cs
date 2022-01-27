/*
 MIT License

Copyright (c) 2021 jbienz

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
#define EXTENDED_INPUT_WINDOWS
#endif

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Extended key codes that are not supported by Unity.
/// </summary>
public enum ExtendedKeyCode : int
{
    /// <summary>
    /// Media Next Key
    /// </summary>
    MediaNext = 0xB0,

    /// <summary>
    /// Media Previous Key
    /// </summary>
    MediaPrevious = 0xB1,

    /// <summary>
    /// Media Play / Pause Key
    /// </summary>
    MediaPlayPause = 0xB3,
}

/// <summary>
/// Enables working with extended keys which Unity does not support.
/// </summary>
/// <remarks>
/// Currently this class only works on Windows. The method <see cref="GetKey(int)"/> is the only method that would
/// needs to be updated to support other operating systems.
/// </remarks>
static public class ExtendedInput
{
    #region Nested Types
    /// <summary>
    /// Keeps track of the state of a key and frame time.
    /// </summary>
    private class KeyState
    {
        /// <summary>
        /// The frame when the key was updated.
        /// </summary>
        public int Frame;

        /// <summary>
        /// Whether the key was pressed on that frame.
        /// </summary>
        public bool Pressed;
    }
    #endregion // Nested Types

    #region Member Variables
    static private Dictionary<int, KeyState> keyStates = new Dictionary<int, KeyState>();
    #endregion // Member Variables

    #region Imported Functions
    #if EXTENDED_INPUT_WINDOWS
    [DllImport("User32.dll")]
    static private extern short GetAsyncKeyState(int vKey);
    #endif // EXTENDED_INPUT_WINDOWS
    #endregion // Imported Functions

    /// <summary>
    /// Gets the state of the specified key for the current frame.
    /// </summary>
    /// <param name="keyCode">
    /// The key to test.
    /// </param>
    /// <param name="isPressed">
    /// Indicates if the key was pressed.
    /// </param>
    /// <param name="didChange">
    /// Indicates if <paramref name="isPressed"/> changed on the current frame.
    /// </param>
    static private void GetKeyCurrentFrame(int keyCode, out bool isPressed, out bool didChange)
    {
        // Try to get an existing state for the key code
        // If not found, create one
        KeyState keyState;
        if (!keyStates.TryGetValue(keyCode, out keyState))
        {
            keyStates[keyCode] = (keyState = new KeyState());
        }

        // If the state is already from the current frame, just reuse it
        if (keyState.Frame == Time.frameCount)
        {
            // Update the out var to use last state
            isPressed = keyState.Pressed;

            // We know it did change on this frame since the frame number is the same
            didChange = true;
        }

        // The state is not from the current frame. Get the new state.
        isPressed = GetKey(keyCode);

        // Check to see if it's changed since it was recorded.
        if (isPressed == keyState.Pressed)
        {
            // Nothing changed
            didChange = false;
        }
        else
        {
            // Store the frame when it changed
            keyState.Frame = Time.frameCount;

            // Store the new state
            keyState.Pressed = isPressed;

            // It did change on this frame
            didChange = true;
        }
    }

    /// <summary>
    /// Gets a value that indicates if the specified key is currently held.
    /// </summary>
    /// <param name="keyCode">
    /// The key code to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified key is held; otherwise <c>false</c>.
    /// </returns>
    static public bool GetKey(int keyCode)
    {
        #if EXTENDED_INPUT_WINDOWS
        byte[] result = System.BitConverter.GetBytes(GetAsyncKeyState((int)keyCode));
        if (result[0] == 1) { return true; }
        #endif // EXTENDED_INPUT_WINDOWS

        // TODO: How do we handle this on Mac and other platforms?

        // Not held
        return false;
    }

    /// <summary>
    /// Gets a value that indicates if the specified key is currently held.
    /// </summary>
    /// <param name="keyCode">
    /// The <see cref="ExtendedKeyCode"/> to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified key is held; otherwise <c>false</c>.
    /// </returns>
    static public bool GetKey(ExtendedKeyCode keyCode) { return GetKey((int)keyCode); }

    /// <summary>
    /// Gets a value that indicates if the specified key was pressed on the current frame.
    /// </summary>
    /// <param name="keyCode">
    /// The key to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified key was pressed on the current frame; otherwise <c>false</c>.
    /// </returns>
    static public bool GetKeyDown(int keyCode)
    {
        bool isPressed;
        bool didChange;
        GetKeyCurrentFrame(keyCode, out isPressed, out didChange);
        return (didChange && isPressed);
    }

    /// <summary>
    /// Gets a value that indicates if the specified key was pressed on the current frame.
    /// </summary>
    /// <param name="keyCode">
    /// The <see cref="ExtendedKeyCode"/> test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified key was pressed on the current frame; otherwise <c>false</c>.
    /// </returns>
    static public bool GetKeyDown(ExtendedKeyCode keyCode) { return GetKeyDown((int)keyCode); }

    /// <summary>
    /// Gets a value that indicates if the specified key was released on the current frame.
    /// </summary>
    /// <param name="keyCode">
    /// The key to test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified key was released on the current frame; otherwise <c>false</c>.
    /// </returns>
    static public bool GetKeyUp(int keyCode)
    {
        bool isPressed;
        bool didChange;
        GetKeyCurrentFrame(keyCode, out isPressed, out didChange);
        return (didChange && !isPressed);
    }

    /// <summary>
    /// Gets a value that indicates if the specified key was released on the current frame.
    /// </summary>
    /// <param name="keyCode">
    /// The <see cref="ExtendedKeyCode"/> test.
    /// </param>
    /// <returns>
    /// <c>true</c> if the specified key was released on the current frame; otherwise <c>false</c>.
    /// </returns>
    static public bool GetKeyUp(ExtendedKeyCode keyCode) { return GetKeyUp((int)keyCode); }
}