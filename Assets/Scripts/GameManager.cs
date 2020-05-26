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
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace HoloBlaster {
    public class GameManager : MonoBehaviour {
        private static GameManager _instance;
        public static GameManager Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<GameManager>();
                }

                return _instance;
            }
        }

        enum GameMode {
            Menu,
            Playing,
            Credits
        }

        KeywordRecognizer voiceCommands;
        public GameObject Game;
        public GameObject Menu;
        public GameObject Credits;
        public GameObject LoggerUI;

        private GameMode currentGameMode = GameMode.Menu;
        private Logger logger;

        void Start() {
            logger = Logger.Instance;

            voiceCommands = new KeywordRecognizer(new string[] { "play", "menu" });
            voiceCommands.OnPhraseRecognized += VoiceCommands_OnPhraseRecognized;
            voiceCommands.Start();

            for (int j = 0; j < LoggerUI.gameObject.transform.childCount; j++)
            {
                GameObject child = LoggerUI.gameObject.transform.GetChild(j).gameObject;
                child.SetActive(false);
            }

            SetGameMode(GameMode.Menu);
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.M) == true) {
                SetGameMode(GameMode.Menu);
            } else if (Input.GetKeyDown(KeyCode.P) == true) {
                SetGameMode(GameMode.Playing);
            } else if (Input.GetKeyDown(KeyCode.C) == true) {
                SetGameMode(GameMode.Credits);
            }
        }

        public void ShowGame() {
            SetGameMode(GameMode.Playing);
        }

        public void ShowCredits() {
            SetGameMode(GameMode.Credits);
        }

        public void ShowMenu() {
            SetGameMode(GameMode.Menu);
        }

        public void ToggleLoggerUI() {
            for (int j = 0; j < LoggerUI.gameObject.transform.childCount; j++) {
                GameObject child = LoggerUI.gameObject.transform.GetChild(j).gameObject;
                child.SetActive(!child.activeSelf);
            }
        }

        private void VoiceCommands_OnPhraseRecognized(PhraseRecognizedEventArgs args) {
            if (args.text == "play") {
                SetGameMode(GameMode.Playing);
            } else if (args.text == "menu") {
                SetGameMode(GameMode.Menu);
            }
        }

        void SetGameMode(GameMode modeToSet) {
            logger.Log($"SetGameMode {modeToSet}");

            currentGameMode = modeToSet;

            if (currentGameMode == GameMode.Playing) {
                // Turn off the pointer because the Gun takes care of interactions
                PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOff);
            }
            if (currentGameMode == GameMode.Menu || currentGameMode == GameMode.Credits) {
                // Else turn the pointer back on 
                PointerUtils.SetHandRayPointerBehavior(PointerBehavior.Default);
            }

            Menu.SetActive(currentGameMode == GameMode.Menu);
            Game.SetActive(currentGameMode == GameMode.Playing);
            Credits.SetActive(currentGameMode == GameMode.Credits);
        }
    }
}