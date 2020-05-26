/***************************************************************************
 *   This file is part of the HoloBlaster project                          *
 *   Copyright (C) 2020 by J Paris Morgan                                  *
 *                                                                         *
 **                   GNU General Public License Usage                    **
 *                                                                         *
 *   This library is free software: you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation, either version 3 of the License, or     *
 *   (at your option) any later version.                                   *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>. *
 *                                                                         *
 *   This library is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 ****************************************************************************/
using TMPro;
using UnityEngine;

namespace HoloBlaster {
    /// <summary>
    /// Class that manages logging messages to a quad visible when playing the game
    /// </summary>
    public class Logger : MonoBehaviour {
        public TextMeshPro textMeshPro;

        private const bool shouldLogToConsole = true;

        private static Logger _Instance;
        public static Logger Instance {
            get {
                if (_Instance == null) {
                    _Instance = FindObjectOfType<Logger>();
                }

                return _Instance;
            }
        }

        void Start() {

        }

        void Update() {

        }

        /// <summary>
        /// Logs the message to the UI and the console.
        /// </summary>
        /// <remarks>
        /// This should be called from the main thread only! If you want to call it from another thread, then update this method.
        /// </remarks>
        /// <returns>The feedback text control if it found it</returns>
        public void Log(string message) {
            if (textMeshPro.isActiveAndEnabled) {
                textMeshPro.SetText(message);
            }
            if (shouldLogToConsole && !message.Contains("diff")) {
                Debug.Log(message);
            }
        }
    }
}