// Copyright 2010 Giovanni Botta

// This file is part of GeomClone.

// GeomClone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// GeomClone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeomClone.  If not, see <http://www.gnu.org/licenses/>.

#region Using directives
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace GameEngine.Helpers
{
    public class Input
    {

        #region Constructor
        public Input() { Initialize(); }
        #endregion

        #region Methods
        private void Initialize()
        {

            keysPressedLastFrame = new List<Keys>();
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            gamePadState = new GamePadState[4];
            gamePadStateLastFrame = new GamePadState[4];

            actionsToDoOnKeyPressed = new Dictionary<Keys, Action>();
            actionsToDoOnKeyTriggered = new Dictionary<Keys, Action>();
            actionsToDoOnKeyReleased = new Dictionary<Keys, Action>();
            actionsToDoOnKeyPressedToUpdate = new Dictionary<Keys, Action>();
            actionsToDoOnKeyTriggeredToUpdate = new Dictionary<Keys, Action>();
            actionsToDoOnKeyReleasedToUpdate = new Dictionary<Keys, Action>();

            actionsToDoOnButtonPressed = new Dictionary<Buttons, Action>[4];
            actionsToDoOnButtonTriggered = new Dictionary<Buttons, Action>[4];
            actionsToDoOnButtonReleased = new Dictionary<Buttons, Action>[4];
            leftStickActions = new List<ActionStick>[4];
            rightStickActions = new List<ActionStick>[4];
            actionsToDoOnButtonPressedToUpdate = new Dictionary<Buttons, Action>[4];
            actionsToDoOnButtonTriggeredToUpdate = new Dictionary<Buttons, Action>[4];
            actionsToDoOnButtonReleasedToUpdate = new Dictionary<Buttons, Action>[4];
            leftStickActionsToUpdate = new List<ActionStick>[4];
            rightStickActionsToUpdate = new List<ActionStick>[4];
            for (int ii = 0; ii < 4; ii++)
            {
                actionsToDoOnButtonPressed[ii] = new Dictionary<Buttons, Action>();
                actionsToDoOnButtonTriggered[ii] = new Dictionary<Buttons, Action>();
                actionsToDoOnButtonReleased[ii] = new Dictionary<Buttons, Action>();
                leftStickActions[ii] = new List<ActionStick>();
                rightStickActions[ii] = new List<ActionStick>();
                actionsToDoOnButtonPressedToUpdate[ii] = new Dictionary<Buttons, Action>();
                actionsToDoOnButtonTriggeredToUpdate[ii] = new Dictionary<Buttons, Action>();
                actionsToDoOnButtonReleasedToUpdate[ii] = new Dictionary<Buttons, Action>();
                leftStickActionsToUpdate[ii] = new List<ActionStick>();
                rightStickActionsToUpdate[ii] = new List<ActionStick>(); 
            }
            gamePadState[0] = GamePad.GetState(PlayerIndex.One);
            gamePadState[1] = GamePad.GetState(PlayerIndex.Two);
            gamePadState[2] = GamePad.GetState(PlayerIndex.Three);
            gamePadState[3] = GamePad.GetState(PlayerIndex.Four);
        }
        #endregion

        #region Members
        // keyboard state
        private KeyboardState keyboardState;
        private List<Keys> keysPressedLastFrame;
        // gamepads states
        private GamePadState[] gamePadState, gamePadStateLastFrame;
        // keyboard actions maps        
        private Dictionary<Keys, Action> actionsToDoOnKeyPressed;
        private Dictionary<Keys, Action> actionsToDoOnKeyTriggered;
        private Dictionary<Keys, Action> actionsToDoOnKeyReleased;
        private Dictionary<Keys, Action> actionsToDoOnKeyPressedToUpdate;
        private Dictionary<Keys, Action> actionsToDoOnKeyTriggeredToUpdate;
        private Dictionary<Keys, Action> actionsToDoOnKeyReleasedToUpdate;
        // gamepad actions maps
        private List<ActionStick>[] leftStickActions, rightStickActions;
        private Dictionary<Buttons, Action>[] actionsToDoOnButtonPressed;
        private Dictionary<Buttons, Action>[] actionsToDoOnButtonTriggered;
        private Dictionary<Buttons, Action>[] actionsToDoOnButtonReleased;
        private List<ActionStick>[] leftStickActionsToUpdate, rightStickActionsToUpdate;
        private Dictionary<Buttons, Action>[] actionsToDoOnButtonPressedToUpdate;
        private Dictionary<Buttons, Action>[] actionsToDoOnButtonTriggeredToUpdate;
        private Dictionary<Buttons, Action>[] actionsToDoOnButtonReleasedToUpdate;
        // Action to perform on button pressed, hold or released
        public delegate void Action(GameTime time);
        // Action to perform using the left or right stick
        public delegate void ActionStick(GameTime time, Vector2 stick);
        #endregion

        #region Keyboard Properties
        private bool IsSpecialKey(Keys key)
        {
            // All keys except A-Z, 0-9 and `-\[];',./= (and space) are special keys.
            // With shift pressed this also results in this keys:
            // ~_|{}:"<>? !@#$%^&*().
            int keyNum = (int)key;
            if ((keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z) ||
                (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9) ||
                key == Keys.Space || // well, space ^^
                key == Keys.OemTilde || // `~
                key == Keys.OemMinus || // -_
                key == Keys.OemPipe || // \|
                key == Keys.OemOpenBrackets || // [{
                key == Keys.OemCloseBrackets || // ]}
                key == Keys.OemSemicolon || // ;:
                key == Keys.OemQuotes || // '"
                key == Keys.OemComma || // ,<
                key == Keys.OemPeriod || // .>
                key == Keys.OemQuestion || // /?
                key == Keys.OemPlus) // =+
                return false;

            // Else is is a special key
            return true;
        }

        private char KeyToChar(Keys key, bool shiftPressed)
        {
            // If key will not be found, just return space
            char ret = ' ';
            int keyNum = (int)key;
            if (keyNum >= (int)Keys.A && keyNum <= (int)Keys.Z)
            {
                if (shiftPressed)
                    ret = key.ToString()[0];
                else
                    ret = key.ToString().ToLower()[0];
            } // if (keyNum)
            else if (keyNum >= (int)Keys.D0 && keyNum <= (int)Keys.D9 &&
                shiftPressed == false)
            {
                ret = (char)((int)'0' + (keyNum - Keys.D0));
            } // else if
            else if (key == Keys.D1 && shiftPressed)
                ret = '!';
            else if (key == Keys.D2 && shiftPressed)
                ret = '@';
            else if (key == Keys.D3 && shiftPressed)
                ret = '#';
            else if (key == Keys.D4 && shiftPressed)
                ret = '$';
            else if (key == Keys.D5 && shiftPressed)
                ret = '%';
            else if (key == Keys.D6 && shiftPressed)
                ret = '^';
            else if (key == Keys.D7 && shiftPressed)
                ret = '&';
            else if (key == Keys.D8 && shiftPressed)
                ret = '*';
            else if (key == Keys.D9 && shiftPressed)
                ret = '(';
            else if (key == Keys.D0 && shiftPressed)
                ret = ')';
            else if (key == Keys.OemTilde)
                ret = shiftPressed ? '~' : '`';
            else if (key == Keys.OemMinus)
                ret = shiftPressed ? '_' : '-';
            else if (key == Keys.OemPipe)
                ret = shiftPressed ? '|' : '\\';
            else if (key == Keys.OemOpenBrackets)
                ret = shiftPressed ? '{' : '[';
            else if (key == Keys.OemCloseBrackets)
                ret = shiftPressed ? '}' : ']';
            else if (key == Keys.OemSemicolon)
                ret = shiftPressed ? ':' : ';';
            else if (key == Keys.OemQuotes)
                ret = shiftPressed ? '"' : '\'';
            else if (key == Keys.OemComma)
                ret = shiftPressed ? '<' : '.';
            else if (key == Keys.OemPeriod)
                ret = shiftPressed ? '>' : ',';
            else if (key == Keys.OemQuestion)
                ret = shiftPressed ? '?' : '/';
            else if (key == Keys.OemPlus)
                ret = shiftPressed ? '+' : '=';

            // Return result
            return ret;
        } // KeyToChar(key)

        /// <summary>
        /// Handle keyboard input helper method to catch keyboard input
        /// for an input text. Only used to enter the player name in the game.
        /// </summary>
        /// <param name="inputText">Input text</param>
        private void HandleKeyboardInput(ref string inputText)
        {
            // Is a shift key pressed (we have to check both, left and right)
            bool isShiftPressed =
                keyboardState.IsKeyDown(Keys.LeftShift) ||
                keyboardState.IsKeyDown(Keys.RightShift);

            // Go through all pressed keys
            foreach (Keys pressedKey in keyboardState.GetPressedKeys())
                // Only process if it was not pressed last frame
                if (keysPressedLastFrame.Contains(pressedKey) == false)
                {
                    // No special key?
                    if (IsSpecialKey(pressedKey) == false &&
                        // Max. allow 32 chars
                        inputText.Length < 32)
                    {
                        // Then add the letter to our inputText.
                        // Check also the shift state!
                        inputText += KeyToChar(pressedKey, isShiftPressed);
                    } // if (IsSpecialKey)
                    else if (pressedKey == Keys.Back &&
                        inputText.Length > 0)
                    {
                        // Remove 1 character at end
                        inputText = inputText.Substring(0, inputText.Length - 1);
                    } // else if
                } // foreach if (WasKeyPressedLastFrame)
        } // HandleKeyboardInput(inputText)

        public bool KeyboardKeyTriggered(Keys key)
        {
            return keyboardState.IsKeyDown(key) &&
                keysPressedLastFrame.Contains(key) == false;
        }

        public bool KeyboardKeyPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }

        public bool KeyboardKeyReleased(Keys key)
        {
            return !keyboardState.IsKeyDown(key) && keysPressedLastFrame.Contains(key) == true;

        }
        #endregion

        #region GamePad Properties
        private bool IsGamePadConnected(int id)
        {
            return gamePadState[id].IsConnected;
        }

        public bool GamePadButtonPressed(int id, Buttons b)
        {
            return gamePadState[id].IsButtonDown(b);
        }

        public bool GamePadButtonTriggered(int id, Buttons b)
        {
            return gamePadState[id].IsButtonDown(b) && gamePadStateLastFrame[id].IsButtonUp(b);
        }

        public bool GamePadButtonReleased(int id, Buttons b)
        {
            return gamePadState[id].IsButtonUp(b) && gamePadStateLastFrame[id].IsButtonDown(b);
        }
        #endregion

        #region Update
        internal void Update(GameTime time)
        {
            // Handle keyboard input
            UpdateControls();

            keysPressedLastFrame.Clear();
            keysPressedLastFrame.AddRange(keyboardState.GetPressedKeys());
            keyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            for (int ii = 0; ii < 4; ii++)
            {
                gamePadStateLastFrame[ii] = gamePadState[ii];
            }

            gamePadState[0] = GamePad.GetState(PlayerIndex.One);
            gamePadState[1] = GamePad.GetState(PlayerIndex.Two);
            gamePadState[2] = GamePad.GetState(PlayerIndex.Three);
            gamePadState[3] = GamePad.GetState(PlayerIndex.Four);

            PerformActions(time);

        }
        private void UpdateControls()
        {
            foreach (KeyValuePair<Keys, Action> kvp in actionsToDoOnKeyPressedToUpdate)
            {
                actionsToDoOnKeyPressed[kvp.Key] = kvp.Value;
            }
            foreach (KeyValuePair<Keys, Action> kvp in actionsToDoOnKeyReleasedToUpdate)
            {
                actionsToDoOnKeyReleased[kvp.Key] = kvp.Value;
            }
            foreach (KeyValuePair<Keys, Action> kvp in actionsToDoOnKeyTriggeredToUpdate)
            {
                actionsToDoOnKeyTriggered[kvp.Key] = kvp.Value;
            }
            actionsToDoOnKeyPressedToUpdate.Clear();
            actionsToDoOnKeyReleasedToUpdate.Clear();
            actionsToDoOnKeyTriggeredToUpdate.Clear();

            for (int ii = 0; ii < 4; ii++)
            {
                foreach (ActionStick a in leftStickActionsToUpdate[ii])
                {
                    leftStickActions[ii].Add(a);
                }
                foreach (ActionStick a in rightStickActionsToUpdate[ii])
                {
                    rightStickActions[ii].Add(a);
                }
                foreach (KeyValuePair<Buttons, Action> kvp in actionsToDoOnButtonPressedToUpdate[ii])
                {
                    actionsToDoOnButtonPressed[ii][kvp.Key] = kvp.Value;
                }
                foreach (KeyValuePair<Buttons, Action> kvp in actionsToDoOnButtonReleasedToUpdate[ii])
                {
                    actionsToDoOnButtonReleased[ii][kvp.Key] = kvp.Value;
                }
                foreach (KeyValuePair<Buttons, Action> kvp in actionsToDoOnButtonTriggeredToUpdate[ii])
                {
                    actionsToDoOnButtonTriggered[ii][kvp.Key] = kvp.Value;
                }
                leftStickActionsToUpdate[ii].Clear();
                rightStickActionsToUpdate[ii].Clear();
                actionsToDoOnButtonPressedToUpdate[ii].Clear();
                actionsToDoOnButtonReleasedToUpdate[ii].Clear();
                actionsToDoOnButtonTriggeredToUpdate[ii].Clear();
            }
        }
        #endregion

        #region Input logic
        #region Keyboard
        public void SetActionOnPressed(Keys key, Action action)
        {
            actionsToDoOnKeyPressedToUpdate[key] = action;
        }
        public void SetActionOnTriggered(Keys key, Action action)
        {
            actionsToDoOnKeyTriggeredToUpdate[key] = action;
        }
        public void SetActionOnReleased(Keys key, Action action)
        {
            actionsToDoOnKeyReleasedToUpdate[key] = action;
        }
        #endregion

        #region Gamepads
        public void SetLeftStickAction(PlayerIndex id, ActionStick action)
        {
            switch (id) 
            {
                case PlayerIndex.One:
                    leftStickActionsToUpdate[0].Add(action);
                    break;
                case PlayerIndex.Two:
                    leftStickActionsToUpdate[1].Add(action);
                    break;
                case PlayerIndex.Three:
                    leftStickActionsToUpdate[2].Add(action);
                    break;
                case PlayerIndex.Four:
                    leftStickActionsToUpdate[3].Add(action);
                    break;
            }
        }

        public void SetRightStickAction(PlayerIndex id, ActionStick action)
        {
            switch (id)
            {
                case PlayerIndex.One:
                    rightStickActionsToUpdate[0].Add(action);
                    break;
                case PlayerIndex.Two:
                    rightStickActionsToUpdate[1].Add(action);
                    break;
                case PlayerIndex.Three:
                    rightStickActionsToUpdate[2].Add(action);
                    break;
                case PlayerIndex.Four:
                    rightStickActionsToUpdate[3].Add(action);
                    break;
            }
        }

        public void SetActionOnPressed(PlayerIndex id, Buttons key, Action action)
        {
            switch (id)
            {
                case PlayerIndex.One:
                    actionsToDoOnButtonPressedToUpdate[0].Add(key, action);
                    break;
                case PlayerIndex.Two:
                    actionsToDoOnButtonPressedToUpdate[1].Add(key, action);
                    break;
                case PlayerIndex.Three:
                    actionsToDoOnButtonPressedToUpdate[2].Add(key, action);
                    break;
                case PlayerIndex.Four:
                    actionsToDoOnButtonPressedToUpdate[3].Add(key, action);
                    break;
            }

        }
        public void SetActionOnTriggered(PlayerIndex id, Buttons key, Action action)
        {
            switch (id)
            {
                case PlayerIndex.One:
                    actionsToDoOnButtonTriggeredToUpdate[0].Add(key, action);
                    break;
                case PlayerIndex.Two:
                    actionsToDoOnButtonTriggeredToUpdate[1].Add(key, action);
                    break;
                case PlayerIndex.Three:
                    actionsToDoOnButtonTriggeredToUpdate[2].Add(key, action);
                    break;
                case PlayerIndex.Four:
                    actionsToDoOnButtonTriggeredToUpdate[3].Add(key, action);
                    break;
            }
        }
        public void SetActionOnReleased(PlayerIndex id, Buttons key, Action action)
        {
            switch (id)
            {
                case PlayerIndex.One:
                    actionsToDoOnButtonReleasedToUpdate[0].Add(key, action);
                    break;
                case PlayerIndex.Two:
                    actionsToDoOnButtonReleasedToUpdate[1].Add(key, action);
                    break;
                case PlayerIndex.Three:
                    actionsToDoOnButtonReleasedToUpdate[2].Add(key, action);
                    break;
                case PlayerIndex.Four:
                    actionsToDoOnButtonReleasedToUpdate[3].Add(key, action);
                    break;
            }
        }
        #endregion

        #region Actions
        private void PerformKeyboardActions(GameTime time)
        {
            foreach (KeyValuePair<Keys, Action> kvp in actionsToDoOnKeyPressed)
            {
                if (KeyboardKeyPressed(kvp.Key))
                    kvp.Value(time);
            }
            foreach (KeyValuePair<Keys, Action> kvp in actionsToDoOnKeyTriggered)
            {
                if (KeyboardKeyTriggered(kvp.Key))
                    kvp.Value(time);
            }
            foreach (KeyValuePair<Keys,Action> kvp in actionsToDoOnKeyReleased)
            {
                if (KeyboardKeyReleased(kvp.Key))
                    kvp.Value(time);
            }
        }

        private void PerformGamepadActions(GameTime time)
        {
            for (int ii = 0; ii < 4; ii++)
            {
                if (IsGamePadConnected(ii))
                {
                    foreach (KeyValuePair<Buttons, Action> kvp in actionsToDoOnButtonPressed[ii])
                    {
                        if (GamePadButtonPressed(ii, kvp.Key))
                            kvp.Value(time);
                    }
                    foreach (KeyValuePair<Buttons, Action> kvp in actionsToDoOnButtonTriggered[ii])
                    {
                        if (GamePadButtonTriggered(ii, kvp.Key))
                            kvp.Value(time);
                    }
                    foreach (KeyValuePair<Buttons, Action> kvp in actionsToDoOnButtonReleased[ii])
                    {
                        if (GamePadButtonReleased(ii, kvp.Key))
                            kvp.Value(time);
                    }
                    foreach (ActionStick a in leftStickActions[ii])
                    {
                        a(time, new Vector2(gamePadState[ii].ThumbSticks.Left.X, -gamePadState[ii].ThumbSticks.Left.Y));
                    }
                    foreach (ActionStick a in rightStickActions[ii])
                    {
                        a(time, new Vector2(gamePadState[ii].ThumbSticks.Right.X, -gamePadState[ii].ThumbSticks.Right.Y));
                    }
                }
            }
        }

        private void PerformActions(GameTime time)
        {
            PerformKeyboardActions(time);
            PerformGamepadActions(time);
        }

        public void ResetActions()
        {
            actionsToDoOnKeyPressed.Clear();
            actionsToDoOnKeyReleased.Clear();
            actionsToDoOnKeyTriggered.Clear();
            for (int ii = 0; ii < 4; ii++) 
            {
                actionsToDoOnButtonPressed[ii].Clear();
                actionsToDoOnButtonTriggered[ii].Clear();
                actionsToDoOnButtonReleased[ii].Clear();
                leftStickActions[ii].Clear();
                rightStickActions[ii].Clear();
            }
        }
        public void RemoveAction(Keys k)
        {
            actionsToDoOnKeyTriggered.Remove(k);
            actionsToDoOnKeyPressed.Remove(k);
            actionsToDoOnKeyReleased.Remove(k);
        }
        public void RemoveAction(PlayerIndex player, Buttons b)
        {
            switch (player)
            {
                case PlayerIndex.One:
                    actionsToDoOnButtonPressed[0].Remove(b);
                    actionsToDoOnButtonTriggered[0].Remove(b);
                    actionsToDoOnButtonReleased[0].Remove(b);
                    break;
                case PlayerIndex.Two:
                    actionsToDoOnButtonPressed[1].Remove(b);
                    actionsToDoOnButtonTriggered[1].Remove(b);
                    actionsToDoOnButtonReleased[1].Remove(b);
                    break;
                case PlayerIndex.Three:
                    actionsToDoOnButtonPressed[2].Remove(b);
                    actionsToDoOnButtonTriggered[2].Remove(b);
                    actionsToDoOnButtonReleased[2].Remove(b);
                    break;
                case PlayerIndex.Four:
                    actionsToDoOnButtonPressed[3].Remove(b);
                    actionsToDoOnButtonTriggered[3].Remove(b);
                    actionsToDoOnButtonReleased[3].Remove(b);
                    break;
            }
        }
        public void RemoveRightStickAction(PlayerIndex player)
        {
            switch (player)
            {
                case PlayerIndex.One:
                    rightStickActions[0].Clear();
                    break;
                case PlayerIndex.Two:
                    rightStickActions[1].Clear();
                    break;
                case PlayerIndex.Three:
                    rightStickActions[2].Clear();
                    break;
                case PlayerIndex.Four:
                    rightStickActions[3].Clear();
                    break;
            }
        }
        public void RemoveLeftStickAction(PlayerIndex player)
        {
            switch (player)
            {
                case PlayerIndex.One:
                    leftStickActions[0].Clear();
                    break;
                case PlayerIndex.Two:
                    leftStickActions[1].Clear();
                    break;
                case PlayerIndex.Three:
                    leftStickActions[2].Clear();
                    break;
                case PlayerIndex.Four:
                    leftStickActions[3].Clear();
                    break;
            }
        }
        #endregion
        #endregion
    }
}
